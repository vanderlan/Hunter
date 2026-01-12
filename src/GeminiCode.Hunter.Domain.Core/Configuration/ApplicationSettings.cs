namespace GeminiCode.Hunter.Domain.Core.Configuration;

/// <summary>
/// Application-wide settings including timezone configuration
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Target timezone ID for displaying dates/times in responses
    /// Default: "E. South America Standard Time" (Brazil UTC-3)
    /// Linux equivalent: "America/Sao_Paulo"
    /// </summary>
    public string TimeZoneId { get; set; } = "E. South America Standard Time";
    
    /// <summary>
    /// Enable automatic timezone conversion in API responses
    /// When true, all DateTime values in responses are converted to the configured timezone
    /// </summary>
    public bool ConvertTimezoneInResponses { get; set; } = true;
}
