using System;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class JobPosition : BaseEntity
{
    public long JobPositionId { get; set; }
    public required string Title { get; set; }
    public required string ExternalUrl { get; set; }
    public required string CompanyCode { get; set; }
    public required string JobCode { get; set; }
    public string? Summary { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime LastCheckedAt { get; set; }
}
