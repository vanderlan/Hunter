using GeminiCode.Hunter.Application.Core.UseCases.Public.Notifications.ReferralRequestCreated;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.ConfirmPublicCandidateEmail;

public class ConfirmPublicCandidateEmailRequestHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<ConfirmPublicCandidateEmailRequestHandler> logger)
    : IRequestHandler<ConfirmPublicCandidateEmailRequest, ConfirmPublicCandidateEmailResponse>
{
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(7);

    public async Task<ConfirmPublicCandidateEmailResponse> Handle(ConfirmPublicCandidateEmailRequest request, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.CandidateRepository.GetByEmailConfirmationTokenAsync(request.Token)
            ?? throw NotFoundException.WithMessage("This confirmation link is invalid. Please request a new one from the CV match page.");

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        if (candidate.IsEmailConfirmed)
        {
            return new ConfirmPublicCandidateEmailResponse
            {
                Name = candidate.Name,
                Email = candidate.Email ?? string.Empty,
                IsAlreadyConfirmed = true
            };
        }

        if (candidate.EmailConfirmationSentAt is null || now - candidate.EmailConfirmationSentAt.Value > TokenLifetime)
        {
            throw new BusinessException("This confirmation link has expired. Please upload your CV again to receive a new one.")
            {
                Title = "Link expired"
            };
        }

        candidate.IsEmailConfirmed = true;
        candidate.EmailConfirmedAt = now;

        unitOfWork.CandidateRepository.Update(candidate);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Public candidate {CandidateId} confirmed email", candidate.CandidateId);

        // Referral requests made before confirming were held back; release them now.
        var pendingRequests = await unitOfWork.ReferralRequestRepository.GetPendingByCandidateIdAsync(candidate.CandidateId);

        foreach (var pending in pendingRequests)
        {
            await publisher.Publish(new ReferralRequestCreatedNotification { ReferralRequestId = pending.PublicId }, cancellationToken);
        }

        return new ConfirmPublicCandidateEmailResponse
        {
            Name = candidate.Name,
            Email = candidate.Email ?? string.Empty,
            IsAlreadyConfirmed = false,
            PendingReferralRequests = pendingRequests.Count
        };
    }
}
