using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreatStream.Detection.Engine.Abstractions;
using ThreatStream.Detection.Engine.Configuration;
using ThreatStream.Detection.Engine.Services;

namespace ThreatStream.Detection.Engine;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectionEngine(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DetectionEngineOptions>(configuration.GetSection("DetectionEngine"));

        services.AddDbContext<DetectionDbContext>(options =>
        {
            var connectionString = configuration["DetectionEngine:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("DetectionEngine:ConnectionString is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<DetectionRuleService>();

        services.AddScoped<IAlertManagementService, AlertManagementService>();

        services.AddHttpClient<IDetectionQueryExecutor, ElasticsearchDetectionQueryExecutor>((_, client) =>
        {
            var options = configuration
                .GetSection("DetectionEngine")
                .Get<DetectionEngineOptions>()
                ?? new DetectionEngineOptions();

            client.BaseAddress = new Uri(options.ElasticsearchUrl.TrimEnd('/') + "/");

            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
