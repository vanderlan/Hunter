using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Queries.GetPaginated;

public class UserGetPaginatedRequest : UserFilter, IRequest<PagedList<UserGetPaginatedResponse>>;