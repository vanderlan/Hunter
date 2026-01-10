namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;

public class HiringProcessGetPaginatedResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobCode { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}
