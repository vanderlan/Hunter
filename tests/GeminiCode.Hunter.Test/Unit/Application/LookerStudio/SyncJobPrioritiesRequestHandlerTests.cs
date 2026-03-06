using GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.SyncJobPriorities;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.LookerStudio;

public class SyncJobPrioritiesRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILookerStudioService> _lookerStudioServiceMock;
    private readonly Mock<ICsvParserService> _csvParserServiceMock;
    private readonly Mock<ILogger<SyncJobPrioritiesRequestHandler>> _loggerMock;
    private readonly Mock<IJobPositionRepository> _jobPositionRepositoryMock;
    private readonly SyncJobPrioritiesRequestHandler _handler;

    public SyncJobPrioritiesRequestHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _lookerStudioServiceMock = new Mock<ILookerStudioService>();
        _csvParserServiceMock = new Mock<ICsvParserService>();
        _loggerMock = new Mock<ILogger<SyncJobPrioritiesRequestHandler>>();
        _jobPositionRepositoryMock = new Mock<IJobPositionRepository>();

        _unitOfWorkMock.Setup(x => x.JobPositionRepository).Returns(_jobPositionRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        _handler = new SyncJobPrioritiesRequestHandler(
            _unitOfWorkMock.Object,
            _lookerStudioServiceMock.Object,
            _csvParserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithOldPattern_ShouldMatchByJobCode()
    {
        // Arrange
        var jobPosition = new JobPosition
        {
            JobPositionId = 1,
            PublicId = "pub-1",
            Title = "Frontend Engineer B. - Job0522",
            ExternalUrl = "https://example.com",
            CompanyCode = "ACME",
            JobCode = "Job0522",
            IsActive = true,
            LastCheckedAt = DateTime.UtcNow
        };

        var priorities = new List<JobPriorityDto>
        {
            new JobPriorityDto("Job0522", "High", "USA")
        };

        _lookerStudioServiceMock.Setup(x => x.DownloadPriorityCsvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("csv-content");
        _csvParserServiceMock.Setup(x => x.ParsePriorityCsv("csv-content"))
            .Returns(priorities);
        _jobPositionRepositoryMock.Setup(x => x.GetActivePositionsAsync())
            .ReturnsAsync([jobPosition]);

        // Act
        var result = await _handler.Handle(new SyncJobPrioritiesRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(1, result.MatchedJobs);
        Assert.Equal(0, result.UnmatchedJobs);
        Assert.Contains("Job0522", result.UpdatedJobCodes);
        Assert.Equal("High", jobPosition.Priority);
        Assert.Equal("USA", jobPosition.CountryRestrictions);
    }

    [Fact]
    public async Task Handle_WithNewPattern_ShouldMatchByTitle()
    {
        // Arrange
        var jobPosition = new JobPosition
        {
            JobPositionId = 2,
            PublicId = "pub-2",
            Title = "GraphQL Engineer B.",
            ExternalUrl = "https://example.com",
            CompanyCode = "ACME",
            JobCode = "Job1234",
            IsActive = true,
            LastCheckedAt = DateTime.UtcNow
        };

        // New pattern: no job code, uses JobTitle for matching
        var priorities = new List<JobPriorityDto>
        {
            new JobPriorityDto(string.Empty, "Medium", "Brazil", "GraphQL Engineer")
        };

        _lookerStudioServiceMock.Setup(x => x.DownloadPriorityCsvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("csv-content");
        _csvParserServiceMock.Setup(x => x.ParsePriorityCsv("csv-content"))
            .Returns(priorities);
        _jobPositionRepositoryMock.Setup(x => x.GetActivePositionsAsync())
            .ReturnsAsync([jobPosition]);

        // Act
        var result = await _handler.Handle(new SyncJobPrioritiesRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(1, result.MatchedJobs);
        Assert.Equal(0, result.UnmatchedJobs);
        Assert.Contains("GraphQL Engineer", result.UpdatedJobCodes);
        Assert.Equal("Medium", jobPosition.Priority);
        Assert.Equal("Brazil", jobPosition.CountryRestrictions);
    }

    [Fact]
    public async Task Handle_WithMixedPatterns_ShouldMatchBoth()
    {
        // Arrange
        var oldPatternJob = new JobPosition
        {
            JobPositionId = 1,
            PublicId = "pub-1",
            Title = "Python Engineer with AWS B. - Job8903",
            ExternalUrl = "https://example.com",
            CompanyCode = "ACME",
            JobCode = "Job8903",
            IsActive = true,
            LastCheckedAt = DateTime.UtcNow
        };

        var newPatternJob = new JobPosition
        {
            JobPositionId = 2,
            PublicId = "pub-2",
            Title = "GraphQL Engineer Senior",
            ExternalUrl = "https://example.com",
            CompanyCode = "ACME",
            JobCode = "Job9999",
            IsActive = true,
            LastCheckedAt = DateTime.UtcNow
        };

        var priorities = new List<JobPriorityDto>
        {
            new JobPriorityDto("Job8903", "High", "USA"),
            new JobPriorityDto(string.Empty, "Low", "Global", "GraphQL Engineer")
        };

        _lookerStudioServiceMock.Setup(x => x.DownloadPriorityCsvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("csv-content");
        _csvParserServiceMock.Setup(x => x.ParsePriorityCsv("csv-content"))
            .Returns(priorities);
        _jobPositionRepositoryMock.Setup(x => x.GetActivePositionsAsync())
            .ReturnsAsync([oldPatternJob, newPatternJob]);

        // Act
        var result = await _handler.Handle(new SyncJobPrioritiesRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.MatchedJobs);
        Assert.Equal(0, result.UnmatchedJobs);
        Assert.Equal("High", oldPatternJob.Priority);
        Assert.Equal("Low", newPatternJob.Priority);
    }

    [Fact]
    public async Task Handle_WithNewPatternNotFoundInDatabase_ShouldReportUnmatched()
    {
        // Arrange
        var jobPosition = new JobPosition
        {
            JobPositionId = 1,
            PublicId = "pub-1",
            Title = "Java Developer",
            ExternalUrl = "https://example.com",
            CompanyCode = "ACME",
            JobCode = "Job1111",
            IsActive = true,
            LastCheckedAt = DateTime.UtcNow
        };

        var priorities = new List<JobPriorityDto>
        {
            new JobPriorityDto(string.Empty, "High", "USA", "GraphQL Engineer")
        };

        _lookerStudioServiceMock.Setup(x => x.DownloadPriorityCsvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("csv-content");
        _csvParserServiceMock.Setup(x => x.ParsePriorityCsv("csv-content"))
            .Returns(priorities);
        _jobPositionRepositoryMock.Setup(x => x.GetActivePositionsAsync())
            .ReturnsAsync([jobPosition]);

        // Act
        var result = await _handler.Handle(new SyncJobPrioritiesRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(0, result.MatchedJobs);
        Assert.Equal(1, result.UnmatchedJobs);
        Assert.Contains("GraphQL Engineer", result.UnmatchedJobCodes);
    }
}
