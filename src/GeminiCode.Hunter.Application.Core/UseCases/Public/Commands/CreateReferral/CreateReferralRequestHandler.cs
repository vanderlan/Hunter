using GeminiCode.Hunter.Application.Core.UseCases.Public.Notifications.ReferralRequestCreated;
using GeminiCode.Hunter.Domain.Core.Constants;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.CreateReferral;

public class CreateReferralRequestHandler(
    IUnitOfWork unitOfWork,
    IParameterService parameterService,
    IPublisher publisher,
    ILogger<CreateReferralRequestHandler> logger)
    : IRequestHandler<CreateReferralRequest, CreateReferralResponse>
{
    public const int DefaultMinimumMatchForReferral = 50;

    public async Task<CreateReferralResponse> Handle(CreateReferralRequest request, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.CandidateRepository.GetByPublicTokenAsync(request.CandidateToken);

        if (candidate is null || candidate.Source != CandidateSource.Public)
        {
            throw NotFoundException.WithMessage("We could not find your CV analysis. Please upload your CV again.");
        }

        if (!candidate.HasAcceptedTerms || string.IsNullOrWhiteSpace(candidate.Email))
        {
            throw new BusinessException("Save your profile with your email before asking for a referral.")
            {
                Title = "Profile required"
            };
        }

        var jobPosition = await unitOfWork.JobPositionRepository.GetByPublicCodeAsync(request.JobCode);

        if (jobPosition is null || !jobPosition.IsPubliclyVisible)
        {
            throw NotFoundException.WithMessage("This position is no longer open.");
        }

        var existingRequest = await unitOfWork.ReferralRequestRepository
            .GetByCandidateAndJobPositionAsync(candidate.CandidateId, jobPosition.JobPositionId);

        if (existingRequest is not null)
        {
            throw new ConflictException("You already asked for a referral for this position.");
        }

        var existingProcess = await unitOfWork.HiringProcessRepository
            .GetByCandidateAndJobPositionAsync(candidate.CandidateId, jobPosition.JobPositionId);

        if (existingProcess is not null)
        {
            throw new ConflictException("You are already in a hiring process for this position.");
        }

        var lastMatch = await unitOfWork.CvAnalysisRepository
            .GetLastAnalysisMatchAsync(candidate.CandidateId, jobPosition.JobPositionId);

        var minimumMatch = await parameterService.GetValueAsIntAsync(
            ParameterConstants.MinimumMatchPercentageReferralRequest, DefaultMinimumMatchForReferral);

        if (lastMatch is null || lastMatch.MatchPercentage < minimumMatch)
        {
            throw new BusinessException($"Referral requests are available for matches of {minimumMatch}% or higher.")
            {
                Title = "Match too low"
            };
        }

        var referralRequest = new ReferralRequest
        {
            CandidateId = candidate.CandidateId,
            JobPositionId = jobPosition.JobPositionId,
            Status = ReferralRequestStatus.Pending,
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            MatchPercentage = lastMatch.MatchPercentage
        };

        await unitOfWork.ReferralRequestRepository.AddAsync(referralRequest);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Referral request {ReferralRequestId} created for job {JobCode}", referralRequest.PublicId, jobPosition.PublicCode);

        if (candidate.IsEmailConfirmed)
        {
            await publisher.Publish(new ReferralRequestCreatedNotification { ReferralRequestId = referralRequest.PublicId }, cancellationToken);
        }

        return new CreateReferralResponse
        {
            JobTitle = jobPosition.Title,
            Status = referralRequest.Status.ToString(),
            IsEmailConfirmationRequired = !candidate.IsEmailConfirmed
        };
    }
}
