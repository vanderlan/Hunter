using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;
using static GeminiCode.Hunter.Domain.Core.Constants.DomainConstants;

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
        List<string> toEmails,
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

            if (!toEmails.Any())
            {
                _logger.LogInformation("No users to notify (all notifications disabled)");
                return;
            }

            _logger.LogInformation("Sending job changes notification email to {Count} recipients individually", toEmails.Count);

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

            // Send separate email to each recipient to maintain privacy
            var successCount = 0;
            var failureCount = 0;

            foreach (var email in toEmails)
            {
                try
                {
                    var mail = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, "Hunter Job Monitor"),
                        Subject = $"Hunter Job Alert – {newJobs.Count} New, {closedJobs.Count} Closed",
                        Body = htmlBody,
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8,
                        SubjectEncoding = Encoding.UTF8
                    };

                    mail.To.Add(email);

                    using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                    {
                        Credentials = new NetworkCredential(_fromEmail, _password),
                        EnableSsl = _enableSsl
                    };

                    await smtp.SendMailAsync(mail);
                    successCount++;
                    _logger.LogDebug("Job changes notification sent successfully to {Email}", email);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex, "Failed to send job changes notification to {Email}", email);
                }
            }

            _logger.LogInformation("Job changes notification completed: {SuccessCount} succeeded, {FailureCount} failed out of {TotalCount} total", 
                successCount, failureCount, toEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job changes notification emails");
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
                Subject = $"Hunter Candidate Matches - {totalMatches} Matches for {positions.Count} Position(s)",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
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
                Subject = "Welcome to Hunter!",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
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

    public async Task SendCandidateReferralNotificationAsync(
        List<string> toEmails,
        string candidateName,
        string candidateEmail,
        string jobTitle,
        string jobCode,
        string referredByUserName,
        string? referralReason)
    {
        try
        {
            if (!toEmails.Any())
            {
                _logger.LogInformation("No admin users to notify about candidate referral");
                return;
            }

            _logger.LogInformation("Sending candidate referral notification to {Count} admins", toEmails.Count);

            var templateData = new Dictionary<string, object>
            {
                ["CandidateName"] = candidateName,
                ["CandidateEmail"] = candidateEmail,
                ["JobTitle"] = jobTitle,
                ["JobCode"] = jobCode,
                ["ReferredByUserName"] = referredByUserName,
                ["ReferralReason"] = referralReason ?? "",
                ["HasReferralReason"] = !string.IsNullOrWhiteSpace(referralReason),
                ["ApplicationUrl"] = _applicationUrl
            };

            var htmlBody = _templateService.RenderCandidateReferralTemplate(templateData);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter"),
                Subject = $"New Candidate Referral - {candidateName} for {jobTitle}",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            foreach (var email in toEmails)
            {
                mail.To.Add(email);
            }

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Candidate referral notification sent successfully to {Emails}", 
                string.Join(", ", toEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send candidate referral notification");
        }
    }

    public async Task SendStatusChangeNotificationAsync(
        string toEmail,
        string candidateName,
        string jobTitle,
        string oldStatus,
        string newStatus,
        string changedByUserName,
        string? notes)
    {
        try
        {
            _logger.LogInformation("Sending status change notification to {Email}", toEmail);

            var templateData = new Dictionary<string, object>
            {
                ["CandidateName"] = candidateName,
                ["JobTitle"] = jobTitle,
                ["OldStatus"] = FormatStatusName(oldStatus),
                ["NewStatus"] = FormatStatusName(newStatus),
                ["ChangedByUserName"] = changedByUserName,
                ["Notes"] = notes ?? "",
                ["HasNotes"] = !string.IsNullOrWhiteSpace(notes),
                ["ApplicationUrl"] = _applicationUrl
            };

            var htmlBody = _templateService.RenderStatusChangeTemplate(templateData);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter"),
                Subject = $"Status Update - {candidateName} moved to {FormatStatusName(newStatus)}",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Status change notification sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status change notification");
        }
    }

    public async Task SendNoteAddedNotificationAsync(
        string toEmail,
        string candidateName,
        string jobTitle,
        string noteText,
        string addedByUserName)
    {
        try
        {
            _logger.LogInformation("Sending note added notification to {Email}", toEmail);

            var templateData = new Dictionary<string, object>
            {
                ["CandidateName"] = candidateName,
                ["JobTitle"] = jobTitle,
                ["NoteText"] = noteText,
                ["AddedByUserName"] = addedByUserName,
                ["ApplicationUrl"] = _applicationUrl
            };

            var htmlBody = _templateService.RenderNoteAddedTemplate(templateData);

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Hunter"),
                Subject = $"New Note Added - {candidateName}",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Note added notification sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send note added notification");
        }
    }

    private static string FormatStatusName(string status)
    {
        // Convert from enum name to display name
        return status switch
        {
            "ConsultantTechnicalInterview" => "Consultant Technical Interview",
            "ClientInterview" => "Client Interview",
            _ => status
        };
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
        return percentage >= AIMatching.HighMatchPercentage 
            ? "high" 
            : percentage >= AIMatching.MediumMatchPercentage 
            ? "medium" 
            : "low";
    }
}
