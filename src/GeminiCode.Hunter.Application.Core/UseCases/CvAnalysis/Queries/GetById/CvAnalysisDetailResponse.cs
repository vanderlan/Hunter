using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetById;

public class CvAnalysisDetailResponse
{
    public string AnalysisId { get; set; } = string.Empty;
    public CandidateInfoDto CandidateInfo { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
    public string AnalyzedByUserName { get; set; } = string.Empty;
    public int TotalPositionsAnalyzed { get; set; }
    public int MatchesFound { get; set; }
    public List<JobMatchDto> Matches { get; set; } = [];
}
