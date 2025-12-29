using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static GeminiCode.Hunter.Domain.Core.Authentication.AuthorizationConfiguration;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion(1.0)]
[ApiController]
[Route("[controller]")]
[AuthorizeFor(Roles.Admin)]
public class CandidatesController(IMediator mediator) : ControllerBase
{
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
        var response = await mediator.Send(request);

        return Ok(new
        {
            message = "CV analysis completed successfully",
            data = response
        });
    }
}
