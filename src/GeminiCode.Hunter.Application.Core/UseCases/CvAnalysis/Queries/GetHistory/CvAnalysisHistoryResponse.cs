namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetHistory;

public class CvAnalysisHistoryResponse
{
    public string AnalysisId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public string AnalyzedByUserName { get; set; } = string.Empty;
    public int TotalPositionsAnalyzed { get; set; }
    public int MatchesFound { get; set; }
}
