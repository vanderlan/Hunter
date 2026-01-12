using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetHistory;

public class GetCvAnalysisHistoryRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<GetCvAnalysisHistoryRequestHandler> logger)
    : IRequestHandler<GetCvAnalysisHistoryRequest, PagedList<CvAnalysisHistoryResponse>>
{
    public async Task<PagedList<CvAnalysisHistoryResponse>> Handle(
        GetCvAnalysisHistoryRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting CV analysis history. Page: {Page}, Quantity: {Quantity}", 
            request.Page, request.Quantity);

        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        var analyses = await unitOfWork.CvAnalysisRepository.GetByUserIdAsync(
            user.UserId, 
            request.Page, 
            request.Quantity);

        var totalCount = await unitOfWork.CvAnalysisRepository.GetCountByUserIdAsync(user.UserId);

        var response = analyses.Select(a => new CvAnalysisHistoryResponse
        {
            AnalysisId = a.PublicId,
            CandidateName = a.Candidate?.Name ?? "Unknown",
            CandidateEmail = a.Candidate?.Email ?? "",
            AnalyzedAt = a.AnalyzedAt.ToApplicationTime(), // Convert to configured timezone
            AnalyzedByUserName = a.User?.Name ?? "",
            TotalPositionsAnalyzed = a.TotalPositionsAnalyzed,
            MatchesFound = a.MatchesFound
        }).ToList();

        logger.LogInformation("Retrieved {Count} analysis history records", response.Count);

        return new PagedList<CvAnalysisHistoryResponse>(response, totalCount);
    }
}
