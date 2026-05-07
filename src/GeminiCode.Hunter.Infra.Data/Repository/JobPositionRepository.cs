using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public sealed class JobPositionRepository(DataContext dataContext) : BaseEntityRepository<JobPosition>(dataContext), IJobPositionRepository
{
    public async Task<JobPosition?> GetByExternalUrlAsync(string externalUrl)
    {
        return await DataContext.JobPositions
            .FirstOrDefaultAsync(x => x.ExternalUrl == externalUrl);
    }

    public async Task<List<JobPosition>> GetActivePositionsAsync()
    {
        return await DataContext.JobPositions
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsInactiveAsync(long jobPositionId)
    {
        var position = await DataContext.JobPositions.FindAsync(jobPositionId);
        if (position != null)
        {
            position.IsActive = false;
            position.ClosedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        }
    }

    public async Task MarkAsClosedAsync(long jobPositionId)
    {
        await MarkAsInactiveAsync(jobPositionId);
    }

    public async Task<PagedList<JobPosition>> ListPaginateAsync(JobPositionFilter filter)
    {
        IQueryable<JobPosition> query = DataContext.Set<JobPosition>();

        query = ApplyFilters(filter, query);

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var entityList = await query.OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<JobPosition>(entityList, await query.CountAsync());
    }

    private static IQueryable<JobPosition> ApplyFilters(JobPositionFilter filter, IQueryable<JobPosition> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var searchTerm = filter.Query.ToLower();
            query = query.Where(x => 
                                x.Title.ToLower().Contains(searchTerm) 
                                || x.Requirements.ToLower().Contains(searchTerm)
                                || x.Responsibilities.ToLower().Contains(searchTerm)
                                || x.MustHaveSkills.ToLower().Contains(searchTerm)
                                || x.NiceToHaveSkills.ToLower().Contains(searchTerm)                             
                                );
        }

        return query;
    }
}
