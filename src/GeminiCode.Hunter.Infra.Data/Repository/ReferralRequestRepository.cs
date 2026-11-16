using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public sealed class ReferralRequestRepository(DataContext dataContext)
    : BaseEntityRepository<ReferralRequest>(dataContext), IReferralRequestRepository
{
    public override async Task<ReferralRequest> GetByIdAsync(string publicId)
    {
        return await DataContext.ReferralRequests
            .Include(x => x.Candidate)
            .Include(x => x.JobPosition)
            .Include(x => x.ResolvedByUser)
            .Include(x => x.HiringProcess)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.PublicId == publicId);
    }

    public async Task<PagedList<ReferralRequest>> ListPaginateAsync(ReferralRequestFilter filter)
    {
        IQueryable<ReferralRequest> query = DataContext.ReferralRequests
            .Include(x => x.Candidate)
            .Include(x => x.JobPosition)
            .Include(x => x.ResolvedByUser)
            .Include(x => x.HiringProcess)
            .Where(x => x.Candidate!.IsEmailConfirmed);

        if (filter.Status.HasValue)
        {
            query = query.Where(x => (int)x.Status == filter.Status.Value);
        }

        if (filter.JobPositionId.HasValue)
        {
            query = query.Where(x => x.JobPositionId == filter.JobPositionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var term = filter.Query.ToLower();
            query = query.Where(x =>
                x.Candidate!.Name!.ToLower().Contains(term) ||
                x.Candidate!.Email!.ToLower().Contains(term) ||
                x.JobPosition!.Title.ToLower().Contains(term));
        }

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var items = await query
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<ReferralRequest>(items, await query.CountAsync());
    }

    public async Task<ReferralRequest?> GetByCandidateAndJobPositionAsync(long candidateId, long jobPositionId)
    {
        return await DataContext.ReferralRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CandidateId == candidateId && x.JobPositionId == jobPositionId);
    }

    public async Task<List<ReferralRequest>> GetPendingByCandidateIdAsync(long candidateId)
    {
        return await DataContext.ReferralRequests
            .Include(x => x.JobPosition)
            .Where(x => x.CandidateId == candidateId && x.Status == ReferralRequestStatus.Pending)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await DataContext.ReferralRequests
            .Where(x => x.Status == ReferralRequestStatus.Pending && x.Candidate!.IsEmailConfirmed)
            .CountAsync();
    }

    public async Task DeleteByCandidateIdAsync(long candidateId)
    {
        var requests = await DataContext.ReferralRequests
            .Where(x => x.CandidateId == candidateId)
            .ToListAsync();

        DataContext.ReferralRequests.RemoveRange(requests);
    }

    public async Task DeleteByJobPositionIdAsync(long jobPositionId)
    {
        var requests = await DataContext.ReferralRequests
            .Where(x => x.JobPositionId == jobPositionId)
            .ToListAsync();

        DataContext.ReferralRequests.RemoveRange(requests);
    }

    public async Task<bool> ExistsByJobPositionIdAsync(long jobPositionId)
    {
        return await DataContext.ReferralRequests.AnyAsync(x => x.JobPositionId == jobPositionId);
    }
}
