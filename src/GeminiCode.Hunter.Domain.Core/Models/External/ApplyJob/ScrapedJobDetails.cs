namespace GeminiCode.Hunter.Domain.Core.Models.External.ApplyJob;

public record ScrapedJobDetails(
    string Summary,
    string Responsibilities,
    string Requirements,
    string MustHaveSkills,
    string NiceToHaveSkills
);