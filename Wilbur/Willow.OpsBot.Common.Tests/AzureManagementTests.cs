using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Willow.OpsBot.Common.Models;
using Willow.OpsBot.Common.Models.AzGraph;
using Willow.OpsBot.Common.Services;
using Xunit;

namespace Willow.OpsBot.Common.Tests;

public class AzureManagementTests
{
    private readonly AzureManagement _azureManagement = new(new Mock<ILogger<AzureManagement>>().Object);

    [Fact]
    public void ShouldContinueCheckingRulesEvenIfOneRuleFailsToRemove()
    {
        var mockLogger = new Mock<ILogger<AzureManagement>>();
        var azureManagement = new AzureManagement(mockLogger.Object);
        var called = 0;
        azureManagement.CleanupRules(new ResourceLocation
        {
            Name = "test",
            SubscriptionId = Guid.NewGuid().ToString(),
            ResourceGroup = "rg",
            Type = "test/type"
        }, new[]
        {
            new GenericFirewallRule("throw", $"throw{AzureManagement.OpsBotIdentifier}1", "rule", "127.0.0.1",
                "127.0.0.1"),
            new GenericFirewallRule("throw", $"throw{AzureManagement.OpsBotIdentifier}1", "rule", "127.0.0.1",
                "127.0.0.1"),
            new GenericFirewallRule("continue", $"remove{AzureManagement.OpsBotIdentifier}1", "rule", "127.0.0.1",
                "127.0.0.1")
        }, s =>
        {
            called++;
            if (s.Contains("throw")) throw new Exception("Test Exception");
            return Task.CompletedTask;
        });

        called.Should().Be(3);

        mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<object>(o => o.ToString()!.Contains("Failed to remove")),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(2));
    }

    [Fact]
    public void ShouldCallRemoveRuleForRuleToBeRemoves()
    {
        var wasCalled = false;
        _azureManagement.CleanupRules(new ResourceLocation
            {
                Name = "test",
                SubscriptionId = Guid.NewGuid().ToString(),
                ResourceGroup = "rg",
                Type = "test/type"
            },
            new[]
            {
                new GenericFirewallRule("a", $"remove{AzureManagement.OpsBotIdentifier}1", "rule", "127.0.0.1",
                    "127.0.0.1")
            }, s =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            });

        wasCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("aRule")]
    [InlineData("aRule_WAB_asdf")]
    [InlineData("aRule_OB_asdf")]
    [InlineData("office")]
    [InlineData("User Home")]
    public void ShouldNotRemoveRulesWithoutDates(string ruleName)
    {
        AzureManagement.RuleShouldBeRemoved(ruleName).Should().BeFalse();
        ;
    }


    [Theory]
    [InlineData("ClientIPAddress_2021-07-22_02:22:09")]
    [InlineData("ClientIPAddress_2021-07-22")]
    [InlineData("user_WAB_20210818232644")]
    [InlineData("usesr_OB_1627280253")]
    public void ShouldRemoveOldRule(string ruleName)
    {
        AzureManagement.RuleShouldBeRemoved(ruleName).Should().BeTrue();
        ;
    }

    [Fact]
    public void ShouldKeepRecentOpsBotRules()
    {
        var ruleName = $"usesr_OB_{DateTime.UtcNow.AddHours(-23).ToDateTimeOffset().ToUnixTimeSeconds()}";
        AzureManagement.RuleShouldBeRemoved(ruleName).Should().BeFalse();
    }

    [Fact]
    public void ShouldKeepRecentWabBotRules()
    {
        var ruleName = $"usesr_WAB_{DateTime.UtcNow.AddHours(-23):yyyyMMddHHmmss}";
        AzureManagement.RuleShouldBeRemoved(ruleName).Should().BeFalse();
    }

    [Fact]
    public void ShouldKeepRecentOtherRules()
    {
        var ruleName = $"usesr_WAB_{DateTime.UtcNow.AddDays(-26):yyyy-M-d}";
        AzureManagement.RuleShouldBeRemoved(ruleName).Should().BeFalse();
    }

    [Fact]
    public void RuleNameShouldContainFirstPartOfEmail()
    {
        var ruleName = AzureManagement.MakeRuleName("asdf@gmail.com");
        ruleName.Should().StartWithEquivalentOf("asdf");
    }

    [Fact]
    public void RuleNameShouldContainBotIdentifier()
    {
        var ruleName = AzureManagement.MakeRuleName("asdf@gmail.com");
        ruleName.Should().Contain(AzureManagement.OpsBotIdentifier);
    }
}
