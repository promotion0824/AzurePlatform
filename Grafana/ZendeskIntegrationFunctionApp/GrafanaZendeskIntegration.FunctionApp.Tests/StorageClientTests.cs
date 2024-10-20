// -----------------------------------------------------------------------
// <copyright file="StorageClientTests.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using global::GrafanaZendeskIntegration.FunctionApp.Services;
using Microsoft.Extensions.Logging;
using Moq;

[TestClass]
[ExcludeFromCodeCoverage]
public partial class StorageClientTests
{
    private readonly Mock<ILoggerFactory> loggerFactory = new();
    private readonly Mock<ILogger> log = new();
    private StorageClient storageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZendeskClientTests"/> class.
    /// </summary>
    public StorageClientTests()
    {
        this.loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.log.Object);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task SaveGrafanaAlert_Succeeds()
    {
        Mock<ILogger> log = new();
        try
        {
            await storageClient.LogMessage("{'test': 'payload'}", "GrafanaAlert", Models.LogMessageType.AlertMessage);
        }
        catch (Exception ex)
        {
            Assert.Fail("Expected success but got: " + ex.Message);
        }
    }

    [TestMethod]
    public async Task GetZendeskTicketLog_Should_Return_Null_When_No_Ticket_Logs_Exist()
    {
        var page = Page<TableEntity>.FromValues(
            [
            ],
            continuationToken: null,
            Mock.Of<Response>());

        var pages = AsyncPageable<TableEntity>.FromPages(new[] { page });

        var mockTableClient = new Mock<TableClient>(MockBehavior.Strict);
        mockTableClient
            .Setup(x => x.QueryAsync<TableEntity>(
                It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(pages);

        this.storageClient = new StorageClient(Options.Create(new StorageClientOptions()), mockTableClient.Object, this.loggerFactory.Object);
        var result = await this.storageClient.GetZendeskTicketLog("testAlertHash");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetZendeskTicketLog_Should_Return_Latest_Open_Ticket_Log()
    {
        // Arrange test data for table query response
        var page = Page<TableEntity>.FromValues(
        [
            new TableEntity { PartitionKey = "testAlertHash", RowKey = "1", ["UpdatedAt"] = DateTime.Now.AddDays(-1), ["TimesTriggered"] = 1, ["TicketStatus"] = "open" },
            new TableEntity { PartitionKey = "testAlertHash", RowKey = "2", ["UpdatedAt"] = DateTime.Now, ["TimesTriggered"] = 2, ["TicketStatus"] = "open" },
            new TableEntity { PartitionKey = "testAlertHash", RowKey = "3", ["UpdatedAt"] = DateTime.Now.AddDays(-2), ["TimesTriggered"] = 3, ["TicketStatus"] = "closed" },
        ],
        continuationToken: null,
        Mock.Of<Response>());
        var pages = AsyncPageable<TableEntity>.FromPages(new[] { page });

        // Set up the mock table client
        var mockTableClient = new Mock<TableClient>(MockBehavior.Strict);
        mockTableClient
            .Setup(x => x.QueryAsync<TableEntity>(
                It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(pages);
        this.storageClient = new StorageClient(Options.Create(new StorageClientOptions()), mockTableClient.Object, this.loggerFactory.Object);

        var result = await this.storageClient.GetZendeskTicketLog("testAlertHash");

        Assert.IsNotNull(result);
        Assert.AreEqual("2", result.ZendeskTicketId);
    }
}
