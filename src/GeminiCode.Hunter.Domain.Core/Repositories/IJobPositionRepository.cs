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
    Task<JobPosition?> GetByJobCodeAsync(string jobCode);
    Task<List<JobPosition>> GetActivePositionsAsync();
    Task<List<JobPosition>> GetByCompanyCodeAsync(string companyCode);
    Task MarkAsInactiveAsync(long jobPositionId);
    Task MarkAsClosedAsync(long jobPositionId);
    Task<PagedList<JobPosition>> ListPaginateAsync(JobPositionFilter filter);
}
