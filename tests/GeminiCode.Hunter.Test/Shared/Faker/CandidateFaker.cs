using System;
using Bogus;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class CandidateFaker
{
    public static Candidate Get()
    {
        return new Faker<Candidate>()
             .RuleFor(o => o.Name, f => f.Name.FullName())
             .RuleFor(o => o.Email, f => f.Internet.Email())
             .RuleFor(o => o.PhoneNumber, f => f.Phone.PhoneNumber())
             .RuleFor(o => o.UserId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.AnalyzedAt, f => DateTime.UtcNow)
             .Generate();
    }

    public static CandidateUpdateRequest GetCandidateUpdateRequest()
    {
        return new Faker<CandidateUpdateRequest>()
           .RuleFor(o => o.Name, f => f.Name.FullName())
           .RuleFor(o => o.Email, f => f.Internet.Email())
           .RuleFor(o => o.PhoneNumber, f => f.Phone.PhoneNumber())
           .Generate();
    }
}
