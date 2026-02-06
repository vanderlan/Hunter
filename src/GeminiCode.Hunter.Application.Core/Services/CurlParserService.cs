using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.Services;

public interface ICurlParserService
{
    (string Cookies, string XsrfToken, string DatasourceId, string ReportId, string SessionId) ParseCurlCommand(string curlCommand);
}

public class CurlParserService(ILogger<CurlParserService> logger) : ICurlParserService
{
    private readonly ILogger<CurlParserService> _logger = logger;

    public (string Cookies, string XsrfToken, string DatasourceId, string ReportId, string SessionId) ParseCurlCommand(string curlCommand)
    {
        try
        {
            _logger.LogDebug("Parsing bash curl command (length: {length})", curlCommand.Length);

            // Normalize bash line continuations (\)
            var normalizedCommand = curlCommand
                .Replace("\\\r\n", " ")
                .Replace("\\\n", " ")
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("  ", " ")
                .Trim();

            // Extract cookies (bash format: -b 'cookies')
            string cookies = ExtractCookies(normalizedCommand);
            if (string.IsNullOrEmpty(cookies))
            {
                throw new InvalidOperationException("Could not extract cookies from curl command. Make sure you used 'Copy as cURL (bash)' from Chrome DevTools and copied the batchedDataV2 request.");
            }

            // Extract XSRF token (bash format: -H 'x-rap-xsrf-token: ...')
            string xsrfToken = ExtractXsrfToken(normalizedCommand);
            if (string.IsNullOrEmpty(xsrfToken))
            {
                throw new InvalidOperationException("Could not extract XSRF token. Make sure you copied the batchedDataV2 request (the one that happens when you refresh the page).");
            }

            // Extract data payload and parse JSON to get datasourceId and reportId (batchedDataV2 format)
            var (datasourceId, reportId) = ExtractDataPayload(normalizedCommand);
            
            if (string.IsNullOrEmpty(datasourceId) || string.IsNullOrEmpty(reportId))
            {
                throw new InvalidOperationException("Could not extract datasourceId or reportId. Make sure you copied the batchedDataV2 POST request, not other requests.");
            }

            // Extract sessionId from data payload
            string sessionId = ExtractSessionId(normalizedCommand);

            _logger.LogInformation("Successfully parsed curl command. DatasourceId: {datasourceId}, ReportId: {reportId}, SessionId: {sessionId}", 
                datasourceId, reportId, sessionId ?? "(none)");

            return (cookies, xsrfToken, datasourceId, reportId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing curl command");
            throw;
        }
    }

    private string ExtractCookies(string curlCommand)
    {
        // Bash format: -b 'cookie_string'
        var match = Regex.Match(curlCommand, @"-b\s+'([^']+)'", RegexOptions.Singleline);
        
        if (match.Success)
        {
            var cookies = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted cookies (length: {length})", cookies.Length);
            return cookies;
        }

        _logger.LogWarning("Could not find cookies. Make sure you used 'Copy as cURL (bash)' format.");
        return string.Empty;
    }

    private string ExtractXsrfToken(string curlCommand)
    {
        // Bash format: -H 'x-rap-xsrf-token: token_value'
        var match = Regex.Match(curlCommand, @"-H\s+'x-rap-xsrf-token:\s*([^']+)'", RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var token = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted XSRF token: {token}", token.Substring(0, Math.Min(20, token.Length)) + "...");
            return token;
        }

        _logger.LogWarning("Could not find XSRF token in curl command");
        return string.Empty;
    }

    private (string DatasourceId, string ReportId) ExtractDataPayload(string curlCommand)
    {
        // Bash format: --data-raw '{"dataRequest":[{...}]}'
        var dataMatch = Regex.Match(curlCommand, @"--data-raw\s+'(.+?)'(?:\s|$)", RegexOptions.Singleline);
        
        if (!dataMatch.Success)
        {
            _logger.LogWarning("Could not find --data-raw in curl command");
            return (string.Empty, string.Empty);
        }

        var dataPayload = dataMatch.Groups[1].Value;
        _logger.LogDebug("Found data payload (length: {length})", dataPayload.Length);

        // Extract datasourceId from JSON: "datasourceId":"xxx" (nested in dataRequest[0].datasetSpec.dataset[0])
        var datasourceMatch = Regex.Match(dataPayload, @"""datasourceId"":\s*""([^""]+)""", RegexOptions.IgnoreCase);
        var datasourceId = datasourceMatch.Success ? datasourceMatch.Groups[1].Value : string.Empty;

        // Extract reportId from JSON: "reportId":"xxx" (nested in dataRequest[0].requestContext.reportContext)
        var reportMatch = Regex.Match(dataPayload, @"""reportId"":\s*""([^""]+)""", RegexOptions.IgnoreCase);
        var reportId = reportMatch.Success ? reportMatch.Groups[1].Value : string.Empty;

        if (!string.IsNullOrEmpty(datasourceId) && !string.IsNullOrEmpty(reportId))
        {
            _logger.LogDebug("Extracted datasourceId: {datasourceId}, reportId: {reportId}", datasourceId, reportId);
        }
        else
        {
            _logger.LogWarning("Could not extract datasourceId or reportId from data payload");
        }

        return (datasourceId, reportId);
    }

    private string ExtractSessionId(string curlCommand)
    {
        // SessionId is in the data payload for getFirebaseAuthToken: "sessionId":"xxx"
        var dataMatch = Regex.Match(curlCommand, @"--data-raw\s+'(.+?)'(?:\s|$)", RegexOptions.Singleline);
        
        if (!dataMatch.Success)
        {
            return string.Empty;
        }

        var dataPayload = dataMatch.Groups[1].Value;
        
        // Extract sessionId from JSON
        var sessionMatch = Regex.Match(dataPayload, @"""sessionId"":\s*""([^""]+)""", RegexOptions.IgnoreCase);
        
        if (sessionMatch.Success)
        {
            var sessionId = sessionMatch.Groups[1].Value;
            _logger.LogDebug("Extracted sessionId: {sessionId}", sessionId);
            return sessionId;
        }

        _logger.LogDebug("SessionId not found in data payload (this is OK for batchedDataV2 requests)");
        return string.Empty;
    }
}
