using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models.External.ApplyJob;
using HtmlAgilityPack;

namespace GeminiCode.Hunter.Application.Core.Services;

public class JobScraperService(IConfiguration configuration, ILogger<JobScraperService> logger) : IJobScraperService
{
    private readonly string _careersUrl = configuration["JobScraper:CareersUrl"] 
        ?? throw new InvalidOperationException("JobScraper:CareersUrl configuration is missing");

    public async Task<List<ScrapedJobPosition>> ScrapeJobsAsync()
    {
        try
        {
            logger.LogInformation("Starting job scraping from {Url}", _careersUrl);

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(_careersUrl);

            var nodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'/apply/')]");

            if (nodes == null)
            {
                logger.LogWarning("No job links found");
                return [];
            }

            var positions = nodes
                .Select(n => new
                {
                    Title = WebUtility.HtmlDecode(n.InnerText.Trim()),
                    Url = n.GetAttributeValue("href", "").Trim()
                })
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Title) &&
                    !x.Title.Equals("View All Jobs", StringComparison.OrdinalIgnoreCase) &&
                    IsRealJobUrl(x.Url)
                )
                .Select(x => ParseJobPosition(x.Title, x.Url))
                .Where(x => x != null)
                .DistinctBy(p => p!.Url)
                .Cast<ScrapedJobPosition>()
                .ToList();

            logger.LogInformation("Scraped {Count} job positions", positions.Count);

            return positions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scraping jobs from {Url}", _careersUrl);
            throw;
        }
    }

    public async Task<ScrapedJobDetails> ScrapeJobDetailsAsync(string url)
    {
        try
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var bodyNode = doc.DocumentNode.SelectSingleNode("//body") 
                ?? doc.DocumentNode;

            var rawHtml = bodyNode.InnerHtml;

            return new ScrapedJobDetails(RawHtmlContent: rawHtml);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scraping job details from {Url}", url);
            throw;
        }
    }

    private static bool IsRealJobUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 3 && segments[0] == "apply";
    }

    private static ScrapedJobPosition? ParseJobPosition(string title, string url)
    {
        try
        {
            // Extract company code and job code from title
            // Format examples: 
            //   "CompanyCode - Title - JobCodeXXXX"
            //   "MS- Ssr. Data Engineer B. - Job6642" (company code without space before dash)

            // Find the first dash (with or without surrounding spaces)
            var firstDashIndex = title.IndexOf('-');
            if (firstDashIndex == -1)
                return null;

            // Extract company code (everything before the first dash, trimmed)
            var companyCode = title.Substring(0, firstDashIndex).Trim();

            // Limit to 10 characters to match database constraint
            if (companyCode.Length > 10)
                companyCode = companyCode.Substring(0, 10);

            // Split by " - " to get the last part for job code
            var parts = title.Split(" - ", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return null;

            var jobCodePart = parts[^1].Trim(); // Last part

            // Extract job code (e.g., "Job2495" from "Identity - Job2495")
            var jobCode = jobCodePart.Contains("Job") 
                ? jobCodePart.Split(' ').LastOrDefault(p => p.StartsWith("Job"))?.Trim() ?? jobCodePart
                : jobCodePart;

            return new ScrapedJobPosition(title, url, companyCode, jobCode);
        }
        catch
        {
            return null;
        }
    }
}
