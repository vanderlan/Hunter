using System;
using Bogus;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.AddNote;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class HiringProcessFaker
{
    public static HiringProcess Get()
    {
        return new Faker<HiringProcess>()
             .RuleFor(o => o.CandidateId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.JobPositionId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.Status, f => HiringProcessStatus.Screening)
             .RuleFor(o => o.CreatedByUserId, f => f.Random.Long(1, 1000))
             .RuleFor(o => o.ReferralReason, f => f.Lorem.Sentence())
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .Generate();
    }

    public static StartHiringProcessRequest GetStartHiringProcessRequest()
    {
        return new Faker<StartHiringProcessRequest>()
           .RuleFor(o => o.CandidateId, f => Guid.NewGuid().ToString())
           .RuleFor(o => o.JobPositionId, f => Guid.NewGuid().ToString())
           .RuleFor(o => o.ReferralReason, f => f.Lorem.Sentence())
           .Generate();
    }

    public static UpdateHiringProcessStatusRequest GetUpdateHiringProcessStatusRequest()
    {
        return new Faker<UpdateHiringProcessStatusRequest>()
            .RuleFor(o => o.NewStatus, f => HiringProcessStatus.Screening)
            .Generate();
    }

    public static AddHiringProcessNoteRequest GetAddHiringProcessNoteRequest()
    {
        return new Faker<AddHiringProcessNoteRequest>()
            .RuleFor(o => o.Note, f => f.Lorem.Paragraph())
            .Generate();
    }
}
