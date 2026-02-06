using MediatR;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;

public class GetActiveJobsRequestHandler(IUnitOfWork unitOfWork): IRequestHandler<GetActiveJobsRequest, GetActiveJobsResponse>
{
    public async Task<GetActiveJobsResponse> Handle(GetActiveJobsRequest request, CancellationToken cancellationToken)
    {
        var jobs = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();

        var response = new GetActiveJobsResponse
        {
            Jobs = jobs.Select(jobPosition => new JobPositionDto
            {
                PublicId = jobPosition.PublicId,
                Title = jobPosition.Title,
                ExternalUrl = jobPosition.ExternalUrl,
                CompanyCode = jobPosition.CompanyCode,
                JobCode = jobPosition.JobCode,
                Summary = jobPosition.Summary,
                Responsibilities = jobPosition.Responsibilities,
                Requirements = jobPosition.Requirements,
                MustHaveSkills = jobPosition.MustHaveSkills,
                NiceToHaveSkills = jobPosition.NiceToHaveSkills,
                Priority = jobPosition.Priority,
                CountryRestrictions = jobPosition.CountryRestrictions,
                CreatedAt = jobPosition.CreatedAt,
                LastCheckedAt = jobPosition.LastCheckedAt
            }).ToList(),
            TotalCount = jobs.Count
        };

        return response;
    }
}
