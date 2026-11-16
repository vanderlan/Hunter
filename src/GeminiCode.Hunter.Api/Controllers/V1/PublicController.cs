using Asp.Versioning;
using GeminiCode.Hunter.Api.Configuration;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.AnalyzePublicCv;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.ConfirmPublicCandidateEmail;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.CreateReferral;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.RegisterPublicCandidate;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositionByCode;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositions;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace GeminiCode.Hunter.Api.Controllers.V1;

/// <summary>
/// Anonymous surface of Hunter: the public job board and the self-service CV match.
/// Nothing returned here may contain internal ids, external URLs or company details.
/// </summary>
[ApiVersion(1.0)]
[Route("[controller]")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Public)]
public sealed class PublicController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("JobPositions")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Open positions visible to anonymous visitors", typeof(GetPublicJobPositionsResponse))]
    public async Task<IActionResult> GetJobPositions()
    {
        return Ok(await Mediator.Send(new GetPublicJobPositionsRequest()));
    }

    [HttpGet("JobPositions/{code}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A single open position", typeof(GetPublicJobPositionByCodeResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetJobPosition(string code)
    {
        var position = await Mediator.Send(new GetPublicJobPositionByCodeRequest(code));

        if (position is null)
            return NotFound();

        return Ok(position);
    }

    /// <summary>
    /// Matches an anonymous visitor's CV (PDF, max 10MB) against the open positions.
    /// </summary>
    [HttpPost("CvMatch")]
    [RequestTimeout(300000)]
    [EnableRateLimiting(RateLimitPolicies.PublicCvMatch)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AnalyzePublicCvResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CvMatch([FromForm] AnalyzePublicCvRequest request)
    {
        // Binding the request type (instead of a bare IFormFile) lets the
        // FluentValidation filter reject bad uploads before the handler runs.
        var response = await Mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Saves the visitor's contact details and sends the email confirmation link.
    /// Calling it again re-sends the link.
    /// </summary>
    [HttpPost("Candidates/{token}/Register")]
    [ProducesResponseType(typeof(RegisterPublicCandidateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(string token, [FromBody] RegisterPublicCandidateRequest request)
    {
        request.CandidateToken = token;
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("Candidates/Confirm")]
    [ProducesResponseType(typeof(ConfirmPublicCandidateEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmPublicCandidateEmailRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// The visitor asks someone inside Hunter to refer them for a matched position.
    /// </summary>
    [HttpPost("Candidates/{token}/ReferralRequests")]
    [ProducesResponseType(typeof(CreateReferralResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ExceptionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReferralRequest(string token, [FromBody] CreateReferralRequest request)
    {
        request.CandidateToken = token;
        return Created(await Mediator.Send(request));
    }
}
