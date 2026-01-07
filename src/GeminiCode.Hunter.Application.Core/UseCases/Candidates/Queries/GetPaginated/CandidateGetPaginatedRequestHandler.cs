using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetPaginated;

public class CandidateGetPaginatedRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CandidateGetPaginatedRequest, PagedList<CandidateGetPaginatedResponse>>
{
    public async Task<PagedList<CandidateGetPaginatedResponse>> Handle(
        CandidateGetPaginatedRequest request, 
        CancellationToken cancellationToken)
    {
        // Get current user to check if admin
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);
        
        // If not admin, filter by current user's candidates only
        if (user != null && !currentUser.Profile.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            request.UserId = user.UserId;
        }

        var candidates = await unitOfWork.CandidateRepository.ListPaginateAsync(request);

        var candidatesPaginated = candidates.Items
            .Select(candidate => (CandidateGetPaginatedResponse)candidate)
            .ToList();

        return new PagedList<CandidateGetPaginatedResponse>(candidatesPaginated, candidates.Count);
    }
}
