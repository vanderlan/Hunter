using MediatR;
using Microsoft.Extensions.Logging;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Queries.GetCredentialStatus;

public class GetCredentialStatusQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCredentialStatusQueryHandler> logger)
    : IRequestHandler<GetCredentialStatusQuery, GetCredentialStatusResponse>
{
    public async Task<GetCredentialStatusResponse> Handle(
        GetCredentialStatusQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking Looker Studio credential status");

        var credential = await unitOfWork.LookerStudioCredentialRepository.GetActiveCredentialAsync();

        if (credential == null)
        {
            logger.LogWarning("No active Looker Studio credentials found");
            return new GetCredentialStatusResponse
            {
                HasActiveCredential = false,
                Message = "No active credentials found. Please add credentials in the Admin panel."
            };
        }

        var now = DateTime.UtcNow;
        var isExpired = credential.ExpiresAt < now;
        var daysUntilExpiration = isExpired ? 0 : (int)(credential.ExpiresAt - now).TotalDays;

        // Get last sync time from any job position
        var jobPositions = await unitOfWork.JobPositionRepository.GetAllAsync();
        var lastSyncedAt = jobPositions
            .Where(j => j.PriorityLastSyncedAt.HasValue)
            .OrderByDescending(j => j.PriorityLastSyncedAt)
            .FirstOrDefault()?.PriorityLastSyncedAt;

        var message = isExpired
            ? "Credentials have expired. Please update them in the Admin panel."
            : $"Credentials are active and will expire in {daysUntilExpiration} day(s).";

        return new GetCredentialStatusResponse
        {
            HasActiveCredential = true,
            CredentialId = credential.PublicId,
            ExpiresAt = credential.ExpiresAt,
            IsExpired = isExpired,
            LastSyncedAt = lastSyncedAt,
            DaysUntilExpiration = daysUntilExpiration,
            UpdatedByUserId = credential.UpdatedByUserId,
            CreatedAt = credential.CreatedAt,
            Message = message
        };
    }
}
