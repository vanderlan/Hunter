using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository.Implementations;

public class ParameterRepository(DataContext dataContext) : BaseEntityRepository<Parameter>(dataContext), IParameterRepository
{
    public async Task<Parameter?> GetByKeyAsync(string key)
    {
        return await DataContext.Parameters
            .FirstOrDefaultAsync(x => x.Key == key);
    }

    public async Task<List<Parameter>> GetAllAsync()
    {
        return await DataContext.Parameters
            .OrderBy(x => x.Key)
            .ToListAsync();
    }
}
