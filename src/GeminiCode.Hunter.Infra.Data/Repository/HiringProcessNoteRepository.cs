using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class HiringProcessNoteRepository(DataContext dataContext) : BaseEntityRepository<HiringProcessNote>(dataContext), IHiringProcessNoteRepository
{
    public async Task<List<HiringProcessNote>> GetByHiringProcessIdAsync(long hiringProcessId)
    {
        return await DataContext.Set<HiringProcessNote>()
            .Include(x => x.User)
            .Where(x => x.HiringProcessId == hiringProcessId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }
}
