using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.AddNote;

public class AddHiringProcessNoteRequest : IRequest
{
    public string HiringProcessId { get; set; } = string.Empty;
    public required string Note { get; set; }
}
