using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Update;

public class PromptUpdateRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<PromptUpdateRequestHandler> logger)
    : IRequestHandler<PromptUpdateRequest>
{
    public async Task Handle(PromptUpdateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating prompt with PublicId: {PublicId}", request.PublicId);

        var prompt = await unitOfWork.PromptRepository.GetByIdAsync(request.PublicId);
        
        if (prompt == null)
        {
            logger.LogWarning("Prompt not found with PublicId: {PublicId}", request.PublicId);
            throw new NotFoundException(request.PublicId);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            prompt.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            prompt.Text = request.Text;
        }

        unitOfWork.PromptRepository.Update(prompt);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Prompt updated successfully: {PublicId}, Key: {Key}", 
            request.PublicId, prompt.Key);
    }
}
