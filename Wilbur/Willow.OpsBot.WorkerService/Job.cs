using Microsoft.Extensions.Options;
using Willow.OpsBot.Common.Services;
using Willow.OpsBot.WorkerService.Models.Options;

namespace Willow.OpsBot.WorkerService;

public class Job : BackgroundService
{
    private readonly ILogger<Job> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IAzureManagement _azureManagement;
    private readonly CleanupOptions _cleanupOptions;

    public Job(ILogger<Job> logger, IHostApplicationLifetime appLifetime, IAzureManagement azureManagement,
        IOptions<CleanupOptions> cleanupOptions)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _azureManagement = azureManagement;
        _cleanupOptions = cleanupOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
            await _azureManagement.RemoveTimeBasedFirewallRule(_cleanupOptions.ServersExcluded);
            _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception!");
        }
        finally
        {
            _appLifetime.StopApplication();
        }
    }
}
