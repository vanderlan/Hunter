namespace GeminiCode.Hunter.Domain.Core.Entities;

public class Prompt : BaseEntity
{
    public long PromptId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Text { get; set; }
}
