using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Notifications;

public class JobChangesDetectedNotificationHandlerTests
{
    private readonly Mock<ILogger<JobChangesDetectedNotificationHandler>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly JobChangesDetectedNotificationHandler _handler;

    public JobChangesDetectedNotificationHandlerTests()
    {
        _loggerMock = new Mock<ILogger<JobChangesDetectedNotificationHandler>>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);

        _handler = new JobChangesDetectedNotificationHandler(
            _loggerMock.Object,
            _emailServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOnlyNewPositions_ShouldNotifyAllUsersWithNotificationsEnabled()
    {
        // Arrange
        var notification = new JobChangesDetectedNotification
        {
            NewPositionsCount = 2,
            ClosedPositionsCount = 0,
            UpdatedPositionsCount = 0,
            NewJobTitles = new List<string> { "Senior Developer", "Junior Developer" },
            ClosedJobTitles = new List<string>(),
            SyncTimestamp = DateTime.UtcNow
        };

        var allUsers = new List<User>
        {
            new User { UserId = 1, Name = "Admin User", Email = "admin@test.com", Profile = UserProfile.Admin, Password = "test", ReceiveJobChangeNotifications = true },
            new User { UserId = 2, Name = "Partner User", Email = "partner@test.com", Profile = UserProfile.Partner, Password = "test", ReceiveJobChangeNotifications = true }
        };

        _userRepositoryMock
            .Setup(x => x.GetUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(allUsers);

        _userRepositoryMock
            .Setup(x => x.GetAdminUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(allUsers.Where(u => u.Profile == UserProfile.Admin).ToList());

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.Is<List<string>>(emails => emails.Count == 2 && emails.Contains("admin@test.com") && emails.Contains("partner@test.com")),
                It.Is<List<string>>(newJobs => newJobs.Count == 2),
                It.Is<List<string>>(closedJobs => closedJobs.Count == 0),
                It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOnlyClosedPositions_ShouldNotifyOnlyAdminUsers()
    {
        // Arrange
        var notification = new JobChangesDetectedNotification
        {
            NewPositionsCount = 0,
            ClosedPositionsCount = 2,
            UpdatedPositionsCount = 0,
            NewJobTitles = new List<string>(),
            ClosedJobTitles = new List<string> { "Old Position 1", "Old Position 2" },
            SyncTimestamp = DateTime.UtcNow
        };

        var allUsers = new List<User>
        {
            new User { UserId = 1, Name = "Admin User", Email = "admin@test.com", Profile = UserProfile.Admin, Password = "test", ReceiveJobChangeNotifications = true },
            new User { UserId = 2, Name = "Partner User", Email = "partner@test.com", Profile = UserProfile.Partner, Password = "test", ReceiveJobChangeNotifications = true }
        };

        var adminUsers = allUsers.Where(u => u.Profile == UserProfile.Admin).ToList();

        _userRepositoryMock
            .Setup(x => x.GetUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(allUsers);

        _userRepositoryMock
            .Setup(x => x.GetAdminUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(adminUsers);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.Is<List<string>>(emails => emails.Count == 1 && emails.Contains("admin@test.com")),
                It.Is<List<string>>(newJobs => newJobs.Count == 0),
                It.Is<List<string>>(closedJobs => closedJobs.Count == 2),
                It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBothNewAndClosedPositions_ShouldSendSeparateNotifications()
    {
        // Arrange
        var notification = new JobChangesDetectedNotification
        {
            NewPositionsCount = 1,
            ClosedPositionsCount = 1,
            UpdatedPositionsCount = 0,
            NewJobTitles = new List<string> { "New Position" },
            ClosedJobTitles = new List<string> { "Closed Position" },
            SyncTimestamp = DateTime.UtcNow
        };

        var allUsers = new List<User>
        {
            new User { UserId = 1, Name = "Admin User", Email = "admin@test.com", Profile = UserProfile.Admin, Password = "test", ReceiveJobChangeNotifications = true },
            new User { UserId = 2, Name = "Partner User", Email = "partner@test.com", Profile = UserProfile.Partner, Password = "test", ReceiveJobChangeNotifications = true }
        };

        var adminUsers = allUsers.Where(u => u.Profile == UserProfile.Admin).ToList();

        _userRepositoryMock
            .Setup(x => x.GetUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(allUsers);

        _userRepositoryMock
            .Setup(x => x.GetAdminUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(adminUsers);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        // Should send notification for new positions to all users
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.Is<List<string>>(emails => emails.Count == 2),
                It.Is<List<string>>(newJobs => newJobs.Count == 1),
                It.Is<List<string>>(closedJobs => closedJobs.Count == 0),
                It.IsAny<DateTime>()),
            Times.Once);

        // Should send notification for closed positions to admin users only
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.Is<List<string>>(emails => emails.Count == 1 && emails.Contains("admin@test.com")),
                It.Is<List<string>>(newJobs => newJobs.Count == 0),
                It.Is<List<string>>(closedJobs => closedJobs.Count == 1),
                It.IsAny<DateTime>()),
            Times.Once);

        // Total of 2 email sends
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenNoUsersWithNotificationsEnabled_ShouldNotSendEmails()
    {
        // Arrange
        var notification = new JobChangesDetectedNotification
        {
            NewPositionsCount = 1,
            ClosedPositionsCount = 1,
            UpdatedPositionsCount = 0,
            NewJobTitles = new List<string> { "New Position" },
            ClosedJobTitles = new List<string> { "Closed Position" },
            SyncTimestamp = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(new List<User>());

        _userRepositoryMock
            .Setup(x => x.GetAdminUsersWithJobNotificationsEnabledAsync())
            .ReturnsAsync(new List<User>());

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoChanges_ShouldNotSendEmails()
    {
        // Arrange
        var notification = new JobChangesDetectedNotification
        {
            NewPositionsCount = 0,
            ClosedPositionsCount = 0,
            UpdatedPositionsCount = 5,
            NewJobTitles = new List<string>(),
            ClosedJobTitles = new List<string>(),
            SyncTimestamp = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendJobChangesNotificationAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>()),
            Times.Never);
    }
}
