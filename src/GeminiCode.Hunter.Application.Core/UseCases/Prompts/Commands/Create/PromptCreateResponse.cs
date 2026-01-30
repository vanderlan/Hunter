namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Create;

public class PromptCreateResponse
{
    public required string PublicId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Text { get; set; }
}
