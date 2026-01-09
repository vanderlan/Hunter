using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.UpdatePreferences;

public class UserUpdatePreferencesRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UserUpdatePreferencesRequest>
{
    public async Task Handle(UserUpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);

        user.ReceiveJobChangeNotifications = request.ReceiveJobChangeNotifications;

        unitOfWork.UserRepository.Update(user);

        await unitOfWork.CommitAsync();
    }
}
