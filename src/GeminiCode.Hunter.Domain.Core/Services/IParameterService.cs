using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

/// <summary>
/// Service for managing system parameters
/// </summary>
public interface IParameterService
{
    /// <summary>
    /// Gets a parameter value by its key
    /// </summary>
    /// <param name="key">The parameter key</param>
    /// <param name="defaultValue">Default value if parameter not found</param>
    /// <returns>The parameter value or default value</returns>
    Task<string> GetValueAsync(string key, string defaultValue = "");
    
    /// <summary>
    /// Gets a parameter value as an integer
    /// </summary>
    /// <param name="key">The parameter key</param>
    /// <param name="defaultValue">Default value if parameter not found or cannot be parsed</param>
    /// <returns>The parameter value as integer or default value</returns>
    Task<int> GetValueAsIntAsync(string key, int defaultValue = 0);
}
