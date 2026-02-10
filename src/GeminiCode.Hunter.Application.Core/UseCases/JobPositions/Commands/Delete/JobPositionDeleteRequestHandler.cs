using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Delete;

public class JobPositionDeleteRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<JobPositionDeleteRequestHandler> logger) : IRequestHandler<JobPositionDeleteRequest>
{
    public async Task Handle(JobPositionDeleteRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing delete request for JobPosition {JobPositionId}", request.PublicId);
        
        var jobPosition = await unitOfWork.JobPositionRepository.GetByIdAsync(request.PublicId);

        if (jobPosition == null)
            throw new NotFoundException(request.PublicId);

        // Check if there are active HiringProcesses for this JobPosition
        var hasActiveProcesses = await unitOfWork.HiringProcessRepository.HasActiveProcessByJobPositionIdAsync(jobPosition.JobPositionId);
        
        if (hasActiveProcesses)
        {
            throw new ConflictException(
                $"Cannot delete job position '{jobPosition.Title}' because there are active hiring processes associated with it. " +
                "Please cancel or complete all hiring processes before deleting this job position.");
        }

        // Delete related CvAnalysisMatches
        logger.LogInformation("Deleting CvAnalysisMatches for JobPosition {JobPositionId}", request.PublicId);
        await unitOfWork.CvAnalysisRepository.DeleteMatchesByJobPositionIdAsync(jobPosition.JobPositionId);

        // Delete the JobPosition
        logger.LogInformation("Deleting JobPosition {JobPositionId}", request.PublicId);
        await unitOfWork.JobPositionRepository.DeleteAsync(request.PublicId);
        
        // Commit all changes
        await unitOfWork.CommitAsync();
        
        logger.LogInformation("JobPosition {JobPositionId} and related CvAnalysisMatches successfully deleted", request.PublicId);
    }
}
