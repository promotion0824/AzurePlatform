// -----------------------------------------------------------------------
// <copyright file="StorageClient.GetZendeskTicketLog.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using global::GrafanaZendeskIntegration.FunctionApp.Models;

public partial class StorageClient : IStorageClient
{
    /// <summary>
    /// Retrieves the latest open Zendesk ticket log for a given alert hash.
    /// </summary>
    /// <param name="alertHash">The hash of the alert to retrieve the ticket log for.</param>
    /// <param name="log">The logger instance.</param>
    /// <returns>The latest open Zendesk ticket log for the given alert hash, or null if no open ticket logs exist.</returns>
    public virtual async Task<ZendeskTicketLog> GetZendeskTicketLog(string alertHash)
    {
        // Retrieve existing tickets with the same commonLabelsHash
        var query = $"PartitionKey eq '{alertHash}'";
        var zendeskTicketLogs = new List<ZendeskTicketLog>();

        await foreach (var entity in tableClient.QueryAsync<TableEntity>(query))
        {
            var mapping = new ZendeskTicketLog
            {
                AlertHash = entity.PartitionKey,
                ZendeskTicketId = entity.RowKey,
                UpdatedAt = (DateTime)entity.GetDateTime("UpdatedAt"),
                TimesTriggered = (int)entity.GetInt32("TimesTriggered"),
                TicketStatus = entity.GetString("TicketStatus"),
            };
            zendeskTicketLogs.Add(mapping);
        }

        // get the zendeskTicketLog with the latest UpdatedAt and TicketStatus != "closed"
        var zendeskTicketLog = zendeskTicketLogs.Where(x =>
                x.UpdatedAt == zendeskTicketLogs.Max(x => x.UpdatedAt) && x.TicketStatus.ToLower() != "closed")
            .FirstOrDefault();

        return zendeskTicketLog;
    }
}
