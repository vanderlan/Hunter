using GeminiCode.Hunter.Api.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using GeminiCode.Hunter.Api.Configuration;
using GeminiCode.Hunter.Application.Core;
using GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.Create;
using GeminiCode.Hunter.Application.Core.BackgroundServices;
using GeminiCode.Hunter.Crosscutting.Ioc.Dependencies;
using GeminiCode.Hunter.Domain.Core.Authentication;
using GeminiCode.Hunter.Domain.Core.Configuration;
using GeminiCode.Hunter.Domain.Core.Extensions;
using Microsoft.FeatureManagement;

namespace GeminiCode.Hunter.Api;

public static class Bootstrapper
{
    public static void ConfigureApp(this IApplicationBuilder app)
    {
        app.UseSwagger();
        
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "Hunter API");
        });

        app.UseCors(options => options.WithOrigins("*")
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .AllowAnyHeader());

        app.ConfigureGlobalization();

        app.ConfigureHealthCheck();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors();

        // Register ApplicationSettings
        var applicationSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>() ?? new ApplicationSettings();
        services.AddSingleton(applicationSettings);
        
        // Initialize DateTimeExtensions with ApplicationSettings
        DateTimeExtensions.Initialize(applicationSettings);

        services.ConfigureAuthentication(configuration);

        services.AddControllers();

        services.AddValidatorsFromAssemblyContaining<UserCreateRequestValidator>();

        services.ConfigureApiVersioning();

        services.AddMvc(options =>
                        {
                            options.Filters.Add<CustomValidationAttribute>();
                        })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddLocalization(options => options.ResourcesPath = @"Resources");
        services.AddApplicationHealthChecks(configuration);

        services.ConfigureSwagger();

        services.AddDatabaseContext();
        services.AddUnitOfWork();
        services.AddJobServices();
        services.AddAIServices();
        services.AddNotificationServices();
        services.AddFeatureManagement();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IApplicationAssembly).Assembly));

        // Register background service for automatic job synchronization
        services.AddHostedService<JobSyncBackgroundService>();
    }
}
