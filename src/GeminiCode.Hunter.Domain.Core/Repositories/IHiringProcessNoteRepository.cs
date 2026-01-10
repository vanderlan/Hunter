using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IHiringProcessNoteRepository : IBaseEntityRepository<HiringProcessNote>
{
    Task<List<HiringProcessNote>> GetByHiringProcessIdAsync(long hiringProcessId);
}
