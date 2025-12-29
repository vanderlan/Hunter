using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;

public class GetActiveJobsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActiveJobsRequest, GetActiveJobsResponse>
{
    public async Task<GetActiveJobsResponse> Handle(GetActiveJobsRequest request, CancellationToken cancellationToken)
    {
        var jobs = string.IsNullOrWhiteSpace(request.CompanyCode)
            ? await unitOfWork.JobPositionRepository.GetActivePositionsAsync()
            : await unitOfWork.JobPositionRepository.GetByCompanyCodeAsync(request.CompanyCode);

        var response = new GetActiveJobsResponse
        {
            Jobs = jobs.Select(j => new JobPositionDto
            {
                PublicId = j.PublicId,
                Title = j.Title,
                ExternalUrl = j.ExternalUrl,
                CompanyCode = j.CompanyCode,
                JobCode = j.JobCode,
                Summary = j.Summary,
                MustHaveSkills = j.MustHaveSkills,
                NiceToHaveSkills = j.NiceToHaveSkills,
                CreatedAt = j.CreatedAt,
                LastCheckedAt = j.LastCheckedAt
            }).ToList(),
            TotalCount = jobs.Count
        };

        return response;
    }
}
