using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.Services;

public class ParameterService : IParameterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ParameterService> _logger;

    public ParameterService(IUnitOfWork unitOfWork, ILogger<ParameterService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GetValueAsync(string key, string defaultValue = "")
    {
        try
        {
            var parameter = await _unitOfWork.ParameterRepository.GetByKeyAsync(key);
            
            if (parameter == null)
            {
                _logger.LogWarning("Parameter not found: {Key}, returning default value: {DefaultValue}", key, defaultValue);
                return defaultValue;
            }

            return parameter.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parameter: {Key}, returning default value: {DefaultValue}", key, defaultValue);
            return defaultValue;
        }
    }

    public async Task<int> GetValueAsIntAsync(string key, int defaultValue = 0)
    {
        try
        {
            var value = await GetValueAsync(key);
            
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (int.TryParse(value, out var intValue))
                return intValue;

            _logger.LogWarning("Parameter value cannot be parsed as int: {Key}={Value}, returning default value: {DefaultValue}", 
                key, value, defaultValue);
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parameter as int: {Key}, returning default value: {DefaultValue}", key, defaultValue);
            return defaultValue;
        }
    }
}
