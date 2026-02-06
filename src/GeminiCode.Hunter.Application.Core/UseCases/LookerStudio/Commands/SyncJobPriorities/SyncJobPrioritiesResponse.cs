namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.SyncJobPriorities;

public class SyncJobPrioritiesResponse
{
    public int TotalPrioritiesInCsv { get; set; }
    public int MatchedJobs { get; set; }
    public int UnmatchedJobs { get; set; }
    public int UpdatedJobs { get; set; }
    public List<string> UnmatchedJobCodes { get; set; } = new();
    public List<string> UpdatedJobCodes { get; set; } = new();
    public string Message { get; set; }
}
