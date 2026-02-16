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
            var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
                ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

            var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.CandidateId)
                ?? throw new NotFoundException($"Candidate with ID {request.CandidateId} not found");

            var candidateFile = await unitOfWork.CandidateFileRepository
                .GetByCandidateAndFileTypeAsync(candidate.CandidateId, "Resume");
                
            if (candidateFile == null || candidateFile.File == null || candidateFile.File.FileData == null)
            {
                throw new BusinessException("Candidate does not have a CV file stored. Please upload a CV first.");
            }

            string cvText;
            using (var stream = new MemoryStream(candidateFile.File.FileData))
            {
                cvText = await pdfService.ExtractTextFromPdfAsync(stream);
            }

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

            var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(cvText, jobSummaries);
            
            var filteredMatches = analysisResult.Matches
                .OrderByDescending(m => m.MatchPercentage)
                .ToList();

            var analyzedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            candidate.AnalyzedAt = analyzedAt;
            candidate.UserId = user.UserId;
            unitOfWork.CandidateRepository.Update(candidate);
            await unitOfWork.CommitAsync();

            await SaveAnalysisHistoryAsync(candidate.CandidateId, user.UserId, analyzedAt, jobPositions.Count, filteredMatches, jobPositions);

            var hasResume = candidateFile != null;

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
                        WhatIsMissingForPerfectMatch = match.WhatIsMissingForPerfectMatch,
                        Priority = job.Priority,
                        CountryRestrictions = job.CountryRestrictions
                    };
                }).ToList()
            };

            logger.LogInformation("CV re-analysis completed: {MatchCount} matches found for candidate: {CandidateId}", 
                filteredMatches.Count, request.CandidateId);

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
            await unitOfWork.CvAnalysisRepository.DeleteByCandidateIdAsync(candidateId, userId);

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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving CV analysis history for candidate {CandidateId}", candidateId);
        }
    }
}
