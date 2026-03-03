using GeminiCode.Hunter.Application.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.LookerStudio;

public class CsvParserServiceTests
{
    private readonly CsvParserService _service;

    public CsvParserServiceTests()
    {
        var loggerMock = new Mock<ILogger<CsvParserService>>();
        _service = new CsvParserService(loggerMock.Object);
    }

    [Fact]
    public void ParsePriorityCsv_WithOldPattern_ShouldExtractJobCode()
    {
        // Arrange
        var csv = "Role,Priority,CountryRestrictions\r\nFrontend Engineer B. - Job0522,High,USA\r\n";

        // Act
        var result = _service.ParsePriorityCsv(csv);

        // Assert
        Assert.Single(result);
        Assert.Equal("Job0522", result[0].JobCode);
        Assert.Equal("High", result[0].Priority);
        Assert.Equal("USA", result[0].CountryRestrictions);
        Assert.Null(result[0].JobTitle);
    }

    [Fact]
    public void ParsePriorityCsv_WithNewPattern_ShouldUseFullTitleForMatching()
    {
        // Arrange
        var csv = "Role,Priority,CountryRestrictions\r\nGraphQL Engineer,Medium,Brazil\r\n";

        // Act
        var result = _service.ParsePriorityCsv(csv);

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result[0].JobCode);
        Assert.Equal("GraphQL Engineer", result[0].JobTitle);
        Assert.Equal("Medium", result[0].Priority);
        Assert.Equal("Brazil", result[0].CountryRestrictions);
    }

    [Fact]
    public void ParsePriorityCsv_WithMixedPatterns_ShouldHandleBothPatterns()
    {
        // Arrange
        var csv = "Role,Priority,CountryRestrictions\r\n" +
                  "Python Engineer with AWS B. - Job8903,High,USA\r\n" +
                  "GraphQL Engineer,Medium,Brazil\r\n";

        // Act
        var result = _service.ParsePriorityCsv(csv);

        // Assert
        Assert.Equal(2, result.Count);

        var oldPattern = result[0];
        Assert.Equal("Job8903", oldPattern.JobCode);
        Assert.Null(oldPattern.JobTitle);
        Assert.Equal("High", oldPattern.Priority);

        var newPattern = result[1];
        Assert.Equal(string.Empty, newPattern.JobCode);
        Assert.Equal("GraphQL Engineer", newPattern.JobTitle);
        Assert.Equal("Medium", newPattern.Priority);
    }
}
