using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Extensions;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Queries.GetPaginated;

public class ReferralRequestGetPaginatedResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public string? CandidateName { get; set; }
    public string? CandidateEmail { get; set; }
    public string? CandidatePhoneNumber { get; set; }
    public string JobPositionId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal? MatchPercentage { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserName { get; set; }
    public string? ResolutionNote { get; set; }
    public string? HiringProcessId { get; set; }

    public static ReferralRequestGetPaginatedResponse From(ReferralRequest request)
    {
        return new ReferralRequestGetPaginatedResponse
        {
            PublicId = request.PublicId,
            Status = request.Status.ToString(),
            CandidateId = request.Candidate?.PublicId ?? string.Empty,
            CandidateName = request.Candidate?.Name,
            CandidateEmail = request.Candidate?.Email,
            CandidatePhoneNumber = request.Candidate?.PhoneNumber,
            JobPositionId = request.JobPosition?.PublicId ?? string.Empty,
            JobTitle = request.JobPosition?.Title ?? string.Empty,
            MatchPercentage = request.MatchPercentage,
            Message = request.Message,
            CreatedAt = request.CreatedAt.ToApplicationTime(),
            ResolvedAt = request.ResolvedAt?.ToApplicationTime(),
            ResolvedByUserName = request.ResolvedByUser?.Name,
            ResolutionNote = request.ResolutionNote,
            HiringProcessId = request.HiringProcess?.PublicId
        };
    }
}
