using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Application.Core.Helpers;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using FileEntity = GeminiCode.Hunter.Domain.Core.Entities.File;
using CvAnalysisEntity = GeminiCode.Hunter.Domain.Core.Entities.CvAnalysis;
using CvAnalysisMatchEntity = GeminiCode.Hunter.Domain.Core.Entities.CvAnalysisMatch;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvRequestHandler(
    IUnitOfWork unitOfWork,
    IPdfService pdfService,
    IAIService openAIService,
    ICvInfoExtractionService cvInfoExtractionService,
    ICurrentUser currentUser,
    ILogger<AnalyzeCvRequestHandler> logger)
    : IRequestHandler<AnalyzeCvRequest, AnalyzeCvResponse>
{
    public async Task<AnalyzeCvResponse> Handle(AnalyzeCvRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting CV analysis for file: {FileName} by user: {UserId}", 
            request.CvFile.FileName, currentUser.Id);

        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
                ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

            if (request.CvFile.ContentType != "application/pdf")
            {
                throw new InvalidOperationException("Only PDF files are supported");
            }

            if (request.CvFile.Length == 0)
            {
                throw new InvalidOperationException("The uploaded file is empty");
            }

            string cvText;
            using (var stream = request.CvFile.OpenReadStream())
            {
                cvText = await pdfService.ExtractTextFromPdfAsync(stream);
            }

            var (name, email, phone) = await cvInfoExtractionService.ExtractBasicInfoWithAIAsync(cvText);
            var candidate = await StoreCandidateAsync(name, email, phone, cvText, user.UserId);
            var jobPositions = await unitOfWork.JobPositionRepository.GetActivePositionsAsync();
            
            if (!jobPositions.Any())
            {
                logger.LogWarning("No active job positions found in database");
                return new AnalyzeCvResponse
                {
                    TotalPositionsAnalyzed = 0,
                    MatchesFound = 0,
                    Matches = []
                };
            }

            var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
                j.JobCode,
                j.Title,
                j.CompanyCode,
                j.Summary,
                j.Responsibilities,
                j.Requirements,
                j.MustHaveSkills,
                j.NiceToHaveSkills
            )).ToList();

            var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(cvText, jobSummaries);
            var filteredMatches = analysisResult.Matches
                .OrderByDescending(m => m.MatchPercentage)
                .ToList();

            await StoreCvPdfAsync(request.CvFile, candidate.CandidateId);
            await SaveAnalysisHistoryAsync(candidate.CandidateId, user.UserId, jobPositions.Count, filteredMatches, jobPositions);

            var response = new AnalyzeCvResponse
            {
                TotalPositionsAnalyzed = jobPositions.Count,
                MatchesFound = filteredMatches.Count,
                CandidateInfo = new CandidateInfoDto
                {
                    CandidateId = candidate.PublicId,
                    Name = candidate.Name,
                    Email = candidate.Email,
                    HasResume = true
                },
                Matches = filteredMatches.Select(match =>
                {
                    var job = jobPositions.First(j => j.JobCode == match.JobCode);
                    return new JobMatchDto
                    {
                        JobPositionId = job.PublicId,
                        JobCode = match.JobCode,
                        JobTitle = match.JobTitle,
                        CompanyCode = job.CompanyCode,
                        ExternalUrl = job.ExternalUrl,
                        MatchPercentage = match.MatchPercentage,
                        MatchReason = match.MatchReason,
                        MatchingSkills = match.MatchingSkills,
                        MissingSkills = match.MissingSkills,
                        Summary = job.Summary,
                        MustHaveSkills = job.MustHaveSkills,
                        NiceToHaveSkills = job.NiceToHaveSkills,
                        ReferralReason = match.ReferralReason,
                        WhatIsMissingForPerfectMatch = match.WhatIsMissingForPerfectMatch,
                        Priority = job.Priority,
                        CountryRestrictions = job.CountryRestrictions
                    };
                }).ToList()
            };

            logger.LogInformation("CV analysis completed: {MatchCount} matches found for candidate {CandidateName}", 
                filteredMatches.Count, candidate.Name);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing CV for file: {FileName}", request.CvFile.FileName);
            throw;
        }
    }

    private async Task<Candidate> StoreCandidateAsync(string? name, string? email, string? phone, string cvText, long userId)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        var formattedName = name.FormatName();
        var trimmedEmail = email?.Trim();
        var trimmedPhone = phone?.Trim();

        Candidate? existingCandidate = null;
        
        if (!string.IsNullOrWhiteSpace(trimmedEmail))
        {
            existingCandidate = await unitOfWork.CandidateRepository.GetByEmailAsync(trimmedEmail);
        }

        if (existingCandidate != null)
        {
            existingCandidate.Name = formattedName;
            existingCandidate.PhoneNumber = trimmedPhone;
            existingCandidate.AnalyzedAt = now;
            existingCandidate.UserId = userId;
            
            unitOfWork.CandidateRepository.Update(existingCandidate);
            await unitOfWork.CommitAsync();
            
            return existingCandidate;
        }
        else
        {
            var candidate = new Candidate
            {
                UserId = userId,
                Name = formattedName,
                Email = trimmedEmail,
                PhoneNumber = trimmedPhone,
                AnalyzedAt = now
            };

            await unitOfWork.CandidateRepository.AddAsync(candidate);
            await unitOfWork.CommitAsync();
            
            return candidate;
        }
    }

    private async Task StoreCvPdfAsync(Microsoft.AspNetCore.Http.IFormFile cvFile, long candidateId)
    {
        try
        {
            var existingCandidateFile = await unitOfWork.CandidateFileRepository
                .GetByCandidateAndFileTypeAsync(candidateId, "Resume");

            if (existingCandidateFile != null)
            {
                await unitOfWork.CandidateFileRepository.DeleteAsync(existingCandidateFile.PublicId);
                await unitOfWork.FileRepository.DeleteAsync(existingCandidateFile.File.PublicId);
            }

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

            var candidateFile = new CandidateFile
            {
                CandidateId = candidateId,
                File = file,
                FileType = "Resume"
            };

            await unitOfWork.CandidateFileRepository.AddAsync(candidateFile);
            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error storing CV PDF for candidate {CandidateId}", candidateId);
        }
    }

    private async Task SaveAnalysisHistoryAsync(
        long candidateId, 
        long userId, 
        int totalPositions, 
        List<JobMatchResult> matches,
        List<JobPosition> jobPositions)
    {
        try
        {
            await unitOfWork.CvAnalysisRepository.DeleteByCandidateIdAndUserAsync(candidateId, userId);

            var cvAnalysis = new CvAnalysisEntity
            {
                CandidateId = candidateId,
                UserId = userId,
                AnalyzedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                TotalPositionsAnalyzed = totalPositions,
                MatchesFound = matches.Count
            };

            await unitOfWork.CvAnalysisRepository.AddAsync(cvAnalysis);
            await unitOfWork.CommitAsync();

            foreach (var match in matches)
            {
                var job = jobPositions.First(j => j.JobCode == match.JobCode);
                
                var analysisMatch = new CvAnalysisMatchEntity
                {
                    CvAnalysisId = cvAnalysis.CvAnalysisId,
                    JobPositionId = job.JobPositionId,
                    MatchPercentage = match.MatchPercentage,
                    MatchReason = match.MatchReason,
                    MatchingSkills = System.Text.Json.JsonSerializer.Serialize(match.MatchingSkills),
                    MissingSkills = System.Text.Json.JsonSerializer.Serialize(match.MissingSkills),
                    JobTitle = job.Title,
                    JobCode = job.JobCode,
                    CompanyCode = job.CompanyCode,
                    JobSummary = job.Summary,
                    MustHaveSkills = job.MustHaveSkills,
                    NiceToHaveSkills = job.NiceToHaveSkills,
                    WhatIsMissingForPerfectMatch = match.WhatIsMissingForPerfectMatch
                };

                cvAnalysis.Matches.Add(analysisMatch);
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving CV analysis history for candidate {CandidateId}", candidateId);
        }
    }
}
