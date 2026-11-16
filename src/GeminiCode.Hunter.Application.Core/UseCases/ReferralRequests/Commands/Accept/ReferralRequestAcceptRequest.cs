using MediatR;
using System.Text.Json.Serialization;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Accept;

public class ReferralRequestAcceptRequest : IRequest<ReferralRequestAcceptResponse>
{
    [JsonIgnore]
    public string PublicId { get; set; } = string.Empty;
    public string? Note { get; set; }
}
