#nullable enable
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Willow.OpsBot.Api.Logging;

public class VersionTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName();
        // Set version number explicitly rather than relying on app insights default logic which will set the .net version as app version when in containers
        var version = assemblyName?.Version?.ToString();
        if (!string.IsNullOrWhiteSpace(version))
        {
            telemetry.Context.Component.Version = version;
        }
    }
}
