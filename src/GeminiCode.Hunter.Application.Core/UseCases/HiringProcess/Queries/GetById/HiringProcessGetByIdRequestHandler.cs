using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetById;

public class HiringProcessGetByIdRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<HiringProcessGetByIdRequestHandler> logger)
    : IRequestHandler<HiringProcessGetByIdRequest, HiringProcessGetByIdResponse?>
{
    public async Task<HiringProcessGetByIdResponse?> Handle(HiringProcessGetByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting hiring process by ID: {PublicId}", request.PublicId);

        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(request.PublicId);
        
        if (hiringProcess == null)
        {
            logger.LogWarning("Hiring process not found: {PublicId}", request.PublicId);
            return null;
        }

        // Get status history
        var statusHistory = await unitOfWork.HiringProcessStatusHistoryRepository
            .GetByHiringProcessIdAsync(hiringProcess.HiringProcessId);

        // Get notes
        var notes = await unitOfWork.HiringProcessNoteRepository
            .GetByHiringProcessIdAsync(hiringProcess.HiringProcessId);

        // Check if candidate has resume
        var hasResume = false;
        if (hiringProcess.Candidate != null)
        {
            var candidateFile = await unitOfWork.CandidateFileRepository
                .GetByCandidateAndFileTypeAsync(hiringProcess.Candidate.CandidateId, "Resume");
            hasResume = candidateFile != null;
        }

        var response = new HiringProcessGetByIdResponse
        {
            PublicId = hiringProcess.PublicId,
            Status = hiringProcess.Status.ToString(),
            Candidate = new CandidateInfo
            {
                PublicId = hiringProcess.Candidate?.PublicId ?? string.Empty,
                Name = hiringProcess.Candidate?.Name,
                Email = hiringProcess.Candidate?.Email,
                PhoneNumber = hiringProcess.Candidate?.PhoneNumber,
                HasResume = hasResume
            },
            JobPosition = new JobPositionInfo
            {
                PublicId = hiringProcess.JobPosition?.PublicId ?? string.Empty,
                Title = hiringProcess.JobPosition?.Title ?? string.Empty,
                CompanyCode = hiringProcess.JobPosition?.CompanyCode ?? string.Empty,
                JobCode = hiringProcess.JobPosition?.JobCode ?? string.Empty
            },
            CreatedByUser = new UserInfo
            {
                Name = hiringProcess.CreatedByUser?.Name ?? string.Empty,
                Email = hiringProcess.CreatedByUser?.Email ?? string.Empty
            },
            LastUpdatedByUser = hiringProcess.LastUpdatedByUser != null ? new UserInfo
            {
                Name = hiringProcess.LastUpdatedByUser.Name,
                Email = hiringProcess.LastUpdatedByUser.Email
            } : null,
            ReferralReason = hiringProcess.ReferralReason,
            StatusHistory = statusHistory.Select(h => new StatusHistoryItem
            {
                FromStatus = h.FromStatus.ToString(),
                ToStatus = h.ToStatus.ToString(),
                ChangedByUserName = h.ChangedByUser?.Name ?? string.Empty,
                Notes = h.Notes,
                CreatedAt = h.CreatedAt
            }).ToList(),
            Notes = notes.Select(n => new NoteItem
            {
                PublicId = n.PublicId,
                Note = n.Note,
                UserName = n.User?.Name ?? string.Empty,
                CreatedAt = n.CreatedAt
            }).ToList(),
            CreatedAt = hiringProcess.CreatedAt,
            LastUpdated = hiringProcess.LastUpdated
        };

        logger.LogInformation("Hiring process retrieved successfully: {PublicId}", request.PublicId);
        return response;
    }
}
