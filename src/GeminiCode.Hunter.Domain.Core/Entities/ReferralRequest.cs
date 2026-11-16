using System;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Domain.Core.Entities;

/// <summary>
/// A public candidate asking someone inside the platform to refer them for a
/// position. Accepting it creates a regular <see cref="HiringProcess"/>.
/// </summary>
public class ReferralRequest : BaseEntity
{
    public long ReferralRequestId { get; set; }
    public required long CandidateId { get; set; }
    public required long JobPositionId { get; set; }
    public ReferralRequestStatus Status { get; set; } = ReferralRequestStatus.Pending;
    public string? Message { get; set; }
    public decimal? MatchPercentage { get; set; }
    public long? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
    public long? HiringProcessId { get; set; }

    public virtual Candidate? Candidate { get; set; }
    public virtual JobPosition? JobPosition { get; set; }
    public virtual User? ResolvedByUser { get; set; }
    public virtual HiringProcess? HiringProcess { get; set; }
}
