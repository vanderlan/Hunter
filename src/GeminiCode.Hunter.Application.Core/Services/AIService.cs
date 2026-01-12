using GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using static GeminiCode.Hunter.Domain.Core.Constants.DomainConstants;

namespace GeminiCode.Hunter.Application.Core.Services;

public class AIService : IAIService
{
    private ChatClient _chatClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _provider;
    private string _model;

    public AIService(IConfiguration configuration, ILogger<AIService> logger)
    {
        _provider = configuration["AI:Provider"] ?? AIProviders.OpenAI;

        SetupProvider(configuration, logger);

        _logger = logger;
    }

    public async Task<JobEnrichmentResult> EnrichJobDetailsAsync(string jobTitle, string rawHtmlContent)
    {
        try
        {
            _logger.LogInformation("Enriching job details for: {JobTitle} using {Provider}", jobTitle, _provider);

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

            ChatCompletionOptions options;
            
            if (_provider.Equals(AIProviders.DeepSeek, StringComparison.OrdinalIgnoreCase))
            {
                options = new ChatCompletionOptions
                {
                    Temperature = 0.2f,
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                };
            }
            else
            {
                options = new ChatCompletionOptions
                {
                    Temperature = 0.2f,
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "job_enrichment",
                        jsonSchema: BinaryData.FromString(GetJobEnrichmentSchema()),
                        jsonSchemaIsStrict: true
                    )
                };
            }

            var chatCompletion = await _chatClient.CompleteChatAsync(messages, options);

            var responseContent = chatCompletion.Value.Content[0].Text;
            
            _logger.LogDebug("{Provider} Job Enrichment Response: {Response}", _provider, responseContent);

            var result = JsonSerializer.Deserialize<OpenAIJobEnrichmentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize {_provider} job enrichment response");

            _logger.LogInformation("Successfully enriched job details for: {JobTitle} using {Provider}", jobTitle, _provider);

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
            _logger.LogError(ex, "Error enriching job details for: {JobTitle} using {Provider}", jobTitle, _provider);
            throw;
        }
    }

    public async Task<CvAnalysisResult> AnalyzeCvAgainstJobsAsync(
        string cvText, 
        List<JobPositionSummary> jobPositions)
    {
        try
        {
            _logger.LogInformation("Starting CV analysis against {JobCount} positions using {Provider} ({Model})", 
                jobPositions.Count, _provider, _model);

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

            ChatCompletionOptions options;
            
            if (_provider.Equals(AIProviders.DeepSeek, StringComparison.OrdinalIgnoreCase))
            {
                options = new ChatCompletionOptions
                {
                    Temperature = 0.3f,
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                };
            }
            else
            {
                options = new ChatCompletionOptions
                {
                    Temperature = 0.3f,
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "cv_analysis",
                        jsonSchema: BinaryData.FromString(GetResponseSchema()),
                        jsonSchemaIsStrict: true
                    )
                };
            }

            var chatCompletion = await _chatClient.CompleteChatAsync(messages, options);

            var responseContent = chatCompletion.Value.Content[0].Text;
            
            _logger.LogDebug("{Provider} CV Analysis Response: {Response}", _provider, responseContent);

            var result = JsonSerializer.Deserialize<OpenAICvAnalysisResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize {_provider} response");

            var cvAnalysisResult = new CvAnalysisResult(
                result.Matches.Select(m => new JobMatchResult(
                    m.JobCode,
                    m.JobTitle,
                    m.MatchPercentage,
                    m.MatchReason,
                    [.. m.MatchingSkills],
                    [.. m.MissingSkills],
                    m.ReferralReason,
                    m.WhatIsMissingForPerfectMatch
                )).ToList()
            );

            _logger.LogInformation("CV analysis completed using {Provider}. Found {MatchCount} matches", 
                _provider, cvAnalysisResult.Matches.Count);

            return cvAnalysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV with {Provider}", _provider);
            throw;
        }
    }

    public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 100)
    {
        try
        {
            _logger.LogInformation("Getting {Provider} completion with max tokens: {MaxTokens}", _provider, maxTokens);

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
            
            _logger.LogDebug("{Provider} Response: {Response}", _provider, response);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion from {Provider}", _provider);
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
        sb.AppendLine();
        sb.AppendLine("# OUTPUT FORMAT");
        sb.AppendLine("Return a JSON object with this exact structure:");
        sb.AppendLine("```json");
        sb.AppendLine("{");
        sb.AppendLine("  \"summary\": \"string\",");
        sb.AppendLine("  \"responsibilities\": \"string\",");
        sb.AppendLine("  \"skills\": \"string\",");
        sb.AppendLine("  \"mustHaveSkills\": \"string\",");
        sb.AppendLine("  \"niceToHaveSkills\": \"string\"");
        sb.AppendLine("}");
        sb.AppendLine("```");
        
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
        sb.AppendLine("5. Generate 'What is missing for a perfect match?' (2-4 sentences) explaining:");
        sb.AppendLine("   - Specific skills, experiences, or qualifications the candidate needs to develop");
        sb.AppendLine("   - Areas where the candidate could improve to reach 100% match");
        sb.AppendLine("   - Be constructive and actionable");
        sb.AppendLine("   - If match is already very high (95%+), mention minor improvements or continued development");
        sb.AppendLine();
        sb.AppendLine($"Return ONLY positions with match percentage > {AIMatching.MinimumMatchPercentage}%, ordered by match percentage (highest first).");
        sb.AppendLine("Validate CV File if you are not able to find important information such as: Name, e-mail, phone number or the file is inconsistent with a usual CV format, reject then and returns no matches.");
        sb.AppendLine();
        sb.AppendLine("# OUTPUT FORMAT");
        sb.AppendLine("Return a JSON object with this exact structure:");
        sb.AppendLine("```json");
        sb.AppendLine("{");
        sb.AppendLine("  \"matches\": [");
        sb.AppendLine("    {");
        sb.AppendLine("      \"jobCode\": \"string\",");
        sb.AppendLine("      \"jobTitle\": \"string\",");
        sb.AppendLine("      \"matchPercentage\": 85.5,");
        sb.AppendLine("      \"matchReason\": \"string\",");
        sb.AppendLine("      \"matchingSkills\": [\"skill1\", \"skill2\"],");
        sb.AppendLine("      \"missingSkills\": [\"skill3\", \"skill4\"],");
        sb.AppendLine("      \"whatIsMissingForPerfectMatch\": \"string\"");
        sb.AppendLine("    }");
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        sb.AppendLine("```");
        
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
                  },
                  "whatIsMissingForPerfectMatch": { "type": "string" }
                },
                "required": ["jobCode", "jobTitle", "matchPercentage", "matchReason", "matchingSkills", "missingSkills", "whatIsMissingForPerfectMatch"],
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
        List<string> MissingSkills,
        string? ReferralReason,
        string WhatIsMissingForPerfectMatch
    );

    private void SetupProvider(IConfiguration configuration, ILogger<AIService> logger)
    {
        string apiKey;
        string baseUrl = null;

        if (_provider.Equals(AIProviders.DeepSeek, StringComparison.OrdinalIgnoreCase))
        {
            apiKey = configuration["DeepSeek:ApiKey"];
            _model = configuration["DeepSeek:Model"];
            baseUrl = AIProviders.DeepSeekUrl;

            logger.LogInformation("Using DeepSeek AI provider with model: {Model}", _model);
        }
        else
        {
            apiKey = configuration["OpenAI:ApiKey"];
            _model = configuration["OpenAI:Model"];

            logger.LogInformation("Using OpenAI provider with model: {Model}", _model);
        }

        if (!string.IsNullOrEmpty(baseUrl))
        {
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(baseUrl)
            };

            var apiKeyCredential = new ApiKeyCredential(apiKey);
            var client = new OpenAIClient(apiKeyCredential, options);
            _chatClient = client.GetChatClient(_model);
        }
        else
        {
            _chatClient = new ChatClient(_model, apiKey);
        }
    }
}
