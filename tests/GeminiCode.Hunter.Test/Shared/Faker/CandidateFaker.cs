using System;
using Bogus;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Helpers;

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

    public static Candidate GetPublic(bool hasAcceptedTerms = true, bool isEmailConfirmed = false)
    {
        return new Faker<Candidate>()
             .RuleFor(o => o.CandidateId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.Name, f => f.Name.FullName())
             .RuleFor(o => o.Email, f => f.Internet.Email())
             .RuleFor(o => o.PhoneNumber, f => f.Phone.PhoneNumber())
             .RuleFor(o => o.UserId, f => (long?)null)
             .RuleFor(o => o.Source, f => CandidateSource.Public)
             .RuleFor(o => o.PublicToken, f => SecureTokenGenerator.NewToken())
             .RuleFor(o => o.HasAcceptedTerms, f => hasAcceptedTerms)
             .RuleFor(o => o.IsEmailConfirmed, f => isEmailConfirmed)
             .RuleFor(o => o.EmailConfirmationToken, f => SecureTokenGenerator.NewToken())
             .RuleFor(o => o.EmailConfirmationSentAt, f => DateTime.UtcNow.AddMinutes(-5))
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
