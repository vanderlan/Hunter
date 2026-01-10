using System;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class HiringProcess : BaseEntity
{
    public long HiringProcessId { get; set; }
    public required long CandidateId { get; set; }
    public required long JobPositionId { get; set; }
    public required HiringProcessStatus Status { get; set; }
    public required long CreatedByUserId { get; set; }
    public long? LastUpdatedByUserId { get; set; }
    
    public virtual Candidate? Candidate { get; set; }
    public virtual JobPosition? JobPosition { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual User? LastUpdatedByUser { get; set; }
}
