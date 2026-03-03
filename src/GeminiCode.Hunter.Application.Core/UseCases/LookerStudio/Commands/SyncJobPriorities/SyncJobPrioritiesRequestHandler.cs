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
                Domain.Core.Entities.JobPosition? jobPosition = null;
                var matchIdentifier = !string.IsNullOrEmpty(priority.JobCode) ? priority.JobCode : priority.JobTitle ?? string.Empty;

                if (!string.IsNullOrEmpty(priority.JobCode) && jobPositionsDict.TryGetValue(priority.JobCode, out jobPosition))
                {
                    // Old pattern: matched by job code
                }
                else if (!string.IsNullOrEmpty(priority.JobTitle))
                {
                    // New pattern: match by title (Looker Studio title is part of DB job title)
                    jobPosition = allJobPositions.FirstOrDefault(j =>
                        j.Title.Contains(priority.JobTitle, StringComparison.OrdinalIgnoreCase));
                }

                if (jobPosition != null)
                {
                    // Update priority information
                    var oldPriority = jobPosition.Priority;
                    var oldCountry = jobPosition.CountryRestrictions;
                    
                    jobPosition.Priority = priority.Priority;
                    jobPosition.CountryRestrictions = priority.CountryRestrictions;
                    jobPosition.PriorityLastSyncedAt = now;

                    unitOfWork.JobPositionRepository.Update(jobPosition);

                    response.MatchedJobs++;
                    response.UpdatedJobCodes.Add(matchIdentifier);

                    logger.LogInformation("Updated job {identifier}: Priority {oldPriority} : {newPriority}, Country {oldCountry} : {newCountry}", 
                        matchIdentifier, oldPriority ?? "(none)", priority.Priority, 
                        oldCountry ?? "(none)", priority.CountryRestrictions);
                }
                else
                {
                    response.UnmatchedJobs++;
                    response.UnmatchedJobCodes.Add(matchIdentifier);
                    
                    logger.LogWarning("Job {identifier} from CSV not found in database", matchIdentifier);
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
