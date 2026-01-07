using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetById;

public class JobPositionGetByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<JobPositionGetByIdRequest, JobPositionGetByIdResponse>
{
    public async Task<JobPositionGetByIdResponse> Handle(JobPositionGetByIdRequest request, CancellationToken cancellationToken)
    {
        var jobPosition = await unitOfWork.JobPositionRepository.GetByIdAsync(request.PublicId);

        if (jobPosition == null)
            throw new NotFoundException(request.PublicId);

        return (JobPositionGetByIdResponse)jobPosition;
    }
}
