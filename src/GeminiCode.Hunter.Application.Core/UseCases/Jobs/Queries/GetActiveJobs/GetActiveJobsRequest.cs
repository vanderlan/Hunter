using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;

public class GetActiveJobsRequest : IRequest<GetActiveJobsResponse>
{
    public string? CompanyCode { get; set; }
}
