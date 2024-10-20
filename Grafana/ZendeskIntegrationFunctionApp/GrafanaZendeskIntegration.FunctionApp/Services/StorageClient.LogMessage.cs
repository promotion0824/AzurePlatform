// -----------------------------------------------------------------------
// <copyright file="StorageClient.LogMessage.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using GrafanaZendeskIntegration.FunctionApp.Models;

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

/// <summary>
///     The storage client.
/// </summary>
public partial class StorageClient
{
    /// <summary>
    ///     The log message.
    /// </summary>
    /// <param name="messageBody">
    ///     The message body.
    /// </param>
    /// <param name="messageName">
    ///     The message name.
    /// </param>
    /// <param name="logMessageType">
    ///     The log message type.
    /// </param>
    /// <param name="log">
    ///     The log.
    /// </param>
    /// <returns>
    ///     The <see cref="Task" />.
    /// </returns>
    public async Task LogMessage(string messageBody, string messageName, LogMessageType logMessageType)
    {
        // Don't save Alert Message when not configured to do so
        if (!this.storageClientOptions.SaveAlertsMessages)
        {
            return;
        }

        string containerEndpoint;
        switch (logMessageType)
        {
            case LogMessageType.AlertMessage:
                {
                    log.LogInformation("Saving Alert Message: {MessageName}", messageName);
                    if (!this.storageClientOptions.SaveAlertsMessages)
                    {
                        return;
                    }

                    containerEndpoint =
                        $"https://{this.storageClientOptions.StorageAccountName}.blob.core.windows.net/{this.storageClientOptions.AlertMessagesContainerName}";
                    break;
                }

            case LogMessageType.ZendeskTicket:
                {
                    log.LogInformation("Saving Zendesk Ticket Message: {MessageName}", messageName);
                    if (!this.storageClientOptions.SaveZendeskTicketMessages)
                    {
                        return;
                    }

                    containerEndpoint =
                        $"https://{this.storageClientOptions.StorageAccountName}.blob.core.windows.net/{this.storageClientOptions.ZendeskMessagesContainerName}";
                    break;
                }

            case LogMessageType.CustomerSiteMessage:
                {
                    log.LogInformation("Saving Customer Site Message: {MessageName}", messageName);
                    if (!this.storageClientOptions.SaveCustomerSiteMessages)
                    {
                        return;
                    }

                    containerEndpoint =
                        $"https://{this.storageClientOptions.StorageAccountName}.blob.core.windows.net/{this.storageClientOptions.CustomerSiteStorageContainerName}";
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(logMessageType), logMessageType, null);
        }

        var credential = new DefaultAzureCredential();
        var containerClient = new BlobContainerClient(new Uri(containerEndpoint), credential);
        try
        {
            await containerClient.CreateIfNotExistsAsync();
        }
        catch (RequestFailedException e)
        {
            log.LogCritical(e, "Error creating container: {ContainerEndpoint} :", containerEndpoint);
        }
        catch (Exception ex)
        {
            log.LogCritical(ex, "Unknown exception");
        }

        byte[] alertBytes = Encoding.ASCII.GetBytes(messageBody);

        using MemoryStream stream = new(alertBytes);
        var random = new Random();

        var filename = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:d19}_{random.Next(65536):x4}_{messageName}.json";
        log.LogInformation("Uploading message to container: {ContainerEndpoint}, filename {FileName}", containerEndpoint, filename);
        try
        {
            await containerClient.UploadBlobAsync(filename, stream);
        }
        catch (RequestFailedException e)
        {
            log.LogCritical(e, "Error uploading blob: {FileName}", filename);
        }
        catch (Exception ex)
        {
            log.LogCritical(ex, "An exception occurred while uploading a blob");
        }
    }
}
