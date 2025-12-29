using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvRequestHandler(
    IUnitOfWork unitOfWork,
    IPdfService pdfService,
    IOpenAIService openAIService,
    ILogger<AnalyzeCvRequestHandler> logger)
    : IRequestHandler<AnalyzeCvRequest, AnalyzeCvResponse>
{
    public async Task<AnalyzeCvResponse> Handle(AnalyzeCvRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting CV analysis for file: {FileName}", request.CvFile.FileName);

        try
        {
            // Validate file
            if (request.CvFile.ContentType != "application/pdf")
            {
                throw new InvalidOperationException("Only PDF files are supported");
            }

            if (request.CvFile.Length == 0)
            {
                throw new InvalidOperationException("The uploaded file is empty");
            }

            // Extract text from PDF
            string cvText;
            using (var stream = request.CvFile.OpenReadStream())
            {
                cvText = await pdfService.ExtractTextFromPdfAsync(stream);
            }

            logger.LogInformation("Extracted {Length} characters from CV", cvText.Length);

            // Get all active job positions
            var jobPositions = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();
            
            if (!jobPositions.Any())
            {
                logger.LogWarning("No active job positions found in database");
                return new AnalyzeCvResponse
                {
                    TotalPositionsAnalyzed = 0,
                    MatchesFound = 0,
                    Matches = new List<JobMatchDto>()
                };
            }

            logger.LogInformation("Found {Count} active job positions to analyze", jobPositions.Count);

            // Prepare job summaries for OpenAI
            var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
                j.JobCode,
                j.Title,
                j.CompanyCode,
                j.Summary,
                j.Responsibilities,
                j.Requirements,
                j.MustHaveSkills,
                j.NiceToHaveSkills
            )).ToList();

            // Analyze CV against jobs using OpenAI
            var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(cvText, jobSummaries);

            // Filter matches > 20% and order by match percentage descending
            var filteredMatches = analysisResult.Matches
                .Where(m => m.MatchPercentage > 20)
                .OrderByDescending(m => m.MatchPercentage)
                .ToList();

            logger.LogInformation("Found {Count} matches with >20% adherence", filteredMatches.Count);

            // Map to response DTOs with full job details
            var response = new AnalyzeCvResponse
            {
                TotalPositionsAnalyzed = jobPositions.Count,
                MatchesFound = filteredMatches.Count,
                Matches = filteredMatches.Select(match =>
                {
                    var job = jobPositions.First(j => j.JobCode == match.JobCode);
                    return new JobMatchDto
                    {
                        JobCode = match.JobCode,
                        JobTitle = match.JobTitle,
                        CompanyCode = job.CompanyCode,
                        ExternalUrl = job.ExternalUrl,
                        MatchPercentage = match.MatchPercentage,
                        MatchReason = match.MatchReason,
                        MatchingSkills = match.MatchingSkills,
                        MissingSkills = match.MissingSkills,
                        Summary = job.Summary,
                        MustHaveSkills = job.MustHaveSkills,
                        NiceToHaveSkills = job.NiceToHaveSkills
                    };
                }).ToList()
            };

            logger.LogInformation("CV analysis completed successfully");

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing CV");
            throw;
        }
    }
}
