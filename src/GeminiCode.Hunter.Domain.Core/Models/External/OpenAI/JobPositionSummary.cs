namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

public record JobPositionSummary(
  string Title,
  string? Summary,
  string? Responsibilities,
  string? Requirements,
  string? MustHaveSkills,
  string? NiceToHaveSkills
);
