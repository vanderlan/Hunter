using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface ICvInfoExtractionService
{
    (string? name, string? email, string? phone) ExtractBasicInfo(string cvText);
    Task<string?> ExtractNameWithAIAsync(string cvText);
}
