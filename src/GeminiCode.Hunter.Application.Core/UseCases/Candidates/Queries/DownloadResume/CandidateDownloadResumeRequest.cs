using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.DownloadResume;

public class CandidateDownloadResumeRequest : IRequest<CandidateDownloadResumeResponse>
{
    public string CandidateId { get; set; }

    public CandidateDownloadResumeRequest(string candidateId)
    {
        CandidateId = candidateId;
    }
}
