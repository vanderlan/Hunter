using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository;

public class LookerStudioCredentialRepository(DataContext dataContext) 
    : BaseEntityRepository<LookerStudioCredential>(dataContext), ILookerStudioCredentialRepository
{
    public async Task<LookerStudioCredential?> GetActiveCredentialAsync()
    {
        return await DataContext.LookerStudioCredentials
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task DeactivateAllCredentialsAsync()
    {
        var activeCredentials = await DataContext.LookerStudioCredentials
            .Where(x => x.IsActive)
            .ToListAsync();

        foreach (var credential in activeCredentials)
        {
            credential.IsActive = false;
            DataContext.LookerStudioCredentials.Update(credential);
        }
    }
}
