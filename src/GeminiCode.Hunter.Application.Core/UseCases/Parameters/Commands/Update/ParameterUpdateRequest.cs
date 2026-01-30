using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Update;

public class ParameterUpdateRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
