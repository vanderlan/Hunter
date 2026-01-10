using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;

public class HiringProcessGetPaginatedRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<HiringProcessGetPaginatedRequestHandler> logger)
    : IRequestHandler<HiringProcessGetPaginatedRequest, PagedList<HiringProcessGetPaginatedResponse>>
{
    public async Task<PagedList<HiringProcessGetPaginatedResponse>> Handle(
        HiringProcessGetPaginatedRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting paginated hiring processes. Page: {Page}, Quantity: {Quantity}", 
            request.Page, request.Quantity);

        var pagedHiringProcesses = await unitOfWork.HiringProcessRepository.ListPaginateAsync(request);

        var response = pagedHiringProcesses.Items.Select(hp => new HiringProcessGetPaginatedResponse
        {
            PublicId = hp.PublicId,
            Status = hp.Status.ToString(),
            CandidateName = hp.Candidate?.Name ?? "Unknown",
            CandidateEmail = hp.Candidate?.Email ?? "",
            JobTitle = hp.JobPosition?.Title ?? "",
            JobCode = hp.JobPosition?.JobCode ?? "",
            CompanyCode = hp.JobPosition?.CompanyCode ?? "",
            CreatedByUserName = hp.CreatedByUser?.Name ?? "",
            CreatedAt = hp.CreatedAt,
            LastUpdated = hp.LastUpdated
        }).ToList();

        logger.LogInformation("Retrieved {Count} hiring processes", response.Count);

        return new PagedList<HiringProcessGetPaginatedResponse>(response, pagedHiringProcesses.Count);
    }
}
