using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface ILookerStudioService
{
    Task<string> DownloadPriorityCsvAsync(CancellationToken cancellationToken);
    Task<bool> ValidateCredentialsAsync();
}
