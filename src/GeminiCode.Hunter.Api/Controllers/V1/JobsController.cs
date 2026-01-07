using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Commands.SyncJobs;
using GeminiCode.Hunter.Application.Core.UseCases.Jobs.Queries.GetActiveJobs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[Route("[controller]")]
[AuthorizeFor(Roles.Admin)]
public class JobsController(IMediator mediator) : ControllerBase
{
    [HttpPost("Sync")]
    public async Task<IActionResult> SyncJobs()
    {
        var request = new SyncJobsRequest();
        var response = await mediator.Send(request);
        
        return Ok(new
        {
            message = "Job synchronization completed",
            data = response
        });
    }

    [HttpGet("Active")]
    public async Task<IActionResult> GetActiveJobs([FromQuery] string companyCode = null)
    {
        var request = new GetActiveJobsRequest { CompanyCode = companyCode };
        var response = await mediator.Send(request);
        
        return Ok(new
        {
            message = "Active jobs retrieved successfully",
            data = response
        });
    }
}
