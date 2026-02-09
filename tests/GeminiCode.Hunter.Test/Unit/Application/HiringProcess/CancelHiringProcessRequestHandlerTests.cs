using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Cancel;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.HiringProcess;

public class CancelHiringProcessRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<CancelHiringProcessRequestHandler>> _loggerMock;
    private readonly CancelHiringProcessRequestHandler _handler;

    public CancelHiringProcessRequestHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUser>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<CancelHiringProcessRequestHandler>>();

        _handler = new CancelHiringProcessRequestHandler(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenHiringProcessNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var publicId = "non-existent-id";
        var request = new CancelHiringProcessRequest
        {
            PublicId = publicId,
            CancelReason = "Test cancellation reason"
        };

        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync((GeminiCode.Hunter.Domain.Core.Entities.HiringProcess?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Contains(publicId, exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var publicId = "test-public-id";
        var userId = "user-id";
        
        var hiringProcess = new GeminiCode.Hunter.Domain.Core.Entities.HiringProcess
        {
            HiringProcessId = 1,
            PublicId = publicId,
            CandidateId = 1,
            JobPositionId = 1,
            Status = HiringProcessStatus.PendingAnalysis,
            CreatedByUserId = 1
        };

        var request = new CancelHiringProcessRequest
        {
            PublicId = publicId,
            CancelReason = "Test cancellation reason"
        };

        _currentUserMock.Setup(x => x.Id).Returns(userId);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync(hiringProcess);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Contains(userId, exception.Message);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteHiringProcessAndRelatedData()
    {
        // Arrange
        var publicId = "test-public-id";
        var userId = "user-id";
        
        var user = new User
        {
            UserId = 1,
            PublicId = userId,
            Name = "Test User",
            Email = "test@test.com",
            Password = "password",
            Profile = UserProfile.Admin
        };

        var hiringProcess = new Hunter.Domain.Core.Entities.HiringProcess
        {
            HiringProcessId = 1,
            PublicId = publicId,
            CandidateId = 1,
            JobPositionId = 1,
            Status = HiringProcessStatus.PendingAnalysis,
            CreatedByUserId = 1,
            Candidate = new Candidate
            {
                CandidateId = 1,
                PublicId = "candidate-id",
                UserId = 1,
                Name = "Test Candidate",
                Email = "candidate@test.com"
            },
            JobPosition = new JobPosition
            {
                JobPositionId = 1,
                PublicId = "job-id",
                Title = "Test Position",
                JobCode = "JOB-001",
                CompanyCode = "COMP-001",
                ExternalUrl = "https://example.com/job"
            }
        };

        var statusHistories = new List<HiringProcessStatusHistory>
        {
            new()
            {
                HiringProcessStatusHistoryId = 1,
                PublicId = "history-1",
                HiringProcessId = 1,
                FromStatus = HiringProcessStatus.New,
                ToStatus = HiringProcessStatus.PendingAnalysis,
                ChangedByUserId = 1
            }
        };

        var notes = new List<HiringProcessNote>
        {
            new()
            {
                HiringProcessNoteId = 1,
                PublicId = "note-1",
                HiringProcessId = 1,
                Note = "Test note",
                UserId = 1
            }
        };

        var adminUsers = new List<User>
        {
            new()
            {
                UserId = 2,
                PublicId = "admin-id",
                Name = "Admin User",
                Email = "admin@test.com",
                Password = "password",
                Profile = UserProfile.Admin
            }
        };

        var request = new CancelHiringProcessRequest
        {
            PublicId = publicId,
            CancelReason = "Test cancellation reason"
        };

        _currentUserMock.Setup(x => x.Id).Returns(userId);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync(hiringProcess);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.LoadRelatedEntitiesAsync(hiringProcess))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.HiringProcessStatusHistoryRepository.SearchByAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<HiringProcessStatusHistory, bool>>>()))
            .ReturnsAsync(statusHistories);
        _unitOfWorkMock.Setup(x => x.HiringProcessNoteRepository.SearchByAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<HiringProcessNote, bool>>>()))
            .ReturnsAsync(notes);
        _unitOfWorkMock.Setup(x => x.UserRepository.SearchByAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(adminUsers);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.HiringProcessStatusHistoryRepository.DeleteAsync(It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.HiringProcessNoteRepository.DeleteAsync(It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.HiringProcessRepository.DeleteAsync(publicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _emailServiceMock.Verify(
            x => x.SendHiringProcessCancelledNotificationAsync(
                It.Is<List<string>>(emails => emails.Count == 1 && emails[0] == "admin@test.com"),
                "Test Candidate",
                "Test Position",
                "JOB-001",
                "Test User",
                "Test cancellation reason"),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoAdminUsers_ShouldNotSendEmail()
    {
        // Arrange
        var publicId = "test-public-id";
        var userId = "user-id";
        
        var user = new User
        {
            UserId = 1,
            PublicId = userId,
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
            Status = HiringProcessStatus.PendingAnalysis,
            CreatedByUserId = 1,
            Candidate = new Candidate
            {
                CandidateId = 1,
                PublicId = "candidate-id",
                UserId = 1,
                Name = "Test Candidate",
                Email = "candidate@test.com"
            },
            JobPosition = new JobPosition
            {
                JobPositionId = 1,
                PublicId = "job-id",
                Title = "Test Position",
                JobCode = "JOB-001",
                CompanyCode = "COMP-001",
                ExternalUrl = "https://example.com/job"
            }
        };

        var request = new CancelHiringProcessRequest
        {
            PublicId = publicId,
            CancelReason = "Test cancellation reason"
        };

        _currentUserMock.Setup(x => x.Id).Returns(userId);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByIdAsync(publicId))
            .ReturnsAsync(hiringProcess);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.LoadRelatedEntitiesAsync(hiringProcess))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.HiringProcessStatusHistoryRepository.SearchByAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<HiringProcessStatusHistory, bool>>>()))
            .ReturnsAsync(new List<HiringProcessStatusHistory>());
        _unitOfWorkMock.Setup(x => x.HiringProcessNoteRepository.SearchByAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<HiringProcessNote, bool>>>()))
            .ReturnsAsync(new List<HiringProcessNote>());
        _unitOfWorkMock.Setup(x => x.UserRepository.SearchByAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>()); // No admin users

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendHiringProcessCancelledNotificationAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
}
