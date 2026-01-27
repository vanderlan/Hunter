using MediatR;
using GeminiCode.Hunter.Domain.Core.Models;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Notifications.JobChangesDetected;

public class JobChangesDetectedNotification : INotification
{
    public int NewPositionsCount { get; set; }
    public int ClosedPositionsCount { get; set; }
    public int UpdatedPositionsCount { get; set; }
    public int TotalScraped { get; set; }
    public List<string> NewJobTitles { get; set; } = [];
    public List<string> ClosedJobTitles { get; set; } = [];
    public List<JobPositionEmailData> NewJobPositions { get; set; } = [];
    public List<JobPositionEmailData> ClosedJobPositions { get; set; } = [];
    public DateTime SyncTimestamp { get; set; }
}
