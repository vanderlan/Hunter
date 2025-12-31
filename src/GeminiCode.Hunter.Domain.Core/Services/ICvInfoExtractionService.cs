using System.Text.RegularExpressions;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface ICvInfoExtractionService
{
    (string? name, string? email, string? phone) ExtractBasicInfo(string cvText);
}
