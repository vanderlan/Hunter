using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetById;

public record JobPositionGetByIdRequest(string PublicId) : IRequest<JobPositionGetByIdResponse>;
