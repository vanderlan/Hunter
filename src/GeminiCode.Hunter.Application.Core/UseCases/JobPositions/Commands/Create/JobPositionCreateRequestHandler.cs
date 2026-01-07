using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Localization;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;

public class JobPositionCreateRequestHandler(
        IUnitOfWork unitOfWork,
        IStringLocalizer<HunterResources> resourceMessages) : IRequestHandler<JobPositionCreateRequest, JobPositionCreateResponse>
{
    public async Task<JobPositionCreateResponse> Handle(JobPositionCreateRequest request, CancellationToken cancellationToken)
    {
        await ValidateJobPosition(request);

        var added = await unitOfWork.JobPositionRepository.AddAsync((JobPosition)request);
        await unitOfWork.CommitAsync();

        return (JobPositionCreateResponse)added;
    }

    private async Task ValidateJobPosition(JobPositionCreateRequest request)
    {
        var jobPositionFound = await unitOfWork.JobPositionRepository.GetByJobCodeAsync(request.JobCode);

        if (jobPositionFound != null)
            throw new ConflictException(resourceMessages[JobPositionMessages.JobCodeExists], resourceMessages[ExceptionsTitles.ValidationError]);
    }
}
