using System;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class LookerStudioCredential : BaseEntity
{
    public long LookerStudioCredentialId { get; set; }
    public required string EncryptedCookieData { get; set; }
    public required string XsrfToken { get; set; }
    public required string DatasourceId { get; set; }
    public required string ReportId { get; set; }
    public string? SessionId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public required string UpdatedByUserId { get; set; }
}
