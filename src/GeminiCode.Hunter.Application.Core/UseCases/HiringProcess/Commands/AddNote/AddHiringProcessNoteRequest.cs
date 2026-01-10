using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.AddNote;

public class AddHiringProcessNoteRequest : IRequest
{
    public required string HiringProcessId { get; set; }
    public required string Note { get; set; }
}
