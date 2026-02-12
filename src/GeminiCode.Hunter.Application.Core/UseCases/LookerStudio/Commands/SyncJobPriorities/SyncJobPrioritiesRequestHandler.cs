using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Application.Core.Services;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.SyncJobPriorities;

public class SyncJobPrioritiesRequestHandler(
    IUnitOfWork unitOfWork,
    ILookerStudioService lookerStudioService,
    ICsvParserService csvParserService,
    ILogger<SyncJobPrioritiesRequestHandler> logger)
    : IRequestHandler<SyncJobPrioritiesRequest, SyncJobPrioritiesResponse>
{
    public async Task<SyncJobPrioritiesResponse> Handle(
        SyncJobPrioritiesRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting job priorities sync from Looker Studio");

        var response = new SyncJobPrioritiesResponse();

        try
        {
            // 1. Download CSV from Looker Studio
            var csvContent = await lookerStudioService.DownloadPriorityCsvAsync(cancellationToken);
            logger.LogInformation("Successfully downloaded CSV from Looker Studio");

            // 2. Parse CSV
            var priorities = csvParserService.ParsePriorityCsv(csvContent);
            response.TotalPrioritiesInCsv = priorities.Count;
            logger.LogInformation("Parsed {count} priorities from CSV", priorities.Count);

            // 3. Get all job positions
            var allJobPositions = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();
            var jobPositionsDict = allJobPositions.ToDictionary(j => j.JobCode, j => j);

            logger.LogInformation("Found {count} job positions in database", jobPositionsDict.Count);

            // 4. Update job positions with priorities
            var now = DateTime.UtcNow;

            foreach (var priority in priorities)
            {
                if (jobPositionsDict.TryGetValue(priority.JobCode, out var jobPosition))
                {
                    // Update priority information
                    var oldPriority = jobPosition.Priority;
                    var oldCountry = jobPosition.CountryRestrictions;
                    
                    jobPosition.Priority = priority.Priority;
                    jobPosition.CountryRestrictions = priority.CountryRestrictions;
                    jobPosition.PriorityLastSyncedAt = now;

                    unitOfWork.JobPositionRepository.Update(jobPosition);

                    response.MatchedJobs++;
                    response.UpdatedJobCodes.Add(priority.JobCode);

                    logger.LogInformation("Updated job {jobCode}: Priority {oldPriority} : {newPriority}, Country {oldCountry} : {newCountry}", 
                        priority.JobCode, oldPriority ?? "(none)", priority.Priority, 
                        oldCountry ?? "(none)", priority.CountryRestrictions);
                }
                else
                {
                    response.UnmatchedJobs++;
                    response.UnmatchedJobCodes.Add(priority.JobCode);
                    
                    logger.LogWarning("Job code {jobCode} from CSV not found in database", 
                        priority.JobCode);
                }
            }

            // 5. Commit changes
            response.UpdatedJobs = response.MatchedJobs;
            await unitOfWork.CommitAsync();

            response.Message = $"Successfully synced {response.MatchedJobs} job priorities. " +
                             $"{response.UnmatchedJobs} job codes not found in database.";

            logger.LogInformation(
                "Job priorities sync completed. Matched: {matched}, Unmatched: {unmatched}, Updated: {updated}",
                response.MatchedJobs, response.UnmatchedJobs, response.UpdatedJobs);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error syncing job priorities");
            throw;
        }
    }
}
