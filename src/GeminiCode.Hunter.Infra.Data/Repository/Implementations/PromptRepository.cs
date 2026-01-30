using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository.Implementations;

public class PromptRepository(DataContext dataContext) : BaseEntityRepository<Prompt>(dataContext), IPromptRepository
{
    public async Task<Prompt?> GetByKeyAsync(string key)
    {
        return await DataContext.Prompts
            .FirstOrDefaultAsync(x => x.Key == key);
    }

    public async Task<List<Prompt>> GetAllAsync()
    {
        return await DataContext.Prompts
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}
