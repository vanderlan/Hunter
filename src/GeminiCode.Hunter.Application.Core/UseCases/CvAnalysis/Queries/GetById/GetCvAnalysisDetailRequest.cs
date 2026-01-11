using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetById;

public record GetCvAnalysisDetailRequest(string AnalysisId) : IRequest<CvAnalysisDetailResponse>;
