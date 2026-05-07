namespace GeminiCode.Hunter.Domain.Core.Entities;

public class CvAnalysisMatch : BaseEntity
{
    public long CvAnalysisMatchId { get; set; }
    public long CvAnalysisId { get; set; }
    public long JobPositionId { get; set; }

    public decimal MatchPercentage { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public string MatchingSkills { get; set; } = string.Empty;
    public string MissingSkills { get; set; } = string.Empty;
    public string? WhatIsMissingForPerfectMatch { get; set; }

    public string JobTitle { get; set; } = string.Empty;
    public string? JobSummary { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }

    public virtual CvAnalysis CvAnalysis { get; set; } = null!;
    public virtual JobPosition JobPosition { get; set; } = null!;
}
