using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GeminiCode.Hunter.Api.Controllers.V1;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetPaginated;
using GeminiCode.Hunter.Test.Shared.Faker;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Api.Controllers.V1;

public class JobPositionsControllerTest
{
    private JobPositionsController _jobPositionsController;
    private readonly JobPosition _validJobPosition = JobPositionFaker.Get();

    public JobPositionsControllerTest()
    {
        SetupMediatorMock();
    }

    [Fact]
    public async Task GetJobPositionById_WithValidId_ReturnsValidJobPosition()
    {
        //arrange & act
        var result = await _jobPositionsController.Get(_validJobPosition.PublicId);

        var contentResult = (OkObjectResult)result;
        var jobPosition = (JobPositionGetByIdResponse)contentResult.Value;

        //assert
        Assert.IsType<OkObjectResult>(contentResult);
        Assert.Equal(200, contentResult.StatusCode);

        Assert.IsType<JobPositionGetByIdResponse>(contentResult.Value);
        Assert.Equal(_validJobPosition.Title, jobPosition.Title);
        Assert.Equal(_validJobPosition.JobCode, jobPosition.JobCode);
    }

    [Fact]
    public async Task PostJobPosition_WithValidData_CreateAJobPosition()
    {
        //arrange & act
        var result = await _jobPositionsController.Post(JobPositionFaker.GetJobPositionCreateRequest());

        var contentResult = (CreatedResult)result;

        //assert
        Assert.IsType<CreatedResult>(contentResult);
        Assert.Equal(201, contentResult.StatusCode);
    }

    [Fact]
    public async Task PutJobPosition_WithValidData_UpdateJobPosition()
    {
        //arrange & act
        var result = await _jobPositionsController.Put(_validJobPosition.PublicId, JobPositionFaker.GetJobPositionUpdateRequest());

        var contentResult = (AcceptedResult)result;

        //assert
        Assert.IsType<AcceptedResult>(contentResult);
        Assert.Equal(202, contentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteJobPosition_WithExistentId_DeleteJobPosition()
    {
        //arrange & act
        var result = await _jobPositionsController.Delete(_validJobPosition.PublicId);

        var contentResult = (NoContentResult)result;

        //assert
        Assert.IsType<NoContentResult>(contentResult);
        Assert.Equal(204, contentResult.StatusCode);
    }

    [Fact]
    public async Task GetJobPositions_WithValidFilter_ReturnsAListOfJobPositions()
    {
        //arrange & act
        var result = await _jobPositionsController.Get(new JobPositionGetPaginatedRequest());

        var contentResult = (OkObjectResult)result;
        var jobPositionPagedList = (PagedList<JobPositionGetPaginatedResponse>)contentResult.Value;

        //assert
        Assert.Equal(4, jobPositionPagedList.Count);
        Assert.Equal(200, contentResult.StatusCode);
    }

    private void SetupMediatorMock()
    {
        var mediatorMock = new Mock<IMediator>();

        var jobPositionList = new List<JobPositionGetPaginatedResponse>(4)
        {
            (JobPositionGetPaginatedResponse)_validJobPosition,
            (JobPositionGetPaginatedResponse)JobPositionFaker.Get(),
            (JobPositionGetPaginatedResponse)JobPositionFaker.Get(),
            (JobPositionGetPaginatedResponse)JobPositionFaker.Get()
        };

        var jobPositionListPaginated = new PagedList<JobPositionGetPaginatedResponse>(jobPositionList, 4);

        mediatorMock.Setup(x => x.Send(It.IsAny<JobPositionGetPaginatedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobPositionListPaginated);

        mediatorMock.Setup(x => x.Send(It.IsAny<JobPositionGetByIdRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((JobPositionGetByIdResponse)_validJobPosition);

        _jobPositionsController = new JobPositionsController(mediatorMock.Object);
    }
}
