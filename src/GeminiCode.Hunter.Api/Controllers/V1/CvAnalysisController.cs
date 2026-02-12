using Asp.Versioning;
using GeminiCode.Hunter.Api.Attributes;
using GeminiCode.Hunter.Api.Controllers.Base;
using GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetHistory;
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
public sealed class CvAnalysisController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("history")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with CV analysis history", typeof(PagedList<CvAnalysisHistoryResponse>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetHistory([FromQuery] GetCvAnalysisHistoryRequest request)
    {
        var result = await Mediator.Send(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "A success response with CV analysis details", typeof(CvAnalysisDetailResponse))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await Mediator.Send(new GetCvAnalysisDetailRequest(id));
        return Ok(result);
    }
}
