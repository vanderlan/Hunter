using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Extensions;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetPaginated;

public class JobPositionGetPaginatedResponse
{
    public string PublicId { get; set; }
    public long JobPositionId { get; set; }
    public string Title { get; set; }
    public string ExternalUrl { get; set; }
    public string CompanyCode { get; set; }
    public string JobCode { get; set; }
    public string? Summary { get; set; }
    public string? Priority { get; set; }
    public string? CountryRestrictions { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime LastCheckedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }

    public static explicit operator JobPositionGetPaginatedResponse(JobPosition jobPosition)
    {
        if (jobPosition is null)
            return default;

        return new JobPositionGetPaginatedResponse
        {
            PublicId = jobPosition.PublicId,
            JobPositionId = jobPosition.JobPositionId,
            Title = jobPosition.Title,
            ExternalUrl = jobPosition.ExternalUrl,
            CompanyCode = jobPosition.CompanyCode,
            JobCode = jobPosition.JobCode,
            Summary = jobPosition.Summary,
            Priority = jobPosition.Priority,
            CountryRestrictions = jobPosition.CountryRestrictions,
            IsActive = jobPosition.IsActive,
            ClosedAt = jobPosition.ClosedAt?.ToApplicationTime(), // Convert to configured timezone
            LastCheckedAt = jobPosition.LastCheckedAt.ToApplicationTime(), // Convert to configured timezone
            LastUpdated = jobPosition.LastUpdated.ToApplicationTime(), // Convert to configured timezone
            CreatedAt = jobPosition.CreatedAt.ToApplicationTime() // Convert to configured timezone
        };
    }
}
