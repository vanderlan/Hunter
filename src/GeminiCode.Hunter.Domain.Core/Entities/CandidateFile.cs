namespace GeminiCode.Hunter.Domain.Core.Entities;

public class CandidateFile : BaseEntity
{
    public long CandidateFileId { get; set; }
    public long CandidateId { get; set; }
    public long FileId { get; set; }
    public required string FileType { get; set; }
    
    public virtual Candidate Candidate { get; set; }
    public virtual File File { get; set; }
}
