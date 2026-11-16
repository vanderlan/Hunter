using System;
using System.Collections.Generic;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.ConfirmPublicCandidateEmail;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Notifications.ReferralRequestCreated;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Test.Shared.Faker;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Public;

public class ConfirmPublicCandidateEmailRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ConfirmPublicCandidateEmailRequestHandler _handler;

    public ConfirmPublicCandidateEmailRequestHandlerTests()
    {
        _handler = new ConfirmPublicCandidateEmailRequestHandler(
            _unitOfWorkMock.Object,
            _publisherMock.Object,
            Mock.Of<ILogger<ConfirmPublicCandidateEmailRequestHandler>>());
    }

    [Fact]
    public async Task Handle_WhenTokenUnknown_ShouldThrowNotFound()
    {
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByEmailConfirmationTokenAsync("nope"))
            .ReturnsAsync((Candidate?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new ConfirmPublicCandidateEmailRequest { Token = "nope" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ShouldThrowBusinessException()
    {
        var candidate = CandidateFaker.GetPublic();
        candidate.EmailConfirmationSentAt = DateTime.UtcNow.AddDays(-8);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByEmailConfirmationTokenAsync(candidate.EmailConfirmationToken!))
            .ReturnsAsync(candidate);

        await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.Handle(new ConfirmPublicCandidateEmailRequest { Token = candidate.EmailConfirmationToken! }, CancellationToken.None));

        Assert.False(candidate.IsEmailConfirmed);
    }

    [Fact]
    public async Task Handle_WhenAlreadyConfirmed_ShouldBeIdempotent()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: true);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByEmailConfirmationTokenAsync(candidate.EmailConfirmationToken!))
            .ReturnsAsync(candidate);

        var response = await _handler.Handle(new ConfirmPublicCandidateEmailRequest { Token = candidate.EmailConfirmationToken! }, CancellationToken.None);

        Assert.True(response.IsAlreadyConfirmed);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldConfirmAndReleasePendingRequests()
    {
        var candidate = CandidateFaker.GetPublic();
        var pending = new List<ReferralRequest> { ReferralRequestFaker.Get(candidate), ReferralRequestFaker.Get(candidate) };

        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByEmailConfirmationTokenAsync(candidate.EmailConfirmationToken!))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetPendingByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(pending);

        var response = await _handler.Handle(new ConfirmPublicCandidateEmailRequest { Token = candidate.EmailConfirmationToken! }, CancellationToken.None);

        Assert.True(candidate.IsEmailConfirmed);
        Assert.NotNull(candidate.EmailConfirmedAt);
        Assert.False(response.IsAlreadyConfirmed);
        Assert.Equal(2, response.PendingReferralRequests);
        _unitOfWorkMock.Verify(x => x.CandidateRepository.Update(candidate), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ReferralRequestCreatedNotification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
