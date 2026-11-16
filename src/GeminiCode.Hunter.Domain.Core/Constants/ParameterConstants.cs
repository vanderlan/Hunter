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

    /// <summary>
    /// Maximum number of anonymous CV analyses accepted per day across all visitors (cost guard)
    /// </summary>
    public const string PublicCvMatchDailyLimit = "PUBLIC_CV_MATCH_DAILY_LIMIT";

    /// <summary>
    /// Match percentage above which a public candidate may ask for a referral (e.g., "50")
    /// </summary>
    public const string MinimumMatchPercentageReferralRequest = "MINIMUM_MATCH_PERCENTAGE_REFERRAL_REQUEST";
}
