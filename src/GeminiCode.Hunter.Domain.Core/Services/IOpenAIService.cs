using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IOpenAIService
{
    Task<CvAnalysisResult> AnalyzeCvAgainstJobsAsync(string cvText, List<JobPositionSummary> jobPositions);
}