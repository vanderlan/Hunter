namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositionByCode;

public class GetPublicJobPositionByCodeResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public DateTime PublishedAt { get; set; }
}
