using FluentAssertions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Snapshooter.Xunit;
using Willow.OpsBot.Api.Dialogs;
using Willow.OpsBot.Api.Models.Options;
using Willow.OpsBot.Api.Services;
using Willow.OpsBot.Api.Tests.Common;
using Willow.OpsBot.Common.Models.Enums;
using Willow.OpsBot.Common.Services;
using Xunit;
using Xunit.Abstractions;

namespace Willow.OpsBot.Api.Tests.Dialogs;

public class AccessDialogTests : BotTestBase
{
    private readonly Mock<FirewallAccessDialog> _mockFirewallAccessDialog;
    private readonly Mock<ILogger<AccessDialog>> _mockLogger = new();
    private readonly Mock<ILogger<FirewallAccessDialog>> _mockLoggerFirewallAccessDialog = new();
    private readonly Mock<IConfiguration> _mockConfiguration = new();

    public AccessDialogTests(ITestOutputHelper output)
        : base(output)
    {
        _mockFirewallAccessDialog = SimpleMockFactory.CreateMockDialog<FirewallAccessDialog>(
            new FirewallAccessDetails(), new Mock<IAzureManagement>().Object, _mockLoggerFirewallAccessDialog.Object,
            new Mock<IOptions<AccessServersOptions>>().Object, new Mock<IConfiguration>().Object,
            new Mock<IGraphServiceClientFactory>().Object, Options.Create(new UrlsOptions()));
    }

    [Fact]
    public async Task PromptsForAccessType()
    {
        var sut = new AccessDialog(_mockFirewallAccessDialog.Object, _mockLogger.Object, _mockConfiguration.Object);
        var testClient = new DialogTestClient(Channels.Test, sut, new AccessDetails(),
            new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");

        // Assert
        reply.Attachments.MatchSnapshot();
        reply.InputHint.Should().BeEquivalentTo(InputHints.AcceptingInput);

        var secondReply = await testClient.SendActivityAsync<IMessageActivity>("Firewall");
        secondReply.Text.Should()
            .BeEquivalentTo(
                "FirewallAccessDialog mock invoked with options: FirewallAccessDetails { Server = , Ip = , Type = 0, Prefilled = False }");
        _mockFirewallAccessDialog.Verify(
            i => i.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<FirewallAccessDetails>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleNullOptions()
    {
        var sut = new AccessDialog(_mockFirewallAccessDialog.Object, _mockLogger.Object, _mockConfiguration.Object);
        var testClient = new DialogTestClient(Channels.Test, sut, null, new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");


        reply.InputHint.Should().BeEquivalentTo(InputHints.AcceptingInput);
    }

    [Fact]
    public async Task SkipsPromptsForAccessTypeWhenProvided()
    {
        var sut = new AccessDialog(_mockFirewallAccessDialog.Object, _mockLogger.Object, _mockConfiguration.Object);
        var testClient = new DialogTestClient(Channels.Test, sut, new AccessDetails { Type = AccessTypes.Firewall },
            new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");

        // Assert
        reply.Text.Should()
            .BeEquivalentTo(
                "FirewallAccessDialog mock invoked with options: FirewallAccessDetails { Server = , Ip = , Type = 0, Prefilled = False }");
        _mockFirewallAccessDialog.Verify(
            i => i.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<FirewallAccessDetails>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }
}
