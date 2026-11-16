using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IAIService
{
    Task<CvAnalysisResult> AnalyzeCvAgainstJobsAsync(string cvText, List<JobPositionSummary> jobPositions);
    Task<string> GetCompletionAsync(string prompt, int maxTokens = 100);
    Task<JobEnrichmentResult> EnrichJobDetailsAsync(string jobTitle, string rawHtmlContent);

    /// <summary>
    /// Rewrites an enriched job description removing anything that identifies the
    /// hiring company (names, brands, products, office addresses, internal codes).
    /// </summary>
    Task<PublicJobContentResult> GeneratePublicJobContentAsync(string jobTitle, JobPositionSummary internalContent);
}
