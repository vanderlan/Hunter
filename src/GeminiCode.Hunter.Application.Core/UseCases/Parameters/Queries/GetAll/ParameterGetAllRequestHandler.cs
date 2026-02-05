using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Queries.GetAll;

public class ParameterGetAllRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<ParameterGetAllRequestHandler> logger)
    : IRequestHandler<ParameterGetAllRequest, List<ParameterGetAllResponse>>
{
    public async Task<List<ParameterGetAllResponse>> Handle(ParameterGetAllRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all parameters");

        var parameters = await unitOfWork.ParameterRepository.GetAllAsync();

        var response = parameters.Select(p => new ParameterGetAllResponse
        {
            PublicId = p.PublicId,
            Key = p.Key,
            Value = p.Value
        }).ToList();

        logger.LogInformation("Retrieved {Count} parameters", response.Count);

        return response;
    }
}
