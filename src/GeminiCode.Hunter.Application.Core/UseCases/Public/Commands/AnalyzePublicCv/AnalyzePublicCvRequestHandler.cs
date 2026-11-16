using GeminiCode.Hunter.Application.Core.Helpers;
using GeminiCode.Hunter.Domain.Core.Constants;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Helpers;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CvAnalysisEntity = GeminiCode.Hunter.Domain.Core.Entities.CvAnalysis;
using FileEntity = GeminiCode.Hunter.Domain.Core.Entities.File;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.AnalyzePublicCv;

/// <summary>
/// Anonymous CV match. Stores the CV under a public candidate record that is
/// invisible to internal users until the visitor saves their profile and confirms
/// their email. Unsaved records are purged by <c>DeleteStalePublicCandidates</c>.
/// </summary>
public class AnalyzePublicCvRequestHandler(
    IUnitOfWork unitOfWork,
    IPdfService pdfService,
    IAIService aiService,
    ICvInfoExtractionService cvInfoExtractionService,
    IParameterService parameterService,
    ILogger<AnalyzePublicCvRequestHandler> logger)
    : IRequestHandler<AnalyzePublicCvRequest, AnalyzePublicCvResponse>
{
    public const int DefaultDailyLimit = 200;
    public const int DefaultMinimumMatchForReferral = 50;

    public async Task<AnalyzePublicCvResponse> Handle(AnalyzePublicCvRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting public CV analysis for file: {FileName}", request.CvFile.FileName);

        EnsureFileIsAcceptable(request.CvFile);
        await EnsureDailyLimitNotReachedAsync();

        var jobPositions = await unitOfWork.JobPositionRepository.GetPublicPositionsAsync();
        var minimumMatchForReferral = await parameterService.GetValueAsIntAsync(
            ParameterConstants.MinimumMatchPercentageReferralRequest, DefaultMinimumMatchForReferral);

        if (jobPositions.Count == 0)
        {
            logger.LogWarning("No publicly visible job positions available for public CV analysis");
            return new AnalyzePublicCvResponse
            {
                TotalPositionsAnalyzed = 0,
                MatchesFound = 0,
                MinimumMatchForReferral = minimumMatchForReferral
            };
        }

        string cvText;
        using (var stream = request.CvFile.OpenReadStream())
        {
            cvText = await pdfService.ExtractTextFromPdfAsync(stream);
        }

        if (string.IsNullOrWhiteSpace(cvText))
        {
            throw new BusinessException("We could not read any text from this PDF. Please upload a text-based CV, not a scanned image.");
        }

        var (name, email, phone) = await TryExtractBasicInfoAsync(cvText);

        // Match against the company-neutral content so the AI never sees, and never echoes, the company name.
        var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
            j.Title,
            j.PublicSummary,
            j.PublicResponsibilities,
            j.PublicRequirements,
            j.PublicMustHaveSkills,
            j.PublicNiceToHaveSkills
        )).ToList();

        var analysisResult = await aiService.AnalyzeCvAgainstJobsAsync(cvText, jobSummaries);

        var matches = analysisResult.Matches
            .Select(m => (Match: m, Job: jobPositions.FirstOrDefault(j => string.Equals(j.Title, m.JobTitle, StringComparison.OrdinalIgnoreCase))))
            .Where(x => x.Job is not null)
            .OrderByDescending(x => x.Match.MatchPercentage)
            .ToList();

        var candidate = await StoreCandidateAsync(name, email, phone);
        await StoreCvPdfAsync(request.CvFile, candidate.CandidateId);
        await SaveAnalysisHistoryAsync(candidate.CandidateId, jobPositions.Count, matches);

        logger.LogInformation("Public CV analysis completed: {MatchCount} matches for candidate token {Token}",
            matches.Count, candidate.PublicToken);

        return new AnalyzePublicCvResponse
        {
            CandidateToken = candidate.PublicToken,
            TotalPositionsAnalyzed = jobPositions.Count,
            MatchesFound = matches.Count,
            ExtractedName = candidate.Name,
            ExtractedEmail = candidate.Email,
            ExtractedPhone = candidate.PhoneNumber,
            MinimumMatchForReferral = minimumMatchForReferral,
            Matches = matches.Select(x => new PublicJobMatchDto
            {
                JobCode = x.Job!.PublicCode,
                JobTitle = x.Job.Title,
                MatchPercentage = x.Match.MatchPercentage,
                MatchReason = x.Match.MatchReason,
                MatchingSkills = x.Match.MatchingSkills,
                MissingSkills = x.Match.MissingSkills,
                WhatIsMissingForPerfectMatch = x.Match.WhatIsMissingForPerfectMatch,
                CanRequestReferral = x.Match.MatchPercentage >= minimumMatchForReferral
            }).ToList()
        };
    }

    private static void EnsureFileIsAcceptable(IFormFile file)
    {
        const long maxBytes = 10 * 1024 * 1024;

        if (file.Length == 0)
            throw new BusinessException("The uploaded file is empty.");

        if (file.Length > maxBytes)
            throw new BusinessException("The file is larger than 10MB.");

        var isPdf = string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                    || file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        if (!isPdf)
            throw new BusinessException("Only PDF files are supported.");
    }

    private async Task EnsureDailyLimitNotReachedAsync()
    {
        var dailyLimit = await parameterService.GetValueAsIntAsync(ParameterConstants.PublicCvMatchDailyLimit, DefaultDailyLimit);
        var startOfDayUtc = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);
        var analysesToday = await unitOfWork.CvAnalysisRepository.CountPublicAnalysesSinceAsync(startOfDayUtc);

        if (analysesToday >= dailyLimit)
        {
            logger.LogWarning("Public CV analysis daily limit reached ({Count}/{Limit})", analysesToday, dailyLimit);
            throw new BusinessException("We reached today's limit of free CV analyses. Please come back tomorrow.")
            {
                Title = "Daily limit reached"
            };
        }
    }

    private async Task<(string? name, string? email, string? phone)> TryExtractBasicInfoAsync(string cvText)
    {
        try
        {
            return await cvInfoExtractionService.ExtractBasicInfoWithAIAsync(cvText);
        }
        catch (Exception ex)
        {
            // Pre-filling the form is a convenience; never fail the analysis because of it.
            logger.LogWarning(ex, "Could not extract basic info from public CV");
            return (null, null, null);
        }
    }

    private async Task<Candidate> StoreCandidateAsync(string? name, string? email, string? phone)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var candidate = new Candidate
        {
            UserId = null,
            Source = CandidateSource.Public,
            PublicToken = SecureTokenGenerator.NewToken(),
            Name = name.FormatName(),
            Email = email?.Trim(),
            PhoneNumber = phone?.Trim(),
            AnalyzedAt = now,
            HasAcceptedTerms = false,
            IsEmailConfirmed = false
        };

        await unitOfWork.CandidateRepository.AddAsync(candidate);
        await unitOfWork.CommitAsync();

        return candidate;
    }

    private async Task StoreCvPdfAsync(IFormFile cvFile, long candidateId)
    {
        byte[] fileData;
        using (var memoryStream = new MemoryStream())
        {
            await cvFile.CopyToAsync(memoryStream);
            fileData = memoryStream.ToArray();
        }

        var file = new FileEntity
        {
            FileName = cvFile.FileName,
            ContentType = cvFile.ContentType,
            FileSize = cvFile.Length,
            StorageType = StorageType.Database,
            FileData = fileData
        };

        await unitOfWork.FileRepository.AddAsync(file);

        await unitOfWork.CandidateFileRepository.AddAsync(new CandidateFile
        {
            CandidateId = candidateId,
            File = file,
            FileType = "Resume"
        });

        await unitOfWork.CommitAsync();
    }

    private async Task SaveAnalysisHistoryAsync(long candidateId, int totalPositions, List<(JobMatchResult Match, JobPosition? Job)> matches)
    {
        var cvAnalysis = new CvAnalysisEntity
        {
            CandidateId = candidateId,
            UserId = null,
            AnalyzedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            TotalPositionsAnalyzed = totalPositions,
            MatchesFound = matches.Count
        };

        await unitOfWork.CvAnalysisRepository.AddAsync(cvAnalysis);
        await unitOfWork.CommitAsync();

        foreach (var (match, job) in matches)
        {
            cvAnalysis.Matches.Add(new CvAnalysisMatch
            {
                CvAnalysisId = cvAnalysis.CvAnalysisId,
                JobPositionId = job!.JobPositionId,
                MatchPercentage = match.MatchPercentage,
                MatchReason = match.MatchReason,
                MatchingSkills = System.Text.Json.JsonSerializer.Serialize(match.MatchingSkills),
                MissingSkills = System.Text.Json.JsonSerializer.Serialize(match.MissingSkills),
                JobTitle = job.Title,
                JobSummary = job.PublicSummary,
                MustHaveSkills = job.PublicMustHaveSkills,
                NiceToHaveSkills = job.PublicNiceToHaveSkills,
                WhatIsMissingForPerfectMatch = match.WhatIsMissingForPerfectMatch
            });
        }

        await unitOfWork.CommitAsync();
    }
}
