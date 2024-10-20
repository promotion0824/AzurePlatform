// -----------------------------------------------------------------------
// <copyright file="ZendeskClientTests.PostZendeskTicket.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using GrafanaZendeskIntegration.FunctionApp.Models;
using GrafanaZendeskIntegration.FunctionApp.Services;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using Newtonsoft.Json;

[TestClass]
public partial class ZendeskClientTests
{
    [TestMethod]
    [TestCategory("Integration")]
    public async Task PostZendeskTicket_ReturnsAcceptedStatusCode()
    {
        const string testFilename = "TestFiles/2516949262339143706_GrafanaAlert.json";
        string grafanaAlertsMessage = await File.ReadAllTextAsync(testFilename);

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, null, this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var grafanaAlert = GrafanaAlert.CreateInstance(grafanaAlertsMessage);
        var zendeskTicketMessage = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);
        var zendeskResponse = await zendeskClient.PostZendeskTicket(JsonConvert.SerializeObject(zendeskTicketMessage));

        Assert.IsNotNull(zendeskResponse);
    }
}
