using MediatR;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Crosscutting.Resources;
using Microsoft.Extensions.Localization;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Extensions;
using static GeminiCode.Hunter.Crosscutting.Resources.Messages.MessagesKeys;

namespace GeminiCode.Hunter.Application.Core.UseCases.Auth.Commands.LoginWithRefreshToken;

public class LoginWithRefreshTokenRequestHandler(IUnitOfWork unitOfWork, IStringLocalizer<HunterResources> resourceMessages) : IRequestHandler<LoginWithRefreshTokenRequest, LoginWithRefreshTokenResponse>
{
    public async Task<LoginWithRefreshTokenResponse> Handle(LoginWithRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            throw new UnauthorizedUserException(
                resourceMessages[UserMessages.InvalidRefreshToken],
                resourceMessages[ExceptionsTitles.AuthenticationError]
            );
        }

        var email = GetClaimFromJwtToken(request.Token, ClaimTypes.Email);

        var userRefreshToken = (await unitOfWork.RefreshTokenRepository.SearchByAsync(x => x.Refreshtoken.Equals(request.RefreshToken) && x.Email == email)).FirstOrDefault();

        if (userRefreshToken is not null)
        {
            var user = (await unitOfWork.UserRepository.SearchByAsync(x => x.Email == userRefreshToken.Email)).FirstOrDefault();

            if (user is not null)
            {
                await unitOfWork.RefreshTokenRepository.DeleteAsync(userRefreshToken.PublicId);
                var newRefreshToken = await AddRefreshTokenAsync(user.Email);

                return new LoginWithRefreshTokenResponse
                {
                    Email = user.Email,
                    Name = user.Name,
                    PublicId = user.PublicId,
                    Profile = user.Profile,
                    RefreshToken = newRefreshToken.Refreshtoken
                };
            }

        }

        throw new UnauthorizedUserException(resourceMessages[UserMessages.InvalidRefreshToken], resourceMessages[ExceptionsTitles.AuthenticationError]);
    }

    private string GetClaimFromJwtToken(string jtwToken, string claimName)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(jtwToken);
            var claimValue = jwtSecurityToken.Claims.First(claim => claim.Type == claimName).Value;

            return claimValue;
        }
        catch (Exception)
        {
            throw new UnauthorizedUserException(resourceMessages[UserMessages.InvalidRefreshToken], resourceMessages[ExceptionsTitles.AuthenticationError]);
        }
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