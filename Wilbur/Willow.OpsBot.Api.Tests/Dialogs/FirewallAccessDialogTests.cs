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
using Willow.OpsBot.Api.Tests.Common;
using Willow.OpsBot.Api.Tests.Dialogs.TestData;
using Willow.OpsBot.Api.Tests.Mocks;
using Willow.OpsBot.Common.Models.Enums;
using Willow.OpsBot.Common.Services;
using Xunit;
using Xunit.Abstractions;

namespace Willow.OpsBot.Api.Tests.Dialogs;

public class FirewallAccessDialogTests : BotTestBase
{
    private readonly Mock<ILogger<FirewallAccessDialog>> _mockLoggerFirewallAccessDialog = new();
    private readonly Mock<IAzureManagement> _mockAzureAccess = new();
    private readonly Mock<IConfiguration> _mockConfiguration = new();

    private readonly IOptions<AccessServersOptions> _accessServersOptions =
        Options.Create(new AccessServersOptions
        {
            PostgreSql = new[]
            {
                "test-server-pg-a",
                "test-server-pg-b"
            },
            SqlServer = new[]
            {
                "test-server-sql-a",
                "test-server-sql-b"
            }
        });

    public FirewallAccessDialogTests(ITestOutputHelper output)
        : base(output)
    {
        _mockAzureAccess.Setup(i => i.AccessibleServer(It.IsAny<ServerTypes>(), It.IsAny<string>()))
            .ReturnsAsync((ServerTypes _, string name) => !name.Contains("invalid"));
    }

