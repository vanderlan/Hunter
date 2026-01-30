using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Update;
using GeminiCode.Hunter.Application.Core.UseCases.Parameters.Queries.GetAll;
using GeminiCode.Hunter.Application.Core.UseCases.Parameters.Queries.GetByKey;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[ApiController]
[Route("[controller]")]
[AuthorizeFor(Roles.Admin)]
public class ParametersController(IMediator mediator) : ApiController(mediator)
{
    /// <summary>
    /// Gets all system parameters
    /// </summary>
    /// <returns>List of all parameters</returns>
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a list of all Parameters", typeof(List<ParameterGetAllResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new ParameterGetAllRequest()));
    }

    /// <summary>
    /// Gets a parameter by its key
    /// </summary>
    /// <param name="key">The parameter key</param>
    /// <returns>The parameter if found</returns>
    [HttpGet("key/{key}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a single Parameter", typeof(ParameterGetByKeyResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetByKey(string key)
    {
        var parameter = await Mediator.Send(new ParameterGetByKeyRequest { Key = key });

        if (parameter is null)
            return NotFound();

        return Ok(parameter);
    }

    /// <summary>
    /// Updates a parameter's value
    /// </summary>
    /// <param name="id">The parameter's public ID</param>
    /// <param name="request">The updated parameter value</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Parameter updated successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Parameter not found")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Update(string id, [FromBody] ParameterUpdateRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }
}
