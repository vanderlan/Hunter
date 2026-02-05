using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.Services;

public record JobPriorityDto(string JobCode, string Priority, string CountryRestrictions);

public interface ICsvParserService
{
    List<JobPriorityDto> ParsePriorityCsv(string csvContent);
}

public class CsvParserService(ILogger<CsvParserService> logger) : ICsvParserService
{
    private readonly ILogger<CsvParserService> _logger = logger;

    public List<JobPriorityDto> ParsePriorityCsv(string csvContent)
    {
        var priorities = new List<JobPriorityDto>();

        try
        {
            using var reader = new StringReader(csvContent);
            
            // Skip header line
            var headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine))
            {
                throw new InvalidOperationException("CSV file is empty");
            }

            _logger.LogInformation("Parsing CSV with header: {header}", headerLine);

            string? line;
            int lineNumber = 1;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var priority = ParseCsvLine(line);
                    if (priority != null)
                    {
                        priorities.Add(priority);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing line {lineNumber}: {line}", lineNumber, line);
                }
            }

            _logger.LogInformation("Successfully parsed {count} priorities from CSV", priorities.Count);

            return priorities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CSV content");
            throw;
        }
    }

    private JobPriorityDto? ParseCsvLine(string line)
    {
        // Parse CSV line handling quotes properly
        var fields = ParseCsvFields(line);

        if (fields.Count < 3)
        {
            _logger.LogWarning("Line has less than 3 fields, skipping: {line}", line);
            return null;
        }

        var role = fields[0];
        var priority = fields[1];
        var countryRestrictions = fields[2];

        // Extract job code from role (e.g., "Python Engineer with AWS B. - Job8903" -> "Job8903")
        var jobCodeMatch = Regex.Match(role, @"Job\d+", RegexOptions.IgnoreCase);
        
        if (!jobCodeMatch.Success)
        {
            _logger.LogWarning("Could not extract job code from role: {role}", role);
            return null;
        }

        var jobCode = jobCodeMatch.Value;

        return new JobPriorityDto(jobCode, priority, countryRestrictions);
    }

    private List<string> ParseCsvFields(string line)
    {
        var fields = new List<string>();
        var currentField = string.Empty;
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentField += '"';
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = string.Empty;
            }
            else
            {
                currentField += c;
            }
        }

        // Add last field
        fields.Add(currentField);

        return fields;
    }
}
