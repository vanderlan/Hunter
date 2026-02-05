using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetByKey;

public class PromptGetByKeyRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<PromptGetByKeyRequestHandler> logger)
    : IRequestHandler<PromptGetByKeyRequest, PromptGetByKeyResponse?>
{
    public async Task<PromptGetByKeyResponse?> Handle(PromptGetByKeyRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting prompt by key: {Key}", request.Key);

        var prompt = await unitOfWork.PromptRepository.GetByKeyAsync(request.Key);

        if (prompt == null)
        {
            logger.LogWarning("Prompt not found with key: {Key}", request.Key);
            return null;
        }

        return new PromptGetByKeyResponse
        {
            PublicId = prompt.PublicId,
            Key = prompt.Key,
            Name = prompt.Name,
            Text = prompt.Text
        };
    }
}
