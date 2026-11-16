using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Queries.GetPaginated;

public class ReferralRequestGetPaginatedRequest : ReferralRequestFilter, IRequest<PagedList<ReferralRequestGetPaginatedResponse>>
{
}
