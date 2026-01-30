using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetAll;

public class PromptGetAllRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<PromptGetAllRequestHandler> logger)
    : IRequestHandler<PromptGetAllRequest, List<PromptGetAllResponse>>
{
    public async Task<List<PromptGetAllResponse>> Handle(PromptGetAllRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all prompts");

        var prompts = await unitOfWork.PromptRepository.GetAllAsync();

        var response = prompts.Select(p => new PromptGetAllResponse
        {
            PublicId = p.PublicId,
            Key = p.Key,
            Name = p.Name,
            Text = p.Text
        }).ToList();

        logger.LogInformation("Retrieved {Count} prompts", response.Count);

        return response;
    }
}
