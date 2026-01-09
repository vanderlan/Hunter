using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.DownloadResume;

public class CandidateDownloadResumeRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<CandidateDownloadResumeRequestHandler> logger)
    : IRequestHandler<CandidateDownloadResumeRequest, CandidateDownloadResumeResponse>
{
    public async Task<CandidateDownloadResumeResponse> Handle(CandidateDownloadResumeRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Downloading resume for candidate {CandidateId}", request.CandidateId);

        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.CandidateId);
        
        if (candidate == null)
        {
            throw new NotFoundException($"Candidate with ID {request.CandidateId} not found");
        }

        var candidateFile = await unitOfWork.CandidateFileRepository
            .GetByCandidateAndFileTypeAsync(candidate.CandidateId, "Resume");

        if (candidateFile == null || candidateFile.File == null)
        {
            throw new NotFoundException($"Resume not found for candidate {request.CandidateId}");
        }

        var file = candidateFile.File;

        if (file.FileData == null || file.FileData.Length == 0)
        {
            throw new BusinessException("Resume file data is empty or corrupted");
        }

        logger.LogInformation("Resume downloaded successfully for candidate {CandidateId}", candidate.CandidateId);

        return new CandidateDownloadResumeResponse
        {
            FileData = file.FileData,
            FileName = file.FileName,
            ContentType = file.ContentType
        };
    }
}
