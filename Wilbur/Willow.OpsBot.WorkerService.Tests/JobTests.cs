using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Willow.OpsBot.Common.Services;
using Willow.OpsBot.WorkerService.Models.Options;
using Xunit;

namespace Willow.OpsBot.WorkerService.Tests;

public class JobTests
{
    [Fact]
    public async Task JobStartsCleanupOfFirewallRules()
    {
        var mockAzure = new Mock<IAzureManagement>();
        var mockAppLiveTime = new Mock<IHostApplicationLifetime>();
        var mockLogger = new Mock<ILogger<Job>>();
        var job = new Job(mockLogger.Object, mockAppLiveTime.Object,
            mockAzure.Object, Options.Create(new CleanupOptions()));

        await job.StartAsync(CancellationToken.None);

        mockAzure.Verify(i => i.RemoveTimeBasedFirewallRule(It.IsAny<List<string>?>()), Times.Once);
        mockAppLiveTime.Verify(i => i.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task JobLogsAndStopsOnException()
    {
        var mockAzure = new Mock<IAzureManagement>();
        var mockAppLiveTime = new Mock<IHostApplicationLifetime>();
        var mockLogger = new Mock<ILogger<Job>>();
        var job = new Job(mockLogger.Object, mockAppLiveTime.Object,
            mockAzure.Object, Options.Create(new CleanupOptions()));

        mockAzure.Setup(i => i.RemoveTimeBasedFirewallRule(It.IsAny<List<string>?>()))
            .Throws(new Exception("Test Exception"));

        await job.StartAsync(CancellationToken.None);

        mockAzure.Verify(i => i.RemoveTimeBasedFirewallRule(It.IsAny<List<string>?>()), Times.Once);
        mockAppLiveTime.Verify(i => i.StopApplication(), Times.Once);

        mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<object>(o => o.ToString() == "Unhandled exception!"),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
