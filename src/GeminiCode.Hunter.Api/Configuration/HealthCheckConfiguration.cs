using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace GeminiCode.Hunter.Api.Configuration;

public static class HealthCheckConfiguration
{
    private static readonly string[] DatabaseTags =
            [
                "database"
            ];

    public static void AddApplicationHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddHealthChecks().AddNpgSql(configuration["ConnectionStrings:PostgreSql"], 
            tags: DatabaseTags);

    }

    public static void ConfigureHealthCheck(this IApplicationBuilder app) 
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }
}
