// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Snapshooter.Xunit;
using Willow.OpsBot.Api.Bots;
using Willow.OpsBot.Api.Models.Options;
using Willow.OpsBot.Api.Tests.Common;
using Xunit;

namespace Willow.OpsBot.Api.Tests.Bots;

public class TeamsOpsBotTests
{
    [Fact]
    public async Task ReturnsWelcomeCardOnConversationUpdate()
    {
        // Arrange
        var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");
        var memoryStorage = new MemoryStorage();
        var sut = new TeamsOpsBot<Dialog>(new ConversationState(memoryStorage), new UserState(memoryStorage),
            mockRootDialog.Object, new Mock<ILogger<TeamsOpsBot<Dialog>>>().Object,
            Options.Create(new UrlsOptions { WillowLogo = "https://test.com/logo.png" }));

        // Create conversationUpdate activity
        var conversationUpdateActivity = new Activity
        {
            Type = ActivityTypes.ConversationUpdate,
            MembersAdded = new List<ChannelAccount>
            {
                new() { Id = "theUser" }
            },
            Recipient = new ChannelAccount { Id = "theBot" }
        };
        var testAdapter = new TestAdapter(Channels.Test);

        // Act
        // Send the conversation update activity to the bot.
        await testAdapter.ProcessActivityAsync(conversationUpdateActivity, sut.OnTurnAsync, CancellationToken.None);

        // Assert we got the welcome card
        var reply = (IMessageActivity)await testAdapter.GetNextReplyAsync();
        Assert.Equal(1, reply.Attachments.Count);
        Assert.Equal("application/vnd.microsoft.card.adaptive", reply.Attachments.FirstOrDefault()?.ContentType);
        reply.Attachments.MatchSnapshot();

        // Assert that we started the main dialog.
        reply = (IMessageActivity)await testAdapter.GetNextReplyAsync();
        Assert.Equal("Dialog mock invoked with options: ", reply.Text);
    }

    [Fact]
    public async Task LogsInformationToILogger()
    {
        // Arrange
        var memoryStorage = new MemoryStorage();
        var conversationState = new ConversationState(memoryStorage);
        var userState = new UserState(memoryStorage);

        var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");
        var mockLogger = new Mock<ILogger<TeamsOpsBot<Dialog>>>();
        mockLogger.Setup(x =>
            x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null,
                It.IsAny<Func<object, Exception, string>>()));

        // Run the bot
        var sut = new TeamsOpsBot<Dialog>(conversationState, userState, mockRootDialog.Object, mockLogger.Object,
            Options.Create(new UrlsOptions { WillowLogo = "https://test.com/logo.png" }));
        var testAdapter = new TestAdapter();
        var testFlow = new TestFlow(testAdapter, sut);
        await testFlow.Send("Hi").StartTestAsync();

        // Assert that log was changed with the expected parameters
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(o => o.ToString()!.StartsWith("Running dialog with Message Activity")),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task SavesTurnStateUsingMockWithVirtualSaveChangesAsync()
    {
        // Note: this test requires that SaveChangesAsync is made virtual in order to be able to create a mock.
        var memoryStorage = new MemoryStorage();
        var mockConversationState = new Mock<ConversationState>(memoryStorage)
        {
            CallBase = true
        };

        var mockUserState = new Mock<UserState>(memoryStorage)
        {
            CallBase = true
        };

        var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");
        var mockLogger = new Mock<ILogger<TeamsOpsBot<Dialog>>>();

        // Act
        var sut = new TeamsOpsBot<Dialog>(mockConversationState.Object, mockUserState.Object, mockRootDialog.Object,
            mockLogger.Object, Options.Create(new UrlsOptions { WillowLogo = "https://test.com/logo.png" }));
        var testAdapter = new TestAdapter();
        var testFlow = new TestFlow(testAdapter, sut);
        await testFlow.Send("Hi").StartTestAsync();

        // Assert that SaveChangesAsync was called
        mockConversationState.Verify(
            x => x.SaveChangesAsync(It.IsAny<TurnContext>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mockUserState.Verify(
            x => x.SaveChangesAsync(It.IsAny<TurnContext>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
