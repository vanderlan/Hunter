using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Domain.Core.Models;
using static GeminiCode.Hunter.Domain.Core.Constants.DomainConstants;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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
    private readonly string _smtpLogin;

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
        
        // For SMTP providers like Brevo that use different login than FromEmail
        _smtpLogin = emailConfig["SmtpLogin"] ?? _fromEmail;
    }

    public async Task SendJobChangesNotificationAsync(
        List<string> toEmails,
        List<string> newJobs,
        List<string> closedJobs,
        List<JobPositionEmailData> newJobPositions,
        List<JobPositionEmailData> closedJobPositions,
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
                ["ClosedJobs"] = closedJobs.Cast<object>().ToList(),
                ["ApplicationUrl"] = _applicationUrl,
                ["NewJobPositions"] = newJobPositions.Select(j => new Dictionary<string, object>
                {
                    ["PublicId"] = j.PublicId,
                    ["Title"] = j.Title,
                    ["JobCode"] = j.JobCode,
                    ["CompanyCode"] = j.CompanyCode,
                    ["Url"] = $"{_applicationUrl}/open-positions?jobId={j.PublicId}"
                }).Cast<object>().ToList(),
                ["ClosedJobPositions"] = closedJobPositions.Select(j => new Dictionary<string, object>
                {
                    ["PublicId"] = j.PublicId,
                    ["Title"] = j.Title,
                    ["JobCode"] = j.JobCode,
                    ["CompanyCode"] = j.CompanyCode,
                    ["Url"] = $"{_applicationUrl}/open-positions?jobId={j.PublicId}"
                }).Cast<object>().ToList()
            };

            var htmlBody = _templateService.RenderJobChangesTemplate(templateData);

            // Send separate email to each recipient to maintain privacy
            var successCount = 0;
            var failureCount = 0;

            foreach (var email in toEmails)
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Hunter Job Monitor", _fromEmail));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = $"Hunter Job Alert – {newJobs.Count} New, {closedJobs.Count} Closed";
                    
                    var bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = htmlBody
                    };
                    message.Body = bodyBuilder.ToMessageBody();

                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_smtpLogin, _password);
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);
                    
                    successCount++;
                    _logger.LogDebug("Job changes notification sent successfully to {Email}", email);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex, "Failed to send job changes notification to {Email}. Error: {ErrorMessage}. InnerException: {InnerException}", 
                        email, ex.Message, ex.InnerException?.Message ?? "None");
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hunter CV Matcher", _fromEmail));
            foreach (var email in _toEmails)
            {
                message.To.Add(new MailboxAddress("", email));
            }
            message.Subject = $"Hunter Candidate Matches - {totalMatches} Matches for {positions.Count} Position(s)";
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpLogin, _password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Candidate matching notification email sent successfully to {Emails}", string.Join(", ", _toEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send candidate matching notification email. Error: {ErrorMessage}. InnerException: {InnerException}", 
                ex.Message, ex.InnerException?.Message ?? "None");
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hunter", _fromEmail));
            message.To.Add(new MailboxAddress("", userEmail));
            message.Subject = "Welcome to Hunter!";
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpLogin, _password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Welcome email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email. Error: {ErrorMessage}. InnerException: {InnerException}", 
                ex.Message, ex.InnerException?.Message ?? "None");
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hunter", _fromEmail));
            foreach (var email in toEmails)
            {
                message.To.Add(new MailboxAddress("", email));
            }
            message.Subject = $"New Candidate Referral - {candidateName} for {jobTitle}";
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpLogin, _password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Candidate referral notification sent successfully to {Emails}", 
                string.Join(", ", toEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send candidate referral notification. Error: {ErrorMessage}. InnerException: {InnerException}", 
                ex.Message, ex.InnerException?.Message ?? "None");
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
                ["OldStatus"] = oldStatus,
                ["NewStatus"] = newStatus,
                ["ChangedByUserName"] = changedByUserName,
                ["Notes"] = notes ?? "",
                ["HasNotes"] = !string.IsNullOrWhiteSpace(notes),
                ["ApplicationUrl"] = _applicationUrl
            };

            var htmlBody = _templateService.RenderStatusChangeTemplate(templateData);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hunter", _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"Status Update - {candidateName} moved to {newStatus}";
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpLogin, _password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Status change notification sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status change notification. Error: {ErrorMessage}. InnerException: {InnerException}", 
                ex.Message, ex.InnerException?.Message ?? "None");
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hunter", _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"New Note Added - {candidateName}";
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpLogin, _password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Note added notification sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send note added notification. Error: {ErrorMessage}. InnerException: {InnerException}", 
                ex.Message, ex.InnerException?.Message ?? "None");
        }
    }

    private static string GetMatchLevel(double percentage)
    {
        return percentage >= AIMatching.HighMatchPercentage 
            ? "high" 
            : percentage >= AIMatching.MediumMatchPercentage 
            ? "medium" 
            : "low";
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
}
