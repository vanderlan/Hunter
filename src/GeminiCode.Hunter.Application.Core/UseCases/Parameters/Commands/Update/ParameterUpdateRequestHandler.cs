using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Update;

public class ParameterUpdateRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<ParameterUpdateRequestHandler> logger)
    : IRequestHandler<ParameterUpdateRequest>
{
    public async Task Handle(ParameterUpdateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating parameter with PublicId: {PublicId}", request.PublicId);

        var parameter = await unitOfWork.ParameterRepository.GetByIdAsync(request.PublicId);
        
        if (parameter == null)
        {
            logger.LogWarning("Parameter not found with PublicId: {PublicId}", request.PublicId);
            throw new NotFoundException(request.PublicId);
        }

        parameter.Value = request.Value.Trim();

        unitOfWork.ParameterRepository.Update(parameter);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Parameter updated successfully: {PublicId}, Key: {Key}, Value: {Value}", 
            request.PublicId, parameter.Key, parameter.Value);
    }
}
