using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;
using GeminiCode.Hunter.Domain.Core.Features;
using GeminiCode.Hunter.Domain.Core.Constants;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Notifications.CandidateMatching;

public class CandidateMatchingNotificationHandler(
IUnitOfWork unitOfWork,
IAIService openAIService,
IPdfService pdfService,
IEmailService emailService,
IParameterService parameterService,
IFeatureManager featureManager,
ILogger<CandidateMatchingNotificationHandler> logger)
: INotificationHandler<JobChangesDetectedNotification>
{
    public async Task Handle(JobChangesDetectedNotification notification, CancellationToken cancellationToken)
    {
        // Check if feature is enabled
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.AutoCandidateMatching);
        
        if (!isFeatureEnabled)
        {
            logger.LogInformation("Auto candidate matching feature is disabled. Skipping candidate analysis.");
            return;
        }

        // Only process if there are new positions
        if (notification.NewPositionsCount == 0)
        {
            logger.LogInformation("No new positions detected, skipping candidate matching");
            return;
        }

        logger.LogInformation("Starting candidate matching for {Count} new positions", notification.NewPositionsCount);

        // Get minimum match percentage from database (default to 80 if not configured)
        var minimumMatchPercentage = await parameterService.GetValueAsIntAsync(
            ParameterConstants.MinimumMatchPercentageAutoMatching, 
            80);
        
        logger.LogInformation("Using minimum match percentage: {Percentage}%", minimumMatchPercentage);

        try
        {
            // Get all candidates from database
            var candidates = await unitOfWork.CandidateRepository.SearchByAsync(c => true);
            var candidatesList = candidates.ToList();

            if (!candidatesList.Any())
            {
                logger.LogWarning("No candidates found in database");
                return;
            }

            logger.LogInformation("Found {Count} candidates to analyze", candidatesList.Count);

            // Get the new job positions
            var jobPositions = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();
            var newPositions = jobPositions.Where(j => notification.NewJobTitles.Contains(j.Title)).ToList();

            if (!newPositions.Any())
            {
                logger.LogWarning("Could not find new positions in database");
                return;
            }

            // Analyze candidates against each new position
            var matchResults = new List<PositionMatchResult>();

            foreach (var position in newPositions)
            {
                logger.LogInformation("Analyzing candidates for position: {Title}", position.Title);

                var positionSummary = new JobPositionSummary(
                    position.Title,
                    position.Summary,
                    position.Responsibilities,
                    position.Requirements,
                    position.MustHaveSkills,
                    position.NiceToHaveSkills
                );

                var candidateMatches = new List<CandidateMatchInfo>();

                foreach (var candidate in candidatesList)
                {
                    try
                    {
                        // Get the candidate's CV file
                        var candidateFile = await unitOfWork.CandidateFileRepository.GetByCandidateAndFileTypeAsync(candidate.CandidateId, "Resume");
                        
                        if (candidateFile == null || candidateFile.File == null || candidateFile.File.FileData == null)
                        {
                            logger.LogWarning("Candidate {CandidateId} does not have a CV file stored, skipping", candidate.CandidateId);
                            continue;
                        }

                        // Extract text from the stored PDF
                        string cvText;
                        using (var stream = new MemoryStream(candidateFile.File.FileData))
                        {
                            cvText = await pdfService.ExtractTextFromPdfAsync(stream);
                        }

                        // Analyze this candidate against this position
                        var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(
                            cvText,
                            [positionSummary]
                        );

                        var match = analysisResult.Matches.FirstOrDefault();
                        
                        if (match != null && match.MatchPercentage >= minimumMatchPercentage)
                        {
                            candidateMatches.Add(new CandidateMatchInfo
                            {
                                CandidateName = candidate.Name ?? "Unknown",
                                CandidateEmail = candidate.Email ?? "No email",
                                CandidatePhone = candidate.PhoneNumber,
                                MatchPercentage = (double)match.MatchPercentage,
                                MatchReason = match.MatchReason,
                                MatchingSkills = match.MatchingSkills,
                                MissingSkills = match.MissingSkills
                            });

                            logger.LogInformation("Found match: {Candidate} - {Percentage}% for {Position}",
                                candidate.Name ?? "Unknown",
                                match.MatchPercentage,
                                position.Title);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error analyzing candidate {CandidateId} for position {PositionId}",
                            candidate.CandidateId, position.JobPositionId);
                    }
                }

                if (candidateMatches.Any())
                {
                    matchResults.Add(new PositionMatchResult
                    {
                        JobTitle = position.Title,
                        ExternalUrl = position.ExternalUrl,
                        TotalMatches = candidateMatches.Count,
                        Candidates = candidateMatches.OrderByDescending(c => c.MatchPercentage).ToList()
                    });
                }
            }

            // Send email with matching results
            if (matchResults.Any())
            {
                await SendMatchingEmailAsync(matchResults, notification.SyncTimestamp);
                logger.LogInformation("Candidate matching email sent successfully");
            }
            else
            {
                logger.LogInformation("No candidate matches found for new positions");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during candidate matching process");
        }
    }

    private async Task SendMatchingEmailAsync(List<PositionMatchResult> matchResults, DateTime syncTime)
    {
        try
        {
            logger.LogInformation("Sending candidate matching notification email");

            await emailService.SendCandidateMatchingNotificationAsync(matchResults, syncTime);

            logger.LogInformation("Candidate matching notification email sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send candidate matching notification email");
        }
    }
}

public class PositionMatchResult
{
    public string JobTitle { get; set; }
    public string ExternalUrl { get; set; }
    public int TotalMatches { get; set; }
    public List<CandidateMatchInfo> Candidates { get; set; }
}

public class CandidateMatchInfo
{
    public string CandidateName { get; set; }
    public string CandidateEmail { get; set; }
    public string? CandidatePhone { get; set; }
    public double MatchPercentage { get; set; }
    public string MatchReason { get; set; }
    public List<string> MatchingSkills { get; set; }
    public List<string> MissingSkills { get; set; }
}
