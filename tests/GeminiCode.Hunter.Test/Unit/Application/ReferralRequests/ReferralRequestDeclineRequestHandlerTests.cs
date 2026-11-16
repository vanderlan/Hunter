using GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Decline;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Test.Shared.Faker;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.ReferralRequests;

public class ReferralRequestDeclineRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly ReferralRequestDeclineRequestHandler _handler;
    private readonly User _user = UserFaker.GetDefaultSystemUser();

    public ReferralRequestDeclineRequestHandlerTests()
    {
        _user.UserId = 7;
        _currentUserMock.Setup(x => x.Id).Returns(_user.PublicId);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(_user.PublicId)).ReturnsAsync(_user);

        _handler = new ReferralRequestDeclineRequestHandler(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            Mock.Of<ILogger<ReferralRequestDeclineRequestHandler>>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyAccepted_ShouldThrowConflict()
    {
        var referral = ReferralRequestFaker.Get();
        referral.Status = ReferralRequestStatus.Accepted;
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByIdAsync(referral.PublicId)).ReturnsAsync(referral);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new ReferralRequestDeclineRequest { PublicId = referral.PublicId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPending_ShouldMarkDeclined()
    {
        var referral = ReferralRequestFaker.Get();
        _unitOfWorkMock.Setup(x => x.ReferralRequestRepository.GetByIdAsync(referral.PublicId)).ReturnsAsync(referral);

        await _handler.Handle(new ReferralRequestDeclineRequest { PublicId = referral.PublicId, Note = "Not a fit" }, CancellationToken.None);

        Assert.Equal(ReferralRequestStatus.Declined, referral.Status);
        Assert.Equal(7, referral.ResolvedByUserId);
        Assert.NotNull(referral.ResolvedAt);
        Assert.Equal("Not a fit", referral.ResolutionNote);
        _unitOfWorkMock.Verify(x => x.ReferralRequestRepository.Update(referral), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
