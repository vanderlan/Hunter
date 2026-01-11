namespace GeminiCode.Hunter.Domain.Core.Filters;

public class HiringProcessFilter : BaseFilter
{
    public long? CandidateId { get; set; }
    public long? JobPositionId { get; set; }
    public int? Status { get; set; }
    public long? CreatedByUserId { get; set; }
}
