using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Queries.GetByKey;

public class ParameterGetByKeyRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<ParameterGetByKeyRequestHandler> logger)
    : IRequestHandler<ParameterGetByKeyRequest, ParameterGetByKeyResponse?>
{
    public async Task<ParameterGetByKeyResponse?> Handle(ParameterGetByKeyRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting parameter by key: {Key}", request.Key);

        var parameter = await unitOfWork.ParameterRepository.GetByKeyAsync(request.Key);

        if (parameter == null)
        {
            logger.LogWarning("Parameter not found with key: {Key}", request.Key);
            return null;
        }

        return new ParameterGetByKeyResponse
        {
            PublicId = parameter.PublicId,
            Key = parameter.Key,
            Value = parameter.Value
        };
    }
}
