using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Delete;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Update;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetPaginated;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Commands.SyncJobs;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[Route("[controller]")]
[Authorize]
public class JobPositionsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK,"A success response with a list of JobPositions paginated", typeof(PagedList<JobPositionGetPaginatedResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] JobPositionGetPaginatedRequest filter)
    {
        return Ok(await Mediator.Send(filter));
    }

    [HttpGet("{id}")]
    [SwaggerResponse((int)HttpStatusCode.OK,"A success response with a single JobPosition", typeof(JobPositionGetByIdResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get(string id)
    {
        var jobPosition = await Mediator.Send(new JobPositionGetByIdRequest(id));
        
        if (jobPosition is null)
            return NotFound();

        return Ok(jobPosition);
    }

    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created,"A valid JobPosition created", typeof(JobPositionCreateResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest,"A error response with the error description", typeof(ExceptionResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> Post([FromBody] JobPositionCreateRequest jobPositionCreateRequest)
    {
        return Created(await Mediator.Send(jobPositionCreateRequest));
    }

    [HttpPut("{id}")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest,"A error response with the error description", typeof(ExceptionResponse))]
    [SwaggerResponse((int)HttpStatusCode.Accepted)]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> Put(string id, [FromBody] JobPositionUpdateRequest jobPositionUpdateRequest)
    {
        jobPositionUpdateRequest.PublicId = id;

        await Mediator.Send(jobPositionUpdateRequest);

        return Accepted();
    }

    [HttpDelete("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent)]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> Delete(string id)
    {
        await Mediator.Send(new JobPositionDeleteRequest(id));
        
        return NoContent();
    }

    [HttpPost("Sync")]
    [SwaggerResponse((int)HttpStatusCode.OK,"Job synchronization completed", typeof(SyncJobsResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin)]
    public async Task<IActionResult> SyncJobs()
    {
        var response = await Mediator.Send(new SyncJobsRequest());
        return Ok(response);
    }

    [HttpGet("Active")]
    [SwaggerResponse((int)HttpStatusCode.OK,"Active jobs retrieved successfully", typeof(GetActiveJobsResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [AuthorizeFor(Roles.Admin, Roles.Partner)]
    public async Task<IActionResult> GetActiveJobs([FromQuery] string companyCode = null)
    {
        var request = new GetActiveJobsRequest { CompanyCode = companyCode };
        var response = await Mediator.Send(request);
        return Ok(response);
    }
}
