using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.StoreLookerStudioCredentials;

public class StoreLookerStudioCredentialsRequest : IRequest<StoreLookerStudioCredentialsResponse>
{
    public required string CurlCommand { get; set; }

    /// <summary>
    /// IMPORTANT: Google session cookies expire after approximately 1 hour.
    /// While credentials can be stored with longer expiration for convenience,
    /// they will likely become invalid after ~60 minutes due to Google's session timeout.
    /// 
    /// A background service will attempt to keep the session alive by refreshing tokens
    /// every 45 minutes during business hours, but manual credential updates may still be needed.
    /// 
    /// Recommended: Update credentials each morning for reliable all-day operation.
    /// </summary>
    public int ExpiresInHours { get; set; } = 8; // 8 hours for a full business day
}
