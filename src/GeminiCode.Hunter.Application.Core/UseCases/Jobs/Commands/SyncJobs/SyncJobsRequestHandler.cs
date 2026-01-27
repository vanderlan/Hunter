using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Models;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Commands.SyncJobs;

public class SyncJobsRequestHandler(
    IUnitOfWork unitOfWork,
    IJobScraperService scraperService,
    IAIService openAIService,
    IPublisher publisher,
    ILogger<SyncJobsRequestHandler> logger)
    : IRequestHandler<SyncJobsRequest, SyncJobsResponse>
{
    public async Task<SyncJobsResponse> Handle(SyncJobsRequest request, CancellationToken cancellationToken)
    {
        var response = new SyncJobsResponse();
        var syncTimestamp = DateTime.UtcNow;
        var newJobPositions = new List<JobPositionEmailData>();
        var closedJobPositions = new List<JobPositionEmailData>();

        try
        {
            // Get current time as Unspecified for PostgreSQL compatibility
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Scrape current jobs
            var scrapedJobs = await scraperService.ScrapeJobsAsync();
            response.TotalScraped = scrapedJobs.Count;

            logger.LogInformation("Scraped {Count} jobs", scrapedJobs.Count);

            // Get all existing active positions
            var existingPositions = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();
            var existingUrlsAndTitles = existingPositions
                .ToDictionary(p => p.ExternalUrl, p => p.Title);

            // Identify new positions
            var newJobs = scrapedJobs
                .Where(s => !existingUrlsAndTitles.ContainsKey(s.Url))
                .ToList();

            // Process new positions
            foreach (var newJob in newJobs)
            {
                try
                {
                    // Double-check if position already exists in database (prevents race conditions)
                    var existingPosition = await unitOfWork.JobPositionRepository.GetByExternalUrlAsync(newJob.Url);
                    if (existingPosition != null)
                        continue;

                    // Scrape raw HTML content
                    var scrapedDetails = await scraperService.ScrapeJobDetailsAsync(newJob.Url);

                    // Use OpenAI to extract and enrich job details
                    var enrichedDetails = await openAIService.EnrichJobDetailsAsync(
                        newJob.Title,
                        scrapedDetails.RawHtmlContent
                    );

                    // Create new position
                    var position = new JobPosition
                    {
                        Title = newJob.Title,
                        ExternalUrl = newJob.Url,
                        CompanyCode = newJob.CompanyCode,
                        JobCode = newJob.JobCode,
                        Summary = enrichedDetails.Summary,
                        Responsibilities = enrichedDetails.Responsibilities,
                        Requirements = enrichedDetails.Requirements,
                        MustHaveSkills = enrichedDetails.MustHaveSkills,
                        NiceToHaveSkills = enrichedDetails.NiceToHaveSkills,
                        IsActive = true,
                        LastCheckedAt = now
                    };

                    await unitOfWork.JobPositionRepository.AddAsync(position);
                    await unitOfWork.CommitAsync();

                    response.NewPositions++;
                    response.NewJobTitles.Add(newJob.Title);
                    
                    // Add to email data list
                    newJobPositions.Add(new JobPositionEmailData
                    {
                        PublicId = position.PublicId,
                        Title = position.Title,
                        JobCode = position.JobCode,
                        CompanyCode = position.CompanyCode
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing new job: {Title}", newJob.Title);
                }
            }

            // Identify closed positions
            var scrapedUrls = scrapedJobs.Select(s => s.Url).ToHashSet();
            var closedPositions = existingPositions
                .Where(p => !scrapedUrls.Contains(p.ExternalUrl))
                .ToList();

            // Mark positions as closed
            foreach (var position in closedPositions)
            {
                await unitOfWork.JobPositionRepository.MarkAsClosedAsync(position.JobPositionId);
                response.ClosedPositions++;
                response.ClosedJobTitles.Add(position.Title);
                
                // Add to email data list
                closedJobPositions.Add(new JobPositionEmailData
                {
                    PublicId = position.PublicId,
                    Title = position.Title,
                    JobCode = position.JobCode,
                    CompanyCode = position.CompanyCode
                });
            }

            // Update LastCheckedAt ONLY for active positions with same title
            var stillActivePositions = existingPositions
                .Where(p => scrapedUrls.Contains(p.ExternalUrl))
                .ToList();

            foreach (var position in stillActivePositions)
            {
                // Find the scraped job with same URL
                var scrapedJob = scrapedJobs.FirstOrDefault(s => s.Url == position.ExternalUrl);

                if (scrapedJob != null && scrapedJob.Title == position.Title)
                {
                    if (string.IsNullOrWhiteSpace(position.Requirements))
                    {
                        try
                        {
                            var scrapedDetails = await scraperService.ScrapeJobDetailsAsync(position.ExternalUrl);
                            var enrichedDetails = await openAIService.EnrichJobDetailsAsync(
                                position.Title,
                                scrapedDetails.RawHtmlContent
                            );

                            position.Summary = enrichedDetails.Summary;
                            position.Responsibilities = enrichedDetails.Responsibilities;
                            position.Requirements = enrichedDetails.Requirements;
                            position.MustHaveSkills = enrichedDetails.MustHaveSkills;
                            position.NiceToHaveSkills = enrichedDetails.NiceToHaveSkills;
                            position.LastCheckedAt = now;
                            
                            unitOfWork.JobPositionRepository.Update(position);
                            response.UpdatedPositions++;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error enriching position {Title} with Requirements", position.Title);
                            position.LastCheckedAt = now;
                            unitOfWork.JobPositionRepository.Update(position);
                        }
                    }
                    else
                    {
                        position.LastCheckedAt = now;
                        unitOfWork.JobPositionRepository.Update(position);
                        response.UpdatedPositions++;
                    }
                }
                else if (scrapedJob != null && scrapedJob.Title != position.Title)
                {
                    logger.LogWarning(
                        "Job title changed from '{OldTitle}' to '{NewTitle}'. Treating as closed and new.",
                        position.Title, scrapedJob.Title
                    );

                    await unitOfWork.JobPositionRepository.MarkAsClosedAsync(position.JobPositionId);
                    response.ClosedPositions++;
                    response.ClosedJobTitles.Add(position.Title);
                    
                    // Add to closed email data list
                    closedJobPositions.Add(new JobPositionEmailData
                    {
                        PublicId = position.PublicId,
                        Title = position.Title,
                        JobCode = position.JobCode,
                        CompanyCode = position.CompanyCode
                    });
                }
            }

            // Commit all changes
            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Job synchronization completed. New: {New}, Updated: {Updated}, Closed: {Closed}",
                response.NewPositions,
                response.UpdatedPositions,
                response.ClosedPositions
            );

            // Publish notification if there are changes
            if (response.NewPositions > 0 || response.ClosedPositions > 0)
            {
                await publisher.Publish(new JobChangesDetectedNotification
                {
                    NewPositionsCount = response.NewPositions,
                    ClosedPositionsCount = response.ClosedPositions,
                    UpdatedPositionsCount = response.UpdatedPositions,
                    TotalScraped = response.TotalScraped,
                    NewJobTitles = response.NewJobTitles,
                    ClosedJobTitles = response.ClosedJobTitles,
                    NewJobPositions = newJobPositions,
                    ClosedJobPositions = closedJobPositions,
                    SyncTimestamp = syncTimestamp
                }, cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during job synchronization");
            throw;
        }
    }
}
