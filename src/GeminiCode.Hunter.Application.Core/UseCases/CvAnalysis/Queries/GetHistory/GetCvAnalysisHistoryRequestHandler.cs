using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetHistory;

public class GetCvAnalysisHistoryRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<GetCvAnalysisHistoryRequest, PagedList<CvAnalysisHistoryResponse>>
{
    public async Task<PagedList<CvAnalysisHistoryResponse>> Handle(
        GetCvAnalysisHistoryRequest request, 
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);

        PagedList<Domain.Core.Entities.CvAnalysis> analyses;

        if (currentUser.IsAdmin)
        {
            analyses = await unitOfWork.CvAnalysisRepository.GetAllWithFilterAsync(request);
        }
        else
        {
            analyses = await unitOfWork.CvAnalysisRepository.GetByUserIdWithFilterAsync(user.UserId, request);
        }

        var latestAnalyses = analyses.Items
            .GroupBy(a => a.Candidate?.CandidateId)
            .Select(g => g.OrderByDescending(a => a.AnalyzedAt).First())
            .ToList();

        var response = latestAnalyses.Select(a => new CvAnalysisHistoryResponse
        {
            AnalysisId = a.PublicId,
            CandidateName = a.Candidate?.Name,
            CandidateEmail = a.Candidate?.Email,
            AnalyzedAt = a.AnalyzedAt.ToApplicationTime(),
            AnalyzedByUserName = a.User?.Name,
            TotalPositionsAnalyzed = a.TotalPositionsAnalyzed,
            MatchesFound = a.MatchesFound
        }).ToList();

        return new PagedList<CvAnalysisHistoryResponse>(response, latestAnalyses.Count);
    }
}
