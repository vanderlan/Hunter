using GeminiCode.Hunter.Application.Core.Helpers;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;

public class CandidateUpdateRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<CandidateUpdateRequestHandler> logger) 
    : IRequestHandler<CandidateUpdateRequest>
{
    public async Task Handle(CandidateUpdateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating candidate with PublicId: {PublicId}", request.PublicId);

        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.PublicId);
        
        if (candidate == null)
        {
            logger.LogWarning("Candidate not found with PublicId: {PublicId}", request.PublicId);
            throw new NotFoundException(request.PublicId);
        }

        candidate.Name = request.Name.FormatName();
        candidate.Email = request.Email?.Trim();
        candidate.PhoneNumber = request.PhoneNumber?.Trim();

        unitOfWork.CandidateRepository.Update(candidate);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Candidate updated successfully: {PublicId}, Name: {Name}", 
            request.PublicId, candidate.Name);
    }
}
