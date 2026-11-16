using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Notifications.ReferralRequestCreated;

public class ReferralRequestCreatedNotificationHandler(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<ReferralRequestCreatedNotificationHandler> logger)
    : INotificationHandler<ReferralRequestCreatedNotification>
{
    public async Task Handle(ReferralRequestCreatedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            var referralRequest = await unitOfWork.ReferralRequestRepository.GetByIdAsync(notification.ReferralRequestId);

            if (referralRequest?.Candidate is null || referralRequest.JobPosition is null)
            {
                logger.LogWarning("Referral request {Id} not found for notification", notification.ReferralRequestId);
                return;
            }

            // Both Admins and Partners can refer, so both hear about it.
            var users = await unitOfWork.UserRepository.GetAllAsync();
            var emails = users.Select(u => u.Email).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();

            await emailService.SendReferralRequestNotificationAsync(
                emails,
                referralRequest.Candidate.Name ?? "Unknown",
                referralRequest.Candidate.Email ?? string.Empty,
                referralRequest.JobPosition.Title,
                referralRequest.MatchPercentage,
                referralRequest.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying internal users about referral request {Id}", notification.ReferralRequestId);
        }
    }
}
