namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvResponse
{
    public int TotalPositionsAnalyzed { get; set; }
    public int MatchesFound { get; set; }
    public List<JobMatchDto> Matches { get; set; } = new();
}

public class JobMatchDto
{
    public string JobCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string ExternalUrl { get; set; } = string.Empty;
    public decimal MatchPercentage { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public List<string> MatchingSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public string? Summary { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
}
