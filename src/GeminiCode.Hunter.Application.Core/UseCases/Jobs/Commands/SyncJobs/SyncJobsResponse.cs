namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Commands.SyncJobs;

public class SyncJobsResponse
{
    public int TotalScraped { get; set; }
    public int NewPositions { get; set; }
    public int UpdatedPositions { get; set; }
    public int ClosedPositions { get; set; }
    public List<string> NewJobTitles { get; set; } = [];
    public List<string> ClosedJobTitles { get; set; } = [];
}
