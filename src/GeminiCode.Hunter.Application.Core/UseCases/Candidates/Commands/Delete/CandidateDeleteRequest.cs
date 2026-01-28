using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Delete;

public class CandidateDeleteRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
}
