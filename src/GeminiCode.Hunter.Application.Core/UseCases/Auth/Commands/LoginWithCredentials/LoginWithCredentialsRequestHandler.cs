using MediatR;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Extensions;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Crosscutting.Resources;
using Microsoft.Extensions.Localization;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.Auth.Commands.LoginWithCredentials;

public class LoginWithCredentialsRequestHandler(IUnitOfWork unitOfWork, IStringLocalizer<HunterResources> resourceMessages) : IRequestHandler<LoginWithCredentialsRequest, LoginWithCredentialsResponse>
{
    public async Task<LoginWithCredentialsResponse> Handle(LoginWithCredentialsRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.LoginAsync(request.Email, request.Password.ToSha512());

        if (user is not null)
        {
            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            unitOfWork.UserRepository.Update(user);
            await unitOfWork.CommitAsync();

            var refreshToken = await AddRefreshTokenAsync(user.Email);

            return new LoginWithCredentialsResponse
            {
                Email = user.Email,
                Name = user.Name,
                PublicId = user.PublicId,
                Profile = user.Profile,
                RefreshToken = refreshToken.Refreshtoken
            };
        }

        throw new UnauthorizedUserException(resourceMessages[UserMessages.InvalidCredentials], resourceMessages[ExceptionsTitles.AuthenticationError]);
    }

    private async Task<RefreshToken> AddRefreshTokenAsync(string userEmail)
    {
        var existentRefreshToken = (await unitOfWork.RefreshTokenRepository.SearchByAsync(x => x.Email == userEmail)).FirstOrDefault();

        if (existentRefreshToken is not null)
            return existentRefreshToken;

        var refreshToken = new RefreshToken
        {
            Email = userEmail,
            Refreshtoken = Guid.NewGuid().ToString().ToSha512()
        };

        var addedRefreshToken = await unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
        await unitOfWork.CommitAsync();

        return addedRefreshToken;
    }
}