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

    public async Task<JobEnrichmentResult> EnrichJobDetailsAsync(string jobTitle, string rawHtmlContent)
    {
        try
        {
            _logger.LogInformation("Enriching job details for: {JobTitle}", jobTitle);

            var prompt = BuildJobEnrichmentPrompt(jobTitle, rawHtmlContent);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are an expert HR content analyzer. Your task is to extract structured information " +
                    "from job posting HTML content. Be accurate, concise, and professional. " +
                    "Extract only the information present in the content."
                ),
                new UserChatMessage(prompt)
            };

            var chatCompletion = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.2f,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "job_enrichment",
                    jsonSchema: BinaryData.FromString(GetJobEnrichmentSchema()),
                    jsonSchemaIsStrict: true
                )
            });

            var responseContent = chatCompletion.Value.Content[0].Text;
            
            _logger.LogDebug("OpenAI Job Enrichment Response: {Response}", responseContent);

            var result = JsonSerializer.Deserialize<OpenAIJobEnrichmentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException("Failed to deserialize OpenAI job enrichment response");

            _logger.LogInformation("Successfully enriched job details for: {JobTitle}", jobTitle);

            return new JobEnrichmentResult(
                Summary: result.Summary ?? string.Empty,
                Responsibilities: result.Responsibilities ?? string.Empty,
                Skills: result.Skills ?? string.Empty,
                MustHaveSkills: result.MustHaveSkills ?? string.Empty,
                NiceToHaveSkills: result.NiceToHaveSkills ?? string.Empty
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching job details for: {JobTitle}", jobTitle);
            throw;
        }
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

    public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 100)
    {
        try
        {
            _logger.LogInformation("Getting OpenAI completion with max tokens: {MaxTokens}", maxTokens);

            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var chatCompletion = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = maxTokens
            });

            var response = chatCompletion.Value.Content[0].Text.Trim();
            
            _logger.LogDebug("OpenAI Response: {Response}", response);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion from OpenAI");
            throw;
        }
    }

    private static string BuildJobEnrichmentPrompt(string jobTitle, string rawHtmlContent)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("# TASK");
        sb.AppendLine($"Extract structured information from the following job posting for: **{jobTitle}**");
        sb.AppendLine();
        
        sb.AppendLine("# JOB POSTING HTML CONTENT");
        sb.AppendLine("```html");
        sb.AppendLine(rawHtmlContent);
        sb.AppendLine("```");
        sb.AppendLine();
        
        sb.AppendLine("# EXTRACTION INSTRUCTIONS");
        sb.AppendLine();
        sb.AppendLine("Extract and structure the following information:");
        sb.AppendLine();
        sb.AppendLine("1. **Summary**: A concise 2-3 sentence overview of the position");
        sb.AppendLine("2. **Responsibilities**: Key duties and responsibilities (bullet points or paragraphs)");
        sb.AppendLine("3. **Skills**: ALL technical and soft skills mentioned, comma-separated");
        sb.AppendLine("   - Example: \"Java, C#, SQL Server, Kubernetes, Docker, Agile, Team Leadership\"");
        sb.AppendLine("4. **Must-Have Skills**: Essential/required skills only, comma-separated");
        sb.AppendLine("5. **Nice-to-Have Skills**: Preferred/optional skills, comma-separated");
        sb.AppendLine();
        sb.AppendLine("# IMPORTANT RULES");
        sb.AppendLine("- Extract ONLY information present in the HTML content");
        sb.AppendLine("- Do NOT invent or assume information");
        sb.AppendLine("- If a section has no data, return empty string");
        sb.AppendLine("- For Skills: include ALL skills mentioned (technical, soft, tools, technologies)");
        sb.AppendLine("- Format skills as comma-separated values");
        sb.AppendLine("- Keep text concise and professional");
        
        return sb.ToString();
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
        sb.AppendLine("Return ONLY positions with match percentage > 60%, ordered by match percentage (highest first).");
        sb.AppendLine("Validate CV File if you are not able to find important information such as: Name, e-mail, phone number or the file is inconsistent with a usual CV format, reject then and returns no matches.");
        
        return sb.ToString();
    }

    private static string GetJobEnrichmentSchema()
    {
        return """
        {
          "type": "object",
          "properties": {
            "summary": { "type": "string" },
            "responsibilities": { "type": "string" },
            "skills": { "type": "string" },
            "mustHaveSkills": { "type": "string" },
            "niceToHaveSkills": { "type": "string" }
          },
          "required": ["summary", "responsibilities", "skills", "mustHaveSkills", "niceToHaveSkills"],
          "additionalProperties": false
        }
        """;
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

    private record OpenAIJobEnrichmentResponse(
        string? Summary,
        string? Responsibilities,
        string? Skills,
        string? MustHaveSkills,
        string? NiceToHaveSkills
    );
    
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
