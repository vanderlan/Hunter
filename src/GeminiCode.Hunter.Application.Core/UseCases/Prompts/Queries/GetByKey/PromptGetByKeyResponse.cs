namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetByKey;

public class PromptGetByKeyResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
