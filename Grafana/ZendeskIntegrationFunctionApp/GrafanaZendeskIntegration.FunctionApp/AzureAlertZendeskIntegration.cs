// -----------------------------------------------------------------------
// <copyright file="AzureAlertsZendeskIntegration.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp;

using System.Net;
using global::GrafanaZendeskIntegration.FunctionApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;

/// <summary>
/// Represents the integration between Azure Alerts and Zendesk.
/// </summary>
public class AzureAlertZendeskIntegration
{
    private readonly IStorageClient storageClient;
    private readonly IZendeskClient zendeskClient;
    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureAlertZendeskIntegration"/> class.
    /// </summary>
    /// <param name="loggerFactory">Implementation of an ILoggerFactory.</param>
    /// <param name="storageClient">Storage Client.</param>
    /// <param name="zendeskClient">Zendesk Client.</param>
    public AzureAlertZendeskIntegration(ILoggerFactory loggerFactory, IStorageClient storageClient, IZendeskClient zendeskClient)
    {
        this.storageClient = storageClient;
        this.zendeskClient = zendeskClient;
        log = loggerFactory.CreateLogger<GrafanaZendeskIntegration>();
    }

    /// <summary>
    ///     Function app to convert Alert messages from Azure to Zendesk Tickets.
    /// </summary>
    /// <param name="req">The HTTP request object.</param>
    /// <returns>Status Code returned by Zendesk API.</returns>
    [Function("AzureAlertZendeskIntegration")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "azure/alerts")]
        HttpRequestData req)
    {
        log.LogInformation("Received a message from Azure Monitor");

        if (req?.Body == null || req.Body.Length == 0)
        {
            return new BadRequestObjectResult("Azure Alert was empty.");
        }

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Save the raw Azure Alert if StorageClientOptions:SaveAlertsMessages = true.  Assist with debugging Alerts from Grafana.
        await this.storageClient.LogMessage(requestBody, "AzureAlert", LogMessageType.AlertMessage);

        var azureAlert = JsonConvert.DeserializeObject<AzureMonitorCommonAlertSchema>(requestBody);

        // If the alert is generated by the Azure Action group test, return success and stop processing.
        if (azureAlert.data.essentials.monitorCondition == "Fired" &&
            azureAlert.data.essentials.alertId.Equals(
                "/subscriptions/11111111-1111-1111-1111-111111111111/providers/Microsoft.AlertsManagement/alerts/12345678-1234-1234-1234-1234567890ab"))
        {
            log.LogInformation("Test Alert received.  No Zendesk Ticket will be created.");
            return new OkResult();
        }

        // Calculate the hash of the customProperties
        var alertHash = this.zendeskClient.CalculateAzureAlertCustomPropertiesHash(azureAlert);

        // Determine if there is an existing open Zendesk Ticket with the same customProperties
        var hashMatchedZendeskTicketLog = await this.storageClient.GetZendeskTicketLog(alertHash);
        log.LogInformation("Matched with '{HashMatchedZendeskTicketLog}'", hashMatchedZendeskTicketLog);

        // If there is an existing open Zendesk Ticket with the same customProperties, then update the TimesTriggered in the Zendesk Ticket
        bool openTicket = hashMatchedZendeskTicketLog != null;
        if (openTicket)
        {
            // check that the ticket was not closed in Zendesk before incrementing the TimesTriggered
            var zendeskTicketResponse =
                await this.zendeskClient.ShowZendeskTicket(hashMatchedZendeskTicketLog.ZendeskTicketId);

            if (await this.ProcessOpenTicketResponse(log, zendeskTicketResponse, hashMatchedZendeskTicketLog, alertHash))
            {
                return new OkResult();
            }
        }

        // If there is no existing open Zendesk Ticket with the same customProperties, or the ticket has been closed, then create a new Zendesk Ticket
        log.LogInformation(
            "No Open Zendesk ticket found with the same customProperties, or the ticket was closed, so creating a new one");
        var newZendeskTicket = await this.zendeskClient.ConstructAzureAlertMessage(azureAlert);

        // Save the Zendesk Ticket message if StorageClientOptions:SaveZendeskTicketMessages = true.
        await this.storageClient.LogMessage(newZendeskTicket, "ZendeskTicket", LogMessageType.ZendeskTicket);

        if (!string.IsNullOrEmpty(newZendeskTicket))
        {
            // Post the new Zendesk Ticket
            var zendeskTicketDetail = await this.zendeskClient.PostZendeskTicket(newZendeskTicket);

            if (zendeskTicketDetail.StatusCode is not HttpStatusCode.Created and
                not HttpStatusCode.Accepted)
            {
                var errorMessage = $"Zendesk returned status code {zendeskTicketDetail.StatusCode}.";

                log.LogError(errorMessage);

                var error = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = errorMessage,
                    DetailedMessage = zendeskTicketDetail.ResponseBody,
                };

                return new BadRequestObjectResult(error);
            }

            log.LogInformation($"Zendesk returned ticketId {zendeskTicketDetail.TicketId}");

            // Insert the Zendesk Ticket Log
            var zendeskTicketLog = new ZendeskTicketLog
            {
                AlertHash = alertHash,
                ZendeskTicketId = zendeskTicketDetail.TicketId,
                TimesTriggered = 1,
                TicketStatus = "open",
            };

            await this.storageClient.InsertOrUpdateZendeskTicketLog(zendeskTicketLog);
        }
        else
        {
            string errorMessage =
                "Azure Alert Message was not able to be converted into a Zendesk Ticket OR Environment was not listed in ZendeskClientOptions:EnvironmentsToGenerateTicketsFor. No Incident was raised in Zendesk.";

            log.LogError(errorMessage);

            var error = new ErrorResponseMessage
            {
                Status = "Error",
                Message = errorMessage,
            };

            return new BadRequestObjectResult(error);
        }

        return new OkResult();
    }

    private async Task<bool> ProcessOpenTicketResponse(
    ILogger log,
    ZendeskTicketResponse zendeskTicketResponse,
    ZendeskTicketLog hashMatchedZendeskTicketLog,
    string alertHash)
    {
        switch (zendeskTicketResponse)
        {
            case SimpleError { error: "RecordNotFound" } error:
                log.LogError("Error retrieving Zendesk Ticket {TicketId}: {Error}", hashMatchedZendeskTicketLog.ZendeskTicketId, error);
                log.LogInformation("Closing ticket in local storage as it no longer exists");
                await this.storageClient.UpdateZendeskTicketLog(
                    alertHash,
                    hashMatchedZendeskTicketLog.ZendeskTicketId,
                    hashMatchedZendeskTicketLog.TimesTriggered,
                    "closed");
                break;
            case DetailedError detailedError when !string.IsNullOrEmpty(detailedError.error.title):
                log.LogError("Error retrieving Zendesk Ticket {TicketId}: {Title}", hashMatchedZendeskTicketLog.ZendeskTicketId, detailedError.error.title);
                return true;
            case ZendeskTicket zendeskTicket:
                string[] openStates = { "open", "new" };
                var hashMatchedZendeskTicketStatus = zendeskTicket.ticket.status;
                if (!openStates.Contains(hashMatchedZendeskTicketStatus.ToLower()))
                {
                    // If the ticket is not open in Zendesk, then update the ticket log to closed
                    log.LogInformation(
                        "The existing Zendesk Ticket {ZendeskTicketId} has been {Status}. Updating ticket log.", hashMatchedZendeskTicketLog.ZendeskTicketId, hashMatchedZendeskTicketStatus);
                    await this.storageClient.UpdateZendeskTicketLog(
                        alertHash,
                        hashMatchedZendeskTicketLog.ZendeskTicketId,
                        hashMatchedZendeskTicketLog.TimesTriggered,
                        "closed");
                }
                else
                {
                    // If the ticket is open in Zendesk, then increment the TimesTriggered in the ticket log
                    string message =
                        $"Incrementing TimesTriggered of existing Zendesk Ticket {hashMatchedZendeskTicketLog.ZendeskTicketId}";
                    log.LogInformation(message);
                    await this.zendeskClient.UpdateZendeskTicket(
                        hashMatchedZendeskTicketLog.ZendeskTicketId,
                        hashMatchedZendeskTicketLog.TimesTriggered + 1,
                        "open");
                    await this.storageClient.UpdateZendeskTicketLog(
                        alertHash,
                        hashMatchedZendeskTicketLog.ZendeskTicketId,
                        hashMatchedZendeskTicketLog.TimesTriggered + 1,
                        "open");
                    return true;
                }

                break;
        }

        return false;
    }
}
