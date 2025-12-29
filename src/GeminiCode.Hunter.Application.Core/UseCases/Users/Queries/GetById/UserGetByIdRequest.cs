using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Queries.GetById;

public record UserGetByIdRequest(string PublicId) : IRequest<UserGetByIdResponse>;
