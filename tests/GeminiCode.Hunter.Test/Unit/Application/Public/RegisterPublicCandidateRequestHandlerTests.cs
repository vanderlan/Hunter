using System;
using System.Collections.Generic;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.RegisterPublicCandidate;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Test.Shared.Faker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GeminiCode.Hunter.Test.Unit.Application.Public;

public class RegisterPublicCandidateRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly RegisterPublicCandidateRequestHandler _handler;

    public RegisterPublicCandidateRequestHandlerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Email:ApplicationUrl"] = "https://hunter.test/" })
            .Build();

        _handler = new RegisterPublicCandidateRequestHandler(
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            configuration,
            Mock.Of<ILogger<RegisterPublicCandidateRequestHandler>>());
    }

    [Fact]
    public async Task Handle_WhenCandidateIsInternal_ShouldThrowNotFound()
    {
        var candidate = CandidateFaker.Get();
        candidate.Source = CandidateSource.Internal;
        candidate.PublicToken = "tok";
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync("tok")).ReturnsAsync(candidate);

        var request = new RegisterPublicCandidateRequest { CandidateToken = "tok", Name = "A", Email = "a@b.com", HasAcceptedTerms = true };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenFirstRegistration_ShouldSaveProfileAndSendConfirmationLink()
    {
        var candidate = CandidateFaker.GetPublic(hasAcceptedTerms: false);
        candidate.EmailConfirmationToken = null;
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync(candidate.PublicToken!)).ReturnsAsync(candidate);

        string? sentUrl = null;
        _emailServiceMock
            .Setup(x => x.SendPublicCandidateConfirmationAsync("jane@doe.com", It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, url) => sentUrl = url)
            .Returns(Task.CompletedTask);

        var request = new RegisterPublicCandidateRequest
        {
            CandidateToken = candidate.PublicToken!,
            Name = "jane doe",
            Email = " jane@doe.com ",
            PhoneNumber = "123",
            ReceiveJobMatchNotifications = true,
            HasAcceptedTerms = true
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.True(candidate.HasAcceptedTerms);
        Assert.True(candidate.ReceiveJobMatchNotifications);
        Assert.Equal("jane@doe.com", candidate.Email);
        Assert.False(candidate.IsEmailConfirmed);
        Assert.False(string.IsNullOrEmpty(candidate.EmailConfirmationToken));
        Assert.True(response.IsConfirmationEmailSent);
        Assert.StartsWith("https://hunter.test/confirm?token=", sentUrl);
        Assert.Contains(Uri.EscapeDataString(candidate.EmailConfirmationToken!), sentUrl);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyConfirmedWithSameEmail_ShouldNotResend()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: true);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync(candidate.PublicToken!)).ReturnsAsync(candidate);

        var request = new RegisterPublicCandidateRequest
        {
            CandidateToken = candidate.PublicToken!,
            Name = candidate.Name!,
            Email = candidate.Email!.ToUpperInvariant(),
            HasAcceptedTerms = true
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.True(response.IsEmailConfirmed);
        Assert.True(candidate.IsEmailConfirmed);
        _emailServiceMock.Verify(x => x.SendPublicCandidateConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailChanges_ShouldRequireConfirmationAgain()
    {
        var candidate = CandidateFaker.GetPublic(isEmailConfirmed: true);
        var oldToken = candidate.EmailConfirmationToken;
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync(candidate.PublicToken!)).ReturnsAsync(candidate);

        var request = new RegisterPublicCandidateRequest
        {
            CandidateToken = candidate.PublicToken!,
            Name = candidate.Name!,
            Email = "new@mail.com",
            HasAcceptedTerms = true
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.False(candidate.IsEmailConfirmed);
        Assert.NotEqual(oldToken, candidate.EmailConfirmationToken);
        Assert.False(response.IsEmailConfirmed);
        _emailServiceMock.Verify(x => x.SendPublicCandidateConfirmationAsync("new@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailSendingFails_ShouldStillSaveAndReportIt()
    {
        var candidate = CandidateFaker.GetPublic(hasAcceptedTerms: false);
        _unitOfWorkMock.Setup(x => x.CandidateRepository.GetByPublicTokenAsync(candidate.PublicToken!)).ReturnsAsync(candidate);
        _emailServiceMock
            .Setup(x => x.SendPublicCandidateConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("smtp down"));

        var request = new RegisterPublicCandidateRequest { CandidateToken = candidate.PublicToken!, Name = "A", Email = "a@b.com", HasAcceptedTerms = true };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.False(response.IsConfirmationEmailSent);
        Assert.True(candidate.HasAcceptedTerms);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
