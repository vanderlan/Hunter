using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IEmailService
{
    Task SendJobChangesNotificationAsync(
        List<string> newJobs,
        List<string> closedJobs,
        DateTime syncTime);
}
