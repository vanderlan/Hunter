using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Delete;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.UploadResume;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.DownloadResume;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetPaginated;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Http.Timeouts;
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
    /// Updates a candidate's information (Name, Email, Phone Number)
    /// </summary>
    /// <param name="id">The candidate's public ID</param>
    /// <param name="request">The updated candidate information</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Candidate updated successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Candidate not found")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Update(string id, [FromBody] CandidateUpdateRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }

    /// <summary>
    /// Analyzes a candidate's CV (PDF) against all active job positions
    /// </summary>
    /// <param name="cvFile">The CV file in PDF format (max 10MB)</param>
    /// <returns>List of matching job positions with match percentages and analysis</returns>
    [HttpPost("Analyze")]
    [RequestTimeout(300000)] // 5 minutes timeout for long-running AI analysis
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

    /// <summary>
    /// Uploads a resume (PDF) for a candidate
    /// </summary>
    /// <param name="id">The candidate's public ID</param>
    /// <param name="file">The resume file in PDF format (max 10MB)</param>
    /// <returns>File upload response with file ID and details</returns>
    [HttpPost("{id}/Resume")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CandidateUploadResumeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadResume(string id, IFormFile file)
    {
        var request = new CandidateUploadResumeRequest 
        { 
            CandidateId = id, 
            File = file 
        };
        
        var response = await Mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Downloads the resume for a candidate
    /// </summary>
    /// <param name="id">The candidate's public ID</param>
    /// <returns>The resume PDF file</returns>
    [HttpGet("{id}/Resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DownloadResume(string id)
    {
        var response = await Mediator.Send(new CandidateDownloadResumeRequest(id));
        return File(response.FileData, response.ContentType, response.FileName);
    }

    /// <summary>
    /// Deletes a candidate and all related data (CvAnalysis, Matches, Files, HiringProcess)
    /// Partner users can only delete candidates without hiring processes
    /// Admin users can delete any candidate
    /// </summary>
    /// <param name="id">The candidate's public ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Candidate deleted successfully")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Candidate not found")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Only Admin can delete candidates with hiring processes")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, "Insufficient permissions")]
    public async Task<IActionResult> Delete(string id)
    {
        await Mediator.Send(new CandidateDeleteRequest { PublicId = id });
        return NoContent();
    }
}
