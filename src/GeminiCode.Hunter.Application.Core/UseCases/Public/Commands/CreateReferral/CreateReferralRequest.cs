using MediatR;
using System.Text.Json.Serialization;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.CreateReferral;

/// <summary>A public candidate asking to be referred for a position.</summary>
public class CreateReferralRequest : IRequest<CreateReferralResponse>
{
    [JsonIgnore]
    public string CandidateToken { get; set; } = string.Empty;
    public required string JobCode { get; set; }
    public string? Message { get; set; }
}
