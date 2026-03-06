namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetDashboard;

public class HiringProcessGetDashboardResponse
{
    public Dictionary<string, int> StatusCounts { get; set; } = [];
    public int TotalProcesses { get; set; }
    public int ActiveProcesses { get; set; }
    public int HiredCount { get; set; }
    public int ReprovedCount { get; set; }
}
