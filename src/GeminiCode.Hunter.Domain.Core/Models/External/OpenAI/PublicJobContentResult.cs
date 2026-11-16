namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

/// <summary>
/// Company-neutral version of a job description, safe to show to anonymous visitors.
/// </summary>
public record PublicJobContentResult(
    string Summary,
    string Responsibilities,
    string Requirements,
    string MustHaveSkills,
    string NiceToHaveSkills
);
