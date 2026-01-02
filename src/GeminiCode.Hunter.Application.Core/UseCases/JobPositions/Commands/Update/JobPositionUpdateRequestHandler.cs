using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Localization;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Update;

public class JobPositionUpdateRequestHandler(IUnitOfWork unitOfWork, IStringLocalizer<HunterResources> resourceMessages) 
    : IRequestHandler<JobPositionUpdateRequest>
{
    public async Task Handle(JobPositionUpdateRequest request, CancellationToken cancellationToken)
    {
        var jobPosition = await unitOfWork.JobPositionRepository.GetByIdAsync(request.PublicId);
        
        if (jobPosition == null)
            throw new NotFoundException(request.PublicId);

        await ValidateJobPosition(request);

        jobPosition.Title = request.Title;
        jobPosition.ExternalUrl = request.ExternalUrl;
        jobPosition.CompanyCode = request.CompanyCode;
        jobPosition.JobCode = request.JobCode;
        jobPosition.Summary = request.Summary;
        jobPosition.Responsibilities = request.Responsibilities;
        jobPosition.Requirements = request.Requirements;
        jobPosition.MustHaveSkills = request.MustHaveSkills;
        jobPosition.NiceToHaveSkills = request.NiceToHaveSkills;
        jobPosition.IsActive = request.IsActive;
        jobPosition.LastCheckedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        unitOfWork.JobPositionRepository.Update(jobPosition);

        await unitOfWork.CommitAsync();
    }

    private async Task ValidateJobPosition(JobPositionUpdateRequest request)
    {
        var jobPositionFound = await unitOfWork.JobPositionRepository.GetByJobCodeAsync(request.JobCode);

        if (jobPositionFound != null && jobPositionFound.PublicId != request.PublicId)
            throw new ConflictException(resourceMessages[JobPositionMessages.JobCodeExists], resourceMessages[ExceptionsTitles.ValidationError]);
    }
}
