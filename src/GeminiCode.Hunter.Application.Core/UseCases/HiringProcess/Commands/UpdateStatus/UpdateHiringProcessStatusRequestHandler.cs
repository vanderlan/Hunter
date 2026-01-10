using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;

public class UpdateHiringProcessStatusRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<UpdateHiringProcessStatusRequestHandler> logger)
    : IRequestHandler<UpdateHiringProcessStatusRequest>
{
    public async Task Handle(UpdateHiringProcessStatusRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating hiring process status: {PublicId} to {NewStatus}", 
            request.PublicId, request.NewStatus);

        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(request.PublicId);
        
        if (hiringProcess == null)
        {
            logger.LogWarning("Hiring process not found: {PublicId}", request.PublicId);
            throw new NotFoundException($"Hiring process with ID {request.PublicId} not found");
        }

        var userId = long.Parse(currentUser.Id);
        var oldStatus = hiringProcess.Status;

        // Update status
        hiringProcess.Status = request.NewStatus;
        hiringProcess.LastUpdatedByUserId = userId;
        unitOfWork.HiringProcessRepository.Update(hiringProcess);

        // Create status history entry
        var statusHistory = new Domain.Core.Entities.HiringProcessStatusHistory
        {
            HiringProcessId = hiringProcess.HiringProcessId,
            FromStatus = oldStatus,
            ToStatus = request.NewStatus,
            ChangedByUserId = userId,
            Notes = request.Notes
        };

        await unitOfWork.HiringProcessStatusHistoryRepository.AddAsync(statusHistory);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Hiring process status updated successfully: {PublicId}, {OldStatus} -> {NewStatus}", 
            request.PublicId, oldStatus, request.NewStatus);
    }
}
