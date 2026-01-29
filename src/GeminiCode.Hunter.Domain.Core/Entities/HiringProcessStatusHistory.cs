using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class HiringProcessStatusHistory : BaseEntity
{
    public long HiringProcessStatusHistoryId { get; set; }
    public required long HiringProcessId { get; set; }
    public required HiringProcessStatus FromStatus { get; set; }
    public required HiringProcessStatus ToStatus { get; set; }
    public required long ChangedByUserId { get; set; }
    public string? Notes { get; set; }
    
    public virtual HiringProcess? HiringProcess { get; set; }
    public virtual User? ChangedByUser { get; set; }
}
