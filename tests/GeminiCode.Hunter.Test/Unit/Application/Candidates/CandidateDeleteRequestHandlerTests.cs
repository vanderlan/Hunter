using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Delete;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Test.Shared.Faker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Candidates;

public class CandidateDeleteRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CandidateDeleteRequestHandler>> _loggerMock;
    private readonly CandidateDeleteRequestHandler _handler;

    public CandidateDeleteRequestHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUser>();
        _loggerMock = new Mock<ILogger<CandidateDeleteRequestHandler>>();

        _handler = new CandidateDeleteRequestHandler(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCandidateNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var publicId = "non-existent-id";
        var request = new CandidateDeleteRequest { PublicId = publicId };

        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync((Candidate?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Contains(publicId, exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPartnerUserTriesToDeleteCandidateWithHiringProcess_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var partnerUser = UserFaker.Get();
        partnerUser.Profile = UserProfile.Partner;

        var candidate = CandidateFaker.Get();
        candidate.UserId = partnerUser.UserId;

        var hiringProcess = HiringProcessFaker.Get();
        hiringProcess.CandidateId = candidate.CandidateId;
        hiringProcess.CreatedByUserId = partnerUser.UserId;
        var hiringProcesses = new List<Hunter.Domain.Core.Entities.HiringProcess> { hiringProcess };

        var request = new CandidateDeleteRequest { PublicId = candidate.PublicId };

        _currentUserMock.Setup(x => x.Id).Returns(partnerUser.PublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(candidate.PublicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(partnerUser.PublicId))
            .ReturnsAsync(partnerUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(hiringProcesses);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Contains("Only Admin users can delete candidates with existing hiring processes", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenAdminUserDeletesCandidateWithHiringProcess_ShouldSucceed()
    {
        // Arrange
        var adminUser = UserFaker.Get();
        adminUser.Profile = UserProfile.Admin;

        var candidate = CandidateFaker.Get();
        candidate.UserId = adminUser.UserId;

        var hiringProcess = HiringProcessFaker.Get();
        hiringProcess.CandidateId = candidate.CandidateId;
        hiringProcess.CreatedByUserId = adminUser.UserId;

        var request = new CandidateDeleteRequest { PublicId = candidate.PublicId };

        _currentUserMock.Setup(x => x.Id).Returns(adminUser.PublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(candidate.PublicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(adminUser.PublicId))
            .ReturnsAsync(adminUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync([hiringProcess]);
        _unitOfWorkMock.Setup(x => x.HiringProcessNoteRepository.GetByHiringProcessIdAsync(hiringProcess.HiringProcessId))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(x => x.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.HiringProcessNoteRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.HiringProcessNoteRepository.GetByHiringProcessIdAsync(hiringProcess.HiringProcessId), Times.Once);
        _unitOfWorkMock.Verify(x => x.HiringProcessRepository.DeleteAsync(hiringProcess.PublicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CandidateRepository.DeleteAsync(candidate.PublicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenPartnerUserDeletesCandidateWithoutHiringProcess_ShouldSucceed()
    {
        // Arrange
        var partnerUser = UserFaker.Get();
        partnerUser.Profile = UserProfile.Partner;

        var candidate = CandidateFaker.Get();
        candidate.UserId = partnerUser.UserId;

        var request = new CandidateDeleteRequest { PublicId = candidate.PublicId };

        _currentUserMock.Setup(x => x.Id).Returns(partnerUser.PublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(candidate.PublicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(partnerUser.PublicId))
            .ReturnsAsync(partnerUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(x => x.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CandidateRepository.DeleteAsync(candidate.PublicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenCandidateHasFilesAndCvAnalysis_ShouldDeleteAll()
    {
        // Arrange
        var adminUser = UserFaker.Get();
        adminUser.Profile = UserProfile.Admin;

        var candidate = CandidateFaker.Get();
        candidate.UserId = adminUser.UserId;

        var file = FileFaker.Get();

        var candidateFile = CandidateFileFaker.Get();
        candidateFile.CandidateId = candidate.CandidateId;
        candidateFile.FileId = file.FileId;
        candidateFile.File = file;

        var request = new CandidateDeleteRequest { PublicId = candidate.PublicId };

        _currentUserMock.Setup(x => x.Id).Returns(adminUser.PublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(candidate.PublicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(adminUser.PublicId))
            .ReturnsAsync(adminUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync([candidateFile]);
        _unitOfWorkMock.Setup(x => x.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.FileRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CvAnalysisRepository.DeleteByCandidateIdAsync(candidate.CandidateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CandidateFileRepository.DeleteAsync(candidateFile.PublicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.FileRepository.DeleteAsync(file.PublicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CandidateRepository.DeleteAsync(candidate.PublicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
    }
}
