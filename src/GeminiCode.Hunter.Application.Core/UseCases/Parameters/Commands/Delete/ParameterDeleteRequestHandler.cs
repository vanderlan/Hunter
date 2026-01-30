using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Delete;

public class ParameterDeleteRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<ParameterDeleteRequestHandler> logger)
    : IRequestHandler<ParameterDeleteRequest>
{
    public async Task Handle(ParameterDeleteRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting parameter with PublicId: {PublicId}", request.PublicId);

        var parameter = await unitOfWork.ParameterRepository.GetByIdAsync(request.PublicId);
        
        if (parameter == null)
        {
            logger.LogWarning("Parameter not found with PublicId: {PublicId}", request.PublicId);
            throw new NotFoundException(request.PublicId);
        }

        await unitOfWork.ParameterRepository.DeleteAsync(request.PublicId);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Parameter deleted successfully: {PublicId}, Key: {Key}", 
            request.PublicId, parameter.Key);
    }
}

