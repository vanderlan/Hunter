namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Queries.GetCredentialStatus;

public class GetCredentialStatusResponse
{
    public bool HasActiveCredential { get; set; }
    public string? CredentialId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public int? DaysUntilExpiration { get; set; }
    public string UpdatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string Message { get; set; }
}
