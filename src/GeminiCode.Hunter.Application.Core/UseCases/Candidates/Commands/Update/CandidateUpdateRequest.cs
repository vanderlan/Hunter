using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;

public class CandidateUpdateRequest : IRequest
{
    public string PublicId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
