#nullable enable
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Willow.OpsBot.Api.Logging;

// Filter out successful health check based request telemetry
public class HealthCheckTelemetryFilter : ITelemetryProcessor
{

    private readonly ITelemetryProcessor _next;

    private readonly List<string> _healthCheckPaths = new List<string>
    {
        "/health", "/healthz", "/healthcheck", "/liveness",
        "/readiness", "/status", "/ping"
    };

    public HealthCheckTelemetryFilter(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry { Success: true } req &&
            _healthCheckPaths.Any(p => req.Url.PathAndQuery.StartsWith(p)))
        {
            return;
        }

        _next.Process(item);
    }
}
