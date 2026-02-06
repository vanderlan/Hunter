using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetById;

public class GetCvAnalysisDetailRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<GetCvAnalysisDetailRequestHandler> logger)
    : IRequestHandler<GetCvAnalysisDetailRequest, CvAnalysisDetailResponse>
{
    public async Task<CvAnalysisDetailResponse> Handle(
        GetCvAnalysisDetailRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting CV analysis detail: {AnalysisId}", request.AnalysisId);

        var analysis = await unitOfWork.CvAnalysisRepository.GetWithMatchesAsync(request.AnalysisId)
            ?? throw new NotFoundException($"Analysis with ID {request.AnalysisId} not found");

        if (!currentUser.IsAdmin)
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);
            if (analysis.UserId != user.UserId)
            {
                throw new UnauthorizedUserException("You are not authorized to view this CV analysis");
            }
        }

        var response = new CvAnalysisDetailResponse
        {
            AnalysisId = analysis.PublicId,
            CandidateInfo = new CandidateInfoDto
            {
                CandidateId = analysis.Candidate?.PublicId ?? "",
                Name = analysis.Candidate?.Name,
                Email = analysis.Candidate?.Email,
                HasResume = true
            },
            AnalyzedAt = analysis.AnalyzedAt,
            AnalyzedByUserName = analysis.User?.Name ?? "",
            TotalPositionsAnalyzed = analysis.TotalPositionsAnalyzed,
            MatchesFound = analysis.MatchesFound,
            Matches = analysis.Matches.Select(m => new JobMatchDto
            {
                JobPositionId = m.JobPosition?.PublicId ?? "",
                JobCode = m.JobCode,
                JobTitle = m.JobTitle,
                CompanyCode = m.CompanyCode,
                ExternalUrl = m.JobPosition?.ExternalUrl ?? "",
                MatchPercentage = m.MatchPercentage,
                MatchReason = m.MatchReason,
                MatchingSkills = JsonSerializer.Deserialize<List<string>>(m.MatchingSkills) ?? [],
                MissingSkills = JsonSerializer.Deserialize<List<string>>(m.MissingSkills) ?? [],
                Summary = m.JobSummary,
                MustHaveSkills = m.MustHaveSkills,
                NiceToHaveSkills = m.NiceToHaveSkills,
                WhatIsMissingForPerfectMatch = m.WhatIsMissingForPerfectMatch,
                Priority = m.JobPosition.Priority
            }).OrderByDescending(m => m.MatchPercentage).ToList()
        };

        logger.LogInformation("Retrieved CV analysis detail with {Count} matches", response.Matches.Count);

        return response;
    }
}
