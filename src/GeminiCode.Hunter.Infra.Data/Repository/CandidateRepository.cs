using System;
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

public sealed class CandidateRepository(DataContext dataContext) : BaseEntityRepository<Candidate>(dataContext), ICandidateRepository
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

        // Public candidates are kept separate from the internal records
        return await DataContext.Candidates
            .FirstOrDefaultAsync(x => x.Email == email && x.Source == CandidateSource.Internal);
    }

    public async Task<Candidate?> GetByPublicTokenAsync(string publicToken)
    {
        if (string.IsNullOrWhiteSpace(publicToken))
            return null;

        return await DataContext.Candidates
            .FirstOrDefaultAsync(x => x.PublicToken == publicToken);
    }

    public async Task<Candidate?> GetByEmailConfirmationTokenAsync(string confirmationToken)
    {
        if (string.IsNullOrWhiteSpace(confirmationToken))
            return null;

        return await DataContext.Candidates
            .FirstOrDefaultAsync(x => x.EmailConfirmationToken == confirmationToken);
    }

    public async Task<List<Candidate>> GetStalePublicCandidatesAsync(DateTime unsavedBefore, DateTime unconfirmedBefore)
    {
        return await DataContext.Candidates
            .Where(x => x.Source == CandidateSource.Public && !x.IsEmailConfirmed)
            .Where(x => (!x.HasAcceptedTerms && x.CreatedAt < unsavedBefore)
                     || (x.HasAcceptedTerms && x.CreatedAt < unconfirmedBefore))
            .AsNoTracking()
            .ToListAsync();
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
        // Public candidates only become visible internally after confirming their email
        IQueryable<Candidate> query = DataContext.Set<Candidate>()
            .Include(x => x.User)
            .Where(x => x.Source == CandidateSource.Internal || x.IsEmailConfirmed);

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
