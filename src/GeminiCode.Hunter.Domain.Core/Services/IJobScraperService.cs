using GeminiCode.Hunter.Domain.Core.Models.External.ApplyJob;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IJobScraperService
{
    Task<List<ScrapedJobPosition>> ScrapeJobsAsync();
    Task<ScrapedJobDetails> ScrapeJobDetailsAsync(string url);
}
