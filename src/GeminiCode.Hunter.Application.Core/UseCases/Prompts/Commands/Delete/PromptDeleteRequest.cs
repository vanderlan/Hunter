using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Delete;

public class PromptDeleteRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
}
