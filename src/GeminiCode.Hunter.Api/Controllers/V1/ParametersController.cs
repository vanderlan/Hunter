using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Create;
using GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Delete;
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
public sealed class ParametersController(IMediator mediator) : ApiController(mediator)
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
    /// Creates a new parameter
    /// </summary>
    /// <param name="request">The parameter data</param>
    /// <returns>The created parameter</returns>
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created, "Parameter created successfully", typeof(ParameterCreateResponse))]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "Parameter with same key already exists")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ParameterCreateRequest request)
    {
        var response = await Mediator.Send(request);
        return CreatedAtAction(nameof(GetByKey), new { key = response.Key }, response);
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

    /// <summary>
    /// Deletes a parameter
    /// </summary>
    /// <param name="id">The parameter's public ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Parameter deleted successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Parameter not found")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Delete(string id)
    {
        await Mediator.Send(new ParameterDeleteRequest { PublicId = id });
        return NoContent();
    }
}
