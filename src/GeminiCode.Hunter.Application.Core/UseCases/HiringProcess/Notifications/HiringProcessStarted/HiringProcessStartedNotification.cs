using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.HiringProcessStarted;

public class HiringProcessStartedNotification : INotification
{
    public string HiringProcessId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobCode { get; set; } = string.Empty;
    public string StartedByUserName { get; set; } = string.Empty;
}
