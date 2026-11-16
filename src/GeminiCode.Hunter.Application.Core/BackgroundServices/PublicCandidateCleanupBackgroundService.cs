using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.DeleteStalePublicCandidates;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.BackgroundServices;

/// <summary>
/// Removes CVs uploaded on the public page whose owners never saved or never
/// confirmed their profile. Runs shortly after startup and then every 6 hours.
/// </summary>
public class PublicCandidateCleanupBackgroundService(
    ILogger<PublicCandidateCleanupBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(InitialDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var response = await mediator.Send(new DeleteStalePublicCandidatesRequest(), stoppingToken);

                logger.LogInformation("Public candidate cleanup finished: {Count} removed", response.DeletedCandidates);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during public candidate cleanup");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
