using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ICvAnalysisRepository : IBaseEntityRepository<CvAnalysis>
{
    Task<List<CvAnalysis>> GetByUserIdAsync(long userId, int page = 1, int pageSize = 10);
    Task<int> GetCountByUserIdAsync(long userId);
    Task<List<CvAnalysis>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<int> GetAllCountAsync();
    Task<PagedList<CvAnalysis>> GetByUserIdWithFilterAsync(long userId, CvAnalysisFilter filter);
    Task<PagedList<CvAnalysis>> GetAllWithFilterAsync(CvAnalysisFilter filter);
    Task<CvAnalysis?> GetWithMatchesAsync(string publicId);
    Task DeleteByCandidateIdAndUserAsync(long candidateId, long userId);
    Task<CvAnalysisMatch?> GetLastAnalysisMatchAsync(long candidateId, long jobPositionId);
    Task DeleteMatchesByJobPositionIdAsync(long jobPositionId);
    Task DeleteByCandidateIdAsync(long candidateId);
}
