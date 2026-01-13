using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.StatusChanged;

public class HiringProcessStatusChangedNotificationHandler(
    IEmailService emailService,
    ILogger<HiringProcessStatusChangedNotificationHandler> logger)
    : INotificationHandler<HiringProcessStatusChangedNotification>
{
    public async Task Handle(HiringProcessStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling hiring process status changed notification for process: {HiringProcessId}", 
                notification.HiringProcessId);

            if (string.IsNullOrWhiteSpace(notification.ReferredByUserEmail))
            {
                logger.LogWarning("No referrer email found for hiring process: {HiringProcessId}", 
                    notification.HiringProcessId);
                return;
            }

            logger.LogInformation("Sending status change notification to referrer: {Email}", 
                notification.ReferredByUserEmail);

            // Send email notification to the user who made the referral
            await emailService.SendStatusChangeNotificationAsync(
                notification.ReferredByUserEmail,
                notification.CandidateName,
                notification.JobTitle,
                notification.OldStatus,
                notification.NewStatus,
                notification.ChangedByUserName,
                notification.Notes);

            logger.LogInformation("Status change notification sent for hiring process: {HiringProcessId}", 
                notification.HiringProcessId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling hiring process status changed notification: {ErrorMessage}", ex.Message);
        }
    }
}
