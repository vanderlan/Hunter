namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.RegisterPublicCandidate;

public class RegisterPublicCandidateResponse
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public bool IsConfirmationEmailSent { get; set; }
}
