using System.Net;
using System.Text;
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
            logger.LogInformation("Scraping job details from {Url}", url);

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var root = doc.DocumentNode;

            var details = new ScrapedJobDetails(
                Summary: ExtractSection(root, "Summary"),
                Responsibilities: ExtractSection(root, "Responsibilities"),
                Requirements: ExtractSection(root, "Requirements"),
                MustHaveSkills: ExtractSection(root, "Must-Have Skills"),
                NiceToHaveSkills: ExtractSection(root, "Nice-to-Have Skills")
            );

            logger.LogInformation("Successfully scraped job details from {Url}", url);

            return details;
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
            // Format: "CompanyCode - Title - JobCodeXXXX"
            var parts = title.Split(" - ", StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 2)
                return null;

            var companyCode = parts[0].Trim();
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

    private string ExtractSection(HtmlNode root, string title)
    {
        try
        {
            var header = root.Descendants()
                .FirstOrDefault(n => n.InnerText.Trim().Equals(title, StringComparison.OrdinalIgnoreCase));

            if (header == null)
            {
                logger.LogDebug("Section not found: {Title}", title);
                return string.Empty;
            }

            var sb = new StringBuilder();
            var node = header.NextSibling;

            // List of known section headers to stop at
            var sectionHeaders = new[] { "Summary", "Responsibilities", "Requirements", "Must-Have Skills", "Nice-to-Have Skills", "Nice to Have" };

            while (node != null)
            {
                var text = node.InnerText.Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Stop if we hit another major section header (h2)
                    if (node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
                    {
                        var nodeText = WebUtility.HtmlDecode(text);
                        if (sectionHeaders.Any(h => nodeText.Equals(h, StringComparison.OrdinalIgnoreCase)))
                        {
                            break;
                        }
                      }
                    

                    // Include h3 subsection headers as part of the content
                    if (node.Name.Equals("h3", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"## {WebUtility.HtmlDecode(text)}");
                        sb.AppendLine();
                    }
                    // Stop at short uppercase text that looks like a new section (but not h3)
                    else if (text.Length < 40 && char.IsUpper(text[0]) && !node.Name.Equals("li", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if it's a known section header
                        if (sectionHeaders.Any(h => text.Equals(h, StringComparison.OrdinalIgnoreCase)))
                        {
                            break;
                        }
                        sb.AppendLine(WebUtility.HtmlDecode(text));
                    }
                    else
                    {
                        sb.AppendLine(WebUtility.HtmlDecode(text));
                    }
                }

                node = node.NextSibling;
            }

            return sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting section: {Title}", title);
            return string.Empty;
        }
    }
}
