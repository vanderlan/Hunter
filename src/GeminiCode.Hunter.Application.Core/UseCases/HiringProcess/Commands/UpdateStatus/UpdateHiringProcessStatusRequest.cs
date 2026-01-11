using MediatR;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;

public class UpdateHiringProcessStatusRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
    public required HiringProcessStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}
