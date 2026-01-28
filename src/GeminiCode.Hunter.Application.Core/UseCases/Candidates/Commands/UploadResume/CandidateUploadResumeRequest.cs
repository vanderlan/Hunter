using MediatR;
using Microsoft.AspNetCore.Http;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.UploadResume;

public class CandidateUploadResumeRequest : IRequest<CandidateUploadResumeResponse>
{
    public string CandidateId { get; set; }
    public IFormFile File { get; set; }
}
