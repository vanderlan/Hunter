using MediatR;
using System.Text.Json.Serialization;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.RegisterPublicCandidate;

public class RegisterPublicCandidateRequest : IRequest<RegisterPublicCandidateResponse>
{
    [JsonIgnore]
    public string CandidateToken { get; set; } = string.Empty;
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool ReceiveJobMatchNotifications { get; set; }
    public bool HasAcceptedTerms { get; set; }
}
