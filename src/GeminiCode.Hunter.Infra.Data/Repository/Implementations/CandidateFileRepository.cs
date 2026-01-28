using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository.Implementations;

public sealed class CandidateFileRepository(DataContext context) : BaseEntityRepository<CandidateFile>(context), ICandidateFileRepository
{
    public async Task<List<CandidateFile>> GetByCandidateIdAsync(long candidateId)
    {
        return await DataContext.CandidateFiles
            .Include(cf => cf.File)
            .Include(cf => cf.Candidate)
            .Where(cf => cf.CandidateId == candidateId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CandidateFile> GetByCandidateAndFileTypeAsync(long candidateId, string fileType)
    {
        return await DataContext.CandidateFiles
            .Include(cf => cf.File)
            .Include(cf => cf.Candidate)
            .AsNoTracking()
            .FirstOrDefaultAsync(cf => cf.CandidateId == candidateId && cf.FileType == fileType);
    }
}
