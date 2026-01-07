using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetPaginated;

public class JobPositionGetPaginatedRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<JobPositionGetPaginatedRequest, PagedList<JobPositionGetPaginatedResponse>>
{
    public async Task<PagedList<JobPositionGetPaginatedResponse>> Handle(JobPositionGetPaginatedRequest request, CancellationToken cancellationToken)
    {
        var jobPositions = await unitOfWork.JobPositionRepository.ListPaginateAsync(request);

        var jobPositionsPaginated = jobPositions.Items.Select(jobPosition => (JobPositionGetPaginatedResponse)jobPosition).ToList();

        return new PagedList<JobPositionGetPaginatedResponse>(jobPositionsPaginated, jobPositions.Count);
    }
}
