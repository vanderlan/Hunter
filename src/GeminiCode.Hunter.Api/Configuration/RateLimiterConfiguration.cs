using GeminiCode.Hunter.Domain.Core.Exceptions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace GeminiCode.Hunter.Api.Configuration;

public static class RateLimiterConfiguration
{
    public static void ConfigureRateLimit(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Login rate limit: 5 requests per minute per IP
            options.AddFixedWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:Login:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Login:WindowInSeconds"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // CV Analysis rate limit: 10 requests per 10 minutes per IP
            options.AddFixedWindowLimiter("cvanalysis", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:CvAnalysis:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:CvAnalysis:WindowInSeconds"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = new ExceptionResponse("Too many requests. Please try again later.", NotificationType.Error)
                {
                    Title = "Rate limit exceeded",
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });
    }
}
