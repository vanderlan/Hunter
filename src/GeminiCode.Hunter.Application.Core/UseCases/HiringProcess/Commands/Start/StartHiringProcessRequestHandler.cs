using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.HiringProcessStarted;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using GeminiCode.Hunter.Domain.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;

public class StartHiringProcessRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMediator mediator,
    ILogger<StartHiringProcessRequestHandler> logger)
    : IRequestHandler<StartHiringProcessRequest, StartHiringProcessResponse>
{
    public async Task<StartHiringProcessResponse> Handle(StartHiringProcessRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting hiring process for Candidate: {CandidateId}, JobPosition: {JobPositionId}", 
            request.CandidateId, request.JobPositionId);

        // Get current user entity to obtain UserId
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        // Get candidate
        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.CandidateId);
        if (candidate == null)
        {
            logger.LogWarning("Candidate not found: {CandidateId}", request.CandidateId);
            throw new NotFoundException($"Candidate with ID {request.CandidateId} not found");
        }

        // Get job position
        var jobPosition = await unitOfWork.JobPositionRepository.GetByIdAsync(request.JobPositionId);
        if (jobPosition == null)
        {
            logger.LogWarning("Job position not found: {JobPositionId}", request.JobPositionId);
            throw new NotFoundException($"Job position with ID {request.JobPositionId} not found");
        }

        // Check if hiring process already exists
        var existingProcess = await unitOfWork.HiringProcessRepository
            .GetByCandidateAndJobPositionAsync(candidate.CandidateId, jobPosition.JobPositionId);
        
        if (existingProcess != null)
        {
            logger.LogWarning("Hiring process already exists for Candidate: {CandidateId}, JobPosition: {JobPositionId}", 
                candidate.CandidateId, jobPosition.JobPositionId);
            throw new ConflictException("A hiring process already exists for this candidate and job position");
        }

        // Create hiring process
        var hiringProcess = new Domain.Core.Entities.HiringProcess
        {
            CandidateId = candidate.CandidateId,
            JobPositionId = jobPosition.JobPositionId,
            Status = HiringProcessStatus.ReferredByPartner,
            CreatedByUserId = user.UserId,
            ReferralReason = request.ReferralReason
        };

        await unitOfWork.HiringProcessRepository.AddAsync(hiringProcess);
        
        // Commit to generate HiringProcessId
        await unitOfWork.CommitAsync();

        // Create initial status history entry (now that we have HiringProcessId)
        var statusHistory = new HiringProcessStatusHistory
        {
            HiringProcessId = hiringProcess.HiringProcessId,
            FromStatus = HiringProcessStatus.New,
            ToStatus = HiringProcessStatus.ReferredByPartner,
            ChangedByUserId = user.UserId,
            Notes = "Initial hiring process creation"
        };

        await unitOfWork.HiringProcessStatusHistoryRepository.AddAsync(statusHistory);
        await unitOfWork.CommitAsync();

        // Send notification to admin users
        await mediator.Publish(new HiringProcessStartedNotification
        {
            HiringProcessId = hiringProcess.PublicId,
            CandidateName = candidate.Name ?? "Unknown",
            CandidateEmail = candidate.Email ?? "",
            JobTitle = jobPosition.Title,
            JobCode = jobPosition.JobCode,
            StartedByUserName = currentUser.Name,
            ReferralReason = request.ReferralReason
        }, cancellationToken);

        logger.LogInformation("Hiring process started successfully: {PublicId}", hiringProcess.PublicId);

        return new StartHiringProcessResponse
        {
            PublicId = hiringProcess.PublicId,
            Status = hiringProcess.Status.ToString()
        };
    }
}
