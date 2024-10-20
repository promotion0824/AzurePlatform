// -----------------------------------------------------------------------
// <copyright file="StorageClient.UpdateZendeskTicketLog.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using global::GrafanaZendeskIntegration.FunctionApp.Models;

public partial class StorageClient : IStorageClient
{
    public async Task<ZendeskTicketLog> UpdateZendeskTicketLog(
        string alertHash,
        string zendeskTicketId,
        int timesTriggered,
        string ticketStatus)
    {
        var query = $"PartitionKey eq '{alertHash}' and RowKey eq '{zendeskTicketId}'";
        var matchingLogs = this.tableClient.Query<TableEntity>(query);

        TableEntity response;
        if (matchingLogs.Any())
        {
            // If there are matching logs, update the first one
            response = matchingLogs.First();
            response["TimesTriggered"] = timesTriggered;
            response["UpdatedAt"] = DateTime.UtcNow;
            response["TicketStatus"] = ticketStatus;

            await this.tableClient.UpdateEntityAsync(response, response.ETag, TableUpdateMode.Replace);
        }
        else
        {
            // No matching log exists, create a new record
            var newZendeskTicketLog = new TableEntity(alertHash, zendeskTicketId)
            {
                { "TicketStatus", ticketStatus },
                { "TimesTriggered", timesTriggered },
                { "UpdatedAt", DateTime.UtcNow },
            };

            await this.tableClient.AddEntityAsync(newZendeskTicketLog);
            response = newZendeskTicketLog; // For the return object construction
        }

        return new ZendeskTicketLog
        {
            AlertHash = response.PartitionKey,
            ZendeskTicketId = response.RowKey,
            UpdatedAt = (DateTime)response.GetDateTime("UpdatedAt"),
            TimesTriggered = (int)response.GetInt32("TimesTriggered"),
            TicketStatus = response.GetString("TicketStatus"),
        };
    }
}
