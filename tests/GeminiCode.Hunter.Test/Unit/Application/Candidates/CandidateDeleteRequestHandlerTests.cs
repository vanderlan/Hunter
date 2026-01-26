using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Delete;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        var publicId = "candidate-id";
        var userPublicId = "user-id";
        
        var partnerUser = new User
        {
            UserId = 1,
            PublicId = userPublicId,
            Name = "Partner User",
            Email = "partner@test.com",
            Password = "password",
            Profile = UserProfile.Partner
        };

        var candidate = new Candidate
        {
            CandidateId = 1,
            PublicId = publicId,
            UserId = 1,
            Name = "Test Candidate",
            Email = "candidate@test.com",
            AnalyzedAt = DateTime.UtcNow
        };

        var hiringProcesses = new List<Hunter.Domain.Core.Entities.HiringProcess>
        {
            new()
            {
                HiringProcessId = 1,
                PublicId = "hp-id",
                CandidateId = candidate.CandidateId,
                JobPositionId = 1,
                Status = HiringProcessStatus.ReferredByPartner,
                CreatedByUserId = 1
            }
        };

        var request = new CandidateDeleteRequest { PublicId = publicId };

        _currentUserMock.Setup(x => x.Id).Returns(userPublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userPublicId))
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
        var publicId = "candidate-id";
        var userPublicId = "admin-id";
        
        var adminUser = new User
        {
            UserId = 1,
            PublicId = userPublicId,
            Name = "Admin User",
            Email = "admin@test.com",
            Password = "password",
            Profile = UserProfile.Admin
        };

        var candidate = new Candidate
        {
            CandidateId = 1,
            PublicId = publicId,
            UserId = 1,
            Name = "Test Candidate",
            Email = "candidate@test.com",
            AnalyzedAt = DateTime.UtcNow
        };

        var hiringProcess = new GeminiCode.Hunter.Domain.Core.Entities.HiringProcess
        {
            HiringProcessId = 1,
            PublicId = "hp-id",
            CandidateId = candidate.CandidateId,
            JobPositionId = 1,
            Status = HiringProcessStatus.ReferredByPartner,
            CreatedByUserId = 1
        };

        var request = new CandidateDeleteRequest { PublicId = publicId };

        _currentUserMock.Setup(x => x.Id).Returns(userPublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userPublicId))
            .ReturnsAsync(adminUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(new List<GeminiCode.Hunter.Domain.Core.Entities.HiringProcess> { hiringProcess });
        _unitOfWorkMock.Setup(x => x.HiringProcessNoteRepository.GetByHiringProcessIdAsync(hiringProcess.HiringProcessId))
            .ReturnsAsync(new List<HiringProcessNote>());
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(new List<CandidateFile>());
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
        _unitOfWorkMock.Verify(x => x.CandidateRepository.DeleteAsync(publicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenPartnerUserDeletesCandidateWithoutHiringProcess_ShouldSucceed()
    {
        // Arrange
        var publicId = "candidate-id";
        var userPublicId = "partner-id";
        
        var partnerUser = new User
        {
            UserId = 1,
            PublicId = userPublicId,
            Name = "Partner User",
            Email = "partner@test.com",
            Password = "password",
            Profile = UserProfile.Partner
        };

        var candidate = new Candidate
        {
            CandidateId = 1,
            PublicId = publicId,
            UserId = 1,
            Name = "Test Candidate",
            Email = "candidate@test.com",
            AnalyzedAt = DateTime.UtcNow
        };

        var request = new CandidateDeleteRequest { PublicId = publicId };

        _currentUserMock.Setup(x => x.Id).Returns(userPublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userPublicId))
            .ReturnsAsync(partnerUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(new List<GeminiCode.Hunter.Domain.Core.Entities.HiringProcess>());
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(new List<CandidateFile>());
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
        _unitOfWorkMock.Verify(x => x.CandidateRepository.DeleteAsync(publicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenCandidateHasFilesAndCvAnalysis_ShouldDeleteAll()
    {
        // Arrange
        var publicId = "candidate-id";
        var userPublicId = "admin-id";
        
        var adminUser = new User
        {
            UserId = 1,
            PublicId = userPublicId,
            Name = "Admin User",
            Email = "admin@test.com",
            Password = "password",
            Profile = UserProfile.Admin
        };

        var candidate = new Candidate
        {
            CandidateId = 1,
            PublicId = publicId,
            UserId = 1,
            Name = "Test Candidate",
            Email = "candidate@test.com",
            AnalyzedAt = DateTime.UtcNow
        };

        var file = new GeminiCode.Hunter.Domain.Core.Entities.File
        {
            FileId = 1,
            PublicId = "file-id",
            FileName = "resume.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StorageType = StorageType.Database
        };

        var candidateFile = new CandidateFile
        {
            CandidateFileId = 1,
            PublicId = "cf-id",
            CandidateId = candidate.CandidateId,
            FileId = file.FileId,
            FileType = "Resume",
            File = file
        };

        var request = new CandidateDeleteRequest { PublicId = publicId };

        _currentUserMock.Setup(x => x.Id).Returns(userPublicId);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByIdAsync(publicId))
            .ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userPublicId))
            .ReturnsAsync(adminUser);
        _unitOfWorkMock.Setup(x => x.HiringProcessRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(new List<GeminiCode.Hunter.Domain.Core.Entities.HiringProcess>());
        _unitOfWorkMock.Setup(x => x.CandidateFileRepository.GetByCandidateIdAsync(candidate.CandidateId))
            .ReturnsAsync(new List<CandidateFile> { candidateFile });
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
        _unitOfWorkMock.Verify(x => x.CandidateRepository.DeleteAsync(publicId), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.AtLeastOnce);
    }
}
