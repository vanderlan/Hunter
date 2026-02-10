using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Domain.Core.Filters;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IHiringProcessRepository : IBaseEntityRepository<HiringProcess>
{
    Task<PagedList<HiringProcess>> ListPaginateAsync(HiringProcessFilter filter);
    Task<HiringProcess?> GetByCandidateAndJobPositionAsync(long candidateId, long jobPositionId);
    Task<List<HiringProcess>> GetByCandidateIdAsync(long candidateId);
    Task<Dictionary<HiringProcessStatus, int>> GetDashboardStatsAsync();
    Task<Dictionary<HiringProcessStatus, int>> GetDashboardStatsByUserAsync(long userId);
    Task LoadRelatedEntitiesAsync(HiringProcess hiringProcess);
    Task<bool> HasActiveProcessByJobPositionIdAsync(long jobPositionId);
}
