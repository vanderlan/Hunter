namespace GeminiCode.Hunter.Domain.Core.Entities;

public class HiringProcessNote : BaseEntity
{
    public long HiringProcessNoteId { get; set; }
    public required long HiringProcessId { get; set; }
    public required long UserId { get; set; }
    public required string Note { get; set; }
    
    public virtual HiringProcess? HiringProcess { get; set; }
    public virtual User? User { get; set; }
}
