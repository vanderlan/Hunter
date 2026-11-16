using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositions;

public class GetPublicJobPositionsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPublicJobPositionsRequest, GetPublicJobPositionsResponse>
{
    public async Task<GetPublicJobPositionsResponse> Handle(GetPublicJobPositionsRequest request, CancellationToken cancellationToken)
    {
        var positions = await unitOfWork.JobPositionRepository.GetPublicPositionsAsync();

        return new GetPublicJobPositionsResponse
        {
            Jobs = positions.Select(p => new PublicJobPositionDto
            {
                Code = p.PublicCode,
                Title = p.Title,
                Summary = p.PublicSummary,
                MustHaveSkills = p.PublicMustHaveSkills,
                NiceToHaveSkills = p.PublicNiceToHaveSkills,
                PublishedAt = p.CreatedAt.ToApplicationTime()
            }).ToList(),
            TotalCount = positions.Count
        };
    }
}
