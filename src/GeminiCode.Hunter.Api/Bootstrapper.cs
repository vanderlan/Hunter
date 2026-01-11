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
using Microsoft.FeatureManagement;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

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

        // Configure rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Login rate limit: 5 requests per minute per IP
            options.AddFixedWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:Login:PermitLimit", 5);
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Login:WindowInSeconds", 60));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // CV Analysis rate limit: 10 requests per 10 minutes per IP
            options.AddFixedWindowLimiter("cvanalysis", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:CvAnalysis:PermitLimit", 10);
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:CvAnalysis:WindowInSeconds", 600));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                
                var response = new
                {
                    title = "Rate limit exceeded",
                    errors = new[] { "Too many requests. Please try again later." },
                    type = "Error"
                };
                
                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });
    }
}
