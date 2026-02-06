using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.StoreLookerStudioCredentials;

public class StoreLookerStudioCredentialsRequest : IRequest<StoreLookerStudioCredentialsResponse>
{
    public required string CurlCommand { get; set; }
    public int ExpiresInDays { get; set; } = 7;
}
