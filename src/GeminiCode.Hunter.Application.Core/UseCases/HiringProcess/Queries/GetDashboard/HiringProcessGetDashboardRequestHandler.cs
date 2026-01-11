using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Exceptions;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetDashboard;

public class HiringProcessGetDashboardRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<HiringProcessGetDashboardRequestHandler> logger)
    : IRequestHandler<HiringProcessGetDashboardRequest, HiringProcessGetDashboardResponse>
{
    public async Task<HiringProcessGetDashboardResponse> Handle(
        HiringProcessGetDashboardRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting hiring process dashboard statistics");

        // Get current user to check role
        var user = await unitOfWork.UserRepository.GetByIdAsync(currentUser.Id) 
            ?? throw new NotFoundException($"User with PublicId {currentUser.Id} not found");

        Dictionary<HiringProcessStatus, int> stats;

        // If user is not admin, get stats only for their referrals
        if (!currentUser.Profile.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            stats = await unitOfWork.HiringProcessRepository.GetDashboardStatsByUserAsync(user.UserId);
            logger.LogInformation("Partner user viewing dashboard for their referrals only. UserId: {UserId}", user.UserId);
        }
        else
        {
            stats = await unitOfWork.HiringProcessRepository.GetDashboardStatsAsync();
            logger.LogInformation("Admin user viewing all dashboard statistics");
        }

        var statusCounts = stats.ToDictionary(
            kvp => kvp.Key.ToString(), 
            kvp => kvp.Value
        );

        var totalProcesses = stats.Values.Sum();
        var hiredCount = stats.GetValueOrDefault(HiringProcessStatus.Hired, 0);
        var reprovedCount = stats.GetValueOrDefault(HiringProcessStatus.Reproved, 0);
        var activeProcesses = totalProcesses - hiredCount - reprovedCount;

        var response = new HiringProcessGetDashboardResponse
        {
            StatusCounts = statusCounts,
            TotalProcesses = totalProcesses,
            ActiveProcesses = activeProcesses,
            HiredCount = hiredCount,
            ReprovedCount = reprovedCount
        };

        logger.LogInformation("Dashboard statistics retrieved. Total: {Total}, Active: {Active}, Hired: {Hired}, Reproved: {Reproved}", 
            totalProcesses, activeProcesses, hiredCount, reprovedCount);

        return response;
    }
}
