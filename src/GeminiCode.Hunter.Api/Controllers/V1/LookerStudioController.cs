using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.StoreLookerStudioCredentials;
using GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.SyncJobPriorities;
using GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Queries.GetCredentialStatus;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[Route("[controller]")]
[Authorize]
public class LookerStudioController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("credentials")]
    [SwaggerResponse((int)HttpStatusCode.Created, "Credentials stored successfully", typeof(StoreLookerStudioCredentialsResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid curl command or request", typeof(ExceptionResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> StoreCredentials([FromBody] StoreLookerStudioCredentialsRequest request)
    {
        var response = await Mediator.Send(request);
        return Created(response);
    }

    [HttpGet("credentials/status")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Credential status retrieved", typeof(GetCredentialStatusResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> GetCredentialStatus()
    {
        var response = await Mediator.Send(new GetCredentialStatusQuery());
        return Ok(response);
    }

    [HttpPost("sync-priorities")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Job priorities synced successfully", typeof(SyncJobPrioritiesResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Sync failed", typeof(ExceptionResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> SyncPriorities()
    {
        var response = await Mediator.Send(new SyncJobPrioritiesRequest());
        return Ok(response);
    }
}

