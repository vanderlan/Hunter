using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class HiringProcessRepository(DataContext dataContext) : BaseEntityRepository<HiringProcess>(dataContext), IHiringProcessRepository
{
    public override async Task<HiringProcess> GetByIdAsync(string publicId)
    {
        return await DataContext.Set<HiringProcess>()
            .Include(x => x.Candidate)
            .Include(x => x.JobPosition)
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastUpdatedByUser)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.PublicId == publicId);
    }

    public async Task<PagedList<HiringProcess>> ListPaginateAsync(HiringProcessFilter filter)
    {
        IQueryable<HiringProcess> query = DataContext.Set<HiringProcess>()
            .Include(x => x.Candidate)
            .Include(x => x.JobPosition)
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastUpdatedByUser);

        query = ApplyFilters(filter, query);

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var entityList = await query.OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<HiringProcess>(entityList, await query.CountAsync());
    }

    public async Task<HiringProcess?> GetByCandidateAndJobPositionAsync(long candidateId, long jobPositionId)
    {
        return await DataContext.Set<HiringProcess>()
            .FirstOrDefaultAsync(x => x.CandidateId == candidateId && x.JobPositionId == jobPositionId);
    }

    public async Task<List<HiringProcess>> GetByCandidateIdAsync(long candidateId)
    {
        return await DataContext.Set<HiringProcess>()
            .Include(x => x.JobPosition)
            .Include(x => x.CreatedByUser)
            .Where(x => x.CandidateId == candidateId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<HiringProcessStatus, int>> GetDashboardStatsAsync()
    {
        var stats = await DataContext.Set<HiringProcess>()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<Dictionary<HiringProcessStatus, int>> GetDashboardStatsByUserAsync(long userId)
    {
        var stats = await DataContext.Set<HiringProcess>()
            .Where(x => x.CreatedByUserId == userId)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(x => x.Status, x => x.Count);
    }

    private static IQueryable<HiringProcess> ApplyFilters(HiringProcessFilter filter, IQueryable<HiringProcess> query)
    {
        if (filter.CandidateId.HasValue)
        {
            query = query.Where(x => x.CandidateId == filter.CandidateId.Value);
        }

        if (filter.JobPositionId.HasValue)
        {
            query = query.Where(x => x.JobPositionId == filter.JobPositionId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => (int)x.Status == filter.Status.Value);
        }

        if (filter.CreatedByUserId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserId == filter.CreatedByUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var searchTerm = filter.Query.ToLower();
            query = query.Where(x => 
                x.Candidate!.Name!.ToLower().Contains(searchTerm) || 
                x.JobPosition!.Title.ToLower().Contains(searchTerm));
        }

        return query;
    }
}
