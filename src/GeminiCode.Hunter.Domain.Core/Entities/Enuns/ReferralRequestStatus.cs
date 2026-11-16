using System.ComponentModel;

namespace GeminiCode.Hunter.Domain.Core.Entities.Enuns;

public enum ReferralRequestStatus
{
    [Description("Pending")]
    Pending = 1,

    [Description("Accepted")]
    Accepted = 2,

    [Description("Declined")]
    Declined = 3
}
