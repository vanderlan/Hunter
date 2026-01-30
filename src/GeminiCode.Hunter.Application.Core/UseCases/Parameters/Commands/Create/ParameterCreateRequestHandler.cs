using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Create;

public class ParameterCreateRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<ParameterCreateRequestHandler> logger)
    : IRequestHandler<ParameterCreateRequest, ParameterCreateResponse>
{
    public async Task<ParameterCreateResponse> Handle(ParameterCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating parameter with Key: {Key}", request.Key);

        var existingParameter = await unitOfWork.ParameterRepository.GetByKeyAsync(request.Key);
        
        if (existingParameter != null)
        {
            logger.LogWarning("Parameter with Key '{Key}' already exists", request.Key);
            throw new ConflictException($"Parameter with key '{request.Key}' already exists");
        }

        var parameter = new Parameter
        {
            Key = request.Key.Trim().ToUpperInvariant(),
            Value = request.Value.Trim()
        };

        await unitOfWork.ParameterRepository.AddAsync(parameter);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Parameter created successfully: {PublicId}, Key: {Key}", 
            parameter.PublicId, parameter.Key);

        return new ParameterCreateResponse
        {
            PublicId = parameter.PublicId,
            Key = parameter.Key,
            Value = parameter.Value
        };
    }
}
