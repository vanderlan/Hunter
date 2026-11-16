using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Accept;
using GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Decline;
using GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Queries.GetPaginated;
using GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Queries.GetPendingCount;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

/// <summary>
/// Referral requests made by candidates from the public page. Any Partner or
/// Admin can accept one, which starts a hiring process in their name.
/// </summary>
[ApiVersion(1.0)]
[Route("[controller]")]
[AuthorizeFor(Roles.Admin, Roles.Partner)]
public sealed class ReferralRequestsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Paginated referral requests", typeof(PagedList<ReferralRequestGetPaginatedResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] ReferralRequestGetPaginatedRequest filter)
    {
        return Ok(await Mediator.Send(filter));
    }

    [HttpGet("PendingCount")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Number of pending referral requests", typeof(ReferralRequestGetPendingCountResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetPendingCount()
    {
        return Ok(await Mediator.Send(new ReferralRequestGetPendingCountRequest()));
    }

    [HttpPost("{id}/Accept")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Request accepted and hiring process started", typeof(ReferralRequestAcceptResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "Request already resolved or process already exists")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Accept(string id, [FromBody] ReferralRequestAcceptRequest request)
    {
        request.PublicId = id;
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("{id}/Decline")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Request declined")]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "Request already resolved")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Decline(string id, [FromBody] ReferralRequestDeclineRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }
}
