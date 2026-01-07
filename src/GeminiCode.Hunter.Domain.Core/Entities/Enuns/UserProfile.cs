using System.ComponentModel;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Domain.Core.Entities.Enuns;

public enum UserProfile
{
    [Description(Roles.Admin)]
    Admin = 1,

    [Description(Roles.Partner)]
    Partner = 2
}
