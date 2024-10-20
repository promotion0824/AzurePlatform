// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
using Willow.OpsBot.Api.Dialogs;
using Willow.OpsBot.Api.Models.Options;
using Willow.OpsBot.Api.Services;
using Willow.OpsBot.Api.Tests.Common;
using Willow.OpsBot.Common.Models.Enums;
using Willow.OpsBot.Common.Services;
using Xunit;
using Xunit.Abstractions;

namespace Willow.OpsBot.Api.Tests.Dialogs;

public class MainDialogTests : BotTestBase
{
    private readonly Mock<AccessDialog> _mockAccessDialog;
    private readonly FirewallAccessDialog _mockFirewallAccessDialog;
    private readonly Mock<IConfiguration> _mockConfiguration = new();

    private readonly Mock<ILogger<MainDialog>> _mockLogger = new();
    private readonly Mock<ILogger<AccessDialog>> _mockLoggerAccessDialog = new();
    private readonly Mock<ILogger<FirewallAccessDialog>> _mockLoggerFirewallAccessDialog = new();

    public MainDialogTests(ITestOutputHelper output)
        : base(output)
    {
        _mockFirewallAccessDialog = SimpleMockFactory.CreateMockDialog<FirewallAccessDialog>(
            new FirewallAccessDetails(), new Mock<IAzureManagement>().Object, _mockLoggerFirewallAccessDialog.Object,
            new Mock<IOptions<AccessServersOptions>>().Object, new Mock<IConfiguration>().Object,
            new Mock<IGraphServiceClientFactory>().Object, Options.Create(new UrlsOptions())).Object;

        _mockAccessDialog = SimpleMockFactory.CreateMockDialog<AccessDialog>(new AccessDetails(),
            _mockFirewallAccessDialog, _mockLoggerAccessDialog.Object, _mockConfiguration.Object);
    }


    [Fact]
    public async Task StartsTheAccessDialogWithoutATypeByDefault()
    {
        var sut = new MainDialog(_mockAccessDialog.Object, _mockLogger.Object, _mockConfiguration.Object);
        var testClient =
            new DialogTestClient(Channels.Test, sut, middlewares: new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");

        // Assert
        reply.Text.Should().BeEquivalentTo("AccessDialog mock invoked with options: AccessDetails { Type = 0 }");
        _mockAccessDialog.Verify(
            i => i.BeginDialogAsync(It.IsAny<DialogContext>(), It.Is<AccessDetails>(d => d.Type == 0),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("fw")]
    [InlineData("allow my ip")]
    [InlineData("firewall")]
    [InlineData("sql")]
    public async Task StartsTheAccessDialogWithProvidedType(string msg)
    {
        var sut = new MainDialog(_mockAccessDialog.Object, _mockLogger.Object, _mockConfiguration.Object);
        var testClient =
            new DialogTestClient(Channels.Test, sut, middlewares: new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>(msg);

        // Assert
        _mockAccessDialog.Verify(
            i => i.BeginDialogAsync(It.IsAny<DialogContext>(),
                It.Is<AccessDetails>(d => d.Type == AccessTypes.Firewall), It.IsAny<CancellationToken>()), Times.Once);
        reply.Text.Should().BeEquivalentTo("AccessDialog mock invoked with options: AccessDetails { Type = Firewall }");
    }
}
