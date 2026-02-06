using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Create;

public class PromptCreateRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<PromptCreateRequestHandler> logger)
    : IRequestHandler<PromptCreateRequest, PromptCreateResponse>
{
    public async Task<PromptCreateResponse> Handle(PromptCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating prompt with Key: {Key}", request.Key);

        var existingPrompt = await unitOfWork.PromptRepository.GetByKeyAsync(request.Key);
        
        if (existingPrompt != null)
        {
            logger.LogWarning("Prompt with Key '{Key}' already exists", request.Key);
            throw new ConflictException($"Prompt with key '{request.Key}' already exists");
        }

        var prompt = new Prompt
        {
            Key = request.Key.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Text = request.Text.Trim()
        };

        await unitOfWork.PromptRepository.AddAsync(prompt);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Prompt created successfully: {PublicId}, Key: {Key}", 
            prompt.PublicId, prompt.Key);

        return new PromptCreateResponse
        {
            PublicId = prompt.PublicId,
            Key = prompt.Key,
            Name = prompt.Name,
            Text = prompt.Text
        };
    }
}
