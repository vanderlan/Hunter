using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetDashboard;

public class HiringProcessGetDashboardRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<HiringProcessGetDashboardRequestHandler> logger)
    : IRequestHandler<HiringProcessGetDashboardRequest, HiringProcessGetDashboardResponse>
{
    public async Task<HiringProcessGetDashboardResponse> Handle(
        HiringProcessGetDashboardRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting hiring process dashboard statistics");

        var stats = await unitOfWork.HiringProcessRepository.GetDashboardStatsAsync();

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
