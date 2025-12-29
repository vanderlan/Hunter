using System;
using System.Collections.Generic;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;

public class GetActiveJobsResponse
{
    public List<JobPositionDto> Jobs { get; set; } = new();
    public int TotalCount { get; set; }
}

public class JobPositionDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ExternalUrl { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string JobCode { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastCheckedAt { get; set; }
}
