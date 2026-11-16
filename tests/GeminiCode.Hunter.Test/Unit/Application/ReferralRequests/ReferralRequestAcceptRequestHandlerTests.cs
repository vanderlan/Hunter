using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;
using GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Accept;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Test.Shared.Faker;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.ReferralRequests;

public class ReferralRequestAcceptRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly ReferralRequestAcceptRequestHandler _handler;
    private readonly User _user = UserFaker.GetDefaultSystemUser();

    public ReferralRequestAcceptRequestHandlerTests()
    {
        _user.UserId = 42;
        _currentUserMock.Setup(x => x.Id).Returns(_user.PublicId);
        _currentUserMock.Setup(x => x.Name).Returns(_user.Name);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(_user.PublicId)).ReturnsAsync(_user);

        _handler = new ReferralRequestAcceptRequestHandler(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _mediatorMock.Object,
            Mock.Of<ILogger<ReferralRequestAcceptRequestHandler>>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFound()
    {
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByIdAsync("x")).ReturnsAsync((ReferralRequest?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new ReferralRequestAcceptRequest { PublicId = "x" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAlreadyResolved_ShouldThrowConflict()
    {
        var referral = ReferralRequestFaker.Get();
        referral.Status = ReferralRequestStatus.Declined;
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByIdAsync(referral.PublicId)).ReturnsAsync(referral);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new ReferralRequestAcceptRequest { PublicId = referral.PublicId }, CancellationToken.None));

        _mediatorMock.Verify(x => x.Send(It.IsAny<StartHiringProcessRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCandidateEmailNotConfirmed_ShouldThrowBusinessException()
    {
        var referral = ReferralRequestFaker.Get(CandidateFaker.GetPublic(isEmailConfirmed: false));
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByIdAsync(referral.PublicId)).ReturnsAsync(referral);

        await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.Handle(new ReferralRequestAcceptRequest { PublicId = referral.PublicId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPending_ShouldStartHiringProcessAndMarkAccepted()
    {
        var referral = ReferralRequestFaker.Get();
        var candidatePublicId = referral.Candidate!.PublicId;
        var jobPublicId = referral.JobPosition!.PublicId;
        var hiringProcess = HiringProcessFaker.Get();
        hiringProcess.HiringProcessId = 777;

        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByIdAsync(referral.PublicId)).ReturnsAsync(referral);
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<StartHiringProcessRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StartHiringProcessResponse { PublicId = hiringProcess.PublicId, Status = "PendingAnalysis" });
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(hiringProcess.PublicId)).ReturnsAsync(hiringProcess);

        var response = await _handler.Handle(new ReferralRequestAcceptRequest { PublicId = referral.PublicId, Note = "Known from a past project" }, CancellationToken.None);

        Assert.Equal("Accepted", response.Status);
        Assert.Equal(hiringProcess.PublicId, response.HiringProcessId);
        Assert.Equal(ReferralRequestStatus.Accepted, referral.Status);
        Assert.Equal(42, referral.ResolvedByUserId);
        Assert.Equal(777, referral.HiringProcessId);
        Assert.Equal("Known from a past project", referral.ResolutionNote);

        _mediatorMock.Verify(x => x.Send(It.Is<StartHiringProcessRequest>(r =>
            r.CandidateId == candidatePublicId &&
            r.JobPositionId == jobPublicId &&
            r.ReferralReason!.Contains("public CV match page") &&
            r.ReferralReason.Contains("Known from a past project")), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.ReferralRequestRepository.Update(referral), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
