using System.Text.Json;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.Services;

public class CvInfoExtractionService(
    ILogger<CvInfoExtractionService> logger,
    IAIService openAIService) : ICvInfoExtractionService
{
    public async Task<(string? name, string? email, string? phone)> ExtractBasicInfoWithAIAsync(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            logger.LogWarning("CV text is empty, cannot extract basic info");
            return (null, null, null);
        }

        try
        {
            logger.LogInformation("Extracting candidate information using AI");

            // Take first 2000 characters of CV for extraction (usually contains all basic info)
            var cvSample = cvText.Length > 2000 ? cvText.Substring(0, 2000) : cvText;

            var prompt = $@"Extract the following information from this CV text and return it as a JSON object with these exact fields: ""name"", ""email"", ""phone"".
If any information is not found, use null for that field.

Rules:
- Extract the full name of the candidate
- Extract the email address
- Extract the phone number (keep the original format)
- Return ONLY valid JSON, no additional text

CV Text:
{cvSample}

JSON Response:";

            var response = await openAIService.GetCompletionAsync(prompt, maxTokens: 150);
            
            if (string.IsNullOrWhiteSpace(response))
            {
                logger.LogWarning("AI returned empty response for CV extraction");
                return (null, null, null);
            }

            // Clean up the response to extract JSON
            response = response.Trim();
            
            // Remove markdown code blocks if present
            if (response.StartsWith("```"))
            {
                var lines = response.Split('\n');
                response = string.Join('\n', lines.Skip(1).Take(lines.Length - 2));
            }

            // Parse the JSON response
            var extractedInfo = JsonSerializer.Deserialize<CvExtractedInfo>(response, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (extractedInfo == null)
            {
                logger.LogWarning("Failed to parse AI response as JSON");
                return (null, null, null);
            }

            // Clean up the extracted values
            var name = CleanValue(extractedInfo.Name);
            var email = CleanValue(extractedInfo.Email);
            var phone = CleanValue(extractedInfo.Phone);

            logger.LogInformation("Extracted CV info using AI - Name: {Name}, Email: {Email}, Phone: {Phone}", 
                name ?? "Not found", 
                email ?? "Not found", 
                phone ?? "Not found");

            return (name, email, phone);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting CV information with AI");
            return (null, null, null);
        }
    }

    private static string? CleanValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim().Replace("\"", "").Replace("'", "");

        // Check for placeholder values that indicate "not found"
        if (value.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Not found", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value;
    }

    private class CvExtractedInfo
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
