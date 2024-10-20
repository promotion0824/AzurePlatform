// -----------------------------------------------------------------------
// <copyright file="AzureAlertTests.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1600 // Elements should be documented

using GrafanaZendeskIntegration.FunctionApp.Models;
using GrafanaZendeskIntegration.FunctionApp.Services;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

[TestClass]
[ExcludeFromCodeCoverage]
public class AzureAlertTests
{
    private readonly StorageClient storageClient = new();
    private readonly IOptions<ZendeskClientOptions> zendeskClientOptions = Options.Create(new ZendeskClientOptions());
    private readonly Mock<IHttpClientFactory> mockHttpClientFactory = new();
    private readonly Mock<ILoggerFactory> loggerFactory = new();
    private readonly Mock<ILogger> log = new();

    public AzureAlertTests()
    {
        this.loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.log.Object);
    }

    [TestMethod]
    public async Task AzureMonitorCommonAlertSchema_Should_Parse()
    {
        const string testFilename = "TestFiles/SampleAzureAlert.json";
        var azureAlertMessage = await File.ReadAllTextAsync(testFilename);

        var azureAlert = JsonConvert.DeserializeObject<AzureMonitorCommonAlertSchema>(azureAlertMessage);
        Assert.IsNotNull(azureAlert);
        Assert.AreEqual("azureMonitorCommonAlertSchema", azureAlert.schemaId);
    }

    [TestMethod]
    public async Task AzureMonitorCommonAlertSchema_Should_Parse_AzureStaticMetricAlert_Test_Alerts()
    {
        const string testFilename = @"TestFiles/AzureStaticMetricAlert.json";
        string azureAlertMessage = await File.ReadAllTextAsync(testFilename);

        var azureAlert = JsonConvert.DeserializeObject<AzureMonitorCommonAlertSchema>(azureAlertMessage);

        Assert.IsNotNull(azureAlert);
        Assert.AreEqual("azureMonitorCommonAlertSchema", azureAlert.schemaId);
    }

    //[TestMethod]
    //public async Task Function_Can_Process_Valid_Azure_ActionGroup_Test_Alert()
    //{
    //    const string testFilename = @"TestFiles/AzureStaticMetricAlert.json";
    //    string azureAlertString = await File.ReadAllTextAsync(testFilename);

    //    DefaultHttpContext httpContext = await DefaultHttpContext(azureAlertString);

    //    this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "dev";

    //    Mock<IStorageClient> mockStorageClient = new();
    //    Mock<ZendeskClient> zendeskClient = new(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);

    //    // Assume messages will always log successfully
    //    mockStorageClient
    //        .Setup(x => x.LogMessage(It.IsAny<string>(), "AzureAlert", LogMessageType.AlertMessage))
    //        .Returns(Task.CompletedTask);

    //    AzureAlertZendeskIntegration function = new(this.loggerFactory.Object, mockStorageClient.Object, zendeskClient.Object);

    //    var run = await function.Run(httpContext.Request);

    //    Assert.IsNotNull(run);
    //    Assert.IsInstanceOfType(run, typeof(OkResult));
    //}

    [TestMethod]
    public async Task ConstructAzureAlertMessage_Should_Not_Process_Alerts_For_Unsupported_Environments()
    {
        // Construct Alert Message
        var alert = new AzureMonitorCommonAlertSchema();
        alert.data.customProperties.environment = "unsupported";
        this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "supported";

        // Create Zendesk Client
        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), mockHttpClientFactory.Object, this.loggerFactory.Object);

        var result = await zendeskClient.ConstructAzureAlertMessage(alert);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public async Task ConstructAzureAlertMessage_Should_Process_Alerts_For_Supported_Environments()
    {
        // Construct Alert Message
        var alert = new AzureMonitorCommonAlertSchema();
        alert.data.customProperties.environment = "supported";

        // Create Zendesk Client
        this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "supported";
        Mock<IStorageClient> mockStorageClient = new();

        // Assume messages will always log successfully
        mockStorageClient
            .Setup(x => x.LogMessage(It.IsAny<string>(), "AzureAlert", LogMessageType.AlertMessage))
            .Returns(Task.CompletedTask);
        mockStorageClient
            .Setup(x => x.InsertOrUpdateZendeskTicketLog(It.IsAny<ZendeskTicketLog>()))
            .Returns(Task.CompletedTask);

        // No existing Zendesk Ticket exists with an identical hash
        mockStorageClient
            .Setup(x => x.GetZendeskTicketLog(It.IsAny<string>()))
            .ReturnsAsync(null as ZendeskTicketLog);

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var result = await zendeskClient.ConstructAzureAlertMessage(alert);

        Assert.AreNotEqual(string.Empty, result);
    }

    private static async Task<DefaultHttpContext> DefaultHttpContext(string azureAlertString)
    {
        DefaultHttpContext httpContext = new()
        {
            Request =
            {
                Method = "POST",
                Scheme = "http",
                Host = new HostString("localhost"),
                ContentType = "application/json",
            },
        };
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        await writer.WriteAsync(azureAlertString);
        await writer.FlushAsync();
        stream.Position = 0;
        httpContext.Request.Body = stream;
        return httpContext;
    }
}
