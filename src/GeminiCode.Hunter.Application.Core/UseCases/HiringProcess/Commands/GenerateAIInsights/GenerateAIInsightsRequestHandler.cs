using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.GenerateAIInsights;

public class GenerateAIInsightsRequestHandler(
    IUnitOfWork unitOfWork,
    IAIService aiService,
    IPdfService pdfService,
    ILogger<GenerateAIInsightsRequestHandler> logger)
    : IRequestHandler<GenerateAIInsightsRequest, GenerateAIInsightsResponse>
{
    public async Task<GenerateAIInsightsResponse> Handle(
        GenerateAIInsightsRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating AI insights for hiring process: {PublicId}", request.PublicId);

        var hiringProcess = await unitOfWork.HiringProcessRepository.GetByIdAsync(request.PublicId);
        
        if (hiringProcess == null)
        {
            logger.LogWarning("Hiring process not found: {PublicId}", request.PublicId);
            throw new NotFoundException($"Hiring process with ID {request.PublicId} not found");
        }

        // Get candidate and job position
        if (hiringProcess.Candidate == null || hiringProcess.JobPosition == null)
        {
            throw new BusinessException("Candidate or job position information is missing");
        }

        // Get the candidate's CV from file
        var candidate = await unitOfWork.CandidateRepository.GetByIdAsync(hiringProcess.Candidate.PublicId);
        if (candidate == null)
        {
            throw new BusinessException("Candidate information is not available");
        }

        var candidateFile = await unitOfWork.CandidateFileRepository
            .GetByCandidateAndFileTypeAsync(candidate.CandidateId, "Resume");
            
        if (candidateFile == null || candidateFile.File == null || candidateFile.File.FileData == null)
        {
            throw new BusinessException("Candidate CV file is not available");
        }

        // Extract text from the stored PDF
        string cvText;
        using (var stream = new System.IO.MemoryStream(candidateFile.File.FileData))
        {
            cvText = await pdfService.ExtractTextFromPdfAsync(stream);
        }

        // Get the job position details
        var jobPosition = await unitOfWork.JobPositionRepository.GetByIdAsync(hiringProcess.JobPosition.PublicId);
        if (jobPosition == null)
        {
            throw new BusinessException("Job position information is not available");
        }

        logger.LogInformation("Generating AI insights for candidate {CandidateName} and job {JobTitle}", 
            candidate.Name, jobPosition.Title);

        // Generate referral reason using AI completion with a specific prompt
        var referralPrompt = $@"You are an expert HR recruiter. Based on the following information, write a professional referral reason (2-4 sentences) explaining WHY this candidate is a strong potential fit for the position.

CANDIDATE: {candidate.Name}
POSITION: {jobPosition.Title}
POSITION: {jobPosition.Title}

JOB REQUIREMENTS:
{jobPosition.Summary}
{jobPosition.MustHaveSkills}

CANDIDATE CV SUMMARY:
{(cvText.Length > 2000 ? cvText.Substring(0, 2000) + "..." : cvText)}

Write the referral reason in first person as if you're the recruiter referring them.
Example format: ""I consider [Name] a strong potential talent for [Company] because...""

Focus on:
- Specific strengths and relevant experience
- What makes them stand out for THIS specific role
- Be professional and concise";

        var referralReason = await aiService.GetCompletionAsync(referralPrompt, maxTokens: 300);

        if (string.IsNullOrWhiteSpace(referralReason))
        {
            throw new BusinessException("AI was unable to generate insights for this candidate and position combination");
        }

        // Update the hiring process with the AI-generated referral reason
        hiringProcess.ReferralReason = referralReason.Trim();
        unitOfWork.HiringProcessRepository.Update(hiringProcess);
        await unitOfWork.CommitAsync();

        logger.LogInformation("AI insights generated and saved for hiring process: {PublicId}", request.PublicId);

        return new GenerateAIInsightsResponse
        {
            ReferralReason = referralReason.Trim()
        };
    }
}
