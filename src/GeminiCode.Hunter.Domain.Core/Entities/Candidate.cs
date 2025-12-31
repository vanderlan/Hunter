using System;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class Candidate : BaseEntity
{
    public long CandidateId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public required string CvText { get; set; }
    public DateTime AnalyzedAt { get; set; }
}
