using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetHistory;

public class GetCvAnalysisHistoryRequest : CvAnalysisFilter, IRequest<PagedList<CvAnalysisHistoryResponse>>
{
}
