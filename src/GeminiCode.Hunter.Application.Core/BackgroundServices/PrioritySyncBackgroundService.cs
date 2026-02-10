using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.SyncJobPriorities;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Exceptions;

namespace GeminiCode.Hunter.Application.Core.BackgroundServices;

public class PrioritySyncBackgroundService(
    ILogger<PrioritySyncBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger<PrioritySyncBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(2);
    private static readonly TimeOnly StartTime = new(11, 0);  // 8:00 AM BRT = 11:00 AM UTC
    private static readonly TimeOnly EndTime = new(22, 0);    // 7:00 PM BRT = 10:00 PM UTC
    
    // Email cooldown tracking - prevents email spam for recurring issues
    private static readonly TimeSpan EmailCooldownPeriod = TimeSpan.FromHours(24);
    private static readonly Dictionary<string, DateTime> _lastEmailSentTimes = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Priority Sync Background Service started - Runs every 2 hours between 8 AM and 7 PM BRT (11 AM - 10 PM UTC) (Mon-Fri)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                
                // Check if we should sync now
                if (ShouldSyncNow(now))
                {
                    _logger.LogInformation("Starting scheduled priority sync at {currentTime}", now);
                    await SyncPrioritiesAsync(stoppingToken);
                }

                // Calculate next run time
                var nextRunTime = CalculateNextRunTime(now);
                var delay = nextRunTime - now;

                _logger.LogInformation("Next priority sync scheduled for {nextRunTime} ({delay} from now)", 
                    nextRunTime, delay);
                
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Priority Sync Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during scheduled priority synchronization");
                
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Priority Sync Background Service stopped");
    }

    private static bool ShouldSyncNow(DateTime now)
    {
        // Check if it's a weekday (Monday-Friday)
        if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Check if current time is within the sync window (11 AM - 10 PM UTC = 8 AM - 7 PM BRT)
        var currentTime = TimeOnly.FromDateTime(now);
        return currentTime >= StartTime && currentTime < EndTime;
    }

    private async Task SyncPrioritiesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            // Check if credentials exist and are valid
            var credential = await unitOfWork.LookerStudioCredentialRepository.GetActiveCredentialAsync();
            
            if (credential == null)
            {
                _logger.LogWarning("No active Looker Studio credentials found. Skipping sync.");
                await SendCredentialAlertAsync(emailService, "No Active Credentials", 
                    "Priority sync skipped: No Looker Studio credentials found. Please add credentials in the Admin panel.");
                return;
            }

            if (credential.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Looker Studio credentials expired on {expiresAt}. Skipping sync.", credential.ExpiresAt);
                await SendCredentialAlertAsync(emailService, "Credentials Expired", 
                    $"Priority sync skipped: Looker Studio credentials expired on {credential.ExpiresAt:yyyy-MM-dd HH:mm}. Please update credentials in the Admin panel.");
                return;
            }

            // Perform sync
            var request = new SyncJobPrioritiesRequest();
            var response = await mediator.Send(request, cancellationToken);

            _logger.LogInformation(
                "Priority sync completed: {totalCsv} in CSV, {matched} matched, {unmatched} unmatched, {updated} updated",
                response.TotalPrioritiesInCsv,
                response.MatchedJobs,
                response.UnmatchedJobs,
                response.UpdatedJobs);

            if (response.UnmatchedJobCodes.Any())
            {
                _logger.LogWarning("Unmatched job codes: {unmatchedJobs}",
                    string.Join(", ", response.UnmatchedJobCodes));
            }

            // Check if credentials will expire soon (within 2 days)
            var daysUntilExpiration = (credential.ExpiresAt - DateTime.UtcNow).TotalDays;
            if (daysUntilExpiration <= 2)
            {
                await SendCredentialAlertAsync(emailService, "Credentials Expiring Soon",
                    $"Warning: Looker Studio credentials will expire in {(int)daysUntilExpiration} day(s) on {credential.ExpiresAt:yyyy-MM-dd HH:mm}. Please update them in the Admin panel.");
            }
        }
        catch (ForbiddenException ex)
        {
            _logger.LogError(ex, "Looker Studio credentials are invalid");
            await SendCredentialAlertAsync(emailService, "Invalid Credentials",
                "Priority sync failed: Looker Studio credentials are invalid or expired. Please update them in the Admin panel.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync job priorities");
            await SendCredentialAlertAsync(emailService, "Sync Failed",
                $"Priority sync failed with error: {ex.Message}. Please check the logs for details.");
        }
    }

    private async Task SendCredentialAlertAsync(IEmailService emailService, string subject, string message)
    {
        try
        {
            // Check if we should send email (cooldown check to prevent spam)
            if (!ShouldSendEmail(subject))
            {
                _logger.LogInformation("Email alert skipped due to cooldown period: {subject}. Last sent: {lastSent}", 
                    subject, _lastEmailSentTimes[subject]);
                return;
            }

            await emailService.SendEmailAsync(
                $"[Hunter] Looker Studio - {subject}",
                $@"<html>
                    <body>
                        <h2>Looker Studio Priority Sync Alert</h2>
                        <p><strong>{subject}</strong></p>
                        <p>{message}</p>
                        <hr/>
                        <p><small>This is an automated message from Hunter recruitment system.</small></p>
                        <p><small>Note: You will only receive one email per issue per 24 hours to avoid spam.</small></p>
                    </body>
                   </html>");

            // Record email sent time
            _lastEmailSentTimes[subject] = DateTime.UtcNow;
            
            _logger.LogInformation("Alert email sent: {subject}", subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert email");
        }
    }

    private static bool ShouldSendEmail(string subject)
    {
        // If no email was sent for this subject before, allow it
        if (!_lastEmailSentTimes.TryGetValue(subject, out var lastSentTime))
        {
            return true;
        }

        // Check if cooldown period has passed
        var timeSinceLastEmail = DateTime.UtcNow - lastSentTime;
        return timeSinceLastEmail >= EmailCooldownPeriod;
    }

    private static DateTime CalculateNextRunTime(DateTime currentTime)
    {
        var currentTimeOnly = TimeOnly.FromDateTime(currentTime);
        
        // If we're on a weekend, move to next Monday at start time
        if (currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday)
        {
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)currentTime.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7; // If Sunday, go to next Monday
            
            return currentTime.Date.AddDays(daysUntilMonday).Add(StartTime.ToTimeSpan());
        }

        // If current time is before start time, sync at start time today
        if (currentTimeOnly < StartTime)
        {
            return currentTime.Date.Add(StartTime.ToTimeSpan());
        }

        // If current time is after end time, move to next weekday at start time
        if (currentTimeOnly >= EndTime)
        {
            var nextDay = currentTime.Date.AddDays(1);
            
            // Skip weekend
            if (nextDay.DayOfWeek == DayOfWeek.Saturday)
            {
                nextDay = nextDay.AddDays(2); // Move to Monday
            }
            else if (nextDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1); // Move to Monday
            }
            
            return nextDay.Add(StartTime.ToTimeSpan());
        }

        // Within sync window: schedule next sync in 2 hours
        var nextSync = currentTime.Add(SyncInterval);
        var nextSyncTimeOnly = TimeOnly.FromDateTime(nextSync);

        // If next sync would be after end time, move to next weekday at start time
        if (nextSyncTimeOnly >= EndTime)
        {
            var nextDay = nextSync.Date.AddDays(1);
            
            // Skip weekend
            if (nextDay.DayOfWeek == DayOfWeek.Saturday)
            {
                nextDay = nextDay.AddDays(2); // Move to Monday
            }
            else if (nextDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1); // Move to Monday
            }
            
            return nextDay.Add(StartTime.ToTimeSpan());
        }

        return nextSync;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Priority Sync Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
