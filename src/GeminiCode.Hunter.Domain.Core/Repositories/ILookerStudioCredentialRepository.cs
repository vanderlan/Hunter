using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface ILookerStudioCredentialRepository : IBaseEntityRepository<LookerStudioCredential>
{
    Task<LookerStudioCredential?> GetActiveCredentialAsync();
    Task DeactivateAllCredentialsAsync();
}
