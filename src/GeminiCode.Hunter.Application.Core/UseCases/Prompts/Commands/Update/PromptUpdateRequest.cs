using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Update;

public class PromptUpdateRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Text { get; set; }
}
