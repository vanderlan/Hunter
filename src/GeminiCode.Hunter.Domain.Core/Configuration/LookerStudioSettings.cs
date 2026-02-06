namespace GeminiCode.Hunter.Domain.Core.Configuration;

public class LookerStudioSettings
{
    public string ExportUrl { get; set; } = string.Empty;
    public int RequestTimeoutSeconds { get; set; }
    public string EncryptionKey { get; set; } = string.Empty;
}
