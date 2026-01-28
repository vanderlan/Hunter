using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;

public class StartHiringProcessRequest : IRequest<StartHiringProcessResponse>
{
    public required string CandidateId { get; set; }
    public required string JobPositionId { get; set; }
    public string? ReferralReason { get; set; }
}
