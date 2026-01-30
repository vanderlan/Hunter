using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Delete;

public class PromptDeleteRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<PromptDeleteRequestHandler> logger)
    : IRequestHandler<PromptDeleteRequest>
{
    public async Task Handle(PromptDeleteRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting prompt with PublicId: {PublicId}", request.PublicId);

        var prompt = await unitOfWork.PromptRepository.GetByIdAsync(request.PublicId);
        
        if (prompt == null)
        {
            logger.LogWarning("Prompt not found with PublicId: {PublicId}", request.PublicId);
            throw new NotFoundException(request.PublicId);
        }

        await unitOfWork.PromptRepository.DeleteAsync(request.PublicId);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Prompt deleted successfully: {PublicId}, Key: {Key}", 
            request.PublicId, prompt.Key);
    }
}

