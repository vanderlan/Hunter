using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class CandidateRepository(DataContext dataContext) : BaseEntityRepository<Candidate>(dataContext), ICandidateRepository
{
    public override async Task<Candidate> GetByIdAsync(string publicId)
    {
        return await DataContext.Candidates
            .Include(x => x.User)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.PublicId == publicId);
    }

    public async Task<Candidate?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await DataContext.Candidates
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<List<Candidate>> GetRecentCandidatesAsync(int count)
    {
        return await DataContext.Candidates
            .OrderByDescending(x => x.AnalyzedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await DataContext.Candidates
            .AnyAsync(x => x.Email == email);
    }

    public async Task<PagedList<Candidate>> ListPaginateAsync(CandidateFilter filter)
    {
        IQueryable<Candidate> query = DataContext.Set<Candidate>()
            .Include(x => x.User);

        query = ApplyFilters(filter, query);

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var entityList = await query.OrderByDescending(x => x.AnalyzedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<Candidate>(entityList, await query.CountAsync());
    }

    private static IQueryable<Candidate> ApplyFilters(CandidateFilter filter, IQueryable<Candidate> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var searchTerm = filter.Query.ToLower();
            query = query.Where(x => 
                x.Name!.ToLower().Contains(searchTerm) || 
                x.Email!.ToLower().Contains(searchTerm));
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        return query;
    }
}
