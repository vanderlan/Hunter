using MediatR;
using System.Text.Json.Serialization;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Decline;

public class ReferralRequestDeclineRequest : IRequest
{
    [JsonIgnore]
    public string PublicId { get; set; } = string.Empty;
    public string? Note { get; set; }
}
