using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Delete;

public class JobPositionDeleteRequestHandler(IUnitOfWork unitOfWork) : IRequestHandler<JobPositionDeleteRequest>
{
    public async Task Handle(JobPositionDeleteRequest request, CancellationToken cancellationToken)
    {
        var jobPosition = await unitOfWork.JobPositionRepository.GetByIdAsync(request.PublicId);

        if (jobPosition == null)
            throw new NotFoundException(request.PublicId);

        await unitOfWork.JobPositionRepository.DeleteAsync(request.PublicId);
        await unitOfWork.CommitAsync();
    }
}
