using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IHiringProcessStatusHistoryRepository : IBaseEntityRepository<HiringProcessStatusHistory>
{
    Task<List<HiringProcessStatusHistory>> GetByHiringProcessIdAsync(long hiringProcessId);
}
