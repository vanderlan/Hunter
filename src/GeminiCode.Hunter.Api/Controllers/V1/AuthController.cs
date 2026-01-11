using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using GeminiCode.Hunter.Api.Configuration;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Api.Models;
using GeminiCode.Hunter.Application.Core.UseCases.Auth.Commands.LoginWithCredentials;
using GeminiCode.Hunter.Application.Core.UseCases.Auth.Commands.LoginWithRefreshToken;
using GeminiCode.Hunter.Api.Attributes;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[Route("[controller]")]
[AllowAnonymous]
public class AuthController(IMediator mediator, IConfiguration configuration) : ApiController(mediator)
{
    [HttpPost("Login")]
    [RateLimit(5, 60)] // Max 5 requests per minute
    [SwaggerResponse((int)HttpStatusCode.OK, "A success reponse with a Token, Refresh Token and expiration date", typeof(LoginWithCredentialsResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "A error response with the error description", typeof(ExceptionResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [SwaggerResponse((int)HttpStatusCode.TooManyRequests, "Rate limit exceeded")]
    public async Task<IActionResult> Login([FromBody] LoginWithCredentialsRequest loginWithCredentialsRequest)
    {
        return AuthorizeUser(await Mediator.Send(loginWithCredentialsRequest));
    }

    [HttpPost("RefreshToken")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success reponse with a Token, Refresh Token and expiration date", typeof(LoginWithRefreshTokenResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "A error response with the error description", typeof(ExceptionResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] LoginWithRefreshTokenRequest loginWithRefreshTokenRequest)
    {
        return AuthorizeUser(await Mediator.Send(loginWithRefreshTokenRequest));
    }

    private OkObjectResult AuthorizeUser(LoginWithCredentialsResponse loginWithCredentialsResponse)
    {
        var (token, validTo) = AuthenticationConfiguration.CreateToken(loginWithCredentialsResponse, configuration);

        return Ok(
            new UserApiTokenModel
            {
                Token = token,
                Expiration = validTo,
                RefreshToken = loginWithCredentialsResponse.RefreshToken
            });
    }
}