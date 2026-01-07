using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.Services;

public class CvInfoExtractionService(
    ILogger<CvInfoExtractionService> logger,
    IOpenAIService openAIService) : ICvInfoExtractionService
{
    public (string? name, string? email, string? phone) ExtractBasicInfo(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            logger.LogWarning("CV text is empty, cannot extract basic info");
            return (null, null, null);
        }

        var name = ExtractName(cvText);
        var email = ExtractEmail(cvText);
        var phone = ExtractPhone(cvText);

        logger.LogInformation("Extracted CV info - Name: {Name}, Email: {Email}, Phone: {Phone}", 
            name ?? "Not found", 
            email ?? "Not found", 
            phone ?? "Not found");

        return (name, email, phone);
    }

    public async Task<string?> ExtractNameWithAIAsync(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            logger.LogWarning("CV text is empty, cannot extract name");
            return null;
        }

        try
        {
            logger.LogInformation("Extracting candidate name using OpenAI");

            // Take first 1000 characters of CV for name extraction (usually at the top)
            var cvSample = cvText.Length > 1000 ? cvText.Substring(0, 1000) : cvText;

            var prompt = $@"Extract ONLY the candidate's full name from this CV text. 
Return just the name, nothing else. If you cannot find the name, return 'Unknown'.

CV Text:
{cvSample}

Candidate Name:";

            var name = await openAIService.GetCompletionAsync(prompt, maxTokens: 50);
            
            if (string.IsNullOrWhiteSpace(name) || name.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("OpenAI could not extract name from CV");
                return null;
            }

            name = name.Trim().Replace("\"", "").Replace("'", "");
            
            logger.LogInformation("Extracted name using OpenAI: {Name}", name);
            
            return name;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting name with OpenAI");
            return null;
        }
    }

    private string? ExtractName(string cvText)
    {
        // Try to extract name from first few lines (usually at the top)
        var lines = cvText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
            return null;

        // Look for the first non-empty line that's not too long and contains letters
        foreach (var line in lines.Take(10))
        {
            var trimmed = line.Trim();
            
            // Skip empty lines, email lines, phone lines, and very long lines
            if (string.IsNullOrWhiteSpace(trimmed) || 
                trimmed.Length > 50 || 
                trimmed.Length < 3 ||
                trimmed.Contains('@') || 
                Regex.IsMatch(trimmed, @"\d{3,}"))
            {
                continue;
            }

            // Check if it looks like a name (mostly letters and spaces)
            if (Regex.IsMatch(trimmed, @"^[A-Za-zÀ-ÿ\s'-]{3,50}$"))
            {
                return trimmed;
            }
        }

        return null;
    }

    private string? ExtractEmail(string cvText)
    {
        // Email regex pattern
        var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
        var match = Regex.Match(cvText, emailPattern);

        return match.Success ? match.Value : null;
    }

    private string? ExtractPhone(string cvText)
    {
        // Phone patterns (various formats)
        var phonePatterns = new[]
        {
            @"\+?\d{1,3}[-.\s]?\(?\d{2,3}\)?[-.\s]?\d{3,4}[-.\s]?\d{4}", // International format
            @"\(?\d{2,3}\)?[-.\s]?\d{4,5}[-.\s]?\d{4}",                   // Brazilian format
            @"\d{10,11}",                                                  // Compact format
        };

        foreach (var pattern in phonePatterns)
        {
            var match = Regex.Match(cvText, pattern);
            if (match.Success)
            {
                // Clean up the phone number
                var phone = match.Value.Trim();
                
                // Skip if it looks like a date (e.g., 2024-12-29)
                if (Regex.IsMatch(phone, @"^\d{4}[-/]\d{2}[-/]\d{2}$"))
                    continue;

                return phone;
            }
        }

        return null;
    }
}
