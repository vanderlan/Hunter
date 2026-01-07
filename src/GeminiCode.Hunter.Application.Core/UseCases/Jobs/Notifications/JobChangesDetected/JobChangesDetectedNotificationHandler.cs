using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

public class JobChangesDetectedNotificationHandler(
    ILogger<JobChangesDetectedNotificationHandler> logger,
    IEmailService emailService)
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
            await emailService.SendJobChangesNotificationAsync(
                [.. notification.NewJobTitles],
                [.. notification.ClosedJobTitles],
                DateTime.SpecifyKind(notification.SyncTimestamp, DateTimeKind.Utc));
        }
        else
        {
            logger.LogInformation("No new or closed positions, skipping email notification");
        }
    }
}
