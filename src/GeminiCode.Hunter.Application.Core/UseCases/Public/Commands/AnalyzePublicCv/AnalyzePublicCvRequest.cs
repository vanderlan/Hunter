using MediatR;
using Microsoft.AspNetCore.Http;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.AnalyzePublicCv;

public class AnalyzePublicCvRequest : IRequest<AnalyzePublicCvResponse>
{
    public required IFormFile CvFile { get; set; }
}
