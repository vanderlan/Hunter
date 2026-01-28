using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Delete;

public class CandidateDeleteRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<CandidateDeleteRequestHandler> logger)
    : IRequestHandler<CandidateDeleteRequest>
{
    public async Task Handle(CandidateDeleteRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting candidate deletion. PublicId: {PublicId}, User: {UserId}", 
            request.PublicId, currentUser.Id);

        // Get the candidate
        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(request.PublicId);

        if (candidate == null)
        {
            logger.LogWarning("Candidate not found with PublicId: {PublicId}", request.PublicId);
            throw new NotFoundException($"Candidate with PublicId {request.PublicId} not found");
        }

        // Get current user to check profile
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);

        // Check if candidate has any hiring processes
        var hiringProcesses = await unitOfWork.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId);

        // Permission check: Partner can delete ONLY if no hiring process exists
        // Admin can delete always
        if (user.Profile != UserProfile.Admin && hiringProcesses.Any())
        {
            logger.LogWarning("Partner user {UserId} attempted to delete candidate {CandidateId} with existing hiring processes", 
                currentUser.Id, request.PublicId);
            throw new UnauthorizedAccessException(
                "Only Admin users can delete candidates with existing hiring processes");
        }

        logger.LogInformation("User {UserId} has permission to delete candidate {CandidateId}", 
            currentUser.Id, request.PublicId);

        // Delete related data in correct order (respecting foreign key constraints)
        
        // 1. Delete Hiring Process related data FIRST (must be done before candidate deletion)
        if (hiringProcesses.Any())
        {
            logger.LogInformation("Deleting {Count} hiring processes for candidate {CandidateId}", 
                hiringProcesses.Count, candidate.CandidateId);

            foreach (var hiringProcess in hiringProcesses)
            {
                // Delete hiring process notes first
                var notes = await unitOfWork.HiringProcessNoteRepository.GetByHiringProcessIdAsync(hiringProcess.HiringProcessId);
                foreach (var note in notes)
                {
                    await unitOfWork.HiringProcessNoteRepository.DeleteAsync(note.PublicId);
                }

                // Delete hiring process (will cascade delete status history)
                await unitOfWork.HiringProcessRepository.DeleteAsync(hiringProcess.PublicId);
            }

            await unitOfWork.CommitAsync();
            logger.LogInformation("Hiring processes deleted successfully for candidate {CandidateId}", candidate.CandidateId);
        }

        // 2. Delete CV Analysis data (CvAnalysisMatch has cascade delete from CvAnalysis)
        logger.LogInformation("Deleting CV analysis records for candidate {CandidateId}", candidate.CandidateId);
        await unitOfWork.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId);

        // 3. Delete Candidate Files and related File entities
        var candidateFiles = await unitOfWork.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId);
        if (candidateFiles.Any())
        {
            logger.LogInformation("Deleting {Count} files for candidate {CandidateId}", 
                candidateFiles.Count, candidate.CandidateId);

            foreach (var candidateFile in candidateFiles)
            {
                await unitOfWork.CandidateFileRepository.DeleteAsync(candidateFile.PublicId);
                await unitOfWork.FileRepository.DeleteAsync(candidateFile.File.PublicId);
            }

            await unitOfWork.CommitAsync();
            logger.LogInformation("Files deleted successfully for candidate {CandidateId}", candidate.CandidateId);
        }

        // 4. Finally, delete the candidate
        logger.LogInformation("Deleting candidate entity {CandidateId}", candidate.CandidateId);
        await unitOfWork.CandidateRepository.DeleteAsync(request.PublicId);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Candidate {CandidateId} and all related data deleted successfully", candidate.CandidateId);
    }
}
