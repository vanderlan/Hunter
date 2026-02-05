namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.StoreLookerStudioCredentials;

public class StoreLookerStudioCredentialsResponse
{
    public string CredentialId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsValid { get; set; }
    public string Message { get; set; }
}
