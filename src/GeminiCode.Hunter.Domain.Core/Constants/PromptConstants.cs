namespace GeminiCode.Hunter.Domain.Core.Constants;

/// <summary>
/// Constants for Prompt keys used in the system
/// </summary>
public static class PromptConstants
{
    /// <summary>
    /// Key for the job enrichment prompt used to extract structured data from job postings
    /// </summary>
    public const string JobEnrichment = "JOB_ENRICHMENT";
    
    /// <summary>
    /// Key for the CV analysis prompt used to match candidates against job positions
    /// </summary>
    public const string CvAnalysis = "CV_ANALYSIS";
}
