using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IPromptRepository : IBaseEntityRepository<Prompt>
{
    Task<Prompt?> GetByKeyAsync(string key);
    Task<List<Prompt>> GetAllAsync();
}
