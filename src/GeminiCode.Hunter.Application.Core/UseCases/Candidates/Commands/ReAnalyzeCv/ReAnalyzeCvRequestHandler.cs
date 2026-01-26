using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using CvAnalysisEntity = GeminiCode.Hunter.Domain.Core.Entities.CvAnalysis;
using CvAnalysisMatchEntity = GeminiCode.Hunter.Domain.Core.Entities.CvAnalysisMatch;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.ReAnalyzeCv;

public class ReAnalyzeCvRequestHandler(
    IUnitOfWork unitOfWork,
    IAIService openAIService,
    IPdfService pdfService,
    ICurrentUser currentUser,
    ILogger<ReAnalyzeCvRequestHandler> logger)
    : IRequestHandler<ReAnalyzeCvRequest, ReAnalyzeCvResponse>
{
    public async Task<ReAnalyzeCvResponse> Handle(ReAnalyzeCvRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Re-analyzing CV for candidate: {CandidateId} by user: {UserId}", 
            request.CandidateId, currentUser.Id);

        try
        {
            // Get current user entity to obtain UserId
            var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
                ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

            // Get existing candidate
            var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.CandidateId)
                ?? throw new NotFoundException($"Candidate with ID {request.CandidateId} not found");

            // Get the candidate's CV file
            var candidateFile = await unitOfWork.CandidateFileRepository
                .GetByCandidateAndFileTypeAsync(candidate.CandidateId, "Resume");
                
            if (candidateFile == null || candidateFile.File == null || candidateFile.File.FileData == null)
            {
                throw new BusinessException("Candidate does not have a CV file stored. Please upload a CV first.");
            }

            // Extract text from the stored PDF
            string cvText;
            using (var stream = new System.IO.MemoryStream(candidateFile.File.FileData))
            {
                cvText = await pdfService.ExtractTextFromPdfAsync(stream);
            }
            
            logger.LogInformation("Extracted {Length} characters from CV file", cvText.Length);

            // Get all active job positions
            var jobPositions = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();
            
            if (!jobPositions.Any())
            {
                logger.LogWarning("No active job positions found in database");
                return new ReAnalyzeCvResponse
                {
                    TotalPositionsAnalyzed = 0,
                    MatchesFound = 0,
                    Matches = []
                };
            }

            logger.LogInformation("Found {Count} active job positions to analyze", jobPositions.Count);

            // Prepare job summaries for AI
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

            // Analyze CV against jobs using AI
            var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(cvText, jobSummaries);

            // Filter matches > 60% and order by match percentage descending
            var filteredMatches = analysisResult.Matches
                .Where(m => m.MatchPercentage > 60)
                .OrderByDescending(m => m.MatchPercentage)
                .ToList();

            logger.LogInformation("Found {Count} matches with >60% adherence", filteredMatches.Count);

            // Update analyzed timestamp after successful analysis
            var analyzedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            candidate.AnalyzedAt = analyzedAt;
            candidate.UserId = user.UserId; // Update to current analyzing user
            unitOfWork.CandidateRepository.Update(candidate);
            await unitOfWork.CommitAsync();

            // Save analysis history (overwrites previous analysis)
            await SaveAnalysisHistoryAsync(candidate.CandidateId, user.UserId, analyzedAt, jobPositions.Count, filteredMatches, jobPositions);

            // Check if candidate has a resume file (reusing candidateFile from earlier)
            var hasResume = candidateFile != null;

            // Map to response DTOs with full job details
            var response = new ReAnalyzeCvResponse
            {
                TotalPositionsAnalyzed = jobPositions.Count,
                MatchesFound = filteredMatches.Count,
                CandidateInfo = new CandidateInfoDto
                {
                    CandidateId = candidate.PublicId,
                    Name = candidate.Name,
                    Email = candidate.Email,
                    HasResume = hasResume
                },
                Matches = filteredMatches.Select(match =>
                {
                    var job = jobPositions.First(j => j.JobCode == match.JobCode);
                    return new JobMatchDto
                    {
                        JobPositionId = job.PublicId,
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
                        NiceToHaveSkills = job.NiceToHaveSkills,
                        ReferralReason = match.ReferralReason,
                        WhatIsMissingForPerfectMatch = match.WhatIsMissingForPerfectMatch
                    };
                }).ToList()
            };

            logger.LogInformation("CV re-analysis completed successfully");

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error re-analyzing CV for candidate {CandidateId}", request.CandidateId);
            throw;
        }
    }

    private async Task SaveAnalysisHistoryAsync(
        long candidateId, 
        long userId,
        DateTime analyzedAt,
        int totalPositions, 
        List<JobMatchResult> matches,
        List<JobPosition> jobPositions)
    {
        try
        {
            logger.LogInformation("Saving CV analysis history for candidate {CandidateId}", candidateId);

            // Delete previous analysis records for this candidate (overwrite)
            logger.LogInformation("Deleting previous analysis records for candidate {CandidateId}", candidateId);
            await unitOfWork.CvAnalysisRepository.DeleteByCandidateIdAsync(candidateId);

            var cvAnalysis = new CvAnalysisEntity
            {
                CandidateId = candidateId,
                UserId = userId,
                AnalyzedAt = analyzedAt,
                TotalPositionsAnalyzed = totalPositions,
                MatchesFound = matches.Count
            };

            await unitOfWork.CvAnalysisRepository.AddAsync(cvAnalysis);
            await unitOfWork.CommitAsync();

            // Save matches
            foreach (var match in matches)
            {
                var job = jobPositions.First(j => j.JobCode == match.JobCode);
                
                var analysisMatch = new CvAnalysisMatchEntity
                {
                    CvAnalysisId = cvAnalysis.CvAnalysisId,
                    JobPositionId = job.JobPositionId,
                    MatchPercentage = match.MatchPercentage,
                    MatchReason = match.MatchReason,
                    MatchingSkills = System.Text.Json.JsonSerializer.Serialize(match.MatchingSkills),
                    MissingSkills = System.Text.Json.JsonSerializer.Serialize(match.MissingSkills),
                    JobTitle = job.Title,
                    JobCode = job.JobCode,
                    CompanyCode = job.CompanyCode,
                    JobSummary = job.Summary,
                    MustHaveSkills = job.MustHaveSkills,
                    NiceToHaveSkills = job.NiceToHaveSkills,
                    WhatIsMissingForPerfectMatch = match.WhatIsMissingForPerfectMatch
                };

                cvAnalysis.Matches.Add(analysisMatch);
            }

            await unitOfWork.CommitAsync();

            logger.LogInformation("CV analysis history saved successfully. Analysis ID: {AnalysisId}", cvAnalysis.PublicId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving CV analysis history for candidate {CandidateId}", candidateId);
            // Don't throw - we don't want to fail the entire analysis if history save fails
        }
    }
}
