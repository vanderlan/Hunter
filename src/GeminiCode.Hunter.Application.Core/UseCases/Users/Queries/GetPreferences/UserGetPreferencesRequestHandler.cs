using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Queries.GetPreferences;

public class UserGetPreferencesRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UserGetPreferencesRequest, UserGetPreferencesResponse>
{
    public async Task<UserGetPreferencesResponse> Handle(UserGetPreferencesRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id);

        return new UserGetPreferencesResponse
        {
            ReceiveJobChangeNotifications = user.ReceiveJobChangeNotifications
        };
    }
}
