using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Decline;

public class ReferralRequestDeclineRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<ReferralRequestDeclineRequestHandler> logger)
    : IRequestHandler<ReferralRequestDeclineRequest>
{
    public async Task Handle(ReferralRequestDeclineRequest request, CancellationToken cancellationToken)
    {
        var referralRequest = await unitOfWork.ReferralRequestRepository.GetByIdAsync(request.PublicId)
            ?? throw new NotFoundException($"Referral request {request.PublicId} not found");

        if (referralRequest.Status != ReferralRequestStatus.Pending)
        {
            throw new ConflictException($"This referral request was already {referralRequest.Status.ToString().ToLower()}.");
        }

        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id)
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        referralRequest.Candidate = null;
        referralRequest.JobPosition = null;
        referralRequest.ResolvedByUser = null;
        referralRequest.HiringProcess = null;

        referralRequest.Status = ReferralRequestStatus.Declined;
        referralRequest.ResolvedByUserId = user.UserId;
        referralRequest.ResolvedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        referralRequest.ResolutionNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        unitOfWork.ReferralRequestRepository.Update(referralRequest);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Referral request {Id} declined by {User}", referralRequest.PublicId, currentUser.Name);
    }
}
