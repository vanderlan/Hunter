using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _toEmail;
    private readonly string _password;
    private readonly bool _enableSsl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        
        var emailConfig = configuration.GetSection("Email");
        _smtpHost = emailConfig["SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
        _fromEmail = emailConfig["FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail is required");
        _toEmail = emailConfig["ToEmail"] ?? _fromEmail;
        _password = emailConfig["Password"] ?? throw new InvalidOperationException("Email:Password is required");
        _enableSsl = bool.Parse(emailConfig["EnableSsl"] ?? "true");
    }

    public async Task SendJobChangesNotificationAsync(
        List<string> newJobs,
        List<string> closedJobs,
        DateTime syncTime)
    {
        try
        {
            if (!newJobs.Any() && !closedJobs.Any())
            {
                _logger.LogDebug("No job changes to notify");
                return;
            }

            _logger.LogInformation("Sending job changes notification email");

            var body = BuildJobChangesEmailBody(newJobs, closedJobs, syncTime);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter Job Monitor"),
                Subject = $"Hunter Job Alert – {newJobs.Count} New, {closedJobs.Count} Closed",
                Body = body,
                IsBodyHtml = false
            };

            mail.To.Add(_toEmail);

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Job changes notification email sent successfully to {Email}", _toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job changes notification email");
        }
    }

    public async Task SendCandidateMatchingNotificationAsync(
        object matchResults,
        DateTime syncTime)
    {
        try
        {
            _logger.LogInformation("Sending candidate matching notification email");

            // Deserialize matchResults to get details
            var matchData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(matchResults));
            
            var body = BuildCandidateMatchingEmailBody(matchResults, syncTime);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter CV Matcher"),
                Subject = "Hunter Candidate Matches for New Positions",
                Body = body,
                IsBodyHtml = false
            };

            mail.To.Add(_toEmail);

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Candidate matching notification email sent successfully to {Email}", _toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send candidate matching notification email");
        }
    }

    private static string BuildJobChangesEmailBody(
        List<string> newJobs,
        List<string> closedJobs,
        DateTime syncTime)
    {
        var body = new StringBuilder();

        body.AppendLine("=======================================");
        body.AppendLine("     HUNTER JOB CHANGES DETECTED");
        body.AppendLine("=======================================");
        body.AppendLine();
        body.AppendLine($"Sync Time: {syncTime:yyyy-MM-dd HH:mm:ss}");
        body.AppendLine();

        if (newJobs.Any())
        {
            body.AppendLine("=== NEW POSITIONS OPENED ===");
            body.AppendLine($"Total: {newJobs.Count}");
            body.AppendLine();
            
            foreach (var job in newJobs)
            {
                body.AppendLine($"  ✓ {job}");
            }
            body.AppendLine();
        }

        if (closedJobs.Any())
        {
            body.AppendLine("=== POSITIONS CLOSED ===");
            body.AppendLine($"Total: {closedJobs.Count}");
            body.AppendLine();
            
            foreach (var job in closedJobs)
            {
                body.AppendLine($"  ✗ {job}");
            }
            body.AppendLine();
        }

        body.AppendLine("=======================================");
        body.AppendLine();
        body.AppendLine("This is an automated notification from Hunter CV Analyzer.");
        body.AppendLine("Check your email for candidate matches.");

        return body.ToString();
    }

    private static string BuildCandidateMatchingEmailBody(object matchResults, DateTime syncTime)
    {
        var body = new StringBuilder();
        var resultsJson = JsonConvert.SerializeObject(matchResults, Formatting.Indented);
        var results = JsonConvert.DeserializeObject<List<dynamic>>(resultsJson);

        body.AppendLine("=======================================");
        body.AppendLine("  CANDIDATE MATCHES FOR NEW POSITIONS");
        body.AppendLine("=======================================");
        body.AppendLine();
        body.AppendLine($"Analysis Time: {syncTime:yyyy-MM-dd HH:mm:ss}");
        
        if (results != null && results.Any())
        {
            body.AppendLine($"New Positions Analyzed: {results.Count}");
            
            int totalMatches = 0;
            foreach (var result in results)
            {
                totalMatches += (int)result.TotalMatches;
            }
            body.AppendLine($"Total Candidate Matches: {totalMatches}");
            body.AppendLine();

            foreach (var result in results)
            {
                body.AppendLine("=======================================");
                body.AppendLine($"Position: {result.JobTitle}");
                body.AppendLine($"Job Code: {result.JobCode}");
                body.AppendLine($"Company: {result.CompanyCode}");
                body.AppendLine($"Matching Candidates: {result.TotalMatches}");
                body.AppendLine();

                var candidates = result.Candidates as IEnumerable<dynamic>;
                if (candidates != null)
                {
                    int count = 0;
                    foreach (var candidate in candidates)
                    {
                        if (count >= 10) break; // Top 10 per position
                        
                        body.AppendLine($"  ✓ {candidate.CandidateName} - {candidate.MatchPercentage}%");
                        body.AppendLine($"    Email: {candidate.CandidateEmail}");
                        if (candidate.CandidatePhone != null)
                        {
                            body.AppendLine($"    Phone: {candidate.CandidatePhone}");
                        }
                        body.AppendLine($"    Match Reason: {candidate.MatchReason}");
                        body.AppendLine();
                        
                        count++;
                    }

                    if ((int)result.TotalMatches > 10)
                    {
                        body.AppendLine($"  ... and {(int)result.TotalMatches - 10} more candidates");
                        body.AppendLine();
                    }
                }
            }
        }

        body.AppendLine("=======================================");
        body.AppendLine();
        body.AppendLine("This is an automated notification from Hunter CV Analyzer.");
        body.AppendLine("Contact candidates to schedule interviews.");

        return body.ToString();
    }
}
