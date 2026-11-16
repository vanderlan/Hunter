using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IJobPositionRepository : IBaseEntityRepository<JobPosition>
{
    Task<JobPosition?> GetByExternalUrlAsync(string externalUrl);
    Task<List<JobPosition>> GetActivePositionsAsync();
    Task MarkAsInactiveAsync(long jobPositionId);
    Task MarkAsClosedAsync(long jobPositionId);
    Task<PagedList<JobPosition>> ListPaginateAsync(JobPositionFilter filter);

    Task<JobPosition?> GetByPublicCodeAsync(string publicCode);
    Task<bool> ExistsByPublicCodeAsync(string publicCode);

    /// <summary>Active positions that already have company-neutral public content.</summary>
    Task<List<JobPosition>> GetPublicPositionsAsync();
}
