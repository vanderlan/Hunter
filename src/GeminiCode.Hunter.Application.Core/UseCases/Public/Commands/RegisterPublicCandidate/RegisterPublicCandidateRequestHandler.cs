using GeminiCode.Hunter.Application.Core.Helpers;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Helpers;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.RegisterPublicCandidate;

/// <summary>
/// Saves the visitor's contact details against their public CV and sends the
/// email confirmation link. Calling it again re-sends the link.
/// </summary>
public class RegisterPublicCandidateRequestHandler(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<RegisterPublicCandidateRequestHandler> logger)
    : IRequestHandler<RegisterPublicCandidateRequest, RegisterPublicCandidateResponse>
{
    public async Task<RegisterPublicCandidateResponse> Handle(RegisterPublicCandidateRequest request, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.CandidateRepository.GetByPublicTokenAsync(request.CandidateToken);

        if (candidate is null || candidate.Source != CandidateSource.Public)
        {
            throw NotFoundException.WithMessage("We could not find your CV analysis. Please upload your CV again.");
        }

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        var email = request.Email.Trim();
        var emailChanged = !string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase);

        candidate.Name = request.Name.FormatName();
        candidate.Email = email;
        candidate.PhoneNumber = request.PhoneNumber?.Trim();
        candidate.ReceiveJobMatchNotifications = request.ReceiveJobMatchNotifications;

        if (!candidate.HasAcceptedTerms)
        {
            candidate.HasAcceptedTerms = true;
            candidate.TermsAcceptedAt = now;
        }

        if (emailChanged || !candidate.IsEmailConfirmed)
        {
            candidate.IsEmailConfirmed = false;
            candidate.EmailConfirmedAt = null;
            candidate.EmailConfirmationToken = SecureTokenGenerator.NewToken();
            candidate.EmailConfirmationSentAt = now;
        }

        unitOfWork.CandidateRepository.Update(candidate);
        await unitOfWork.CommitAsync();

        var response = new RegisterPublicCandidateResponse
        {
            Email = candidate.Email,
            IsEmailConfirmed = candidate.IsEmailConfirmed
        };

        if (candidate.IsEmailConfirmed)
        {
            return response;
        }

        var applicationUrl = (configuration["Email:ApplicationUrl"] ?? "https://hunter.geminicode.com.br").TrimEnd('/');
        var confirmationUrl = $"{applicationUrl}/confirm?token={Uri.EscapeDataString(candidate.EmailConfirmationToken!)}";

        try
        {
            await emailService.SendPublicCandidateConfirmationAsync(candidate.Email, candidate.Name ?? string.Empty, confirmationUrl);
            response.IsConfirmationEmailSent = true;
        }
        catch (Exception ex)
        {
            // The profile is saved; the UI offers a "resend" that calls this handler again.
            logger.LogError(ex, "Failed to send confirmation email to public candidate {CandidateId}", candidate.CandidateId);
            response.IsConfirmationEmailSent = false;
        }

        return response;
    }
}
