using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Extensions;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetById;

public class CandidateGetByIdResponse
{
    public string PublicId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public string AnalyzedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }

    public static implicit operator CandidateGetByIdResponse(Candidate candidate)
    {
        if (candidate is null)
            return default;

        return new CandidateGetByIdResponse
        {
            PublicId = candidate.PublicId,
            Name = candidate.Name,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            AnalyzedAt = candidate.AnalyzedAt.ToApplicationTime(), // Convert to configured timezone
            AnalyzedByUserName = candidate.User?.Name ?? "Unknown",
            CreatedAt = candidate.CreatedAt.ToApplicationTime() // Convert to configured timezone
        };
    }
}
