using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Accept;

/// <summary>
/// Accepting a referral request is exactly a regular referral: it starts a hiring
/// process created by the accepting user, then marks the request as accepted.
/// </summary>
public class ReferralRequestAcceptRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMediator mediator,
    ILogger<ReferralRequestAcceptRequestHandler> logger)
    : IRequestHandler<ReferralRequestAcceptRequest, ReferralRequestAcceptResponse>
{
    public async Task<ReferralRequestAcceptResponse> Handle(ReferralRequestAcceptRequest request, CancellationToken cancellationToken)
    {
        var referralRequest = await unitOfWork.ReferralRequestRepository.GetByIdAsync(request.PublicId)
            ?? throw new NotFoundException($"Referral request {request.PublicId} not found");

        if (referralRequest.Status != ReferralRequestStatus.Pending)
        {
            throw new ConflictException($"This referral request was already {referralRequest.Status.ToString().ToLower()}.");
        }

        if (referralRequest.Candidate is null || referralRequest.JobPosition is null)
        {
            throw new BusinessException("Referral request is missing its candidate or position.");
        }

        if (!referralRequest.Candidate.IsEmailConfirmed)
        {
            throw new BusinessException("The candidate has not confirmed their email yet.");
        }

        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id)
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        var referralReason = BuildReferralReason(referralRequest.Message, request.Note);

        var started = await mediator.Send(new StartHiringProcessRequest
        {
            CandidateId = referralRequest.Candidate.PublicId,
            JobPositionId = referralRequest.JobPosition.PublicId,
            ReferralReason = referralReason
        }, cancellationToken);

        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(started.PublicId);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // Detach navigations so Update() touches only the request row.
        referralRequest.Candidate = null;
        referralRequest.JobPosition = null;
        referralRequest.ResolvedByUser = null;
        referralRequest.HiringProcess = null;

        referralRequest.Status = ReferralRequestStatus.Accepted;
        referralRequest.ResolvedByUserId = user.UserId;
        referralRequest.ResolvedAt = now;
        referralRequest.ResolutionNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        referralRequest.HiringProcessId = hiringProcess?.HiringProcessId;

        unitOfWork.ReferralRequestRepository.Update(referralRequest);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Referral request {Id} accepted by {User}; hiring process {HiringProcessId} started",
            referralRequest.PublicId, currentUser.Name, started.PublicId);

        return new ReferralRequestAcceptResponse
        {
            ReferralRequestId = referralRequest.PublicId,
            HiringProcessId = started.PublicId,
            Status = referralRequest.Status.ToString()
        };
    }

    private static string BuildReferralReason(string? candidateMessage, string? note)
    {
        var parts = new List<string> { "Requested by the candidate through the public CV match page." };

        if (!string.IsNullOrWhiteSpace(candidateMessage))
            parts.Add($"Candidate message: {candidateMessage.Trim()}");

        if (!string.IsNullOrWhiteSpace(note))
            parts.Add($"Referrer note: {note.Trim()}");

        return string.Join(" ", parts);
    }
}
