using GeminiCode.Hunter.Application.Core.UseCases.Users.Notifications.UserPasswordChanged;
using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Localization;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.ChangePassword;

public class UserChangePasswordRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMediator mediator,
    IStringLocalizer<HunterResources> resourceMessages) : IRequestHandler<UserChangePasswordRequest>
{
    public async Task Handle(UserChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) ?? throw new NotFoundException(currentUser.Id);

        if (user.Password != request.CurrentPassword.ToSha512())
            throw new BusinessException(resourceMessages[UserMessages.InvalidPassword]);

        user.Password = request.NewPassword.ToSha512();

        unitOfWork.UserRepository.Update(user);

        await unitOfWork.CommitAsync();

        await mediator.Publish(new UserPasswordChangedNotification(), cancellationToken);
    }
}
