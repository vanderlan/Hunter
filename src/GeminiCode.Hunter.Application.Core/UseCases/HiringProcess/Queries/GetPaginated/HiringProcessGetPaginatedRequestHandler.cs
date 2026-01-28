using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;

public class HiringProcessGetPaginatedRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<HiringProcessGetPaginatedRequestHandler> logger)
    : IRequestHandler<HiringProcessGetPaginatedRequest, PagedList<HiringProcessGetPaginatedResponse>>
{
    public async Task<PagedList<HiringProcessGetPaginatedResponse>> Handle(
        HiringProcessGetPaginatedRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting paginated hiring processes. Page: {Page}, Quantity: {Quantity}", 
            request.Page, request.Quantity);

        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);

        if (!currentUser.Profile.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            request.CreatedByUserId = user.UserId;
            logger.LogInformation("Partner user filtering by CreatedByUserId: {UserId}", user.UserId);
        }

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
            CreatedAt = hp.CreatedAt.ToApplicationTime(), // Convert to configured timezone
            LastUpdated = hp.LastUpdated.ToApplicationTime() // Convert to configured timezone
        }).ToList();

        logger.LogInformation("Retrieved {Count} hiring processes", response.Count);

        return new PagedList<HiringProcessGetPaginatedResponse>(response, pagedHiringProcesses.Count);
    }
}
