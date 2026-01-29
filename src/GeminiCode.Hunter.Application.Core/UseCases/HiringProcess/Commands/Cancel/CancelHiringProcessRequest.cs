using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Cancel;

public class CancelHiringProcessRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
    public required string CancelReason { get; set; }
}
