using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public sealed class HiringProcessStatusHistoryRepository(DataContext dataContext) : BaseEntityRepository<HiringProcessStatusHistory>(dataContext), IHiringProcessStatusHistoryRepository
{
    public async Task<List<HiringProcessStatusHistory>> GetByHiringProcessIdAsync(long hiringProcessId)
    {
        return await DataContext.Set<HiringProcessStatusHistory>()
            .Include(x => x.ChangedByUser)
            .Where(x => x.HiringProcessId == hiringProcessId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }
}
