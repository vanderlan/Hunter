using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IReferralRequestRepository : IBaseEntityRepository<ReferralRequest>
{
    /// <summary>Lists requests from candidates that are visible internally (email confirmed).</summary>
    Task<PagedList<ReferralRequest>> ListPaginateAsync(ReferralRequestFilter filter);
    Task<ReferralRequest?> GetByCandidateAndJobPositionAsync(long candidateId, long jobPositionId);
    Task<List<ReferralRequest>> GetPendingByCandidateIdAsync(long candidateId);
    Task<int> GetPendingCountAsync();
    Task DeleteByCandidateIdAsync(long candidateId);
    Task DeleteByJobPositionIdAsync(long jobPositionId);
    Task<bool> ExistsByJobPositionIdAsync(long jobPositionId);
}
