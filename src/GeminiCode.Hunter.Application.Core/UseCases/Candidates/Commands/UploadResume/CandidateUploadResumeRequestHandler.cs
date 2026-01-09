using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileEntity = GeminiCode.Hunter.Domain.Core.Entities.File;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.UploadResume;

public class CandidateUploadResumeRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<CandidateUploadResumeRequestHandler> logger)
    : IRequestHandler<CandidateUploadResumeRequest, CandidateUploadResumeResponse>
{
    public async Task<CandidateUploadResumeResponse> Handle(CandidateUploadResumeRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Uploading resume for candidate {CandidateId}", request.CandidateId);

        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.CandidateId);
        
        if (candidate == null)
        {
            throw new FileNotFoundException($"Candidate with ID {request.CandidateId} not found");
        }

        // Delete existing resume if any
        var existingCandidateFile = await unitOfWork.CandidateFileRepository
            .GetByCandidateAndFileTypeAsync(candidate.CandidateId, "Resume");

        if (existingCandidateFile != null)
        {
            logger.LogInformation("Deleting existing resume for candidate {CandidateId}", candidate.CandidateId);
            await unitOfWork.CandidateFileRepository.DeleteAsync(existingCandidateFile.PublicId);
            await unitOfWork.FileRepository.DeleteAsync(existingCandidateFile.File.PublicId);
        }

        // Read file data
        byte[] fileData;
        using (var memoryStream = new MemoryStream())
        {
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            fileData = memoryStream.ToArray();
        }

        // Create File entity
        var file = new FileEntity
        {
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            StorageType = StorageType.Database,
            FileData = fileData
        };

        await unitOfWork.FileRepository.AddAsync(file);

        // Create CandidateFile relationship
        var candidateFile = new CandidateFile
        {
            CandidateId = candidate.CandidateId,
            File = file,
            FileType = "Resume"
        };

        await unitOfWork.CandidateFileRepository.AddAsync(candidateFile);

        await unitOfWork.CommitAsync();

        logger.LogInformation("Resume uploaded successfully for candidate {CandidateId}, File ID: {FileId}", 
            candidate.CandidateId, file.PublicId);

        return new CandidateUploadResumeResponse
        {
            FileId = file.PublicId,
            FileName = file.FileName,
            FileSize = file.FileSize
        };
    }
}
