using System;
using Bogus;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class CandidateFileFaker
{
    public static CandidateFile Get()
    {
        return new Faker<CandidateFile>()
             .RuleFor(o => o.CandidateFileId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.CandidateId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.FileId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.FileType, f => "Resume")
             .Generate();
    }
}
