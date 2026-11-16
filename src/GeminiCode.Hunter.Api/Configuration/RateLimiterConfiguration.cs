using GeminiCode.Hunter.Domain.Core.Exceptions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace GeminiCode.Hunter.Api.Configuration;

/// <summary>Names of the rate limiting policies, shared between configuration and controllers.</summary>
public static class RateLimitPolicies
{
    public const string Login = "login";
    public const string CvAnalysis = "cvanalysis";

    /// <summary>Anonymous read endpoints, partitioned by client IP.</summary>
    public const string Public = "public";

    /// <summary>Anonymous AI-backed CV match, partitioned by client IP. The expensive one.</summary>
    public const string PublicCvMatch = "public-cvmatch";
}

public static class RateLimiterConfiguration
{
    public static void ConfigureRateLimit(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Login rate limit: 5 requests per minute
            options.AddFixedWindowLimiter(RateLimitPolicies.Login, limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:Login:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Login:WindowInSeconds"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Authenticated CV Analysis rate limit: 10 requests per 10 minutes
            options.AddFixedWindowLimiter(RateLimitPolicies.CvAnalysis, limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:CvAnalysis:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:CvAnalysis:WindowInSeconds"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Public endpoints are open to the internet, so limits are per client IP.
            // Requires UseForwardedHeaders so the IP survives the nginx proxy.
            options.AddPolicy(RateLimitPolicies.Public, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ClientKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue<int?>("RateLimiting:Public:PermitLimit") ?? 120,
                        Window = TimeSpan.FromSeconds(configuration.GetValue<int?>("RateLimiting:Public:WindowInSeconds") ?? 60),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimitPolicies.PublicCvMatch, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ClientKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue<int?>("RateLimiting:PublicCvMatch:PermitLimit") ?? 3,
                        Window = TimeSpan.FromSeconds(configuration.GetValue<int?>("RateLimiting:PublicCvMatch:WindowInSeconds") ?? 3600),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

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

    private static string ClientKey(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
