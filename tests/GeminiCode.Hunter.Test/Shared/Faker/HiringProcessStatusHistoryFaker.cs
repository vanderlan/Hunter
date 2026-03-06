using System;
using Bogus;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class HiringProcessStatusHistoryFaker
{
    public static HiringProcessStatusHistory Get()
    {
        return new Faker<HiringProcessStatusHistory>()
             .RuleFor(o => o.HiringProcessStatusHistoryId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.HiringProcessId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.FromStatus, f => HiringProcessStatus.PendingAnalysis)
             .RuleFor(o => o.ToStatus, f => HiringProcessStatus.Screening)
             .RuleFor(o => o.ChangedByUserId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.Notes, f => f.Lorem.Sentence())
             .Generate();
    }
}
