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
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);
        
        if (user != null && !currentUser.Profile.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            request.UserId = user.UserId;
        }

        var candidates = await unitOfWork.CandidateRepository.ListPaginateAsync(request);

        var candidatesPaginated = new List<CandidateGetPaginatedResponse>();

        foreach (var candidate in candidates.Items)
        {
            var hiringProcesses = await unitOfWork.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId);
            var hasHiringProcess = hiringProcesses.Any();
            
            candidatesPaginated.Add(CandidateGetPaginatedResponse.From(candidate, hasHiringProcess));
        }

        return new PagedList<CandidateGetPaginatedResponse>(candidatesPaginated, candidates.Count);
    }
}
