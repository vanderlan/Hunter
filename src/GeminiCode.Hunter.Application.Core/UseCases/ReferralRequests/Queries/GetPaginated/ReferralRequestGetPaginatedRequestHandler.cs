using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Queries.GetPaginated;

public class ReferralRequestGetPaginatedRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ReferralRequestGetPaginatedRequest, PagedList<ReferralRequestGetPaginatedResponse>>
{
    public async Task<PagedList<ReferralRequestGetPaginatedResponse>> Handle(ReferralRequestGetPaginatedRequest request, CancellationToken cancellationToken)
    {
        var paged = await unitOfWork.ReferralRequestRepository.ListPaginateAsync(request);

        var items = paged.Items.Select(ReferralRequestGetPaginatedResponse.From).ToList();

        return new PagedList<ReferralRequestGetPaginatedResponse>(items, paged.Count);
    }
}
