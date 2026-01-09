using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.UpdatePreferences;

public class UserUpdatePreferencesRequest : IRequest
{
    public bool ReceiveJobChangeNotifications { get; set; }
}
