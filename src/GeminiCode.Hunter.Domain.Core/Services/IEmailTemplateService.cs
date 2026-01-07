using System.Collections.Generic;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IEmailTemplateService
{
    string RenderJobChangesTemplate(Dictionary<string, object> data);
    string RenderCandidateMatchesTemplate(Dictionary<string, object> data);
}
