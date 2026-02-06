namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvResponse
{
    public int TotalPositionsAnalyzed { get; set; }
    public int MatchesFound { get; set; }
    public List<JobMatchDto> Matches { get; set; } = [];
    public CandidateInfoDto? CandidateInfo { get; set; }
}

public class CandidateInfoDto
{
    public string CandidateId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool HasResume { get; set; }
}

public class JobMatchDto
{
    public string JobPositionId { get; set; } = string.Empty;
    public string JobCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string ExternalUrl { get; set; } = string.Empty;
    public decimal MatchPercentage { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public List<string> MatchingSkills { get; set; } = [];
    public List<string> MissingSkills { get; set; } = [];
    public string? Summary { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public string? ReferralReason { get; set; }
    public string? WhatIsMissingForPerfectMatch { get; set; }
    public string? Priority { get; set; }
    public string? CountryRestrictions { get; set; }
}
