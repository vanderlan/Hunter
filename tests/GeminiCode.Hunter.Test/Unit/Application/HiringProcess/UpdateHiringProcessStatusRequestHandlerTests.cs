using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.HiringProcess;

public class UpdateHiringProcessStatusRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<UpdateHiringProcessStatusRequestHandler>> _loggerMock;
    private readonly UpdateHiringProcessStatusRequestHandler _handler;

    public UpdateHiringProcessStatusRequestHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<UpdateHiringProcessStatusRequestHandler>>();

        _handler = new UpdateHiringProcessStatusRequestHandler(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenStatusIsSameAsCurrentStatus_ShouldThrowBusinessException()
    {
        // Arrange
        var publicId = "test-public-id";
        var currentStatus = HiringProcessStatus.ReferredByPartner;
        
        var user = new User
        {
            UserId = 1,
            PublicId = "user-public-id",
            Name = "Test User",
            Email = "test@test.com",
            Password = "password",
            Profile = UserProfile.Admin
        };

        var hiringProcess = new GeminiCode.Hunter.Domain.Core.Entities.HiringProcess
        {
            HiringProcessId = 1,
            PublicId = publicId,
            CandidateId = 1,
            JobPositionId = 1,
            Status = currentStatus,
            CreatedByUserId = 1
        };

        var request = new UpdateHiringProcessStatusRequest
        {
            PublicId = publicId,
            NewStatus = currentStatus,
            Notes = "Test notes"
        };

        _currentUserMock.Setup(x => x.Id).Returns(user.PublicId);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(user.PublicId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync(hiringProcess);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Equal("Cannot update to the same status. Please select a different status.", exception.Message);
        
        // Verify that Update and AddAsync were never called
        _unitOfWorkMock.Verify(x => x.HiringProcessRepository.Update(It.IsAny<GeminiCode.Hunter.Domain.Core.Entities.HiringProcess>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.HiringProcessStatusHistoryRepository.AddAsync(It.IsAny<HiringProcessStatusHistory>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStatusIsDifferent_ShouldUpdateSuccessfully()
    {
        // Arrange
        var publicId = "test-public-id";
        var currentStatus = HiringProcessStatus.ReferredByPartner;
        var newStatus = HiringProcessStatus.Screening;
        
        var user = new User
        {
            UserId = 1,
            PublicId = "user-public-id",
            Name = "Test User",
            Email = "test@test.com",
            Password = "password",
            Profile = UserProfile.Admin
        };

        var hiringProcess = new GeminiCode.Hunter.Domain.Core.Entities.HiringProcess
        {
            HiringProcessId = 1,
            PublicId = publicId,
            CandidateId = 1,
            JobPositionId = 1,
            Status = currentStatus,
            CreatedByUserId = 1
        };

        var request = new UpdateHiringProcessStatusRequest
        {
            PublicId = publicId,
            NewStatus = newStatus,
            Notes = "Moving to screening"
        };

        _currentUserMock.Setup(x => x.Id).Returns(user.PublicId);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(user.PublicId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync(hiringProcess);
        _unitOfWorkMock.Setup(x => x.HiringProcessStatusHistoryRepository.AddAsync(It.IsAny<HiringProcessStatusHistory>()))
            .ReturnsAsync((HiringProcessStatusHistory h) => h);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(newStatus, hiringProcess.Status);
        Assert.Equal(user.UserId, hiringProcess.LastUpdatedByUserId);
        
        _unitOfWorkMock.Verify(x => x.HiringProcessRepository.Update(hiringProcess), Times.Once);
        _unitOfWorkMock.Verify(x => x.HiringProcessStatusHistoryRepository.AddAsync(
            It.Is<HiringProcessStatusHistory>(h => 
                h.HiringProcessId == hiringProcess.HiringProcessId &&
                h.FromStatus == currentStatus &&
                h.ToStatus == newStatus &&
                h.ChangedByUserId == user.UserId &&
                h.Notes == request.Notes)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHiringProcessNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var publicId = "non-existent-id";
        var request = new UpdateHiringProcessStatusRequest
        {
            PublicId = publicId,
            NewStatus = HiringProcessStatus.Screening
        };

        var user = new User
        {
            UserId = 1,
            PublicId = "user-public-id",
            Name = "Test User",
            Email = "test@test.com",
            Password = "password",
            Profile = UserProfile.Admin
        };

        _currentUserMock.Setup(x => x.Id).Returns(user.PublicId);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(user.PublicId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync((GeminiCode.Hunter.Domain.Core.Entities.HiringProcess?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Contains(publicId, exception.Message);
    }
}
