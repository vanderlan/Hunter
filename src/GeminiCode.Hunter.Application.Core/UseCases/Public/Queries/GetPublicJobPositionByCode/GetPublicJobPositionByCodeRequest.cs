using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositionByCode;

public record GetPublicJobPositionByCodeRequest(string Code) : IRequest<GetPublicJobPositionByCodeResponse?>;
