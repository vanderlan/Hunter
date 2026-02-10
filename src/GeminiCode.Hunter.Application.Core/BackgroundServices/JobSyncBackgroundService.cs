using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Commands.SyncJobs;

namespace GeminiCode.Hunter.Application.Core.BackgroundServices;

public class JobSyncBackgroundService(
    ILogger<JobSyncBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger<JobSyncBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);
    private static readonly TimeOnly StartTime = new(8, 0);
    private static readonly TimeOnly EndTime = new(22, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var currentTime = TimeOnly.FromDateTime(now);

                if (currentTime >= StartTime && currentTime <= EndTime)
                {
                    await SyncJobsAsync(stoppingToken);
                }

                var nextRunTime = CalculateNextRunTime(now);
                var delay = nextRunTime - now;

                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Job Sync Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during scheduled job synchronization");
                
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Job Sync Background Service stopped");
    }

    private async Task SyncJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            var request = new SyncJobsRequest();
            var response = await mediator.Send(request, cancellationToken);

            _logger.LogInformation(
                "Job sync completed: {TotalScraped} scraped, {New} new, {Updated} updated, {Closed} closed",
                response.TotalScraped,
                response.NewPositions,
                response.UpdatedPositions,
                response.ClosedPositions);

            if (response.NewJobTitles.Any())
            {
                _logger.LogInformation("New jobs added: {NewJobs}",
                    string.Join(", ", response.NewJobTitles));
            }

            if (response.ClosedJobTitles.Any())
            {
                _logger.LogInformation("Jobs closed: {ClosedJobs}",
                    string.Join(", ", response.ClosedJobTitles));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync jobs");
            throw;
        }
    }

    private static DateTime CalculateNextRunTime(DateTime currentTime)
    {
        var nextRun = currentTime.Add(SyncInterval);
        var nextTime = TimeOnly.FromDateTime(nextRun);

        // If next run is outside business hours, schedule for next business day at start time
        if (nextTime < StartTime)
        {
            // Same day, schedule for start time
            nextRun = currentTime.Date.Add(StartTime.ToTimeSpan());
        }
        else if (nextTime > EndTime)
        {
            // Next business day at start time
            nextRun = currentTime.Date.AddDays(1).Add(StartTime.ToTimeSpan());
        }

        // Skip weekends
        while (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Sync Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
