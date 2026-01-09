using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.UpdatePreferences;

public class UserUpdatePreferencesRequestValidator : AbstractValidator<UserUpdatePreferencesRequest>
{
    public UserUpdatePreferencesRequestValidator()
    {
        // No specific validation needed for a boolean field
    }
}
