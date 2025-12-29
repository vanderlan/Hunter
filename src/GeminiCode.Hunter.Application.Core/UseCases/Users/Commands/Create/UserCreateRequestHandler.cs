using GeminiCode.Hunter.Application.Core.UseCases.Users.Notifications.UserCreated;
using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Localization;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.Create;

public class UserCreateRequestHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IStringLocalizer<HunterResources> resourceMessages) : IRequestHandler<UserCreateRequest, UserCreateResponse>
{
    public async Task<UserCreateResponse> Handle(UserCreateRequest request, CancellationToken cancellationToken)
    {
        await ValidateUser(request);

        request.Password = request.Password.ToSha512();

        var added = await unitOfWork.UserRepository.AddAsync((User)request);
        await unitOfWork.CommitAsync();


        await mediator.Publish(new UserCreatedNotification(added), cancellationToken);

        return (UserCreateResponse)added;
    }

    private async Task ValidateUser(UserCreateRequest request)
    {
        var userFound = await unitOfWork.UserRepository.FindByEmailAsync(request.Email);

        if (userFound != null)
            throw new ConflictException(resourceMessages[UserMessages.EmailExists], resourceMessages[ExceptionsTitles.ValidationError]);
    }
}