    [Fact]
    public async Task PromptsForAccessType()
    {
        var sut = new FirewallAccessDialog(_mockAzureAccess.Object, _mockLoggerFirewallAccessDialog.Object,
            _accessServersOptions, _mockConfiguration.Object, new MockGraphServiceClientFactory(),
            Options.Create(new UrlsOptions { IpLookupSite = "https://test.com/" }));

        var testClient = new DialogTestClient(Channels.Test, sut, new FirewallAccessDetails(),
            new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");


        // Assert
        reply.Attachments.MatchSnapshot();

        reply.InputHint.Should().BeEquivalentTo(InputHints.ExpectingInput);
    }

    [Fact]
    public async Task HandleNullOptions()
    {
        var sut = new FirewallAccessDialog(_mockAzureAccess.Object, _mockLoggerFirewallAccessDialog.Object,
            _accessServersOptions, _mockConfiguration.Object, new MockGraphServiceClientFactory(),
            Options.Create(new UrlsOptions { IpLookupSite = "https://test.com/" }));
        var testClient = new DialogTestClient(Channels.Test, sut, null, new[] { new XUnitDialogTestLogger(Output) });

        // Act
        var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");

        reply.InputHint.Should().BeEquivalentTo(InputHints.ExpectingInput);
    }

    [Theory]
    [MemberData(nameof(FirewallAccessTestsDataGenerator.InvalidFlows),
        MemberType = typeof(FirewallAccessTestsDataGenerator))]
    public async Task InvalidDialogFlowTests(TestDataObject testData)
    {
        // Arrange
        var testCaseData = testData.GetObject<FirewallAccessDetailsTestCase>();
        var sut = new FirewallAccessDialog(_mockAzureAccess.Object, _mockLoggerFirewallAccessDialog.Object,
            _accessServersOptions, _mockConfiguration.Object, new MockGraphServiceClientFactory(),
            Options.Create(new UrlsOptions { IpLookupSite = "https://test.com/" }));
        var testClient = new DialogTestClient(Channels.Test, sut, testCaseData.InitialData,
            new[] { new XUnitDialogTestLogger(Output) });

        // Execute the test case
        foreach (var step in testCaseData.UtterancesAndReplies) await ValidateResponse(step, testClient, testCaseData);

        testClient.DialogTurnResult.Status.Should().Be(DialogTurnStatus.Waiting);
    }

    [Theory]
    [MemberData(nameof(FirewallAccessTestsDataGenerator.StartLoginFlows),
        MemberType = typeof(FirewallAccessTestsDataGenerator))]
    public async Task UptoLoginDialogFlowTests(TestDataObject testData)
    {
        // Arrange
        var testCaseData = testData.GetObject<FirewallAccessDetailsTestCase>();
        var sut = new FirewallAccessDialog(_mockAzureAccess.Object, _mockLoggerFirewallAccessDialog.Object,
            _accessServersOptions, _mockConfiguration.Object, new MockGraphServiceClientFactory(),
            Options.Create(new UrlsOptions { IpLookupSite = "https://test.com/" }));
        var testClient = new DialogTestClient(Channels.Test, sut, testCaseData.InitialData,
            new[] { new XUnitDialogTestLogger(Output) });

        // Execute the test case
        foreach (var step in testCaseData.UtterancesAndReplies) await ValidateResponse(step, testClient, testCaseData);

        testClient.DialogContext.Child.ActiveDialog.Id.Should().BeEquivalentTo("OAuthPrompt");
        testClient.DialogTurnResult.Status.Should().Be(DialogTurnStatus.Waiting);
    }

    private static async Task<IMessageActivity?> ProcessTestStep(UtteranceAndReply step, DialogTestClient testClient)
    {
        if (!string.IsNullOrWhiteSpace(step.Utterance))
            return await testClient.SendActivityAsync<IMessageActivity>(step.Utterance);
        if (step.Activity != null) return await testClient.SendActivityAsync<IMessageActivity>(step.Activity);
        return testClient.GetNextReply<IMessageActivity>();
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("127.0.0.1.1", false)]
    [InlineData("127.0.1", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("266.0.0.1", false)]
    [InlineData("blue", false)]
    private void CanDetermineIpAddresses(string? ip, bool isValid)
    {
        FirewallAccessDialog.IsValidIp(ip).Should()
            .Be(isValid, $"{ip} was said to {(isValid ? "Valid" : "Invalid")}");
    }

    [Theory]
    [MemberData(nameof(FirewallAccessTestsDataGenerator.MockLoginFlows),
        MemberType = typeof(FirewallAccessTestsDataGenerator))]
    public async Task MockedLoginDialogFlowTests(TestDataObject testData)
    {
        // Arrange
        var testCaseData = testData.GetObject<FirewallAccessDetailsTestCase>();
        var sut = new FirewallAccessDialog(_mockAzureAccess.Object, _mockLoggerFirewallAccessDialog.Object,
                _accessServersOptions, _mockConfiguration.Object, new MockGraphServiceClientFactory(),
                Options.Create(new UrlsOptions { IpLookupSite = "https://test.com/" }))
            { LoginPromptDialogId = "MockLoginPrompt" };

        PromptValidator<string> promptValidator = (_, _) => Task.FromResult(true);
        var mockLogin = SimpleMockFactory.CreateMockDialog<TextPrompt>(new TokenResponse { Token = "fakeToken" },
            sut.LoginPromptDialogId, promptValidator);


        sut.Dialogs.Add(mockLogin.Object);
        var testClient = new DialogTestClient(Channels.Test, sut, testCaseData.InitialData,
            new[] { new XUnitDialogTestLogger(Output) });

        // Execute the test case
        foreach (var step in testCaseData.UtterancesAndReplies) await ValidateResponse(step, testClient, testCaseData);


        testClient.DialogTurnResult.Result.Should().BeEquivalentTo(testCaseData.ExpectedResult);
        testClient.DialogTurnResult.Status.Should().Be(DialogTurnStatus.Complete);
    }


    private static async Task ValidateResponse(UtteranceAndReply step, DialogTestClient testClient,
        FirewallAccessDetailsTestCase testCaseData)
    {
        try
        {
            var reply = await ProcessTestStep(step, testClient);
            switch (step.ReplyType)
            {
                case ReplyType.Text:
                    var text = reply?.Text;
                    text.Should().NotBeNullOrWhiteSpace();
                    text.Should().BeEquivalentTo(step.ReplyOrSnapshotName);
                    break;
                case ReplyType.Hero:
                    var speak = reply?.Speak;
                    speak.Should().NotBeNullOrWhiteSpace();
                    speak.Should().BeEquivalentTo(step.ReplyOrSnapshotName);
                    break;
                case ReplyType.Card:
                    if (step.ReplyOrSnapshotName == "oauth-prompt")
                        reply?.Attachments.MatchSnapshot(StepSnapshotName(testCaseData, step), options =>
                            options.IgnoreField("[*].content.tokenExchangeResource.id"));
                    else
                        reply?.Attachments.MatchSnapshot(StepSnapshotName(testCaseData, step));

                    break;
                case ReplyType.Skip:
                    break;
                default:
                    throw new Exception($"Unhandled reply type {step.ReplyType}");
            }
        }
        catch (Exception e)
        {
            if (step.ReplyType == ReplyType.Exception)
                e.Message.Should().BeEquivalentTo(step.ReplyOrSnapshotName);
            else
                throw;
        }
    }


    private static string StepSnapshotName(FirewallAccessDetailsTestCase testCaseData, UtteranceAndReply step)
    {
        var name = new XunitSnapshotFullNameReader().ReadSnapshotFullName();
        var snapshotName = $"{name.Filename}.{testCaseData.Name.Replace(" ", "")}.{step.ReplyOrSnapshotName}";
        return snapshotName;
    }
}
