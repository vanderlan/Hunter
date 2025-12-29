using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

public class JobChangesDetectedNotification : INotification
{
    public int NewPositionsCount { get; set; }
    public int ClosedPositionsCount { get; set; }
    public int UpdatedPositionsCount { get; set; }
    public int TotalScraped { get; set; }
    public List<string> NewJobTitles { get; set; } = new();
    public List<string> ClosedJobTitles { get; set; } = new();
    public DateTime SyncTimestamp { get; set; }
}
