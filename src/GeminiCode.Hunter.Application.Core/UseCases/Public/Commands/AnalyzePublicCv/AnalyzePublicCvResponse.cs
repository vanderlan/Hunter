namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.AnalyzePublicCv;

public class AnalyzePublicCvResponse
{
    /// <summary>
    /// Opaque token identifying this visitor's CV. Needed to save the profile
    /// or ask for a referral. Null when nothing was stored (no open positions).
    /// </summary>
    public string? CandidateToken { get; set; }
    public int TotalPositionsAnalyzed { get; set; }
    public int MatchesFound { get; set; }

    /// <summary>Details extracted from the CV, used only to pre-fill the save-profile form.</summary>
    public string? ExtractedName { get; set; }
    public string? ExtractedEmail { get; set; }
    public string? ExtractedPhone { get; set; }

    public int MinimumMatchForReferral { get; set; }
    public List<PublicJobMatchDto> Matches { get; set; } = [];
}

public class PublicJobMatchDto
{
    public string JobCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal MatchPercentage { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public List<string> MatchingSkills { get; set; } = [];
    public List<string> MissingSkills { get; set; } = [];
    public string? WhatIsMissingForPerfectMatch { get; set; }
    public bool CanRequestReferral { get; set; }
}
