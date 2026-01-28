namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetById;

public class HiringProcessGetByIdResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public CandidateInfo Candidate { get; set; } = new();
    public JobPositionInfo JobPosition { get; set; } = new();
    public UserInfo CreatedByUser { get; set; } = new();
    public UserInfo? LastUpdatedByUser { get; set; }
    public List<StatusHistoryItem> StatusHistory { get; set; } = [];
    public List<NoteItem> Notes { get; set; } = [];
    public string? ReferralReason { get; set; }
    public LastAIAnalysisInfo? LastAIAnalysis { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CandidateInfo
{
    public string PublicId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool HasResume { get; set; }
}

public class JobPositionInfo
{
    public string PublicId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string JobCode { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class StatusHistoryItem
{
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NoteItem
{
    public string PublicId { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LastAIAnalysisInfo
{
    public string CvAnalysisPublicId { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public decimal MatchPercentage { get; set; }
}
