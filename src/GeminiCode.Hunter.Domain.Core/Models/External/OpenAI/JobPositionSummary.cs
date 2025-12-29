namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

public record JobPositionSummary(
  string JobCode,
  string Title,
  string CompanyCode,
  string? Summary,
  string? Responsibilities,
  string? Requirements,
  string? MustHaveSkills,
  string? NiceToHaveSkills
);
