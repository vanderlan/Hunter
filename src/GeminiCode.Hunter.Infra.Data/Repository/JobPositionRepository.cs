using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class JobPositionRepository(DataContext dataContext) : BaseEntityRepository<JobPosition>(dataContext), IJobPositionRepository
{
    public async Task<JobPosition?> GetByExternalUrlAsync(string externalUrl)
    {
        return await DataContext.JobPositions
            .FirstOrDefaultAsync(x => x.ExternalUrl == externalUrl);
    }

    public async Task<JobPosition?> GetByJobCodeAsync(string jobCode)
    {
        return await DataContext.JobPositions
            .FirstOrDefaultAsync(x => x.JobCode == jobCode);
    }

    public async Task<List<JobPosition>> GetActivePositionsAsync()
    {
        return await DataContext.JobPositions
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<JobPosition>> GetByCompanyCodeAsync(string companyCode)
    {
        return await DataContext.JobPositions
            .Where(x => x.CompanyCode == companyCode && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsInactiveAsync(long jobPositionId)
    {
        var position = await DataContext.JobPositions.FindAsync(jobPositionId);
        if (position != null)
        {
            position.IsActive = false;
            position.ClosedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        }
    }

    public async Task MarkAsClosedAsync(long jobPositionId)
    {
        await MarkAsInactiveAsync(jobPositionId);
    }
}
