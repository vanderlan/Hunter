using MediatR;
using Microsoft.AspNetCore.Http;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvRequest : IRequest<AnalyzeCvResponse>
{
    public required IFormFile CvFile { get; set; }
}
