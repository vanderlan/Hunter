using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetPaginated;

public class CandidateGetPaginatedRequest : CandidateFilter, IRequest<PagedList<CandidateGetPaginatedResponse>>;
