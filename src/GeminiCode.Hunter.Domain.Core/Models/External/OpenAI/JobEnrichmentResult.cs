namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

public record JobEnrichmentResult(
    string Summary,
    string Responsibilities,
    string Requirements,
    string MustHaveSkills,
    string NiceToHaveSkills
);
