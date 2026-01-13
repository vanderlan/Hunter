using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IEmailService
{
    Task SendJobChangesNotificationAsync(
        List<string> toEmails,
        List<string> newJobs,
        List<string> closedJobs,
        DateTime syncTime);
    
    Task SendCandidateMatchingNotificationAsync(
        object matchResults,
        DateTime syncTime);
    
    Task SendWelcomeEmailAsync(
        string userName,
        string userEmail,
        string password);
    
    Task SendCandidateReferralNotificationAsync(
        List<string> toEmails,
        string candidateName,
        string candidateEmail,
        string jobTitle,
        string jobCode,
        string referredByUserName,
        string? referralReason);
    
    Task SendStatusChangeNotificationAsync(
        string toEmail,
        string candidateName,
        string jobTitle,
        string oldStatus,
        string newStatus,
        string changedByUserName,
        string? notes);
    
    Task SendNoteAddedNotificationAsync(
        string toEmail,
        string candidateName,
        string jobTitle,
        string noteText,
        string addedByUserName);
}
