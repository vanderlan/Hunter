using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IArtificialnteligenceService
{
    Task<CvAnalysisResult> AnalyzeCvAgainstJobsAsync(string cvText, List<JobPositionSummary> jobPositions);
    Task<string> GetCompletionAsync(string prompt, int maxTokens = 100);
    Task<JobEnrichmentResult> EnrichJobDetailsAsync(string jobTitle, string rawHtmlContent);
}