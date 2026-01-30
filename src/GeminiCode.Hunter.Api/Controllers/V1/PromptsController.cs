using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Create;
using GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Delete;
using GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Update;
using GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetAll;
using GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetByKey;
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
public class PromptsController(IMediator mediator) : ApiController(mediator)
{
    /// <summary>
    /// Gets all AI prompts
    /// </summary>
    /// <returns>List of all prompts</returns>
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a list of all Prompts", typeof(List<PromptGetAllResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new PromptGetAllRequest()));
    }

    /// <summary>
    /// Gets a prompt by its key
    /// </summary>
    /// <param name="key">The prompt key</param>
    /// <returns>The prompt if found</returns>
    [HttpGet("key/{key}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a single Prompt", typeof(PromptGetByKeyResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetByKey(string key)
    {
        var prompt = await Mediator.Send(new PromptGetByKeyRequest { Key = key });

        if (prompt is null)
            return NotFound();

        return Ok(prompt);
    }

    /// <summary>
    /// Creates a new prompt
    /// </summary>
    /// <param name="request">The prompt data</param>
    /// <returns>The created prompt</returns>
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created, "Prompt created successfully", typeof(PromptCreateResponse))]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "Prompt with same key already exists")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Create([FromBody] PromptCreateRequest request)
    {
        var response = await Mediator.Send(request);
        return CreatedAtAction(nameof(GetByKey), new { key = response.Key }, response);
    }

    /// <summary>
    /// Updates a prompt's name and/or text
    /// </summary>
    /// <param name="id">The prompt's public ID</param>
    /// <param name="request">The updated prompt data</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Prompt updated successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Prompt not found")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Update(string id, [FromBody] PromptUpdateRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }
}
