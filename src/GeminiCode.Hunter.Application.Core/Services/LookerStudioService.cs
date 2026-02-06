using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Exceptions;

namespace GeminiCode.Hunter.Application.Core.Services;

public interface ILookerStudioService
{
    Task<string> DownloadPriorityCsvAsync(CancellationToken cancellationToken);
    Task<bool> ValidateCredentialsAsync();
}

public class LookerStudioService : ILookerStudioService
{
    private readonly ILogger<LookerStudioService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly HttpClient _httpClient;
    private readonly string _batchDataUrl;
    private readonly int _timeoutSeconds;

    public LookerStudioService(
        IConfiguration configuration,
        ILogger<LookerStudioService> logger,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        HttpClient httpClient)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _httpClient = httpClient;

        var lookerConfig = configuration.GetSection("LookerStudio");
        _batchDataUrl = lookerConfig["BatchDataUrl"] ?? "https://lookerstudio.google.com/u/0/batchedDataV2";
        _timeoutSeconds = int.Parse(lookerConfig["RequestTimeoutSeconds"] ?? "30");
    }

    public async Task<string> DownloadPriorityCsvAsync(CancellationToken cancellationToken)
    {
        var credential = await _unitOfWork.LookerStudioCredentialRepository.GetActiveCredentialAsync()
            ?? throw new NotFoundException("No active Looker Studio credentials found. Please update credentials in Admin panel.");

        if (credential.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Looker Studio credentials expired on {expiresAt}", credential.ExpiresAt);
            throw new UnauthorizedUserException("Looker Studio credentials have expired. Please update credentials in Admin panel.");
        }

        try
        {
            // First attempt
            try
            {
                return await FetchDataFromLookerStudioAsync(credential, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Received 401 Unauthorized. Attempting token refresh...");
                
                // Try to refresh token
                bool refreshed = await RefreshTokenAsync(credential, cancellationToken);
                
                if (!refreshed)
                {
                    _logger.LogError("Token refresh failed. Cannot proceed.");
                    throw new UnauthorizedUserException("Looker Studio session expired and could not be refreshed. Please update credentials in Admin panel.");
                }
                
                // Retry after refresh
                _logger.LogInformation("Token refreshed successfully. Retrying data fetch...");
                return await FetchDataFromLookerStudioAsync(credential, cancellationToken);
            }
        }
        catch (UnauthorizedUserException)
        {
            throw;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading priority data from Looker Studio");
            throw new BusinessException("Failed to download priority data from Looker Studio. " + ex.Message);
        }
    }

    private async Task<bool> RefreshTokenAsync(Domain.Core.Entities.LookerStudioCredential credential, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(credential.SessionId))
            {
                _logger.LogWarning("SessionId not available. Cannot refresh token.");
                return false;
            }

            var cookies = _encryptionService.Decrypt(credential.EncryptedCookieData);
            
            var refreshUrl = "https://lookerstudio.google.com/u/0/getFirebaseAuthToken?appVersion=20260131_0900";
            var request = new HttpRequestMessage(HttpMethod.Post, refreshUrl);
            
            // Add headers
            request.Headers.Add("accept", "application/json, text/plain, */*");
            request.Headers.Add("accept-language", "en-US,en;q=0.9,pt;q=0.8");
            request.Headers.Add("cookie", cookies);
            request.Headers.Add("origin", "https://lookerstudio.google.com");
            request.Headers.Add("referer", $"https://lookerstudio.google.com/u/0/reporting/{credential.ReportId}/page/p_bpbqbqp9ud");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36");
            request.Headers.Add("x-rap-xsrf-token", credential.XsrfToken);
            
            // Build refresh request body
            var requestBody = new
            {
                reportId = credential.ReportId,
                sessionId = credential.SessionId,
                datasourceId = new[] { credential.DatasourceId }
            };
            
            var jsonBody = JsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending token refresh request to Looker Studio");
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var refreshContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Token refresh successful. Response length: {length}", refreshContent.Length);
                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed with status: {status}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return false;
        }
    }

    private async Task<string> FetchDataFromLookerStudioAsync(Domain.Core.Entities.LookerStudioCredential credential, CancellationToken cancellationToken)
    {
        var cookies = _encryptionService.Decrypt(credential.EncryptedCookieData);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_batchDataUrl}?appVersion=20260131_0900");
        
        // Add headers
        request.Headers.Add("accept", "application/json, text/plain, */*");
        request.Headers.Add("accept-language", "en-US,en;q=0.9,pt;q=0.8");
        request.Headers.Add("cookie", cookies);
        request.Headers.Add("origin", "https://lookerstudio.google.com");
        request.Headers.Add("referer", $"https://lookerstudio.google.com/u/0/reporting/{credential.ReportId}/page/p_bpbqbqp9ud");
        request.Headers.Add("sec-fetch-dest", "empty");
        request.Headers.Add("sec-fetch-mode", "cors");
        request.Headers.Add("sec-fetch-site", "same-origin");
        request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36");
        request.Headers.Add("x-rap-xsrf-token", credential.XsrfToken);

        // Build request body (batchedDataV2 format)
        var requestBody = new
        {
            dataRequest = new object[]
            {
                new
                {
                    requestContext = new
                    {
                        reportContext = new
                        {
                            reportId = credential.ReportId,
                            pageId = "p_bpbqbqp9ud",
                            mode = 1,
                            componentId = "cd-9obqbqp9ud",
                            displayType = "simple-table"
                        },
                        requestMode = 0
                    },
                    datasetSpec = new
                    {
                        dataset = new object[]
                        {
                            new
                            {
                                datasourceId = credential.DatasourceId,
                                revisionNumber = 0,
                                parameterOverrides = new object[] { }
                            }
                        },
                        queryFields = new object[]
                        {
                            new { name = "qt_5a80lg6w8c", datasetNs = "d0", tableNs = "t0", dataTransformation = new { sourceFieldName = "calc_ghhj7l5w8c" } },
                            new { name = "qt_iktxej5w8c", datasetNs = "d0", tableNs = "t0", dataTransformation = new { sourceFieldName = "_446936040_" } },
                            new { name = "qt_6kzvc8r0wd", datasetNs = "d0", tableNs = "t0", dataTransformation = new { sourceFieldName = "_n582882639_" } }
                        },
                        sortData = new object[]
                        {
                            new { sortColumn = new { name = "qt_dicimfp8ud", datasetNs = "d0", tableNs = "t0", dataTransformation = new { sourceFieldName = "_n268175732_", aggregation = 5 } }, sortDir = 0 },
                            new { sortColumn = new { name = "qt_5a80lg6w8c", datasetNs = "d0", tableNs = "t0", dataTransformation = new { sourceFieldName = "calc_ghhj7l5w8c" } }, sortDir = 0 }
                        },
                        includeRowsCount = true,
                        paginateInfo = new { startRow = 1, rowsCount = 5000 },
                        dsFilterOverrides = new object[] { },
                        filters = new object[]
                        {
                            new
                            {
                                filterDefinition = new
                                {
                                        and = new
                                        {
                                            filterDefinitions = new object[]
                                            {
                                                new
                                                {
                                                    filterExpression = new
                                                    {
                                                        include = true,
                                                        conceptType = 0,
                                                        concept = new { ns = "t0", name = "qt_dur8sq3w8c" },
                                                        filterConditionType = "IN",
                                                        stringValues = new[] { "Active", "PreFilled", "Selected Candidate" },
                                                        numberValues = new object[] { },
                                                        queryTimeTransformation = new { dataTransformation = new { sourceFieldName = "_396120777_" } }
                                                    }
                                                },
                                                new
                                                {
                                                    filterExpression = new
                                                    {
                                                        include = true,
                                                        conceptType = 0,
                                                        concept = new { ns = "t0", name = "qt_07jpiso0qd" },
                                                        filterConditionType = "EQ",
                                                        stringValues = new[] { "Yes" },
                                                        numberValues = new object[] { },
                                                        queryTimeTransformation = new { dataTransformation = new { sourceFieldName = "_1789582081_" } }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    dataSubsetNs = new { datasetNs = "d0", tableNs = "t0", contextNs = "c0" },
                                    version = 3
                                }
                            },
                            features = new object[] { },
                            dateRanges = new object[] { },
                            contextNsCount = 1,
                            calculatedField = new object[] { },
                            needGeocoding = false,
                            geoFieldMask = new object[] { },
                            multipleGeocodeFields = new object[] { },
                            timezone = "America/Fortaleza"
                        },
                        role = "main",
                        retryHints = new
                        {
                            useClientControlledRetry = true,
                            isLastRetry = false,
                            retryCount = 0,
                            originalRequestId = "cd-9obqbqp9ud_0_0"
                        }
                    }
                }
            };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to Looker Studio batchedDataV2 API");

        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Looker Studio API error. Status: {status}, Response: {response}", 
                response.StatusCode, errorContent);
            
            // Throw specific exception for 401 to trigger retry
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Unauthorized", null, System.Net.HttpStatusCode.Unauthorized);
            }
            
            throw new BusinessException($"Looker Studio API returned error: {response.StatusCode}");
        }

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Remove the )]}' prefix that Google uses for security
        if (rawContent.StartsWith(")]}"))
        {
            rawContent = rawContent.Substring(4);
        }

        _logger.LogDebug("Received response from Looker Studio. Length: {length}", rawContent.Length);

        // Parse JSON and convert to CSV
        var csvContent = ConvertJsonToCsv(rawContent);

        _logger.LogInformation("Successfully converted Looker Studio data to CSV. Rows: {rows}", csvContent.Split('\n').Length - 1);
        _logger.LogDebug("CSV preview (first 500 chars): {preview}", 
            csvContent.Length > 500 ? csvContent.Substring(0, 500) : csvContent);

        return csvContent;
    }

    private string ConvertJsonToCsv(string jsonContent)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonContent);
            var root = jsonDoc.RootElement;

            // Navigate to the data: dataResponse[0].dataSubset[0].dataset.tableDataset
            var dataResponse = root.GetProperty("dataResponse");
            var firstResponse = dataResponse[0];
            var dataSubset = firstResponse.GetProperty("dataSubset");
            var firstSubset = dataSubset[0];
            var dataset = firstSubset.GetProperty("dataset");
            var tableDataset = dataset.GetProperty("tableDataset");
            
            var columns = tableDataset.GetProperty("column");
            var columnCount = columns.GetArrayLength();
            
            if (columnCount < 3)
            {
                throw new BusinessException("Unexpected data format: Expected at least 3 columns");
            }

            // Extract column data (columnar format)
            var roleColumn = columns[0].GetProperty("stringColumn").GetProperty("values");
            var priorityColumn = columns[1].GetProperty("stringColumn").GetProperty("values");
            var countryColumn = columns[2].GetProperty("stringColumn").GetProperty("values");

            var roleValues = roleColumn.EnumerateArray().Select(v => v.GetString() ?? "").ToList();
            var priorityValues = priorityColumn.EnumerateArray().Select(v => v.GetString() ?? "").ToList();
            var countryValues = countryColumn.EnumerateArray().Select(v => v.GetString() ?? "").ToList();

            var rowCount = roleValues.Count;
            _logger.LogInformation("Extracted {rowCount} rows from Looker Studio data", rowCount);

            // Build CSV
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Role,Priority,CountryRestrictions");

            for (int i = 0; i < rowCount; i++)
            {
                // Extract job code from role (e.g., "Python Engineer - Job8903  https://...")
                var roleFull = roleValues[i];
                var jobCode = ExtractJobCode(roleFull);
                
                var priority = priorityValues[i];
                var country = EscapeCsvValue(countryValues[i]);

                csvBuilder.AppendLine($"{jobCode},{priority},{country}");
            }

            return csvBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Looker Studio JSON response");
            throw new BusinessException("Failed to parse Looker Studio response. " + ex.Message);
        }
    }

    private string ExtractJobCode(string roleString)
    {
        // Extract job code from format: "Python Engineer - Job8903  https://..."
        // We want just "Job8903"
        
        var jobIndex = roleString.IndexOf("Job", StringComparison.OrdinalIgnoreCase);
        if (jobIndex == -1)
        {
            _logger.LogWarning("Could not find job code in role string: {role}", roleString);
            return roleString.Split(' ')[0]; // Return first word as fallback
        }

        var afterJob = roleString.Substring(jobIndex);
        var parts = afterJob.Split(new[] { ' ', '\u00a0', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        return parts.Length > 0 ? parts[0] : roleString;
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    public async Task<bool> ValidateCredentialsAsync()
    {
        try
        {
            var csv = await DownloadPriorityCsvAsync(CancellationToken.None);
            return !string.IsNullOrWhiteSpace(csv) && csv.Contains("Job");
        }
        catch (UnauthorizedUserException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Looker Studio credentials");
            return false;
        }
    }
}

