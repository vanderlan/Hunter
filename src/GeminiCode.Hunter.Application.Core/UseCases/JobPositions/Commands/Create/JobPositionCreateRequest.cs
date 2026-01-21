using GeminiCode.Hunter.Domain.Core.Entities;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;

public class JobPositionCreateRequest : IRequest<JobPositionCreateResponse>
{
    public string Title { get; set; }
    public string ExternalUrl { get; set; }
    public string CompanyCode { get; set; }
    public string JobCode { get; set; }
    public string? Summary { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }

    public static explicit operator JobPosition(JobPositionCreateRequest request)
    {
        if (request is null)
            return default;

#pragma warning disable CS9035 // Required member must be set
        return new JobPosition
        {
            Title = request.Title,
            ExternalUrl = request.ExternalUrl,
            CompanyCode = request.CompanyCode,
            JobCode = request.JobCode,
            Summary = request.Summary,
            Responsibilities = request.Responsibilities,
            Requirements = request.Requirements,
            MustHaveSkills = request.MustHaveSkills,
            NiceToHaveSkills = request.NiceToHaveSkills,
            IsActive = true,
            LastCheckedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
#pragma warning restore CS9035
    }
}
