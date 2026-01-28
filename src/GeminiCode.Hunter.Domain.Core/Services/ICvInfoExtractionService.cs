using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface ICvInfoExtractionService
{
    Task<(string? name, string? email, string? phone)> ExtractBasicInfoWithAIAsync(string cvText);
}
