using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GeminiCode.Hunter.Api.Controllers.V1;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetDashboard;
using GeminiCode.Hunter.Test.Shared.Faker;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Api.Controllers.V1;

public class HiringProcessControllerTest
{
    private HiringProcessController _hiringProcessController;
    private readonly HiringProcess _validHiringProcess = HiringProcessFaker.Get();

    public HiringProcessControllerTest()
    {
        SetupMediatorMock();
    }

    [Fact]
    public async Task GetHiringProcessById_WithValidId_ReturnsValidHiringProcess()
    {
        //arrange & act
        var result = await _hiringProcessController.Get(_validHiringProcess.PublicId);

        var contentResult = (OkObjectResult)result;
        var hiringProcess = (HiringProcessGetByIdResponse)contentResult.Value;

        //assert
        Assert.IsType<OkObjectResult>(contentResult);
        Assert.Equal(200, contentResult.StatusCode);

        Assert.IsType<HiringProcessGetByIdResponse>(contentResult.Value);
        Assert.Equal(_validHiringProcess.PublicId, hiringProcess.PublicId);
    }

    [Fact]
    public async Task GetHiringProcessById_WithInvalidId_ReturnsNotFound()
    {
        //arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Send(It.IsAny<HiringProcessGetByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HiringProcessGetByIdResponse)null);

        _hiringProcessController = new HiringProcessController(mediatorMock.Object);

        //act
        var result = await _hiringProcessController.Get("invalid-id");

        //assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_WithValidData_UpdateHiringProcessStatus()
    {
        //arrange & act
        var result = await _hiringProcessController.UpdateStatus(_validHiringProcess.PublicId, HiringProcessFaker.GetUpdateHiringProcessStatusRequest());

        var contentResult = (NoContentResult)result;

        //assert
        Assert.IsType<NoContentResult>(contentResult);
        Assert.Equal(204, contentResult.StatusCode);
    }

    [Fact]
    public async Task AddNote_WithValidData_AddNoteToHiringProcess()
    {
        //arrange & act
        var result = await _hiringProcessController.AddNote(_validHiringProcess.PublicId, HiringProcessFaker.GetAddHiringProcessNoteRequest());

        var contentResult = (NoContentResult)result;

        //assert
        Assert.IsType<NoContentResult>(contentResult);
        Assert.Equal(204, contentResult.StatusCode);
    }

    [Fact]
    public async Task GetHiringProcesses_WithValidFilter_ReturnsAListOfHiringProcesses()
    {
        //arrange & act
        var result = await _hiringProcessController.Get(new HiringProcessGetPaginatedRequest());

        var contentResult = (OkObjectResult)result;
        var hiringProcessPagedList = (PagedList<HiringProcessGetPaginatedResponse>)contentResult.Value;

        //assert
        Assert.Equal(4, hiringProcessPagedList.Count);
        Assert.Equal(200, contentResult.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_ReturnsValidDashboardData()
    {
        //arrange & act
        var result = await _hiringProcessController.GetDashboard();

        var contentResult = (OkObjectResult)result;
        var dashboard = (HiringProcessGetDashboardResponse)contentResult.Value;

        //assert
        Assert.IsType<OkObjectResult>(contentResult);
        Assert.Equal(200, contentResult.StatusCode);
        Assert.IsType<HiringProcessGetDashboardResponse>(contentResult.Value);
    }

    private void SetupMediatorMock()
    {
        var mediatorMock = new Mock<IMediator>();

        var hiringProcessList = new List<HiringProcessGetPaginatedResponse>(4)
        {
            new HiringProcessGetPaginatedResponse { PublicId = _validHiringProcess.PublicId },
            new HiringProcessGetPaginatedResponse { PublicId = HiringProcessFaker.Get().PublicId },
            new HiringProcessGetPaginatedResponse { PublicId = HiringProcessFaker.Get().PublicId },
            new HiringProcessGetPaginatedResponse { PublicId = HiringProcessFaker.Get().PublicId }
        };

        var hiringProcessListPaginated = new PagedList<HiringProcessGetPaginatedResponse>(hiringProcessList, 4);

        mediatorMock.Setup(x => x.Send(It.IsAny<HiringProcessGetPaginatedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hiringProcessListPaginated);

        mediatorMock.Setup(x => x.Send(It.IsAny<HiringProcessGetByIdRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HiringProcessGetByIdResponse { PublicId = _validHiringProcess.PublicId });

        mediatorMock.Setup(x => x.Send(It.IsAny<HiringProcessGetDashboardRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HiringProcessGetDashboardResponse());

        _hiringProcessController = new HiringProcessController(mediatorMock.Object);
    }
}
