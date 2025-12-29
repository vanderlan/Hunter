using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

namespace GeminiCode.Hunter.Application.Core.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAIService> _logger;
    private const string Model = "gpt-4o";

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI:ApiKey configuration is missing");
        
        _chatClient = new ChatClient(Model, apiKey);
        _logger = logger;
    }

    public async Task<CvAnalysisResult> AnalyzeCvAgainstJobsAsync(
        string cvText, 
        List<JobPositionSummary> jobPositions)
    {
        try
        {
            _logger.LogInformation("Starting CV analysis against {JobCount} positions using {Model}", 
                jobPositions.Count, Model);

            var prompt = BuildAnalysisPrompt(cvText, jobPositions);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are an expert HR analyst and recruiter. Your job is to analyze CVs and match candidates " +
                    "with job positions based on skills, experience, and requirements. Provide detailed, accurate " +
                    "assessments with specific reasoning."
                ),
                new UserChatMessage(prompt)
            };

            var chatCompletion = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.3f,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "cv_analysis",
                    jsonSchema: BinaryData.FromString(GetResponseSchema()),
                    jsonSchemaIsStrict: true
                )
            });

            var responseContent = chatCompletion.Value.Content[0].Text;
            
            _logger.LogDebug("OpenAI Response: {Response}", responseContent);

            var result = JsonSerializer.Deserialize<OpenAICvAnalysisResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException("Failed to deserialize OpenAI response");

            var cvAnalysisResult = new CvAnalysisResult(
                result.Matches.Select(m => new JobMatchResult(
                    m.JobCode,
                    m.JobTitle,
                    m.MatchPercentage,
                    m.MatchReason,
                    [.. m.MatchingSkills],
                    [.. m.MissingSkills]
                )).ToList()
            );

            _logger.LogInformation("CV analysis completed. Found {MatchCount} matches", 
                cvAnalysisResult.Matches.Count);

            return cvAnalysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV with OpenAI");
            throw;
        }
    }

    private static string BuildAnalysisPrompt(string cvText, List<JobPositionSummary> jobPositions)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("# TASK");
        sb.AppendLine("Analyze the following CV against the provided job positions and determine the best matches.");
        sb.AppendLine();
        
        sb.AppendLine("# CANDIDATE CV");
        sb.AppendLine("```");
        sb.AppendLine(cvText);
        sb.AppendLine("```");
        sb.AppendLine();
        
        sb.AppendLine("# AVAILABLE JOB POSITIONS");
        sb.AppendLine();
        
        foreach (var job in jobPositions)
        {
            sb.AppendLine($"## Job Code: {job.JobCode}");
            sb.AppendLine($"**Title:** {job.Title}");
            sb.AppendLine($"**Company:** {job.CompanyCode}");
            
            if (!string.IsNullOrWhiteSpace(job.Summary))
            {
                sb.AppendLine($"**Summary:** {job.Summary}");
            }
            
            if (!string.IsNullOrWhiteSpace(job.Responsibilities))
            {
                sb.AppendLine($"**Responsibilities:** {job.Responsibilities}");
            }
            
            if (!string.IsNullOrWhiteSpace(job.Requirements))
            {
                sb.AppendLine($"**Requirements:** {job.Requirements}");
            }
            
            if (!string.IsNullOrWhiteSpace(job.MustHaveSkills))
            {
                sb.AppendLine($"**Must-Have Skills:** {job.MustHaveSkills}");
            }
            
            if (!string.IsNullOrWhiteSpace(job.NiceToHaveSkills))
            {
                sb.AppendLine($"**Nice-to-Have Skills:** {job.NiceToHaveSkills}");
            }
            
            sb.AppendLine();
        }
        
        sb.AppendLine("# INSTRUCTIONS");
        sb.AppendLine("For each job position:");
        sb.AppendLine("1. Calculate a match percentage (0-100) based on:");
        sb.AppendLine("   - Skills alignment (technical and soft skills)");
        sb.AppendLine("   - Experience level and years");
        sb.AppendLine("   - Education background");
        sb.AppendLine("   - Responsibilities match");
        sb.AppendLine("2. Provide a clear, concise reason for the match percentage");
        sb.AppendLine("3. List specific matching skills found in the CV");
        sb.AppendLine("4. List skills required by the job but missing from the CV");
        sb.AppendLine();
        sb.AppendLine("Return ONLY positions with match percentage > 20%, ordered by match percentage (highest first).");
        
        return sb.ToString();
    }

    private static string GetResponseSchema()
    {
        return """
        {
          "type": "object",
          "properties": {
            "matches": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "jobCode": { "type": "string" },
                  "jobTitle": { "type": "string" },
                  "matchPercentage": { "type": "number" },
                  "matchReason": { "type": "string" },
                  "matchingSkills": {
                    "type": "array",
                    "items": { "type": "string" }
                  },
                  "missingSkills": {
                    "type": "array",
                    "items": { "type": "string" }
                  }
                },
                "required": ["jobCode", "jobTitle", "matchPercentage", "matchReason", "matchingSkills", "missingSkills"],
                "additionalProperties": false
              }
            }
          },
          "required": ["matches"],
          "additionalProperties": false
        }
        """;
    }

    private record OpenAICvAnalysisResponse(List<OpenAIJobMatch> Matches);
    
    private record OpenAIJobMatch(
        string JobCode,
        string JobTitle,
        decimal MatchPercentage,
        string MatchReason,
        List<string> MatchingSkills,
        List<string> MissingSkills
    );
}
