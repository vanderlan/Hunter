using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.AddNote;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Cancel;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.GenerateAIInsights;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetDashboard;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[ApiController]
[Route("[controller]")]
[AuthorizeFor(Roles.Admin, Roles.Partner)]
public sealed class HiringProcessController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a list of hiring processes paginated", typeof(PagedList<HiringProcessGetPaginatedResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] HiringProcessGetPaginatedRequest filter)
    {
        return Ok(await Mediator.Send(filter));
    }

    [HttpGet("{id}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with hiring process details", typeof(HiringProcessGetByIdResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get(string id)
    {
        var hiringProcess = await Mediator.Send(new HiringProcessGetByIdRequest(id));

        if (hiringProcess is null)
            return NotFound();

        return Ok(hiringProcess);
    }

    [HttpGet("Dashboard")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Dashboard statistics for hiring processes", typeof(HiringProcessGetDashboardResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetDashboard()
    {
        return Ok(await Mediator.Send(new HiringProcessGetDashboardRequest()));
    }

    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created, "Hiring process started successfully", typeof(StartHiringProcessResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "Hiring process already exists")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Start([FromBody] StartHiringProcessRequest request)
    {
        var response = await Mediator.Send(request);
        return CreatedAtAction(nameof(Get), new { id = response.PublicId }, response);
    }

    [HttpPut("{id}/Status")]
    [AuthorizeFor(Roles.Admin)]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Status updated successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [SwaggerResponse((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateHiringProcessStatusRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }

    [HttpPost("{id}/Notes")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Note added successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> AddNote(string id, [FromBody] AddHiringProcessNoteRequest request)
    {
        request.HiringProcessId = id;
        await Mediator.Send(request);
        return NoContent();
    }

    [HttpPost("{id}/GenerateAIInsights")]
    [AuthorizeFor(Roles.Admin)]
    [SwaggerResponse((int)HttpStatusCode.OK, "AI insights generated successfully", typeof(GenerateAIInsightsResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    [SwaggerResponse((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> GenerateAIInsights(string id)
    {
        var response = await Mediator.Send(new GenerateAIInsightsRequest { PublicId = id });
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Hiring process cancelled successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Cancel(string id, [FromBody] CancelHiringProcessRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }
}
