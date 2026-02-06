using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.Services;

public class PromptService : IPromptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PromptService> _logger;

    public PromptService(IUnitOfWork unitOfWork, ILogger<PromptService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GetPromptTextAsync(string key)
    {
        try
        {
            var prompt = await _unitOfWork.PromptRepository.GetByKeyAsync(key);
            
            if (prompt == null)
            {
                _logger.LogWarning("Prompt not found: {Key}, returning empty string", key);
                return string.Empty;
            }

            return prompt.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt: {Key}, returning empty string", key);
            return string.Empty;
        }
    }
}
