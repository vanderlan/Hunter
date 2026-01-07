using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Domain.Core.Filters;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ICandidateRepository : IBaseEntityRepository<Candidate>
{
    Task<Candidate?> GetByEmailAsync(string email);
    Task<List<Candidate>> GetRecentCandidatesAsync(int count);
    Task<bool> ExistsByEmailAsync(string email);
    Task<PagedList<Candidate>> ListPaginateAsync(CandidateFilter filter);
}
