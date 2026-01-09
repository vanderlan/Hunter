using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;

public class JobPositionCreateResponse
{
    public string PublicId { get; set; }
    public long JobPositionId { get; set; }
    public string Title { get; set; }
    public string ExternalUrl { get; set; }
    public string CompanyCode { get; set; }
    public string JobCode { get; set; }
    public string? Summary { get; set; }
    public string? Responsibilities { get; set; }
    public string? Skills { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime LastCheckedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }

    public static explicit operator JobPositionCreateResponse(JobPosition jobPosition)
    {
        if (jobPosition is null)
            return default;

        return new JobPositionCreateResponse
        {
            PublicId = jobPosition.PublicId,
            JobPositionId = jobPosition.JobPositionId,
            Title = jobPosition.Title,
            ExternalUrl = jobPosition.ExternalUrl,
            CompanyCode = jobPosition.CompanyCode,
            JobCode = jobPosition.JobCode,
            Summary = jobPosition.Summary,
            Responsibilities = jobPosition.Responsibilities,
            Skills = jobPosition.Skills,
            MustHaveSkills = jobPosition.MustHaveSkills,
            NiceToHaveSkills = jobPosition.NiceToHaveSkills,
            IsActive = jobPosition.IsActive,
            ClosedAt = jobPosition.ClosedAt,
            LastCheckedAt = jobPosition.LastCheckedAt,
            LastUpdated = jobPosition.LastUpdated,
            CreatedAt = jobPosition.CreatedAt
        };
    }
}
