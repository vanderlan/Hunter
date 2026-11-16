using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Queries.GetPendingCount;

public class ReferralRequestGetPendingCountRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ReferralRequestGetPendingCountRequest, ReferralRequestGetPendingCountResponse>
{
    public async Task<ReferralRequestGetPendingCountResponse> Handle(ReferralRequestGetPendingCountRequest request, CancellationToken cancellationToken)
    {
        return new ReferralRequestGetPendingCountResponse
        {
            Count = await unitOfWork.ReferralRequestRepository.GetPendingCountAsync()
        };
    }
}
