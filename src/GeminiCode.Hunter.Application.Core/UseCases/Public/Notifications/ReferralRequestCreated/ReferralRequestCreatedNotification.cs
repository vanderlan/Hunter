using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Notifications.ReferralRequestCreated;

/// <summary>
/// Raised when a referral request becomes visible to internal users: either on
/// creation (email already confirmed) or when the candidate confirms the email.
/// </summary>
public class ReferralRequestCreatedNotification : INotification
{
    public string ReferralRequestId { get; set; } = string.Empty;
}
