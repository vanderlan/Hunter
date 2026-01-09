using Microsoft.Extensions.DependencyInjection;
using GeminiCode.Hunter.Infra.Data.UnitOfWork;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Domain.Core.Services;
using GeminiCode.Hunter.Application.Core.Services;

namespace GeminiCode.Hunter.Crosscutting.Ioc.Dependencies;

public static class DependenciesInjectionConfiguration
{
    public static void AddDatabaseContext(this IServiceCollection services)
    {
        services.AddScoped<DataContext>();
    }

    public static void AddUnitOfWork(this IServiceCollection services)
    {
        services.AddTransient<IUnitOfWork, UnitOfWork>();
    }

    public static void AddJobServices(this IServiceCollection services)
    {
        services.AddScoped<IJobScraperService, JobScraperService>();
    }

    public static void AddAIServices(this IServiceCollection services)
    {
        services.AddScoped<IArtificialnteligenceService, ArtificialnteligenceService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<ICvInfoExtractionService, CvInfoExtractionService>();
    }

    public static void AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
    }
}
