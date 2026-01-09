using System;
using Bogus;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Update;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Test.Shared.Faker;

public static class JobPositionFaker
{
    public static JobPosition Get()
    {
        return new Faker<JobPosition>()
             .RuleFor(o => o.Title, f => f.Name.JobTitle())
             .RuleFor(o => o.ExternalUrl, f => f.Internet.Url())
             .RuleFor(o => o.CompanyCode, f => f.Company.CompanyName())
             .RuleFor(o => o.JobCode, f => f.Random.AlphaNumeric(10))
             .RuleFor(o => o.Summary, f => f.Lorem.Paragraph())
             .RuleFor(o => o.Responsibilities, f => f.Lorem.Paragraphs(2))
             .RuleFor(o => o.Skills, f => string.Join(", ", f.Make(8, () => f.Commerce.ProductAdjective())))
             .RuleFor(o => o.MustHaveSkills, f => string.Join(", ", f.Make(5, () => f.Commerce.ProductAdjective())))
             .RuleFor(o => o.NiceToHaveSkills, f => string.Join(", ", f.Make(3, () => f.Commerce.ProductAdjective())))
             .RuleFor(o => o.IsActive, f => true)
             .RuleFor(o => o.PublicId, f => Guid.NewGuid().ToString())
             .RuleFor(o => o.LastCheckedAt, f => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
             .Generate();
    }

    public static JobPositionCreateRequest GetJobPositionCreateRequest()
    {
        return new Faker<JobPositionCreateRequest>()
           .RuleFor(o => o.Title, f => f.Name.JobTitle())
           .RuleFor(o => o.ExternalUrl, f => f.Internet.Url())
           .RuleFor(o => o.CompanyCode, f => f.Random.AlphaNumeric(2))
           .RuleFor(o => o.JobCode, f => f.Random.AlphaNumeric(10))
           .RuleFor(o => o.Summary, f => f.Lorem.Paragraph())
           .RuleFor(o => o.Responsibilities, f => f.Lorem.Paragraphs(2))
           .RuleFor(o => o.Skills, f => string.Join(", ", f.Make(8, () => f.Commerce.ProductAdjective())))
           .RuleFor(o => o.MustHaveSkills, f => string.Join(", ", f.Make(5, () => f.Commerce.ProductAdjective())))
           .RuleFor(o => o.NiceToHaveSkills, f => string.Join(", ", f.Make(3, () => f.Commerce.ProductAdjective())))
           .Generate();
    }

    public static JobPositionUpdateRequest GetJobPositionUpdateRequest()
    {
        return new Faker<JobPositionUpdateRequest>()
            .RuleFor(o => o.Title, f => f.Name.JobTitle())
            .RuleFor(o => o.ExternalUrl, f => f.Internet.Url())
            .RuleFor(o => o.CompanyCode, f => f.Random.AlphaNumeric(2))
            .RuleFor(o => o.JobCode, f => f.Random.AlphaNumeric(10))
            .RuleFor(o => o.Summary, f => f.Lorem.Paragraph())
            .RuleFor(o => o.Responsibilities, f => f.Lorem.Paragraphs(2))
            .RuleFor(o => o.Skills, f => string.Join(", ", f.Make(8, () => f.Commerce.ProductAdjective())))
            .RuleFor(o => o.MustHaveSkills, f => string.Join(", ", f.Make(5, () => f.Commerce.ProductAdjective())))
            .RuleFor(o => o.NiceToHaveSkills, f => string.Join(", ", f.Make(3, () => f.Commerce.ProductAdjective())))
            .RuleFor(o => o.IsActive, f => true)
            .Generate();
    }
}
