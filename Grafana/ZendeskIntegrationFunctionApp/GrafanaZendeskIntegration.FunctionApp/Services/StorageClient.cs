// -----------------------------------------------------------------------
// <copyright file="StorageClient.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public interface IStorageClient
{
    /// <summary>
    ///     Saves a message to Azure Storage.
    /// </summary>
    /// <param name="messageBody">
    ///     The raw message Body.
    /// </param>
    /// <param name="messageName">
    ///     A Name for the message.  Appended to a timestamp. e.g. GrafanaAlert, ZendeskTicket.
    /// </param>
    /// <param name="logMessageType">
    ///     The type of message being logged.
    /// </param>
    /// <returns>
    ///     The <see cref="Task" />.
    /// </returns>
    Task LogMessage(string messageBody, string messageName, LogMessageType logMessageType);

    /// <summary>
    ///
    /// </summary>
    /// <param name="zendeskTicketLog"></param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task InsertOrUpdateZendeskTicketLog(ZendeskTicketLog zendeskTicketLog);

    Task<ZendeskTicketLog> GetZendeskTicketLog(string alertHash);

    Task<ZendeskTicketLog> UpdateZendeskTicketLog(string alertHash, string zendeskTicketId, int timesTriggered,
        string ticketStatus);

}

public partial class StorageClient : IStorageClient
{
    private readonly StorageClientOptions storageClientOptions;
    private readonly TableClient tableClient;
    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageClient"/> class.
    /// </summary>
    public StorageClient()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageClient"/> class.
    /// </summary>
    /// <param name="options">Storage Client Options.</param>
    /// <param name="tableClient">The table client.</param>
    public StorageClient(IOptions<StorageClientOptions> options, TableClient tableClient, ILoggerFactory loggerFactory)
    {
        this.storageClientOptions = options.Value;
        this.tableClient = tableClient;
        this.log = loggerFactory.CreateLogger<ZendeskClient>();
        try
        {
            this.tableClient.CreateIfNotExists();
        }
        catch (Exception ex)
        {
            log.LogCritical(ex,"Error creating container: {TableEndpoint}", tableClient);
        }
    }
}
