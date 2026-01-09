using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ICandidateFileRepository : IBaseEntityRepository<CandidateFile>
{
    Task<List<CandidateFile>> GetByCandidateIdAsync(long candidateId);
    Task<CandidateFile> GetByCandidateAndFileTypeAsync(long candidateId, string fileType);
}
