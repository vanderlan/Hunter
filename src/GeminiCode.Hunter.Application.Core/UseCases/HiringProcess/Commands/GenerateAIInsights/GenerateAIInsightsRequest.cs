using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.GenerateAIInsights;

public class GenerateAIInsightsRequest : IRequest<GenerateAIInsightsResponse>
{
    public string PublicId { get; set; } = string.Empty;
}
