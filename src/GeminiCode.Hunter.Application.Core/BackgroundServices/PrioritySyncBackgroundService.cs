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
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(1);
    private static readonly TimeOnly SyncTime = new(9, 0); // 9 AM daily

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Priority Sync Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var currentTime = TimeOnly.FromDateTime(now);

                // Check if it's time to sync (9 AM)
                if (currentTime.Hour == SyncTime.Hour && currentTime.Minute == SyncTime.Minute)
                {
                    await SyncPrioritiesAsync(stoppingToken);
                }

                // Calculate next run time (next day at 9 AM)
                var nextRunTime = CalculateNextRunTime(now);
                var delay = nextRunTime - now;

                _logger.LogDebug("Next priority sync scheduled for {nextRunTime}", nextRunTime);
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
        catch (UnauthorizedUserException ex)
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
            await emailService.SendEmailAsync(
                $"[Hunter] Looker Studio - {subject}",
                $@"<html>
                    <body>
                        <h2>Looker Studio Priority Sync Alert</h2>
                        <p><strong>{subject}</strong></p>
                        <p>{message}</p>
                        <hr/>
                        <p><small>This is an automated message from Hunter recruitment system.</small></p>
                    </body>
                   </html>");

            _logger.LogInformation("Alert email sent: {subject}", subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert email");
        }
    }

    private static DateTime CalculateNextRunTime(DateTime currentTime)
    {
        var nextRun = currentTime.Date.AddDays(1).Add(SyncTime.ToTimeSpan());

        // Skip weekends
        while (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Priority Sync Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
