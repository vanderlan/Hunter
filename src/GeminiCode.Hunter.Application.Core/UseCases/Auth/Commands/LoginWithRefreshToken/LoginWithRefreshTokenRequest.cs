using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Auth.Commands.LoginWithRefreshToken;

public class LoginWithRefreshTokenRequest : IRequest<LoginWithRefreshTokenResponse>
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}

