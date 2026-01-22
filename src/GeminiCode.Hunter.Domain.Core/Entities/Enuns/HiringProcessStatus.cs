using System.ComponentModel;

namespace GeminiCode.Hunter.Domain.Core.Entities.Enuns;

public enum HiringProcessStatus
{
    [Description("New")]
    New = 0,

    [Description("Referred by Partner")]
    ReferredByPartner = 1,

    [Description("Referred Internally")]
    ReferredInternally = 2,
    
    [Description("Screening")]
    Screening = 3,

    [Description("Technical Interview")]
    ConsultantTechnicalInterview = 4,

    [Description("Client Interview")]
    ClientInterview = 5,

    [Description("Hired")]
    Hired = 6,

    [Description("Reproved")]
    Reproved = 7
}
