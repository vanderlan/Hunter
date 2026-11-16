namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositions;

public class GetPublicJobPositionsResponse
{
    public List<PublicJobPositionDto> Jobs { get; set; } = [];
    public int TotalCount { get; set; }
}

/// <summary>
/// Public projection of a job position. Deliberately excludes internal ids,
/// the external URL, priority and country restrictions.
/// </summary>
public class PublicJobPositionDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public DateTime PublishedAt { get; set; }
}
