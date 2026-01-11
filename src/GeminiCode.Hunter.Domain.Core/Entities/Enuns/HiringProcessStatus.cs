using System.ComponentModel;

namespace GeminiCode.Hunter.Domain.Core.Entities.Enuns;

public enum HiringProcessStatus
{
    [Description("Referred")]
    Referred = 1,

    [Description("Screening")]
    Screening = 2,

    [Description("Consultant Technical Interview")]
    ConsultantTechnicalInterview = 3,

    [Description("Client Interview")]
    ClientInterview = 4,

    [Description("Hired")]
    Hired = 5,

    [Description("Reproved")]
    Reproved = 6
}
