namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Create;

public class ParameterCreateResponse
{
    public required string PublicId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
}
