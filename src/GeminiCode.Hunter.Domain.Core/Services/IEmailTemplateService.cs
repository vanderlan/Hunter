using System.Collections.Generic;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IEmailTemplateService
{
    string RenderJobChangesTemplate(Dictionary<string, object> data);
    string RenderCandidateMatchesTemplate(Dictionary<string, object> data);
    string RenderWelcomeEmailTemplate(Dictionary<string, object> data);
    string RenderCandidateReferralTemplate(Dictionary<string, object> data);
    string RenderStatusChangeTemplate(Dictionary<string, object> data);
    string RenderNoteAddedTemplate(Dictionary<string, object> data);
    string RenderHiringProcessCancelledTemplate(Dictionary<string, object> data);
}
