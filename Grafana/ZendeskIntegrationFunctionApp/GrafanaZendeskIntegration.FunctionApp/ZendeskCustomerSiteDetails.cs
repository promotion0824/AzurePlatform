// -----------------------------------------------------------------------
// <copyright file="ZendeskCustomerSiteDetails.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp;

using System.IO;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using global::GrafanaZendeskIntegration.FunctionApp.Services;

public class ZendeskCustomerSiteDetails
{
    private readonly IOptions<StorageClientOptions> storageClientOptions;
    private readonly TableClient tableClient;
    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZendeskCustomerSiteDetails"/> class.
    /// </summary>
    /// <param name="storageClientOptions"></param>
    /// <param name="zendeskClientOptions"></param>
    public ZendeskCustomerSiteDetails(
        IOptions<StorageClientOptions> storageClientOptions,
        TableClient tableClient,
        ILoggerFactory log)
    {
        this.storageClientOptions = storageClientOptions;
        this.tableClient = tableClient;
        this.log = log.CreateLogger<ZendeskCustomerSiteDetails>();
    }

    /// <summary>
    ///     Webhook to receive changes to Customer & Site details from an instance of WillowTwin
    ///     TODO: This function currently just logs the received message to a storage account. Awaiting actual message format
    ///     from Engineering team.
    /// </summary>
    /// <param name="req"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Function("ZendeskCustomerSiteDetails")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "zendesk/customer-site-details")] HttpRequest req, IStorageClient storageClient)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Save the raw Customer Site Details
        await storageClient.LogMessage(requestBody, "CustomerSiteDetails", LogMessageType.CustomerSiteMessage);

        return new OkResult();
    }
}
