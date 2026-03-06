using System;
using System.Collections.Generic;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class CvAnalysis : BaseEntity
{
    public long CvAnalysisId { get; set; }
    public long CandidateId { get; set; }
    public long UserId { get; set; }
    
    public DateTime AnalyzedAt { get; set; }
    public int TotalPositionsAnalyzed { get; set; }
    public int MatchesFound { get; set; }
    
    public virtual Candidate Candidate { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<CvAnalysisMatch> Matches { get; set; } = [];
}
