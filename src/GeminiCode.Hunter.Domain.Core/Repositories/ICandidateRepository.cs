using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ICandidateRepository : IBaseEntityRepository<Candidate>
{
    Task<Candidate?> GetByEmailAsync(string email);
    Task<List<Candidate>> GetRecentCandidatesAsync(int count);
    Task<bool> ExistsByEmailAsync(string email);
}
