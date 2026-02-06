using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

public class JobChangesDetectedNotificationHandler(
    ILogger<JobChangesDetectedNotificationHandler> logger,
    IEmailService emailService,
    IUnitOfWork unitOfWork)
    : INotificationHandler<JobChangesDetectedNotification>
{
    public async Task Handle(JobChangesDetectedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Job changes notification received: {NewJobs} new, {ClosedJobs} closed, {UpdatedJobs} updated at {SyncTime}",
            notification.NewPositionsCount,
            notification.ClosedPositionsCount,
            notification.UpdatedPositionsCount,
            notification.SyncTimestamp);

        // Send separate notifications for new and closed positions
        await SendNewPositionsNotificationsAsync(notification);
        await SendClosedPositionsNotificationsAsync(notification);
    }

    private async Task SendNewPositionsNotificationsAsync(JobChangesDetectedNotification notification)
    {
        if (notification.NewPositionsCount == 0)
        {
            logger.LogInformation("No new positions to notify");
            return;
        }

        // For new positions, notify all users with notifications enabled
        var usersWithNotifications = await unitOfWork.UserRepository.GetUsersWithJobNotificationsEnabledAsync();
        var emailAddresses = usersWithNotifications.Select(u => u.Email).ToList();

        if (emailAddresses.Any())
        {
            logger.LogInformation("Sending new position notifications to {Count} users", emailAddresses.Count);

            await emailService.SendJobChangesNotificationAsync(
                emailAddresses,
                [.. notification.NewJobTitles],
                [],
                notification.NewJobPositions,
                [],
                DateTime.SpecifyKind(notification.SyncTimestamp, DateTimeKind.Utc));
        }
        else
        {
            logger.LogInformation("No users have job change notifications enabled for new positions");
        }
    }

    private async Task SendClosedPositionsNotificationsAsync(JobChangesDetectedNotification notification)
    {
        if (notification.ClosedPositionsCount == 0)
        {
            logger.LogInformation("No closed positions to notify");
            return;
        }

        // For closed positions, notify only admin users with notifications enabled
        var adminUsersWithNotifications = await unitOfWork.UserRepository.GetAdminUsersWithJobNotificationsEnabledAsync();
        var emailAddresses = adminUsersWithNotifications.Select(u => u.Email).ToList();

        if (emailAddresses.Any())
        {
            logger.LogInformation("Sending closed position notifications to {Count} admin users", emailAddresses.Count);

            await emailService.SendJobChangesNotificationAsync(
                emailAddresses,
                [],
                [.. notification.ClosedJobTitles],
                [],
                notification.ClosedJobPositions,
                DateTime.SpecifyKind(notification.SyncTimestamp, DateTimeKind.Utc));
        }
        else
        {
            logger.LogInformation("No admin users have job change notifications enabled for closed positions");
        }
    }
}
