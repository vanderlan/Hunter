using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.DeleteStalePublicCandidates;

/// <summary>
/// Privacy housekeeping for the public CV match: CVs that were never saved are
/// removed after 24 hours, saved-but-unconfirmed profiles after 7 days.
/// </summary>
public class DeleteStalePublicCandidatesRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteStalePublicCandidatesRequestHandler> logger)
    : IRequestHandler<DeleteStalePublicCandidatesRequest, DeleteStalePublicCandidatesResponse>
{
    public static readonly TimeSpan UnsavedRetention = TimeSpan.FromHours(24);
    public static readonly TimeSpan UnconfirmedRetention = TimeSpan.FromDays(7);

    public async Task<DeleteStalePublicCandidatesResponse> Handle(DeleteStalePublicCandidatesRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        var stale = await unitOfWork.CandidateRepository.GetStalePublicCandidatesAsync(
            now - UnsavedRetention,
            now - UnconfirmedRetention);

        var deleted = 0;

        foreach (var candidate in stale)
        {
            try
            {
                await unitOfWork.ReferralRequestRepository.DeleteByCandidateIdAsync(candidate.CandidateId);
                await unitOfWork.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId);
                await unitOfWork.CommitAsync();

                var files = await unitOfWork.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId);
                foreach (var candidateFile in files)
                {
                    await unitOfWork.CandidateFileRepository.DeleteAsync(candidateFile.PublicId);
                    await unitOfWork.FileRepository.DeleteAsync(candidateFile.File.PublicId);
                }
                await unitOfWork.CommitAsync();

                await unitOfWork.CandidateRepository.DeleteAsync(candidate.PublicId);
                await unitOfWork.CommitAsync();

                deleted++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete stale public candidate {CandidateId}", candidate.CandidateId);
                unitOfWork.DiscardChanges();
            }
        }

        if (deleted > 0)
        {
            logger.LogInformation("Deleted {Count} stale public candidates", deleted);
        }

        return new DeleteStalePublicCandidatesResponse { DeletedCandidates = deleted };
    }
}
