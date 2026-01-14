using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.StatusChanged;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;

public class UpdateHiringProcessStatusRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMediator mediator,
    ILogger<UpdateHiringProcessStatusRequestHandler> logger)
    : IRequestHandler<UpdateHiringProcessStatusRequest>
{
    public async Task Handle(UpdateHiringProcessStatusRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating hiring process status: {PublicId} to {NewStatus}", 
            request.PublicId, request.NewStatus);

        // Get current user entity to obtain UserId
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(request.PublicId);
        
        if (hiringProcess == null)
        {
            logger.LogWarning("Hiring process not found: {PublicId}", request.PublicId);
            throw new NotFoundException($"Hiring process with ID {request.PublicId} not found");
        }

        var oldStatus = hiringProcess.Status;

        // Validate that the new status is different from the current status
        if (oldStatus == request.NewStatus)
        {
            logger.LogWarning("Attempted to update hiring process {PublicId} to the same status: {Status}", 
                request.PublicId, request.NewStatus);
            throw new BusinessException("Cannot update to the same status. Please select a different status.");
        }

        // Load related entities for notification
        await unitOfWork.HiringProcessRepository.LoadRelatedEntitiesAsync(hiringProcess);

        // Update status
        hiringProcess.Status = request.NewStatus;
        hiringProcess.LastUpdatedByUserId = user.UserId;
        unitOfWork.HiringProcessRepository.Update(hiringProcess);

        // Create status history entry
        var statusHistory = new Domain.Core.Entities.HiringProcessStatusHistory
        {
            HiringProcessId = hiringProcess.HiringProcessId,
            FromStatus = oldStatus,
            ToStatus = request.NewStatus,
            ChangedByUserId = user.UserId,
            Notes = request.Notes
        };

        await unitOfWork.HiringProcessStatusHistoryRepository.AddAsync(statusHistory);
        await unitOfWork.CommitAsync();

        // Send notification to the user who made the referral
        if (hiringProcess.CreatedByUser != null && hiringProcess.Candidate != null && hiringProcess.JobPosition != null)
        {
            await mediator.Publish(new HiringProcessStatusChangedNotification
            {
                HiringProcessId = hiringProcess.PublicId,
                CandidateName = hiringProcess.Candidate.Name ?? "Unknown",
                JobTitle = hiringProcess.JobPosition.Title,
                OldStatus = oldStatus.Description(),
                NewStatus = request.NewStatus.Description(),
                ChangedByUserName = user.Name,
                Notes = request.Notes,
                ReferredByUserEmail = hiringProcess.CreatedByUser.Email
            }, cancellationToken);
        }

        logger.LogInformation("Hiring process status updated successfully: {PublicId}, {OldStatus} -> {NewStatus}", 
            request.PublicId, oldStatus, request.NewStatus);
    }
}
