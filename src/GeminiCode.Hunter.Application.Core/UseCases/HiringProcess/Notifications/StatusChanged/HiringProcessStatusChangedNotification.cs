using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.StatusChanged;

public class HiringProcessStatusChangedNotification : INotification
{
    public string HiringProcessId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string ReferredByUserEmail { get; set; } = string.Empty;
}
