using System.Collections.Generic;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface ICsvParserService
{
    List<JobPriorityDto> ParsePriorityCsv(string csvContent);
}

public record JobPriorityDto(string JobCode, string Priority, string CountryRestrictions);