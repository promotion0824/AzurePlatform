// -----------------------------------------------------------------------
// <copyright file="GrafanaZendeskIntegrationTests.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using GrafanaZendeskIntegration.FunctionApp.Models;
using GrafanaZendeskIntegration.FunctionApp.Services;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

[ExcludeFromCodeCoverage]
public partial class ZendeskClientTests
{
    [TestMethod]
    public async Task Function_Can_Process_Valid_Grafana_Alert()
    {
        GrafanaAlert alert = GenerateTestGrafanaAlert();

        string grafanaAlertString = JsonConvert.SerializeObject(alert);

        Mock<IStorageClient> mockStorageClient = new();

        this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "dev";
        Mock<ZendeskClient> zendeskClient = new(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
 
        // Assume messages will always loggerFactory successfully
        mockStorageClient.Setup(x => x.LogMessage(It.IsAny<string>(), "AzureAlert", LogMessageType.AlertMessage)).Returns(Task.CompletedTask);
        mockStorageClient.Setup(x => x.InsertOrUpdateZendeskTicketLog(It.IsAny<ZendeskTicketLog>())).Returns(Task.CompletedTask);

        // No existing Zendesk Ticket exists with an identical hash
        mockStorageClient.Setup(x => x.GetZendeskTicketLog(It.IsAny<string>())).ReturnsAsync(null as ZendeskTicketLog);

        // Assume the Zendesk Ticket is created successfully
        zendeskClient.Setup(x => x.PostZendeskTicket(It.IsAny<string>())).ReturnsAsync(new ZendeskTicketDetail()
            { ResponseBody = "body", StatusCode = HttpStatusCode.Created, TicketId = "ticketId" });

        GrafanaZendeskIntegration function = new(this.loggerFactory.Object, mockStorageClient.Object, zendeskClient.Object);
        var functionContext = new Mock<FunctionContext>().Object;
        var requestData = new MockHttpRequestData(
            functionContext,
            new MemoryStream(Encoding.UTF8.GetBytes(grafanaAlertString)),
            "Post");
        IActionResult run = await function.Run(requestData);

        Assert.IsNotNull(run);
        Assert.IsInstanceOfType(run, typeof(OkResult));
    }

    [TestMethod]
    public async Task Function_Must_Increment_Open_Tickets()
    {
        GrafanaAlert alert = GenerateTestGrafanaAlert();

        string grafanaAlertString = JsonConvert.SerializeObject(alert);

        Mock<IStorageClient> mockStorageClient = new();
        this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "dev";
        Mock<ZendeskClient> zendeskClient = new(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);

        // Assume messages will always loggerFactory successfully
        mockStorageClient
            .Setup(x => x.LogMessage(It.IsAny<string>(), "GrafanaAlert", LogMessageType.AlertMessage))
            .Returns(Task.CompletedTask);
        mockStorageClient
            .Setup(x => x.InsertOrUpdateZendeskTicketLog(It.IsAny<ZendeskTicketLog>()))
            .Returns(Task.CompletedTask);

        // No existing Zendesk Ticket exists with an identical hash
        mockStorageClient.Setup(x => x.GetZendeskTicketLog(It.IsAny<string>())).ReturnsAsync(null as ZendeskTicketLog);

        // Assume the Zendesk Ticket is created successfully
        zendeskClient.Setup(x => x.PostZendeskTicket(It.IsAny<string>())).ReturnsAsync(new ZendeskTicketDetail()
            { ResponseBody = "body", StatusCode = HttpStatusCode.Created, TicketId = "ticketId" });

        GrafanaZendeskIntegration function = new(this.loggerFactory.Object, mockStorageClient.Object, zendeskClient.Object);

        var functionContext = new Mock<FunctionContext>().Object;
        var requestData = new MockHttpRequestData(functionContext,
            new MemoryStream(Encoding.UTF8.GetBytes(grafanaAlertString)), "Post");
        IActionResult run = await function.Run(requestData);

        Assert.IsNotNull(run);
        Assert.IsInstanceOfType(run, typeof(OkResult));
    }

    [TestMethod]
    public async Task Function_Should_Return_BadRequest_When_Request_Is_Null()
    {
        Mock<IStorageClient> mockStorageClient = new();
        Mock<IZendeskClient> zendeskClient = new();
        Mock<ILoggerFactory> log = new();
        log.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        GrafanaZendeskIntegration function = new(log.Object, mockStorageClient.Object, zendeskClient.Object);

        IActionResult run = await function.Run(null);

        Assert.IsNotNull(run);
        Assert.IsInstanceOfType(run, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Function_Should_Return_BadRequest_When_Request_Body_Is_Empty()
    {
        Mock<IStorageClient> mockStorageClient = new();
        Mock<IZendeskClient> zendeskClient = new();
        Mock<ILoggerFactory> mockLog = new();
        mockLog.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        GrafanaZendeskIntegration function = new(mockLog.Object, mockStorageClient.Object, zendeskClient.Object);
        var functionContext = new Mock<FunctionContext>().Object;
        var requestData = new MockHttpRequestData(functionContext,
            new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)), "Post");
        IActionResult run = await function.Run(requestData);
        Assert.IsNotNull(run);
        Assert.IsInstanceOfType(run, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Function_Should_Identify_Merged_Tickets()
    {
        GrafanaAlert alert = GenerateTestGrafanaAlert();
        string grafanaAlertString = JsonConvert.SerializeObject(alert);

        // Merged Zendesk Ticket record
        ZendeskTicket zendeskTicket = new ZendeskTicket
        {
            ticket = new Ticket
            {
                id = 1111,
                status = "closed",
                tags = ["a", "b", "closed_by_merge", "d"],
            },
        };

        // Parent of merged ticket
        ZendeskTicket parentTicket = new ZendeskTicket
        {
            ticket = new Ticket
            {
                id = 2222,
                status = "open",
                tags = ["a", "b", "d"],
                custom_fields =
                [
                    new CustomField
                    {
                        id = 8855585604367,
                        value = "1",
                    },
                ],
            },
        };


        ZendeskTicketAudits zendeskTicketAudits = new ZendeskTicketAudits
        {
            audits = new List<Audit>
            {
                new Audit
                {
                    via = new Models.Via
                    {
                        source = new Models.Source
                        {
                            rel = "merge",
                            from = new Models.From{ ticket_id = "2222" },
                        },
                    },
                },
            }.ToArray(),
        };

        Mock<IStorageClient> mockStorageClient = new();
        Mock<IZendeskClient> zendeskClient = new();
        Mock<ILoggerFactory> mockLog = new();
        mockStorageClient.Setup(x => x.GetZendeskTicketLog(It.IsAny<string>())).ReturnsAsync(new ZendeskTicketLog()
        { ZendeskTicketId = "1111", TicketStatus = "open", AlertHash = "123", TimesTriggered = 1, UpdatedAt = DateTime.Now });
        zendeskClient.Setup(x => x.ShouldProcessAlert(It.IsAny<GrafanaAlert>())).Returns(true);
        zendeskClient.Setup(x => x.ShowZendeskTicket("1111"))
            .ReturnsAsync(zendeskTicket);
        zendeskClient.Setup(x => x.ShowZendeskTicket("2222"))
            .ReturnsAsync(parentTicket);
        zendeskClient.Setup(x => x.GetZendeskTicketAudits(It.IsAny<string>()))
            .ReturnsAsync(zendeskTicketAudits);
        zendeskClient.Setup(x => x.GetMergedTicketParent(It.IsAny<ZendeskTicketAudits>()))
            .Returns("2222");
        mockLog.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        GrafanaZendeskIntegration function = new(mockLog.Object, mockStorageClient.Object, zendeskClient.Object);
        var functionContext = new Mock<FunctionContext>().Object;
        var requestData = new MockHttpRequestData(functionContext, new MemoryStream(Encoding.UTF8.GetBytes(grafanaAlertString)), "Post");
        IActionResult run = await function.Run(requestData);
        zendeskClient.Verify(x => x.UpdateZendeskTicket("2222", 2, "open"));
        Assert.IsNotNull(run);
        Assert.IsInstanceOfType(run, typeof(OkResult));
    }
}
