using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GeminiCode.Hunter.Api.Controllers.V1;
using GeminiCode.Hunter.Application.Core.UseCases.CvAnalysis.Queries.GetById;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Api.Controllers.V1;

public class CvAnalysisControllerTest
{
    private CvAnalysisController _cvAnalysisController;

    public CvAnalysisControllerTest()
    {
        SetupMediatorMock();
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsValidCvAnalysis()
    {
        //arrange
        var analysisId = "test-analysis-id";

        //act
        var result = await _cvAnalysisController.GetById(analysisId);

        var contentResult = (OkObjectResult)result;
        var response = (CvAnalysisDetailResponse)contentResult.Value;

        //assert
        Assert.IsType<OkObjectResult>(contentResult);
        Assert.Equal(200, contentResult.StatusCode);
        Assert.IsType<CvAnalysisDetailResponse>(contentResult.Value);
        Assert.Equal(analysisId, response.AnalysisId);
    }

    private void SetupMediatorMock()
    {
        var mediatorMock = new Mock<IMediator>();

        var mockResponse = new CvAnalysisDetailResponse
        {
            AnalysisId = "test-analysis-id",
            CandidateInfo = new CandidateInfoDto
            {
                CandidateId = "candidate-123",
                Name = "Test Candidate",
                Email = "test@example.com",
                HasResume = true
            },
            TotalPositionsAnalyzed = 5,
            MatchesFound = 2,
            Matches = new List<JobMatchDto>()
        };

        mediatorMock.Setup(x => x.Send(It.IsAny<GetCvAnalysisDetailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        _cvAnalysisController = new CvAnalysisController(mediatorMock.Object);
    }
}
