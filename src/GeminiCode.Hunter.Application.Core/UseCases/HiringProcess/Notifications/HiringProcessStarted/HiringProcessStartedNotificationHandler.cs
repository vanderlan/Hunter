using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.HiringProcessStarted;

public class HiringProcessStartedNotificationHandler(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<HiringProcessStartedNotificationHandler> logger)
    : INotificationHandler<HiringProcessStartedNotification>
{
    public async Task Handle(HiringProcessStartedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling hiring process started notification for process: {HiringProcessId}", 
                notification.HiringProcessId);

            // Get all admin users
            var adminUsers = await unitOfWork.UserRepository
                .SearchByAsync(u => u.Profile == UserProfile.Admin);

            if (!adminUsers.Any())
            {
                logger.LogWarning("No admin users found to notify about hiring process start");
                return;
            }

            var adminEmails = adminUsers.Select(u => u.Email).ToList();

            logger.LogInformation("Sending candidate referral notification to {Count} admin(s)", adminEmails.Count);

            // Send email notification to all admins
            await emailService.SendCandidateReferralNotificationAsync(
                adminEmails,
                notification.CandidateName,
                notification.CandidateEmail,
                notification.JobTitle,
                notification.JobCode,
                notification.StartedByUserName,
                notification.ReferralReason);

            logger.LogInformation("Hiring process started notification completed for process: {HiringProcessId}", 
                notification.HiringProcessId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling hiring process started notification: {ErrorMessage}", ex.Message);
        }
    }
}
