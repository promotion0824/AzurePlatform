// -----------------------------------------------------------------------
// <copyright file="ZendeskClientTests.UpdateTicketTimesTrigered.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using GrafanaZendeskIntegration.FunctionApp.Services;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using System.Net;

public partial class ZendeskClientTests
{
    [TestMethod]
    [TestCategory("Integration")]
    public async Task UpdateTicketTimesTriggered_ReturnsAcceptedStatusCode()
    {
        const string zendeskTicketId = "26551";
        const int timesTriggered = 6;

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, null, this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var statusCode = await zendeskClient.UpdateZendeskTicket(zendeskTicketId, timesTriggered, "open");

        Assert.IsTrue(statusCode.Equals(HttpStatusCode.Accepted));
    }
}
