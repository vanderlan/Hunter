using System;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class Candidate : BaseEntity
{
    public long CandidateId { get; set; }

    /// <summary>
    /// Internal user who uploaded the CV. Null for candidates that came in
    /// through the public CV match page.
    /// </summary>
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public CandidateSource Source { get; set; } = CandidateSource.Internal;

    /// <summary>
    /// Opaque token handed to an anonymous visitor after a public CV match so
    /// they can save their profile or ask for a referral without logging in.
    /// </summary>
    public string? PublicToken { get; set; }

    /// <summary>True once the visitor saved their profile and accepted the terms.</summary>
    public bool HasAcceptedTerms { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }

    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationSentAt { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }

    /// <summary>Opt-in for the future "new position matches you" emails.</summary>
    public bool ReceiveJobMatchNotifications { get; set; }

    public virtual User? User { get; set; }

    /// <summary>
    /// A public candidate is only shown to internal users after confirming
    /// their email. Internal candidates are always visible.
    /// </summary>
    public bool IsVisibleInternally => Source == CandidateSource.Internal || IsEmailConfirmed;
}
