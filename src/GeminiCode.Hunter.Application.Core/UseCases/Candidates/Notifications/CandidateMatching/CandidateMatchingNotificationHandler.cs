using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Notifications.CandidateMatching;

public class CandidateMatchingNotificationHandler(
    IUnitOfWork unitOfWork,
    IOpenAIService openAIService,
    IEmailService emailService,
    ILogger<CandidateMatchingNotificationHandler> logger)
    : INotificationHandler<JobChangesDetectedNotification>
{
    public async Task Handle(JobChangesDetectedNotification notification, CancellationToken cancellationToken)
    {
        // Only process if there are new positions
        if (notification.NewPositionsCount == 0)
        {
            logger.LogInformation("No new positions detected, skipping candidate matching");
            return;
        }

        logger.LogInformation("Starting candidate matching for {Count} new positions", notification.NewPositionsCount);

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
                    position.JobCode,
                    position.Title,
                    position.CompanyCode,
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
                        // Analyze this candidate against this position
                        var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(
                            candidate.CvText,
                            new List<JobPositionSummary> { positionSummary }
                        );

                        var match = analysisResult.Matches.FirstOrDefault();
                        
                        if (match != null && match.MatchPercentage > 20)
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
                        JobCode = position.JobCode,
                        CompanyCode = position.CompanyCode,
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

            var body = BuildEmailBody(matchResults, syncTime);

            await emailService.SendCandidateMatchingNotificationAsync(matchResults, syncTime);

            logger.LogInformation("Candidate matching notification email sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send candidate matching notification email");
        }
    }

    private static string BuildEmailBody(List<PositionMatchResult> matchResults, DateTime syncTime)
    {
        var body = new System.Text.StringBuilder();

        body.AppendLine("???????????????????????????????????????");
        body.AppendLine("  CANDIDATE MATCHES FOR NEW POSITIONS");
        body.AppendLine("???????????????????????????????????????");
        body.AppendLine();
        body.AppendLine($"Analysis Time: {syncTime:yyyy-MM-dd HH:mm:ss}");
        body.AppendLine($"New Positions Analyzed: {matchResults.Count}");
        body.AppendLine($"Total Candidate Matches: {matchResults.Sum(r => r.TotalMatches)}");
        body.AppendLine();

        foreach (var result in matchResults.OrderByDescending(r => r.TotalMatches))
        {
            body.AppendLine("???????????????????????????????????????");
            body.AppendLine($"Position: {result.JobTitle}");
            body.AppendLine($"Job Code: {result.JobCode}");
            body.AppendLine($"Company: {result.CompanyCode}");
            body.AppendLine($"Matching Candidates: {result.TotalMatches}");
            body.AppendLine();

            foreach (var candidate in result.Candidates.Take(10)) // Top 10 per position
            {
                body.AppendLine($"  ? {candidate.CandidateName} - {candidate.MatchPercentage}%");
                body.AppendLine($"    Email: {candidate.CandidateEmail}");
                if (!string.IsNullOrWhiteSpace(candidate.CandidatePhone))
                {
                    body.AppendLine($"    Phone: {candidate.CandidatePhone}");
                }
                body.AppendLine($"    Match Reason: {candidate.MatchReason}");
                body.AppendLine();
            }

            if (result.TotalMatches > 10)
            {
                body.AppendLine($"  ... and {result.TotalMatches - 10} more candidates");
                body.AppendLine();
            }
        }

        body.AppendLine("???????????????????????????????????????");
        body.AppendLine();
        body.AppendLine("This is an automated notification from Hunter CV Analyzer.");
        body.AppendLine("Contact candidates to schedule interviews.");

        return body.ToString();
    }
}

public class PositionMatchResult
{
    public string JobTitle { get; set; }
    public string JobCode { get; set; }
    public string CompanyCode { get; set; }
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
