using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class CandidateRepository(DataContext dataContext) : BaseEntityRepository<Candidate>(dataContext), ICandidateRepository
{
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
}
