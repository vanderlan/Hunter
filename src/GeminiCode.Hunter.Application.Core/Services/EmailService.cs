using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService _templateService;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly List<string> _toEmails;
    private readonly string _password;
    private readonly bool _enableSsl;
    private readonly string _applicationUrl;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IEmailTemplateService templateService)
    {
        _logger = logger;
        _templateService = templateService;
        
        var emailConfig = configuration.GetSection("Email");
        _smtpHost = emailConfig["SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
        _fromEmail = emailConfig["FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail is required");
        
        // Support both single email (ToEmail) and multiple emails (ToEmails)
        var toEmailsConfig = emailConfig.GetSection("ToEmails").Get<List<string>>();
        if (toEmailsConfig != null && toEmailsConfig.Any())
        {
            _toEmails = toEmailsConfig;
        }
        else
        {
            var singleEmail = emailConfig["ToEmail"];
            _toEmails = !string.IsNullOrEmpty(singleEmail) 
                ? [singleEmail] 
                : [_fromEmail];
        }
        
        _password = emailConfig["Password"] ?? throw new InvalidOperationException("Email:Password is required");
        _enableSsl = bool.Parse(emailConfig["EnableSsl"] ?? "true");
        _applicationUrl = emailConfig["ApplicationUrl"] ?? "https://hunter.geminicode.com.br";
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

            _logger.LogInformation("Sending job changes notification email to {Count} recipients", _toEmails.Count);

            var templateData = new Dictionary<string, object>
            {
                ["SyncTime"] = syncTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["NewJobsCount"] = newJobs.Count,
                ["ClosedJobsCount"] = closedJobs.Count,
                ["HasNewJobs"] = newJobs.Any(),
                ["HasClosedJobs"] = closedJobs.Any(),
                ["HasChanges"] = newJobs.Any() || closedJobs.Any(),
                ["NewJobs"] = newJobs.Cast<object>().ToList(),
                ["ClosedJobs"] = closedJobs.Cast<object>().ToList()
            };

            var htmlBody = _templateService.RenderJobChangesTemplate(templateData);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter Job Monitor"),
                Subject = $"Hunter Job Alert – {newJobs.Count} New, {closedJobs.Count} Closed",
                Body = htmlBody,
                IsBodyHtml = true
            };

            foreach (var email in _toEmails)
            {
                mail.To.Add(email);
            }

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Job changes notification email sent successfully to {Emails}", string.Join(", ", _toEmails));
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
            _logger.LogInformation("Sending candidate matching notification email to {Count} recipients", _toEmails.Count);

            var positions = ConvertToPositionsList(matchResults);

            if (!positions.Any())
            {
                _logger.LogWarning("No matching results to send");
                return;
            }

            var totalMatches = positions.Sum(p => p["TotalMatches"] as int? ?? 0);

            var templateData = new Dictionary<string, object>
            {
                ["AnalysisTime"] = syncTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["PositionsCount"] = positions.Count,
                ["TotalMatches"] = totalMatches,
                ["Positions"] = positions.Cast<object>().ToList()
            };

            var htmlBody = _templateService.RenderCandidateMatchesTemplate(templateData);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter CV Matcher"),
                Subject = $"🎯 Hunter Candidate Matches – {totalMatches} Matches for {positions.Count} Position(s)",
                Body = htmlBody,
                IsBodyHtml = true
            };

            foreach (var email in _toEmails)
            {
                mail.To.Add(email);
            }

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Candidate matching notification email sent successfully to {Emails}",
                string.Join(", ", _toEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send candidate matching notification email");
        }
    }

    public async Task SendWelcomeEmailAsync(
        string userName,
        string userEmail,
        string password)
    {
        try
        {
            _logger.LogInformation("Sending welcome email to new user");

            var templateData = new Dictionary<string, object>
            {
                ["UserName"] = userName,
                ["UserEmail"] = userEmail,
                ["Password"] = password,
                ["CurrentYear"] = DateTime.UtcNow.Year,
                ["LoginUrl"] = _applicationUrl
            };

            var htmlBody = _templateService.RenderWelcomeEmailTemplate(templateData);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter"),
                Subject = "Welcome to Hunter! 🎯",
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(userEmail);

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Welcome email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email");
        }
    }

    private List<Dictionary<string, object>> ConvertToPositionsList(object matchResults)
    {
        var positions = new List<Dictionary<string, object>>();

        if (matchResults is IEnumerable<object> results)
        {
            foreach (var result in results)
            {
                var positionDict = new Dictionary<string, object>();
                var resultType = result.GetType();

                foreach (var prop in resultType.GetProperties())
                {
                    var value = prop.GetValue(result);
                    
                    if (prop.Name == "Candidates" && value is IEnumerable<object> candidates)
                    {
                        var candidatesList = new List<Dictionary<string, object>>();
                        var candidatesArray = candidates.Take(10).ToList();

                        foreach (var candidate in candidatesArray)
                        {
                            var candidateDict = new Dictionary<string, object>();
                            var candidateType = candidate.GetType();

                            foreach (var candidateProp in candidateType.GetProperties())
                            {
                                var candidateValue = candidateProp.GetValue(candidate);
                                
                                if (candidateProp.Name == "MatchPercentage" && candidateValue is double matchPct)
                                {
                                    candidateDict["MatchPercentage"] = $"{matchPct:F0}";
                                    candidateDict["MatchLevel"] = GetMatchLevel(matchPct);
                                }
                                else
                                {
                                    candidateDict[candidateProp.Name] = candidateValue;
                                }
                            }

                            candidatesList.Add(candidateDict);
                        }

                        positionDict["Candidates"] = candidatesList.Cast<object>().ToList();
                        
                        var totalMatches = (int)(resultType.GetProperty("TotalMatches")?.GetValue(result) ?? 0);
                        positionDict["HasMoreCandidates"] = totalMatches > 10;
                        positionDict["MoreCandidatesCount"] = Math.Max(0, totalMatches - 10);
                    }
                    else
                    {
                        positionDict[prop.Name] = value;
                    }
                }

                positions.Add(positionDict);
            }
        }

        return positions;
    }

    private static string GetMatchLevel(double percentage)
    {
        return percentage >= 70 ? "high" : percentage >= 50 ? "medium" : "low";
    }
}
