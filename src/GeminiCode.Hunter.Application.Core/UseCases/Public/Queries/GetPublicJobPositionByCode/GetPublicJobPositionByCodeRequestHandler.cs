using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositionByCode;

public class GetPublicJobPositionByCodeRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPublicJobPositionByCodeRequest, GetPublicJobPositionByCodeResponse?>
{
    public async Task<GetPublicJobPositionByCodeResponse?> Handle(GetPublicJobPositionByCodeRequest request, CancellationToken cancellationToken)
    {
        var position = await unitOfWork.JobPositionRepository.GetByPublicCodeAsync(request.Code);

        if (position is null || !position.IsPubliclyVisible)
            return null;

        return new GetPublicJobPositionByCodeResponse
        {
            Code = position.PublicCode,
            Title = position.Title,
            Summary = position.PublicSummary,
            Responsibilities = position.PublicResponsibilities,
            Requirements = position.PublicRequirements,
            MustHaveSkills = position.PublicMustHaveSkills,
            NiceToHaveSkills = position.PublicNiceToHaveSkills,
            PublishedAt = position.CreatedAt.ToApplicationTime()
        };
    }
}
