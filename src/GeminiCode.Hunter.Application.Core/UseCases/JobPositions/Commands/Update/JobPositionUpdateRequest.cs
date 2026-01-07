using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Update;

public class JobPositionUpdateRequest : IRequest
{
    public string PublicId { get; set; }
    public string Title { get; set; }
    public string ExternalUrl { get; set; }
    public string CompanyCode { get; set; }
    public string JobCode { get; set; }
    public string? Summary { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? MustHaveSkills { get; set; }
    public string? NiceToHaveSkills { get; set; }
    public bool IsActive { get; set; }
}
