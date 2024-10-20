#nullable enable
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;

namespace Willow.OpsBot.Api.Logging;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationInsightsTelemetry(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddApplicationInsightsTelemetry();

        // Telemetry filters
        services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>();

        // Telemetry property initializers
        services.AddSingleton<ITelemetryInitializer, VersionTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, RoleNameTelemetryInitializer>();

        services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();

        services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
        services.AddSingleton<TelemetryInitializerMiddleware>();
        services.AddSingleton<TelemetryLoggerMiddleware>();
    }
}
