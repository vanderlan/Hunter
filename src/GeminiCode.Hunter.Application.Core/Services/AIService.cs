using GeminiCode.Hunter.Domain.Core.Constants;
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
    private readonly IConfiguration _configuration;
    private readonly IParameterService _parameterService;
    private readonly IPromptService _promptService;
    private string _provider;
    private string _model;

    public AIService(
        IConfiguration configuration, 
        ILogger<AIService> logger,
        IParameterService parameterService,
        IPromptService promptService)
    {
        _configuration = configuration;
        _logger = logger;
        _parameterService = parameterService;
        _promptService = promptService;
        
        // Provider will be set from parameter service in async methods
        _provider = configuration["AI:Provider"] ?? AIProviders.OpenAI;

        SetupProvider(configuration, logger);
    }

    public async Task<JobEnrichmentResult> EnrichJobDetailsAsync(string jobTitle, string rawHtmlContent)
    {
        try
        {
            // Get active AI provider from parameter
            _provider = await _parameterService.GetValueAsync(ParameterConstants.ActiveAIService, _configuration["AI:Provider"] ?? AIProviders.OpenAI);
            
            // Re-setup provider with new configuration
            SetupProvider(_configuration, _logger);

            _logger.LogInformation("Enriching job details for: {JobTitle} using {Provider}", jobTitle, _provider);

            var prompt = await BuildJobEnrichmentPromptAsync(jobTitle, rawHtmlContent);

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

            var result = JsonSerializer.Deserialize<OpenAIJobEnrichmentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize {_provider} job enrichment response");

            return new JobEnrichmentResult(
                Summary: result.Summary ?? string.Empty,
                Responsibilities: result.Responsibilities ?? string.Empty,
                Requirements: result.Requirements ?? string.Empty,
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
            // Get active AI provider from parameter
            _provider = await _parameterService.GetValueAsync(ParameterConstants.ActiveAIService, _configuration["AI:Provider"] ?? AIProviders.OpenAI);
            
            // Re-setup provider with new configuration
            SetupProvider(_configuration, _logger);

            _logger.LogInformation("Analyzing CV against {JobCount} positions using {Provider}", 
                jobPositions.Count, _provider);

            var prompt = await BuildAnalysisPromptAsync(cvText, jobPositions);

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

            var result = JsonSerializer.Deserialize<OpenAICvAnalysisResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize {_provider} response");

            var cvAnalysisResult = new CvAnalysisResult(
                result.Matches.Select(m => new JobMatchResult(
                    m.JobTitle,
                    m.MatchPercentage,
                    m.MatchReason,
                    [.. m.MatchingSkills],
                    [.. m.MissingSkills],
                    m.ReferralReason,
                    m.WhatIsMissingForPerfectMatch
                )).ToList()
            );

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

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion from {Provider}", _provider);
            throw;
        }
    }

    private async Task<string> BuildJobEnrichmentPromptAsync(string jobTitle, string rawHtmlContent)
    {
        var promptTemplate = await _promptService.GetPromptTextAsync(PromptConstants.JobEnrichment);

        var prompt = promptTemplate
            .Replace("{VAR_jobTitle}", jobTitle)
            .Replace("{VAR_rawHtmlContent}", rawHtmlContent);

        return prompt;
    }

    private async Task<string> BuildAnalysisPromptAsync(string cvText, List<JobPositionSummary> jobPositions)
    {
        var promptTemplate = await _promptService.GetPromptTextAsync(PromptConstants.CvAnalysis);      

        var jobPositionsText = BuildJobPositionsSection(jobPositions);
        
        var minMatchPercentage = await _parameterService.GetValueAsIntAsync(ParameterConstants.MinimumMatchPercentageCvAnalysis, AIMatching.MinimumMatchPercentage);

        var prompt = promptTemplate
            .Replace("{VAR_candidateCVText}", cvText)
            .Replace("{VAR_jobPositions}", jobPositionsText)
            .Replace("{VAR_minimumMatchPercentage}", minMatchPercentage.ToString());

        return prompt;
    }

    private static string BuildJobPositionsSection(List<JobPositionSummary> jobPositions)
    {
        var sb = new StringBuilder();

        foreach (var job in jobPositions)
        {
            sb.AppendLine($"## {job.Title}");

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
            "requirements": { "type": "string" },
            "mustHaveSkills": { "type": "string" },
            "niceToHaveSkills": { "type": "string" }
          },
          "required": ["summary", "responsibilities", "requirements", "mustHaveSkills", "niceToHaveSkills"],
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
        string? Requirements,
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
