using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;
using Microsoft.EntityFrameworkCore;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class CvAnalysisRepository(DataContext dataContext) 
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

    public async Task DeleteByCandidateIdAsync(long candidateId)
    {
        var existingAnalyses = await DataContext.CvAnalyses
            .Include(x => x.Matches)
            .Where(x => x.CandidateId == candidateId)
            .ToListAsync();

        if (existingAnalyses.Any())
        {
            // Delete all matches first (due to foreign key constraint)
            foreach (var analysis in existingAnalyses)
            {
                DataContext.CvAnalysisMatches.RemoveRange(analysis.Matches);
            }

            // Then delete all analyses
            DataContext.CvAnalyses.RemoveRange(existingAnalyses);
            
            await DataContext.SaveChangesAsync();
        }
    }
}
