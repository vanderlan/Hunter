using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Notifications.NoteAdded;

public class HiringProcessNoteAddedNotification : INotification
{
    public string HiringProcessId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string NoteText { get; set; } = string.Empty;
    public string AddedByUserName { get; set; } = string.Empty;
    public string ReferredByUserEmail { get; set; } = string.Empty;
}
