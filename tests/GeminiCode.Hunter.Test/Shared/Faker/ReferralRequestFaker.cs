using System;
using Bogus;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class ReferralRequestFaker
{
    public static ReferralRequest Get(Candidate? candidate = null, JobPosition? jobPosition = null)
    {
        candidate ??= CandidateFaker.GetPublic(isEmailConfirmed: true);
        jobPosition ??= JobPositionFaker.GetPublic();

        return new Faker<ReferralRequest>()
            .RuleFor(o => o.ReferralRequestId, f => f.Random.Long(1, 1000))
            .RuleFor(o => o.CandidateId, f => candidate.CandidateId)
            .RuleFor(o => o.JobPositionId, f => jobPosition.JobPositionId)
            .RuleFor(o => o.Status, f => ReferralRequestStatus.Pending)
            .RuleFor(o => o.Message, f => f.Lorem.Sentence())
            .RuleFor(o => o.MatchPercentage, f => f.Random.Decimal(50, 100))
            .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
            .RuleFor(o => o.Candidate, f => candidate)
            .RuleFor(o => o.JobPosition, f => jobPosition)
            .Generate();
    }
}
