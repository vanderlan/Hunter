using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetByKey;

public class PromptGetByKeyRequest : IRequest<PromptGetByKeyResponse?>
{
    public string Key { get; set; } = string.Empty;
}
