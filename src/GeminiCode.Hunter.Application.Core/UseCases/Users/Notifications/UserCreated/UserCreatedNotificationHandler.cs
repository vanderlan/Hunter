using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Notifications.UserCreated
{
    public class UserCreatedNotificationHandler(
        ILogger<UserCreatedNotificationHandler> logger,
        ICurrentUser currentUser,
        IEmailService emailService)
        : INotificationHandler<UserCreatedNotification>
    {
        public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("A notification {notificationType} has been received. Notification details: {notification}. Action performed by: {currentUserName}",
                nameof(UserCreatedNotification),
                JsonConvert.SerializeObject(notification, Formatting.Indented),
                currentUser.ToString());

            if (notification.SendWelcomeEmail)
            {
                logger.LogInformation("Sending welcome email to new user: {Email}", notification.Entity.Email);
                await emailService.SendWelcomeEmailAsync(
                    notification.Entity.Name,
                    notification.Entity.Email,
                    notification.PlainPassword);
            }

            await Task.CompletedTask;
        }
    }
}
