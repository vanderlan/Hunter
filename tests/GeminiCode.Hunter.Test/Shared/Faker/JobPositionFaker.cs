using System;
using Bogus;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Update;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Helpers;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class JobPositionFaker
{
    public static JobPosition Get()
    {
        return new Faker<JobPosition>()
             .RuleFor(o => o.Title, f => f.Name.JobTitle())
             .RuleFor(o => o.ExternalUrl, f => f.Internet.Url())
             .RuleFor(o => o.Summary, f => f.Lorem.Paragraph())
             .RuleFor(o => o.Responsibilities, f => f.Lorem.Paragraphs(2))
             .RuleFor(o => o.Requirements, f => string.Join(", ", f.Make(8, () => f.Commerce.ProductAdjective())))
             .RuleFor(o => o.MustHaveSkills, f => string.Join(", ", f.Make(5, () => f.Commerce.ProductAdjective())))
             .RuleFor(o => o.NiceToHaveSkills, f => string.Join(", ", f.Make(3, () => f.Commerce.ProductAdjective())))
             .RuleFor(o => o.IsActive, f => true)
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.LastCheckedAt, f => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
             .Generate();
    }

    /// <summary>An active position that already has company-neutral public content.</summary>
    public static JobPosition GetPublic()
    {
        var position = Get();
        position.JobPositionId = new Bogus.Randomizer().Long(1, 1000);
        position.PublicCode = SecureTokenGenerator.NewPublicCode();
        position.PublicSummary = "Public summary";
        position.PublicResponsibilities = "Public responsibilities";
        position.PublicRequirements = "Public requirements";
        position.PublicMustHaveSkills = "C#, SQL";
        position.PublicNiceToHaveSkills = "Docker";
        position.PublicContentGeneratedAt = DateTime.UtcNow;
        return position;
    }

    public static JobPositionCreateRequest GetJobPositionCreateRequest()
    {
        return new Faker<JobPositionCreateRequest>()
           .RuleFor(o => o.Title, f => f.Name.JobTitle())
           .RuleFor(o => o.ExternalUrl, f => f.Internet.Url())
           .RuleFor(o => o.Summary, f => f.Lorem.Paragraph())
           .RuleFor(o => o.Responsibilities, f => f.Lorem.Paragraphs(2))
           .RuleFor(o => o.Requirements, f => string.Join(", ", f.Make(8, () => f.Commerce.ProductAdjective())))
           .RuleFor(o => o.MustHaveSkills, f => string.Join(", ", f.Make(5, () => f.Commerce.ProductAdjective())))
           .RuleFor(o => o.NiceToHaveSkills, f => string.Join(", ", f.Make(3, () => f.Commerce.ProductAdjective())))
           .Generate();
    }

    public static JobPositionUpdateRequest GetJobPositionUpdateRequest()
    {
        return new Faker<JobPositionUpdateRequest>()
            .RuleFor(o => o.Title, f => f.Name.JobTitle())
            .RuleFor(o => o.ExternalUrl, f => f.Internet.Url())
            .RuleFor(o => o.Summary, f => f.Lorem.Paragraph())
            .RuleFor(o => o.Responsibilities, f => f.Lorem.Paragraphs(2))
            .RuleFor(o => o.Requirements, f => string.Join(", ", f.Make(8, () => f.Commerce.ProductAdjective())))
            .RuleFor(o => o.MustHaveSkills, f => string.Join(", ", f.Make(5, () => f.Commerce.ProductAdjective())))
            .RuleFor(o => o.NiceToHaveSkills, f => string.Join(", ", f.Make(3, () => f.Commerce.ProductAdjective())))
            .RuleFor(o => o.IsActive, f => true)
            .Generate();
    }
}
