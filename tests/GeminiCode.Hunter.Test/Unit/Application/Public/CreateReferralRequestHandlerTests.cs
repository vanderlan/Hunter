using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.CreateReferral;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Notifications.ReferralRequestCreated;
using GeminiCode.Hunter.Domain.Core.Constants;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Test.Shared.Faker;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using HiringProcessEntity = GeminiCode.Hunter.Domain.Core.Entities.HiringProcess;

namespace GeminiCode.Hunter.Test.Unit.Application.Public;

public class CreateReferralRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IParameterService> _parameterServiceMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly CreateReferralRequestHandler _handler;

    public CreateReferralRequestHandlerTests()
    {
        _parameterServiceMock
            .Setup(x => x.GetValueAsIntAsync(ParameterConstants.MinimumMatchPercentageReferralRequest, It.IsAny<int>()))
            .ReturnsAsync(50);

        _handler = new CreateReferralRequestHandler(
            _unitOfWorkMock.Object,
            _parameterServiceMock.Object,
            _publisherMock.Object,
            Mock.Of<ILogger<CreateReferralRequestHandler>>());
    }

    [Fact]
    public async Task Handle_WhenTokenUnknown_ShouldThrowNotFound()
    {
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync("bad"))
            .ReturnsAsync((Candidate?)null);

        var request = new CreateReferralRequest { CandidateToken = "bad", JobCode = "abc" };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProfileNotSaved_ShouldThrowBusinessException()
    {
        var candidate = CandidateFaker.GetPublic(hasAcceptedTerms: false);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync(candidate.PublicToken!))
            .ReturnsAsync(candidate);

        var request = new CreateReferralRequest { CandidateToken = candidate.PublicToken!, JobCode = "abc" };

        await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));
        _unitOfWorkMock.Verify(x => x.ReferralRequestRepository.AddAsync(It.IsAny<ReferralRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMatchBelowMinimum_ShouldThrowBusinessException()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: true);
        var job = JobPositionFaker.GetPublic();
        SetupHappyPath(candidate, job, matchPercentage: 30);

        var request = new CreateReferralRequest { CandidateToken = candidate.PublicToken!, JobCode = job.PublicCode };

        await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestAlreadyExists_ShouldThrowConflict()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: true);
        var job = JobPositionFaker.GetPublic();
        SetupHappyPath(candidate, job, matchPercentage: 80);
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByCandidateAndJobPositionAsync(candidate.CandidateId, job.JobPositionId))
            .ReturnsAsync(ReferralRequestFaker.Get(candidate, job));

        var request = new CreateReferralRequest { CandidateToken = candidate.PublicToken!, JobCode = job.PublicCode };

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEmailConfirmed_ShouldCreateRequestAndNotify()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: true);
        var job = JobPositionFaker.GetPublic();
        SetupHappyPath(candidate, job, matchPercentage: 80);

        var request = new CreateReferralRequest { CandidateToken = candidate.PublicToken!, JobCode = job.PublicCode, Message = "  Please!  " };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(job.Title, response.JobTitle);
        Assert.Equal("Pending", response.Status);
        Assert.False(response.IsEmailConfirmationRequired);
        _unitOfWorkMock.Verify(x => x.ReferralRequestRepository.AddAsync(It.Is<ReferralRequest>(r =>
            r.CandidateId == candidate.CandidateId &&
            r.JobPositionId == job.JobPositionId &&
            r.Message == "Please!" &&
            r.MatchPercentage == 80)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ReferralRequestCreatedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailNotConfirmed_ShouldCreateRequestButHoldNotification()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: false);
        var job = JobPositionFaker.GetPublic();
        SetupHappyPath(candidate, job, matchPercentage: 65);

        var request = new CreateReferralRequest { CandidateToken = candidate.PublicToken!, JobCode = job.PublicCode };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.True(response.IsEmailConfirmationRequired);
        _unitOfWorkMock.Verify(x => x.ReferralRequestRepository.AddAsync(It.IsAny<ReferralRequest>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ReferralRequestCreatedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupHappyPath(Candidate candidate, JobPosition job, decimal matchPercentage)
    {
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync(candidate.PublicToken!))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.JobPositionRepository.GetByPublicCodeAsync(job.PublicCode))
            .ReturnsAsync(job);
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByCandidateAndJobPositionAsync(candidate.CandidateId, job.JobPositionId))
            .ReturnsAsync((ReferralRequest?)null);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateAndJobPositionAsync(candidate.CandidateId, job.JobPositionId))
            .ReturnsAsync((HiringProcessEntity?)null);
        _unitOfWorkMock.Setup(x => x.CvAnalysisRepository.GetLastAnalysisMatchAsync(candidate.CandidateId, job.JobPositionId))
            .ReturnsAsync(new CvAnalysisMatch { MatchPercentage = matchPercentage, JobPositionId = job.JobPositionId });
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.AddAsync(It.IsAny<ReferralRequest>()))
            .ReturnsAsync((ReferralRequest r) => r);
    }
}
