using GeminiCode.Hunter.Application.Core.UseCases.Users.Notifications.UserCreated;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Test.Shared.Faker;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Notifications;

public class UserCreatedNotificationHandlerTests
{
    private readonly Mock<ILogger<UserCreatedNotificationHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly UserCreatedNotificationHandler _handler;

    public UserCreatedNotificationHandlerTests()
    {
        _loggerMock = new Mock<ILogger<UserCreatedNotificationHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _emailServiceMock = new Mock<IEmailService>();
        
        _currentUserMock.Setup(x => x.ToString()).Returns("test@user.com");
        
        _handler = new UserCreatedNotificationHandler(
            _loggerMock.Object,
            _currentUserMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSendWelcomeEmailIsFalse_ShouldNotSendEmail()
    {
        // Arrange
        var user = UserFaker.Get();
        var notification = new UserCreatedNotification(user, "TestPassword123", false);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSendWelcomeEmailIsTrue_ShouldSendEmail()
    {
        // Arrange
        var user = UserFaker.Get();
        var plainPassword = "TestPassword123";
        var notification = new UserCreatedNotification(user, plainPassword, true);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendWelcomeEmailAsync(user.Name, user.Email, plainPassword),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSendWelcomeEmailIsTrue_ShouldPassCorrectParameters()
    {
        // Arrange
        var user = UserFaker.Get();
        var plainPassword = "MySecurePassword";
        var notification = new UserCreatedNotification(user, plainPassword, true);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendWelcomeEmailAsync(
                It.Is<string>(name => name == user.Name),
                It.Is<string>(email => email == user.Email),
                It.Is<string>(pwd => pwd == plainPassword)),
            Times.Once);
    }
}
