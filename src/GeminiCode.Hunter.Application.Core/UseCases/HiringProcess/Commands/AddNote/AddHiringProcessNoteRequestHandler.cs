using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.NoteAdded;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.AddNote;

public class AddHiringProcessNoteRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMediator mediator,
    ILogger<AddHiringProcessNoteRequestHandler> logger)
    : IRequestHandler<AddHiringProcessNoteRequest>
{
    public async Task Handle(AddHiringProcessNoteRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding note to hiring process: {HiringProcessId}", request.HiringProcessId);

        // Get current user entity to obtain UserId
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(request.HiringProcessId);
        
        if (hiringProcess == null)
        {
            logger.LogWarning("Hiring process not found: {HiringProcessId}", request.HiringProcessId);
            throw new NotFoundException($"Hiring process with ID {request.HiringProcessId} not found");
        }

        // Load related entities for notification
        await unitOfWork.HiringProcessRepository.LoadRelatedEntitiesAsync(hiringProcess);

        var note = new Domain.Core.Entities.HiringProcessNote
        {
            HiringProcessId = hiringProcess.HiringProcessId,
            UserId = user.UserId,
            Note = request.Note.Trim()
        };

        await unitOfWork.HiringProcessNoteRepository.AddAsync(note);
        await unitOfWork.CommitAsync();

        // Send notification to the user who made the referral
        if (hiringProcess.CreatedByUser != null && hiringProcess.Candidate != null && hiringProcess.JobPosition != null)
        {
            await mediator.Publish(new HiringProcessNoteAddedNotification
            {
                HiringProcessId = hiringProcess.PublicId,
                CandidateName = hiringProcess.Candidate.Name ?? "Unknown",
                JobTitle = hiringProcess.JobPosition.Title,
                NoteText = request.Note.Trim(),
                AddedByUserName = user.Name,
                ReferredByUserEmail = hiringProcess.CreatedByUser.Email
            }, cancellationToken);
        }

        logger.LogInformation("Note added successfully to hiring process: {HiringProcessId}", request.HiringProcessId);
    }
}
