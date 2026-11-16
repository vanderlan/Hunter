namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.ConfirmPublicCandidateEmail;

public class ConfirmPublicCandidateEmailResponse
{
    public string? Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsAlreadyConfirmed { get; set; }
    public int PendingReferralRequests { get; set; }
}
