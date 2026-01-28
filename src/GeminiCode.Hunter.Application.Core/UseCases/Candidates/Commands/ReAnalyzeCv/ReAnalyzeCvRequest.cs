using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.ReAnalyzeCv;

public class ReAnalyzeCvRequest : IRequest<ReAnalyzeCvResponse>
{
    public required string CandidateId { get; set; }
}
