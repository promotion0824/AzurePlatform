// -----------------------------------------------------------------------
// <copyright file="StorageClient.InsertZendeskTicketLog.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using GrafanaZendeskIntegration.FunctionApp.Models;

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Identity;

public partial class StorageClient : IStorageClient
{
    public async Task InsertOrUpdateZendeskTicketLog(ZendeskTicketLog zendeskTicketLog)
    {
        var tableEndpoint = $"https://{this.storageClientOptions.StorageAccountName}.table.core.windows.net/";
        var tableClient = new TableClient(new Uri(tableEndpoint), this.storageClientOptions.ZendeskMessageLogTableName,
            new DefaultAzureCredential());

        // Create a TableEntity to insert or merge
        var entity = new TableEntity(zendeskTicketLog.AlertHash, zendeskTicketLog.ZendeskTicketId)
        {
            { "UpdatedAt", DateTime.UtcNow },
            { "TimesTriggered", zendeskTicketLog.TimesTriggered },
            { "TicketStatus", zendeskTicketLog.TicketStatus },
        };

        // Insert or merge the entity
        await tableClient.UpsertEntityAsync(entity);
    }
}
