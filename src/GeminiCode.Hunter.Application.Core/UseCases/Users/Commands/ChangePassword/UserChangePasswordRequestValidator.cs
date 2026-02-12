using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Crosscutting.Resources.Messages;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.ChangePassword;

public class UserChangePasswordRequestValidator : AbstractValidator<UserChangePasswordRequest>
{
    public UserChangePasswordRequestValidator(IStringLocalizer<HunterResources> stringLocalizer)
    {

        RuleFor(c => c.CurrentPassword)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.UserMessages.EmptyPassword]);

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.UserMessages.EmptyNewPassword]);

        RuleFor(c => c.NewPasswordConfirm)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.UserMessages.EmptyNewPasswordConfirmation]);

        RuleFor(x => x.NewPassword).Equal(x => x.NewPasswordConfirm)
            .WithMessage(stringLocalizer[MessagesKeys.UserMessages.PasswordAndConfirmationDifferent]);
    }
}
