using System;
using Bogus;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class HiringProcessNoteFaker
{
    public static HiringProcessNote Get()
    {
        return new Faker<HiringProcessNote>()
             .RuleFor(o => o.HiringProcessNoteId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.HiringProcessId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.UserId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.Note, f => f.Lorem.Paragraph())
             .Generate();
    }
}
