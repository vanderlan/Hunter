using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.NoteAdded;

public class HiringProcessNoteAddedNotificationHandler(
    IEmailService emailService,
    ILogger<HiringProcessNoteAddedNotificationHandler> logger)
    : INotificationHandler<HiringProcessNoteAddedNotification>
{
    public async Task Handle(HiringProcessNoteAddedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling hiring process note added notification for process: {HiringProcessId}", 
                notification.HiringProcessId);

            if (string.IsNullOrWhiteSpace(notification.ReferredByUserEmail))
            {
                logger.LogWarning("No referrer email found for hiring process: {HiringProcessId}", 
                    notification.HiringProcessId);
                return;
            }

            logger.LogInformation("Sending note added notification to referrer: {Email}", 
                notification.ReferredByUserEmail);

            // Send email notification to the user who made the referral
            await emailService.SendNoteAddedNotificationAsync(
                notification.ReferredByUserEmail,
                notification.CandidateName,
                notification.JobTitle,
                notification.NoteText,
                notification.AddedByUserName);

            logger.LogInformation("Note added notification sent for hiring process: {HiringProcessId}", 
                notification.HiringProcessId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling hiring process note added notification: {ErrorMessage}", ex.Message);
        }
    }
}
