using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Application.Core.Services;
using System.Text;
using System.Text.Json;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.BackgroundServices;

/// <summary>
/// Background service that proactively refreshes Looker Studio authentication tokens
/// every 45-50 minutes to prevent 401 Unauthorized errors.
/// 
/// Google session cookies typically expire after ~1 hour, so this service refreshes them
/// before expiration during business hours (Mon-Fri, 8 AM - 10 PM BRT).
/// </summary>
public class LookerStudioTokenRefreshBackgroundService(
    ILogger<LookerStudioTokenRefreshBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger<LookerStudioTokenRefreshBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    
    // Refresh every 45 minutes (before the ~1 hour Google session expiration)
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(45);
    
    // Business hours: 8 AM - 10 PM BRT (11 AM - 1 AM UTC next day)
    private static readonly TimeOnly StartTime = new(11, 0);  // 8:00 AM BRT = 11:00 AM UTC
    private static readonly TimeOnly EndTime = new(22, 0);    // 7:00 PM BRT = 10:00 PM UTC
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Looker Studio Token Refresh Service started - Refreshes every 45 minutes during business hours (Mon-Fri, 8 AM - 7 PM BRT)");

        // Initial delay of 5 minutes to let the application stabilize
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Check if we should refresh now
                if (ShouldRefreshNow(now))
                {
                    _logger.LogInformation("Starting proactive token refresh at {currentTime} UTC", now);
                    await RefreshTokensAsync(stoppingToken);
                }

                // Wait for next refresh cycle
                await Task.Delay(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Looker Studio Token Refresh Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                
                // Wait a bit before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        _logger.LogInformation("Looker Studio Token Refresh Service stopped");
    }

    private static bool ShouldRefreshNow(DateTime utcNow)
    {
        // Check if it's a weekday (Monday-Friday)
        if (utcNow.DayOfWeek == DayOfWeek.Saturday || utcNow.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Check if current time is within the sync window (11 AM - 10 PM UTC = 8 AM - 7 PM BRT)
        var currentTime = TimeOnly.FromDateTime(utcNow);
        return currentTime >= StartTime && currentTime < EndTime;
    }

    private async Task RefreshTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

        try
        {
            // Get active credential
            var credential = await unitOfWork.LookerStudioCredentialRepository.GetActiveCredentialAsync();
            
            if (credential == null)
            {
                _logger.LogWarning("No active Looker Studio credentials found. Skipping token refresh.");
                return;
            }

            // Check if credentials are about to expire (within 30 minutes)
            var timeUntilExpiration = credential.ExpiresAt - DateTime.UtcNow;
            if (timeUntilExpiration.TotalMinutes < 30)
            {
                _logger.LogWarning("Credentials expire in {minutes} minutes. Token refresh may not work - manual credential update needed soon.",
                    (int)timeUntilExpiration.TotalMinutes);
            }

            // Attempt token refresh
            bool refreshed = await PerformTokenRefreshAsync(credential, encryptionService, httpClient, cancellationToken);
            
            if (refreshed)
            {
                _logger.LogInformation("Token refresh successful for credential {credentialId}", credential.PublicId);
            }
            else
            {
                _logger.LogWarning("Token refresh failed for credential {credentialId}. Will retry in 45 minutes.", credential.PublicId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh Looker Studio tokens");
        }
    }

    private async Task<bool> PerformTokenRefreshAsync(
        LookerStudioCredential credential,
        IEncryptionService encryptionService,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(credential.SessionId))
            {
                _logger.LogWarning("SessionId not available. Cannot refresh token.");
                return false;
            }

            var cookies = encryptionService.Decrypt(credential.EncryptedCookieData);
            
            var refreshUrl = "https://lookerstudio.google.com/u/0/getFirebaseAuthToken?appVersion=20260131_0900";
            var request = new HttpRequestMessage(HttpMethod.Post, refreshUrl);
            
            // Add headers
            request.Headers.Add("accept", "application/json, text/plain, */*");
            request.Headers.Add("accept-language", "en-US,en;q=0.9,pt;q=0.8");
            request.Headers.Add("cookie", cookies);
            request.Headers.Add("origin", "https://lookerstudio.google.com");
            request.Headers.Add("referer", $"https://lookerstudio.google.com/u/0/reporting/{credential.ReportId}/page/p_bpbqbqp9ud");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36");
            request.Headers.Add("x-rap-xsrf-token", credential.XsrfToken);
            
            // Build refresh request body
            var requestBody = new
            {
                reportId = credential.ReportId,
                sessionId = credential.SessionId,
                datasourceId = new[] { credential.DatasourceId }
            };
            
            var jsonBody = JsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending proactive token refresh request to Looker Studio");
            
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            var response = await httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var refreshContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Token refresh successful. Response length: {length}", refreshContent.Length);
                
                // NOTE: Google's getFirebaseAuthToken endpoint doesn't return new cookies in the response body.
                // Instead, it validates the current session and may return new cookies in Set-Cookie headers.
                // However, since we're making this request FROM the server (not browser), we don't get Set-Cookie headers.
                // The refresh request itself keeps the session alive on Google's side.
                
                // The actual token refresh happens server-side at Google.
                // Our stored cookies remain valid as long as we keep the session active with these periodic calls.
                
                return true;
            }
            else
            {
                _logger.LogError("Token refresh failed with status: {status}. Response: {response}", 
                    response.StatusCode, 
                    await response.Content.ReadAsStringAsync(cancellationToken));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during proactive token refresh");
            return false;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Looker Studio Token Refresh Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
