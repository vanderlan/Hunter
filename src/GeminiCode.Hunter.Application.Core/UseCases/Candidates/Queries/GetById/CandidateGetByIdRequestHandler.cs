using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetById;

public class CandidateGetByIdRequestHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<CandidateGetByIdRequest, CandidateGetByIdResponse>
{
    public async Task<CandidateGetByIdResponse> Handle(
        CandidateGetByIdRequest request, 
        CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.PublicId);

        if (candidate == null)
            throw new NotFoundException(request.PublicId);

        return (CandidateGetByIdResponse)candidate;
    }
}
