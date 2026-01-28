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

        List<Domain.Core.Entities.CvAnalysis> analyses;
        int totalCount;

        if (currentUser.IsAdmin)
        {
            analyses = await unitOfWork.CvAnalysisRepository.GetAllAsync(
                request.Page, 
                request.Quantity);
            totalCount = await unitOfWork.CvAnalysisRepository.GetAllCountAsync();
        }
        else
        {
            analyses = await unitOfWork.CvAnalysisRepository.GetByUserIdAsync(
                user.UserId, 
                request.Page, 
                request.Quantity);
            totalCount = await unitOfWork.CvAnalysisRepository.GetCountByUserIdAsync(user.UserId);
        }

        var response = analyses.Select(a => new CvAnalysisHistoryResponse
        {
            AnalysisId = a.PublicId,
            CandidateName = a.Candidate?.Name ?? "Unknown",
            CandidateEmail = a.Candidate?.Email ?? "",
            AnalyzedAt = a.AnalyzedAt.ToApplicationTime(),
            AnalyzedByUserName = a.User?.Name ?? "",
            TotalPositionsAnalyzed = a.TotalPositionsAnalyzed,
            MatchesFound = a.MatchesFound
        }).ToList();

        return new PagedList<CvAnalysisHistoryResponse>(response, totalCount);
    }
}
