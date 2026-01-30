namespace GeminiCode.Hunter.Domain.Core.Constants;

/// <summary>
/// Constants for Parameter keys used in the system
/// </summary>
public static class ParameterConstants
{
    /// <summary>
    /// Key for the active AI service provider (e.g., "OpenAI", "DeepSeek")
    /// </summary>
    public const string ActiveAIService = "ACTIVE_AI_SERVICE";
    
    /// <summary>
    /// Key for minimum match percentage in CV analysis (e.g., "70")
    /// </summary>
    public const string MinimumMatchPercentageCvAnalysis = "MINIMUM_MATCH_PERCENTAGE_CV_ANALYSIS";
    
    /// <summary>
    /// Key for minimum match percentage in automatic candidate matching (e.g., "70")
    /// </summary>
    public const string MinimumMatchPercentageAutoMatching = "MINIMUM_MATCH_PERCENTAGE_AUTO_MATCHING";
}
