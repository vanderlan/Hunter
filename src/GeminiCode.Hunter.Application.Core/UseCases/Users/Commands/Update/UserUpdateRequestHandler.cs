using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Localization;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.Update;

public class UserUpdateRequestHandler(IUnitOfWork unitOfWork, IStringLocalizer<HunterResources> resourceMessages) 
    : IRequestHandler<UserUpdateRequest>
{
    public async Task Handle(UserUpdateRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(request.PublicId);
        await ValidateUser(request);

        user.Email = request.Email;
        user.Name = request.Name;
        user.Profile = request.Profile;
        
        if (request.ReceiveJobChangeNotifications.HasValue)
        {
            user.ReceiveJobChangeNotifications = request.ReceiveJobChangeNotifications.Value;
        }

        unitOfWork.UserRepository.Update(user);

        await unitOfWork.CommitAsync();
    }

    private async Task ValidateUser(UserUpdateRequest request)
    {
        var userFound = await unitOfWork.UserRepository.FindByEmailAsync(request.Email);

        if (userFound != null && userFound.PublicId != request.PublicId)
            throw new ConflictException(resourceMessages[UserMessages.EmailExists], resourceMessages[ExceptionsTitles.ValidationError]);
    }
}