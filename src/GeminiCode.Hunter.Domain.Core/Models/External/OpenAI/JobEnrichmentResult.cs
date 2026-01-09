namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

public record JobEnrichmentResult(
    string Summary,
    string Responsibilities,
    string Skills,
    string MustHaveSkills,
    string NiceToHaveSkills
);
