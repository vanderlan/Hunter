using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Candidates;

public class CandidateUpdateRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CandidateUpdateRequestHandler>> _loggerMock;
    private readonly CandidateUpdateRequestHandler _handler;

    public CandidateUpdateRequestHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CandidateUpdateRequestHandler>>();

        _handler = new CandidateUpdateRequestHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCandidateNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var publicId = "non-existent-id";

        var request = new CandidateUpdateRequest
        {
            PublicId = publicId,
            Name = "Updated Name",
            Email = "updated@test.com",
            PhoneNumber = "123456789"
        };

        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync((Candidate)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(request, CancellationToken.None));

        _unitOfWorkMock.Verify(x => x.CandidateRepository.Update(It.IsAny<Candidate>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateCandidateSuccessfully()
    {
        // Arrange
        var publicId = "test-public-id";

        var candidate = new Candidate
        {
            CandidateId = 1,
            PublicId = publicId,
            UserId = 1,
            Name = "Original Name",
            Email = "original@test.com",
            PhoneNumber = "000000000"
        };

        var request = new CandidateUpdateRequest
        {
            PublicId = publicId,
            Name = "Updated Name",
            Email = "updated@test.com",
            PhoneNumber = "123456789"
        };

        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync(candidate);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(request.Name, candidate.Name);
        Assert.Equal(request.Email, candidate.Email);
        Assert.Equal(request.PhoneNumber, candidate.PhoneNumber);
        
        _unitOfWorkMock.Verify(x => x.CandidateRepository.Update(It.Is<Candidate>(
            c => c.CandidateId == candidate.CandidateId &&
                 c.Name == request.Name &&
                 c.Email == request.Email &&
                 c.PhoneNumber == request.PhoneNumber)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
