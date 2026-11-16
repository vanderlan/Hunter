using GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositionByCode;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Test.Shared.Faker;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Public;

public class GetPublicJobPositionByCodeRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly GetPublicJobPositionByCodeRequestHandler _handler;

    public GetPublicJobPositionByCodeRequestHandlerTests()
    {
        _handler = new GetPublicJobPositionByCodeRequestHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPositionHasNoPublicContent_ShouldReturnNull()
    {
        var position = JobPositionFaker.Get(); // internal content only
        position.PublicCode = "abc123";
        _unitOfWorkMock.Setup(x => x.JobPositionRepository.GetByPublicCodeAsync("abc123")).ReturnsAsync(position);

        var result = await _handler.Handle(new GetPublicJobPositionByCodeRequest("abc123"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenPositionClosed_ShouldReturnNull()
    {
        var position = JobPositionFaker.GetPublic();
        position.IsActive = false;
        _unitOfWorkMock.Setup(x => x.JobPositionRepository.GetByPublicCodeAsync(position.PublicCode)).ReturnsAsync(position);

        var result = await _handler.Handle(new GetPublicJobPositionByCodeRequest(position.PublicCode), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenVisible_ShouldExposeOnlyPublicFields()
    {
        var position = JobPositionFaker.GetPublic();
        _unitOfWorkMock.Setup(x => x.JobPositionRepository.GetByPublicCodeAsync(position.PublicCode)).ReturnsAsync(position);

        var result = await _handler.Handle(new GetPublicJobPositionByCodeRequest(position.PublicCode), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(position.PublicCode, result!.Code);
        Assert.Equal(position.Title, result.Title);
        Assert.Equal(position.PublicSummary, result.Summary);
        Assert.Equal(position.PublicRequirements, result.Requirements);
        // The internal (company-identifying) text must never leak through the public DTO
        Assert.NotEqual(position.Summary, result.Summary);
        Assert.DoesNotContain(position.PublicId, System.Text.Json.JsonSerializer.Serialize(result));
        Assert.DoesNotContain(position.ExternalUrl, System.Text.Json.JsonSerializer.Serialize(result));
    }
}
