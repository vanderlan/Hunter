using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.Delete;

public record UserDeleteRequest(string PublicId) : IRequest;
