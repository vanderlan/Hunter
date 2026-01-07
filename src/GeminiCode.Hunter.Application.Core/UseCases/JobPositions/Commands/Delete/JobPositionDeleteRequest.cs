using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Delete;

public record JobPositionDeleteRequest(string PublicId) : IRequest;
