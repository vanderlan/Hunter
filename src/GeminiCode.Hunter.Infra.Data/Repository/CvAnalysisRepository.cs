using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;
using Microsoft.EntityFrameworkCore;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public sealed class CvAnalysisRepository(DataContext dataContext) 
    : BaseEntityRepository<CvAnalysis>(dataContext), ICvAnalysisRepository
{
    public async Task<List<CvAnalysis>> GetByUserIdAsync(long userId, int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;
        
        return await DataContext.CvAnalyses
            .Include(x => x.Candidate)
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.AnalyzedAt)
            .Skip(skip)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(long userId)
    {
        return await DataContext.CvAnalyses
            .Where(x => x.UserId == userId)
            .CountAsync();
    }

    public async Task<List<CvAnalysis>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;
        
        return await DataContext.CvAnalyses
            .Include(x => x.Candidate)
            .Include(x => x.User)
            .OrderByDescending(x => x.AnalyzedAt)
            .Skip(skip)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetAllCountAsync()
    {
        return await DataContext.CvAnalyses.CountAsync();
    }

    public async Task<PagedList<CvAnalysis>> GetByUserIdWithFilterAsync(long userId, CvAnalysisFilter filter)
    {
        IQueryable<CvAnalysis> query = DataContext.CvAnalyses
            .Include(x => x.Candidate)
            .Include(x => x.User)
            .Where(x => x.UserId == userId);

        query = ApplyFilters(filter, query);

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var entityList = await query.OrderByDescending(x => x.AnalyzedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<CvAnalysis>(entityList, await query.CountAsync());
    }

    public async Task<PagedList<CvAnalysis>> GetAllWithFilterAsync(CvAnalysisFilter filter)
    {
        IQueryable<CvAnalysis> query = DataContext.CvAnalyses
            .Include(x => x.Candidate)
            .Include(x => x.User);

        query = ApplyFilters(filter, query);

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var entityList = await query.OrderByDescending(x => x.AnalyzedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<CvAnalysis>(entityList, await query.CountAsync());
    }

    private static IQueryable<CvAnalysis> ApplyFilters(CvAnalysisFilter filter, IQueryable<CvAnalysis> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var searchTerm = filter.Query.ToLower();
            query = query.Where(x => x.Candidate != null && x.Candidate.Name.ToLower().Contains(searchTerm));
        }

        return query;
    }

    public async Task<CvAnalysis?> GetWithMatchesAsync(string publicId)
    {
        return await DataContext.CvAnalyses
            .Include(x => x.Candidate)
            .Include(x => x.User)
            .Include(x => x.Matches)
                .ThenInclude(x => x.JobPosition)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PublicId == publicId);
    }

    public async Task DeleteByCandidateIdAndUserAsync(long candidateId, long userId)
    {
        var existingAnalyses = await DataContext.CvAnalyses
            .Include(x => x.Matches)
            .Where(x => x.CandidateId == candidateId && x.UserId == userId)
            .ToListAsync();

        await DeletePreviousAnalysis(existingAnalyses);
    }

    private async Task DeletePreviousAnalysis(List<CvAnalysis> existingAnalyses)
    {
        if (existingAnalyses.Any())
        {
            foreach (var analysis in existingAnalyses)
            {
                DataContext.CvAnalysisMatches.RemoveRange(analysis.Matches);
            }

            DataContext.CvAnalyses.RemoveRange(existingAnalyses);
        }
    }

    public async Task DeleteByCandidateIdAsync(long candidateId)
    {
        var existingAnalyses = await DataContext.CvAnalyses
            .Include(x => x.Matches)
            .Where(x => x.CandidateId == candidateId)
            .ToListAsync();

        await DeletePreviousAnalysis(existingAnalyses);
    }

    public async Task<CvAnalysisMatch?> GetLastAnalysisMatchAsync(long candidateId, long jobPositionId)
    {
        return await DataContext.CvAnalysisMatches
            .Include(x => x.CvAnalysis)
            .Where(x => x.CvAnalysis.CandidateId == candidateId && x.JobPositionId == jobPositionId)
            .OrderByDescending(x => x.CvAnalysis.AnalyzedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task DeleteMatchesByJobPositionIdAsync(long jobPositionId)
    {
        var matches = await DataContext.CvAnalysisMatches
            .Where(x => x.JobPositionId == jobPositionId)
            .ToListAsync();

        if (matches.Any())
        {
            DataContext.CvAnalysisMatches.RemoveRange(matches);
            await DataContext.SaveChangesAsync();
        }
    }
}

