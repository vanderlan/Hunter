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
using System.IO;
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
            // Get current user entity to obtain UserId
            var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
                ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

            // Validate file
            if (request.CvFile.ContentType != "application/pdf")
            {
                throw new InvalidOperationException("Only PDF files are supported");
            }

            if (request.CvFile.Length == 0)
            {
                throw new InvalidOperationException("The uploaded file is empty");
            }

            // Extract text from PDF
            string cvText;
            using (var stream = request.CvFile.OpenReadStream())
            {
                cvText = await pdfService.ExtractTextFromPdfAsync(stream);
            }

            logger.LogInformation("Extracted {Length} characters from CV", cvText.Length);

            // Extract basic candidate information
            var (_, email, phone) = cvInfoExtractionService.ExtractBasicInfo(cvText);
            
            // Extract name using OpenAI (more accurate than regex)
            var name = await cvInfoExtractionService.ExtractNameWithAIAsync(cvText);

            // Store or update candidate information BEFORE analysis
            var candidate = await StoreCandidateAsync(name, email, phone, cvText, user.UserId);

            // Get all active job positions
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

            logger.LogInformation("Found {Count} active job positions to analyze", jobPositions.Count);

            // Prepare job summaries for OpenAI
            var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
                j.JobCode,
                j.Title,
                j.CompanyCode,
                j.Summary,
                j.Responsibilities,
                j.MustHaveSkills,
                j.NiceToHaveSkills
            )).ToList();

            // Analyze CV against jobs using OpenAI
            var analysisResult = await openAIService.AnalyzeCvAgainstJobsAsync(cvText, jobSummaries);

            // Filter matches > 60% and order by match percentage descending
            var filteredMatches = analysisResult.Matches
                .Where(m => m.MatchPercentage > 60)
                .OrderByDescending(m => m.MatchPercentage)
                .ToList();

            logger.LogInformation("Found {Count} matches with >60% adherence", filteredMatches.Count);

            // Store the PDF file after successful analysis
            await StoreCvPdfAsync(request.CvFile, candidate.CandidateId);

            // Save analysis history
            await SaveAnalysisHistoryAsync(candidate.CandidateId, user.UserId, jobPositions.Count, filteredMatches, jobPositions);

            // Map to response DTOs with full job details
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
                        NiceToHaveSkills = job.NiceToHaveSkills
                    };
                }).ToList()
            };

            logger.LogInformation("CV analysis completed successfully");

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing CV");
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
            // Update existing candidate with latest CV
            logger.LogInformation("Updating existing candidate with email: {Email}, analyzed by user: {UserId}", 
                trimmedEmail, userId);
            
            existingCandidate.Name = formattedName;
            existingCandidate.PhoneNumber = trimmedPhone;
            existingCandidate.CvText = cvText;
            existingCandidate.AnalyzedAt = now;
            existingCandidate.UserId = userId;
            
            unitOfWork.CandidateRepository.Update(existingCandidate);
            await unitOfWork.CommitAsync();
            
            logger.LogInformation("Candidate information updated successfully with name: {Name}", formattedName);
            return existingCandidate;
        }
        else
        {
            // Create new candidate
            logger.LogInformation("Creating new candidate analyzed by user: {UserId}", userId);
            
            var candidate = new Candidate
            {
                UserId = userId,
                Name = formattedName,
                Email = trimmedEmail,
                PhoneNumber = trimmedPhone,
                CvText = cvText,
                AnalyzedAt = now
            };

            await unitOfWork.CandidateRepository.AddAsync(candidate);
            await unitOfWork.CommitAsync();
            
            logger.LogInformation("Candidate information stored successfully with name: {Name}", formattedName);
            return candidate;
        }
    }

    private async Task StoreCvPdfAsync(Microsoft.AspNetCore.Http.IFormFile cvFile, long candidateId)
    {
        try
        {
            logger.LogInformation("Storing CV PDF for candidate {CandidateId}", candidateId);

            // Delete existing resume if any
            var existingCandidateFile = await unitOfWork.CandidateFileRepository
                .GetByCandidateAndFileTypeAsync(candidateId, "Resume");

            if (existingCandidateFile != null)
            {
                logger.LogInformation("Deleting existing resume for candidate {CandidateId}", candidateId);
                await unitOfWork.CandidateFileRepository.DeleteAsync(existingCandidateFile.PublicId);
                await unitOfWork.FileRepository.DeleteAsync(existingCandidateFile.File.PublicId);
            }

            // Read file data
            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await cvFile.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            // Create File entity
            var file = new FileEntity
            {
                FileName = cvFile.FileName,
                ContentType = cvFile.ContentType,
                FileSize = cvFile.Length,
                StorageType = StorageType.Database,
                FileData = fileData
            };

            await unitOfWork.FileRepository.AddAsync(file);

            // Create CandidateFile relationship
            var candidateFile = new CandidateFile
            {
                CandidateId = candidateId,
                File = file,
                FileType = "Resume"
            };

            await unitOfWork.CandidateFileRepository.AddAsync(candidateFile);
            await unitOfWork.CommitAsync();

            logger.LogInformation("CV PDF stored successfully for candidate {CandidateId}, File ID: {FileId}", 
                candidateId, file.PublicId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error storing CV PDF for candidate {CandidateId}", candidateId);
            // Don't throw - we don't want to fail the entire analysis if file storage fails
            // The CV text and analysis results are already stored
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
            logger.LogInformation("Saving CV analysis history for candidate {CandidateId}", candidateId);

            logger.LogInformation("Deleting previous analysis records for candidate {CandidateId}", candidateId);
            await unitOfWork.CvAnalysisRepository.DeleteByCandidateIdAsync(candidateId);

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

            // Save matches
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
                    NiceToHaveSkills = job.NiceToHaveSkills
                };

                cvAnalysis.Matches.Add(analysisMatch);
            }

            await unitOfWork.CommitAsync();

            logger.LogInformation("CV analysis history saved successfully. Analysis ID: {AnalysisId}", cvAnalysis.PublicId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving CV analysis history for candidate {CandidateId}", candidateId);
            // Don't throw - we don't want to fail the entire analysis if history save fails
        }
    }
}
