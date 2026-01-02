using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvRequestHandler(
    IUnitOfWork unitOfWork,
    IPdfService pdfService,
    IOpenAIService openAIService,
    ICvInfoExtractionService cvInfoExtractionService,
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

            // Extract basic candidate information
            var (_, email, phone) = cvInfoExtractionService.ExtractBasicInfo(cvText);
            
            // Extract name using OpenAI (more accurate than regex)
            var name = await cvInfoExtractionService.ExtractNameWithAIAsync(cvText);

            // Store or update candidate information BEFORE analysis
            await StoreCandidateAsync(name, email, phone, cvText);

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

            // Filter matches > 60% and order by match percentage descending
            var filteredMatches = analysisResult.Matches
                .Where(m => m.MatchPercentage > 60)
                .OrderByDescending(m => m.MatchPercentage)
                .ToList();

            logger.LogInformation("Found {Count} matches with >60% adherence", filteredMatches.Count);

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

    private async Task StoreCandidateAsync(string? name, string? email, string? phone, string cvText)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // Check if candidate already exists by email
        Candidate? existingCandidate = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            existingCandidate = await unitOfWork.CandidateRepository.GetByEmailAsync(email);
        }

        if (existingCandidate != null)
        {
            // Update existing candidate with latest CV
            logger.LogInformation("Updating existing candidate with email: {Email}", email);
            
            existingCandidate.Name = name;
            existingCandidate.PhoneNumber = phone;
            existingCandidate.CvText = cvText;
            existingCandidate.AnalyzedAt = now;
            
            unitOfWork.CandidateRepository.Update(existingCandidate);
        }
        else
        {
            // Create new candidate
            logger.LogInformation("Creating new candidate");
            
            var candidate = new Candidate
            {
                Name = name,
                Email = email,
                PhoneNumber = phone,
                CvText = cvText,
                AnalyzedAt = now
            };

            await unitOfWork.CandidateRepository.AddAsync(candidate);
        }

        await unitOfWork.CommitAsync();
        
        logger.LogInformation("Candidate information stored successfully");
    }
}
