namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Accept;

public class ReferralRequestAcceptResponse
{
    public string ReferralRequestId { get; set; } = string.Empty;
    public string HiringProcessId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
