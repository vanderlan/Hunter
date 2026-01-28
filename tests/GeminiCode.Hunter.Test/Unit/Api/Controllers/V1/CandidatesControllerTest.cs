using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GeminiCode.Hunter.Api.Controllers.V1;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetPaginated;
using GeminiCode.Hunter.Test.Shared.Faker;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Api.Controllers.V1;

public class CandidatesControllerTest
{
    private CandidatesController _candidatesController;
    private readonly Candidate _validCandidate = CandidateFaker.Get();

    public CandidatesControllerTest()
    {
        SetupMediatorMock();
    }

    [Fact]
    public async Task GetCandidateById_WithValidId_ReturnsValidCandidate()
    {
        //arrange & act
        var result = await _candidatesController.Get(_validCandidate.PublicId);

        var contentResult = (OkObjectResult)result;
        var candidate = (CandidateGetByIdResponse)contentResult.Value;

        //assert
        Assert.IsType<OkObjectResult>(contentResult);
        Assert.Equal(200, contentResult.StatusCode);

        Assert.IsType<CandidateGetByIdResponse>(contentResult.Value);
        Assert.Equal(_validCandidate.PublicId, candidate.PublicId);
        Assert.Equal(_validCandidate.Name, candidate.Name);
        Assert.Equal(_validCandidate.Email, candidate.Email);
    }

    [Fact]
    public async Task GetCandidateById_WithInvalidId_ReturnsNotFound()
    {
        //arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Send(It.IsAny<CandidateGetByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidateGetByIdResponse)null);

        _candidatesController = new CandidatesController(mediatorMock.Object);

        //act
        var result = await _candidatesController.Get("invalid-id");

        //assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PutCandidate_WithValidData_UpdateCandidate()
    {
        //arrange & act
        var result = await _candidatesController.Update(_validCandidate.PublicId, CandidateFaker.GetCandidateUpdateRequest());

        var contentResult = (NoContentResult)result;

        //assert
        Assert.IsType<NoContentResult>(contentResult);
        Assert.Equal(204, contentResult.StatusCode);
    }

    [Fact]
    public async Task GetCandidates_WithValidFilter_ReturnsAListOfCandidates()
    {
        //arrange & act
        var result = await _candidatesController.Get(new CandidateGetPaginatedRequest());

        var contentResult = (OkObjectResult)result;
        var candidatePagedList = (PagedList<CandidateGetPaginatedResponse>)contentResult.Value;

        //assert
        Assert.Equal(4, candidatePagedList.Count);
        Assert.Equal(200, contentResult.StatusCode);
    }

    private void SetupMediatorMock()
    {
        var mediatorMock = new Mock<IMediator>();

        var candidateList = new List<CandidateGetPaginatedResponse>(4)
        {
            new CandidateGetPaginatedResponse { PublicId = _validCandidate.PublicId, Name = _validCandidate.Name, Email = _validCandidate.Email },
            new CandidateGetPaginatedResponse { PublicId = CandidateFaker.Get().PublicId },
            new CandidateGetPaginatedResponse { PublicId = CandidateFaker.Get().PublicId },
            new CandidateGetPaginatedResponse { PublicId = CandidateFaker.Get().PublicId }
        };

        var candidateListPaginated = new PagedList<CandidateGetPaginatedResponse>(candidateList, 4);

        mediatorMock.Setup(x => x.Send(It.IsAny<CandidateGetPaginatedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidateListPaginated);

        mediatorMock.Setup(x => x.Send(It.IsAny<CandidateGetByIdRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new CandidateGetByIdResponse
          {
              PublicId = _validCandidate.PublicId,
              Name = _validCandidate.Name,
              Email = _validCandidate.Email
          });

        _candidatesController = new CandidatesController(mediatorMock.Object);
    }
}
