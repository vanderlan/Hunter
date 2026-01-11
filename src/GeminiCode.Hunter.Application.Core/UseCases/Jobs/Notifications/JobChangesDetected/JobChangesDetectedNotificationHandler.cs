using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        logger.LogDebug(
            "Job changes details: {Notification}",
            JsonConvert.SerializeObject(notification, Formatting.Indented));

        // Send email notification if there are changes
        if (notification.NewPositionsCount > 0 || notification.ClosedPositionsCount > 0)
        {
            var usersWithNotifications = await unitOfWork.UserRepository.GetUsersWithJobNotificationsEnabledAsync();
            var emailAddresses = usersWithNotifications.Select(u => u.Email).ToList();

            if (emailAddresses.Any())
            {
                logger.LogInformation("Sending job change notifications to {Count} users asynchronously", emailAddresses.Count);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await emailService.SendJobChangesNotificationAsync(
                            emailAddresses,
                            [.. notification.NewJobTitles],
                            [.. notification.ClosedJobTitles],
                            DateTime.SpecifyKind(notification.SyncTimestamp, DateTimeKind.Utc));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send job changes notification email asynchronously");
                    }
                }, cancellationToken);
            }
            else
            {
                logger.LogInformation("No users have job change notifications enabled");
            }
        }
        else
        {
            logger.LogInformation("No new or closed positions, skipping email notification");
        }
    }
}
