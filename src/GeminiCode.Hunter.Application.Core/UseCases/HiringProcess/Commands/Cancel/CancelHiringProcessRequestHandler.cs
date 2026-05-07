using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Cancel;

public class CancelHiringProcessRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IEmailService emailService,
    ILogger<CancelHiringProcessRequestHandler> logger)
    : IRequestHandler<CancelHiringProcessRequest>
{
    public async Task Handle(CancelHiringProcessRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling hiring process: {PublicId}", request.PublicId);

        // Get hiring process with related entities
        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(request.PublicId);
        
        if (hiringProcess == null)
        {
            logger.LogWarning("Hiring process not found: {PublicId}", request.PublicId);
            throw new NotFoundException($"Hiring process with ID {request.PublicId} not found");
        }

        // Load related entities for email notification
        await unitOfWork.HiringProcessRepository.LoadRelatedEntitiesAsync(hiringProcess);

        // Get current user
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            logger.LogWarning("User not found: {UserId}", currentUser.Id);
            throw new NotFoundException($"User with ID {currentUser.Id} not found");
        }

        // Store data for email notification before deletion
        var candidateName = hiringProcess.Candidate?.Name ?? "Unknown";
        var jobTitle = hiringProcess.JobPosition?.Title ?? "Unknown";
        var jobCode = "N/A";
        var cancelledByUserName = user.Name;

        // Delete status history
        var statusHistories = await unitOfWork.HiringProcessStatusHistoryRepository
            .SearchByAsync(h => h.HiringProcessId == hiringProcess.HiringProcessId);
        
        foreach (var history in statusHistories)
        {
            await unitOfWork.HiringProcessStatusHistoryRepository.DeleteAsync(history.PublicId);
        }

        logger.LogInformation("Deleted {Count} status history records", statusHistories.Count());

        // Delete notes
        var notes = await unitOfWork.HiringProcessNoteRepository
            .SearchByAsync(n => n.HiringProcessId == hiringProcess.HiringProcessId);
        
        foreach (var note in notes)
        {
            await unitOfWork.HiringProcessNoteRepository.DeleteAsync(note.PublicId);
        }

        logger.LogInformation("Deleted {Count} notes", notes.Count());

        // Delete hiring process
        await unitOfWork.HiringProcessRepository.DeleteAsync(request.PublicId);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Hiring process deleted successfully: {PublicId}", request.PublicId);

        // Send notification to all admin users
        try
        {
            var adminUsers = await unitOfWork.UserRepository
                .SearchByAsync(u => u.Profile == UserProfile.Admin);

            if (adminUsers.Any())
            {
                var adminEmails = adminUsers.Select(u => u.Email).ToList();
                
                logger.LogInformation("Sending cancellation notification to {Count} admin(s)", adminEmails.Count);

                await emailService.SendHiringProcessCancelledNotificationAsync(
                    adminEmails,
                    candidateName,
                    jobTitle,
                    jobCode,
                    cancelledByUserName,
                    request.CancelReason);
            }
            else
            {
                logger.LogWarning("No admin users found to notify about hiring process cancellation");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send cancellation notification, but process was deleted successfully");
        }

        logger.LogInformation("Hiring process cancellation completed: {PublicId}", request.PublicId);
    }
}
