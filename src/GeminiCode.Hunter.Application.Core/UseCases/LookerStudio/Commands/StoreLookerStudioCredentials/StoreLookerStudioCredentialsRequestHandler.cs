using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Application.Core.Services;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.StoreLookerStudioCredentials;

public class StoreLookerStudioCredentialsRequestHandler(
    IUnitOfWork unitOfWork,
    ICurlParserService curlParserService,
    IEncryptionService encryptionService,
    ILookerStudioService lookerStudioService,
    ICurrentUser currentUser,
    ILogger<StoreLookerStudioCredentialsRequestHandler> logger) 
    : IRequestHandler<StoreLookerStudioCredentialsRequest, StoreLookerStudioCredentialsResponse>
{
    public async Task<StoreLookerStudioCredentialsResponse> Handle(
        StoreLookerStudioCredentialsRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Storing new Looker Studio credentials for user {userId}", currentUser.Id);

        try
        {
            // 1. Parse curl command
            var (cookies, xsrfToken, datasourceId, reportId, sessionId) = curlParserService.ParseCurlCommand(request.CurlCommand);

            logger.LogInformation("Successfully parsed curl command. DatasourceId: {datasourceId}, ReportId: {reportId}", 
                datasourceId, reportId);

            // 2. Encrypt cookie data
            var encryptedCookies = encryptionService.Encrypt(cookies);

            // 3. Deactivate all existing credentials
            await unitOfWork.LookerStudioCredentialRepository.DeactivateAllCredentialsAsync();

            // 4. Create new credential
            var expiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(request.ExpiresInHours), DateTimeKind.Unspecified);

            var credential = await unitOfWork.LookerStudioCredentialRepository.AddAsync(new LookerStudioCredential
            {
                PublicId = Guid.NewGuid().ToString(),
                EncryptedCookieData = encryptedCookies,
                XsrfToken = xsrfToken,
                DatasourceId = datasourceId,
                ReportId = reportId,
                SessionId = sessionId,
                ExpiresAt = expiresAt,
                IsActive = true,
                UpdatedByUserId = currentUser.Id
            });

            await unitOfWork.CommitAsync();

            logger.LogInformation("Credential stored successfully. Testing validity...");

            // 5. Test credentials
            bool isValid = false;
            string message = "Credentials stored successfully. Background service will keep session alive with proactive refreshes every 45 minutes.";

            try
            {
                isValid = await lookerStudioService.ValidateCredentialsAsync();

                if (isValid)
                {
                    message = "Credentials stored and validated successfully! Session will be kept alive automatically during business hours.";
                    logger.LogInformation("Credentials are valid and working. Proactive token refresh will maintain session.");
                }
                else
                {
                    message = "Credentials stored but validation failed. They may already be expired or invalid. Please copy a fresh curl command.";
                    logger.LogWarning("Credentials validation failed immediately after storage");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not validate credentials immediately, but they were stored");
                message = "Credentials stored but could not be validated at this time. They will be tested during next sync.";
            }

            return new StoreLookerStudioCredentialsResponse
            {
                CredentialId = credential.PublicId,
                ExpiresAt = credential.ExpiresAt,
                IsValid = isValid,
                Message = message
            };
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to parse curl command");
            throw new BusinessException($"Invalid curl command: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error storing Looker Studio credentials");
            throw;
        }
    }
}
