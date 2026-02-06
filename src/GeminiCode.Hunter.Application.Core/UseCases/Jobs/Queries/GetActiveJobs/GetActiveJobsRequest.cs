using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;

public record GetActiveJobsRequest : IRequest<GetActiveJobsResponse>;
