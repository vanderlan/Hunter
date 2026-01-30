using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Create;

public class ParameterCreateRequest : IRequest<ParameterCreateResponse>
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}
