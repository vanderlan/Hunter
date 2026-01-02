using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetPaginated;

public class JobPositionGetPaginatedRequest : JobPositionFilter, IRequest<PagedList<JobPositionGetPaginatedResponse>>;
