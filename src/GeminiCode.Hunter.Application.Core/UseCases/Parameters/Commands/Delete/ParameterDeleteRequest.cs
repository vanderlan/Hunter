using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Delete;

public class ParameterDeleteRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
}
