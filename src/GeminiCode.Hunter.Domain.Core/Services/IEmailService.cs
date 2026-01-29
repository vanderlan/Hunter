using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Models;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IEmailService
{
    Task SendJobChangesNotificationAsync(
        List<string> toEmails,
        List<string> newJobs,
        List<string> closedJobs,
        List<JobPositionEmailData> newJobPositions,
        List<JobPositionEmailData> closedJobPositions,
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
    
    Task SendHiringProcessCancelledNotificationAsync(
        List<string> toEmails,
        string candidateName,
        string jobTitle,
        string jobCode,
        string cancelledByUserName,
        string cancelReason);
}
