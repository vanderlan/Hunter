using Microsoft.AspNetCore.OutputCaching;

namespace GeminiCode.Hunter.Api.Configuration;

public static class CacheConfiguration
{
    private const string CvAnalysisCachePolicyName = "CvAnalysisCache";
    private const int CvAnalysisCacheDurationDays = 7;

    public static void ConfigureOutputCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            // Configure CV Analysis cache policy with 7 days duration
            options.AddPolicy(CvAnalysisCachePolicyName, builder =>
            {
                builder
                    .Expire(TimeSpan.FromDays(CvAnalysisCacheDurationDays))
                    .SetVaryByRouteValue("id")
                    .Tag("cv-analysis");
            });

            // Default policy for other endpoints (disabled by default)
            options.DefaultExpirationTimeSpan = TimeSpan.Zero;
        });
    }

    public static void UseOutputCaching(this IApplicationBuilder app)
    {
        app.UseOutputCache();
    }

    public static string GetCvAnalysisCachePolicyName() => CvAnalysisCachePolicyName;
}
