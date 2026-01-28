using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ICvAnalysisRepository : IBaseEntityRepository<CvAnalysis>
{
    Task<List<CvAnalysis>> GetByUserIdAsync(long userId, int page = 1, int pageSize = 10);
    Task<int> GetCountByUserIdAsync(long userId);
    Task<List<CvAnalysis>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<int> GetAllCountAsync();
    Task<CvAnalysis?> GetWithMatchesAsync(string publicId);
    Task DeleteByCandidateIdAsync(long candidateId);
    Task<CvAnalysisMatch?> GetLastAnalysisMatchAsync(long candidateId, long jobPositionId);
}
