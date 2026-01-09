namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.DownloadResume;

public class CandidateDownloadResumeResponse
{
    public byte[] FileData { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
}
