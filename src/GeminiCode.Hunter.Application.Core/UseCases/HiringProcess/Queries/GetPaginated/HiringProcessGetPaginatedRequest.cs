using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;

public class HiringProcessGetPaginatedRequest : HiringProcessFilter, IRequest<PagedList<HiringProcessGetPaginatedResponse>>
{
}
