using System.ComponentModel;

namespace GeminiCode.Hunter.Domain.Core.Entities.Enuns;

/// <summary>
/// Where a candidate record came from.
/// </summary>
public enum CandidateSource
{
    /// <summary>Uploaded by an authenticated Partner or Admin.</summary>
    [Description("Internal")]
    Internal = 1,

    /// <summary>Self-submitted through the public CV match page.</summary>
    [Description("Public")]
    Public = 2
}
