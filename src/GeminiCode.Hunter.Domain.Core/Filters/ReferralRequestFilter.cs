namespace GeminiCode.Hunter.Domain.Core.Filters;

public class ReferralRequestFilter : BaseFilter
{
    public int? Status { get; set; }
    public long? JobPositionId { get; set; }
}
