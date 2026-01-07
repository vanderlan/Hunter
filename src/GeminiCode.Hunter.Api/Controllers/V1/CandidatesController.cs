using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetPaginated;
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
public class CandidatesController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a list of Candidates paginated", typeof(PagedList<CandidateGetPaginatedResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] CandidateGetPaginatedRequest filter)
    {
        return Ok(await Mediator.Send(filter));
    }

    [HttpGet("{id}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with a single Candidate", typeof(CandidateGetByIdResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Get(string id)
    {
        var candidate = await Mediator.Send(new CandidateGetByIdRequest(id));

        if (candidate is null)
            return NotFound();

        return Ok(candidate);
    }

    /// <summary>
    /// Analyzes a candidate's CV (PDF) against all active job positions
    /// </summary>
    /// <param name="cvFile">The CV file in PDF format (max 10MB)</param>
    /// <returns>List of matching job positions with match percentages and analysis</returns>
    [HttpPost("Analyze")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AnalyzeCv(IFormFile cvFile)
    {
        var request = new AnalyzeCvRequest { CvFile = cvFile };
        var response = await Mediator.Send(request);

        return Ok(new
        {
            message = "CV analysis completed successfully",
            data = response
        });
    }
}
