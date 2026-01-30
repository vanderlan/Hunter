using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

/// <summary>
/// Service for managing AI prompts
/// </summary>
public interface IPromptService
{
    /// <summary>
    /// Gets a prompt text by its key
    /// </summary>
    /// <param name="key">The prompt key</param>
    /// <returns>The prompt text or empty string if not found</returns>
    Task<string> GetPromptTextAsync(string key);
}
