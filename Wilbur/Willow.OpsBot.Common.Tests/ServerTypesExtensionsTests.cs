using FluentAssertions;
using Willow.OpsBot.Common.Extensions;
using Willow.OpsBot.Common.Models.Enums;
using Xunit;

namespace Willow.OpsBot.Common.Tests;

public class ServerTypesExtensionsTests
{
    [Fact]
    public void ShouldThrowExceptionsForUnknownTypes()
    {
        var missingType = "someservice";
        Assert.ThrowsAny<Exception>(() => missingType.GetServerType());
    }

    [Fact]
    public void ShouldThrowExceptionsForUnknownServerTypes()
    {
        var missingType = (ServerTypes)999;
        Assert.ThrowsAny<Exception>(() => missingType.GetAzureType());
    }

    [Fact]
    public void ShouldMapKnownTypes()
    {
        "microsoft.sql/servers".GetServerType().Should().Be(ServerTypes.SqlServer);
        "microsoft.dbforpostgresql/servers".GetServerType().Should().Be(ServerTypes.PostgreSql);
    }

    [Fact]
    public void ShouldMapKnownServerTypes()
    {
        ServerTypes.PostgreSql.GetAzureType().Should().Be("microsoft.dbforpostgresql/servers");
        ServerTypes.SqlServer.GetAzureType().Should().Be("microsoft.sql/servers");
    }
}
