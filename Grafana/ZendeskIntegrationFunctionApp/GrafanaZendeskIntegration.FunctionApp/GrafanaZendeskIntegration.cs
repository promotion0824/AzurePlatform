// -----------------------------------------------------------------------
// <copyright file="GrafanaZendeskIntegration.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.Eventing.Reader;

namespace GrafanaZendeskIntegration.FunctionApp
{
    using System.Net;
    using global::GrafanaZendeskIntegration.FunctionApp.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for integrating Grafana alerts with Zendesk tickets.
    /// </summary>
    public class GrafanaZendeskIntegration
    {
        /// <summary>
        /// The storage client used to interact with Azure Table Storage.
        /// </summary>
        private readonly IStorageClient storageClient;

        /// <summary>
        /// The zendesk client used to interact with Zendesk API.
        /// </summary>
        private readonly IZendeskClient zendeskClient;

        /// <summary>
        private readonly ILogger log;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrafanaZendeskIntegration"/> class.
        /// </summary>
        /// <param name="storageClient">The storage client used to interact with Azure Table Storage.</param>
        /// <param name="zendeskClient">The zendesk client used to interact with Zendesk API.</param>
        public GrafanaZendeskIntegration(ILoggerFactory loggerFactory, IStorageClient storageClient,
            IZendeskClient zendeskClient)
        {
            this.storageClient = storageClient;
            this.zendeskClient = zendeskClient;
            log = loggerFactory.CreateLogger<GrafanaZendeskIntegration>();
        }

        /// <summary>
        ///     Function app to receive Alert messages from Grafana and convert them into Zendesk Tickets.
        ///     Main purpose is to set the field values in Zendesk so Customer Technical Support can quickly identify issues.
        /// </summary>
        /// <param name="req">The HTTP request object.</param>
        /// <returns>Status Code returned by Zendesk API.</returns>
        [Function("GrafanaZendeskIntegration")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "grafana/alerts")] HttpRequestData req)
        {
            log.LogInformation("Received a message from Grafana");

            if (req?.Body == null || req.Body.Length == 0)
            {
                return new BadRequestObjectResult("Grafana Alert was empty.");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Fail fast if the alert won't be processed. Don't bother saving the message to Blob Storage in this case
            this.log.LogInformation(requestBody);
            var grafanaAlert = GrafanaAlert.CreateInstance(requestBody);

            if (!this.zendeskClient.ShouldProcessAlert(grafanaAlert))
            {
                return new OkResult();
            }

            // Save the raw Grafana Alert if StorageClientOptions:SaveAlertsMessages = true.  Assist with debugging Alerts from Grafana.
            await this.storageClient.LogMessage(requestBody, "GrafanaAlert", LogMessageType.AlertMessage);

            // Determine if there is an existing open Zendesk Ticket with the same commonLabels
            log.LogInformation("Checking for existing ticket with the same common labels");

            // Calculate the hash of the commonLabels
            var alertHash = grafanaAlert.CommonLabelHash;
            var hashMatchedZendeskTicketLog = await this.storageClient.GetZendeskTicketLog(alertHash);
            log.LogInformation("Matched with '{MatchedTicket}'", hashMatchedZendeskTicketLog);

            // If there is an existing open Zendesk Ticket with the same commonLabels, then update the TimesTriggered in the Zendesk Ticket
            bool openTicket = !string.IsNullOrEmpty(hashMatchedZendeskTicketLog?.ZendeskTicketId);
            if (openTicket)
            {
                ZendeskTicketResponse zendeskTicketResponse = null;
                try
                {
                    zendeskTicketResponse =
                        await this.zendeskClient.ShowZendeskTicket(hashMatchedZendeskTicketLog.ZendeskTicketId);
                    log.LogInformation("zendeskTicketResponse is a: {Response}", zendeskTicketResponse.GetType());

                    // if the zendeskTicketResponse is for a different ticket than we requested, then update the hashMatchedZendeskTicketLog object with the new ticket number
                    if (zendeskTicketResponse is ZendeskTicket zendeskTicket &&
                        zendeskTicket.ticket.id.ToString() != hashMatchedZendeskTicketLog.ZendeskTicketId)
                    {
                        log.LogInformation("Updating hashMatchedZendeskTicketLog with new ticket number: {TicketId}", zendeskTicket.ticket.id);
                        hashMatchedZendeskTicketLog.ZendeskTicketId = zendeskTicket.ticket.id.ToString();
                    }
                }
                catch (Exception e)
                {
                    log.LogCritical(e, "Error retrieving ticket from Zendesk: {Error}", e.Message);
                }

                if (await this.ProcessOpenTicketResponse(zendeskTicketResponse, hashMatchedZendeskTicketLog,
                        alertHash))
                {
                    return new OkResult();
                }
            }

            // If there is no existing open Zendesk Ticket with the same commonLabels, or the ticket has been closed, then create a new Zendesk Ticket
            log.LogInformation(
                "No Open Zendesk ticket found with the same commonLabels, or the ticket was closed, so creating a new one");

            var newZendeskObject = this.zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);
            string newZendeskTicket = JsonConvert.SerializeObject(newZendeskObject);

            // https://www.rfc-editor.org/rfc/rfc7159
            if (newZendeskTicket == "null")
            {
                var errorMessage =
                    "Grafana Alert Message was not able to be converted into a Zendesk Ticket. No Incident was raised in Zendesk.";

                log.LogError(errorMessage);

                var error = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = errorMessage,
                };

                return new BadRequestObjectResult(error);
            }

            log.LogInformation("Zendesk ticket body created successfully");

            // Save the Zendesk Ticket message if StorageClientOptions:SaveZendeskTicketMessages = true.
            await this.storageClient.LogMessage(newZendeskTicket, "ZendeskTicket", LogMessageType.ZendeskTicket);

            // Post the new Zendesk Ticket
            try
            {
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

                log.LogInformation("Zendesk returned ticketId {TicketId}", zendeskTicketDetail.TicketId);

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
            catch (Exception ex)
            {
                log.LogError(ex, "Error posting Zendesk Ticket");
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkResult();
        }

        private async Task<bool> ProcessOpenTicketResponse(
            ZendeskTicketResponse zendeskTicketResponse,
            ZendeskTicketLog hashMatchedZendeskTicketLog,
            string alertHash)
        {
            switch (zendeskTicketResponse)
            {
                case SimpleError { error: "RecordNotFound" } error:
                    log.LogError("Error retrieving Zendesk Ticket {TicketId}: {Error}",
                        hashMatchedZendeskTicketLog.ZendeskTicketId, error);
                    log.LogInformation("Closing ticket in local storage as it no longer exists");
                    await this.storageClient.UpdateZendeskTicketLog(
                        alertHash,
                        hashMatchedZendeskTicketLog.ZendeskTicketId,
                        hashMatchedZendeskTicketLog.TimesTriggered,
                        "closed");
                    break;
                case DetailedError detailedError when !string.IsNullOrEmpty(detailedError.error.title):
                    log.LogError("Error retrieving Zendesk Ticket {TicketId}: {Title}",
                        hashMatchedZendeskTicketLog.ZendeskTicketId, detailedError.error.title);
                    return true;
                case ZendeskTicket zendeskTicket:
                    var hashMatchedZendeskTicketStatus = zendeskTicket.ticket.status;
                    switch (hashMatchedZendeskTicketStatus)
                    {
                        case "new":
                        case "pending":
                        case "open":
                            // If the ticket is open in Zendesk, then increment the TimesTriggered in the ticket log
                            log.LogInformation("Incrementing TimesTriggered of existing Zendesk Ticket {ZendeskTicketId}", hashMatchedZendeskTicketLog.ZendeskTicketId);
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
                        case "closed":
                        case "solved":
                            if (zendeskTicket.ticket.tags.Contains("closed_by_merge"))
                            {
                                log.LogInformation("The existing ticket has been merged");
                                var audits = await this.zendeskClient.GetZendeskTicketAudits(
                                                                       hashMatchedZendeskTicketLog.ZendeskTicketId);
                                string parentTicketId = this.zendeskClient.GetMergedTicketParent(audits);
                                this.log.LogDebug("Found parent ticket ID in audit logs: {ParentTicketId}", parentTicketId);

                                // If the parent ticket is open, increment times triggered on it, close the merged ticket
                                if (!string.IsNullOrEmpty(parentTicketId))
                                {
                                    var parentTicketResponse = await this.zendeskClient.ShowZendeskTicket(parentTicketId);
                                    if (parentTicketResponse is ZendeskTicket parentTicket &&
                                    (parentTicket.ticket.status == "open" || parentTicket.ticket.status == "new" || parentTicket.ticket.status == "pending"))
                                    {
                                        var timesTriggered = parentTicket.ticket.custom_fields.FirstOrDefault(x => x.id == 8855585604367)?.value;
                                        int result = int.TryParse(timesTriggered, out int parsedValue) ? parsedValue : 0;
                                        log.LogInformation(
                                            "The parent ticket {ParentTicketId} is open. Incrementing TimesTriggered from {TimesTriggered}",
                                            parentTicketId,
                                            result);
                                        var response = await this.zendeskClient.UpdateZendeskTicket(parentTicketId, result + 1, parentTicket.ticket.status);
                                        this.log.LogInformation("Returned response: {Response}", response);

                                        return true;

                                        // Mark the merged ticket as merged
                                        // await this.storageClient.UpdateZendeskTicketLog(alertHash, hashMatchedZendeskTicketLog.ZendeskTicketId, hashMatchedZendeskTicketLog.TimesTriggered, "closed");
                                    }

                                    log.LogInformation("Ticket has been merged but the parent ticket is closed.");
                                    return false;
                                }

                                log.LogInformation("Ticket has been merged, but unable to determine parent ticket id");
                                return false;
                            }

                            // Ticket has been closed but not merged. mark it as closed in table storage
                            log.LogInformation(
                                "The existing Zendesk Ticket {ZendeskTicketId} has been {Status}. Updating ticket log.",
                                hashMatchedZendeskTicketLog.ZendeskTicketId, hashMatchedZendeskTicketStatus);
                            await this.storageClient.UpdateZendeskTicketLog(
                                alertHash,
                                hashMatchedZendeskTicketLog.ZendeskTicketId,
                                hashMatchedZendeskTicketLog.TimesTriggered,
                                "closed");
                            return false;
                        default:
                            log.LogWarning(
                                "The existing Zendesk Ticket {ZendeskTicketId} is in an unknown state: {Status}. Updating ticket log.",
                                hashMatchedZendeskTicketLog.ZendeskTicketId, hashMatchedZendeskTicketStatus);
                            return true;
                    }
            }

            return false;
        }
    }
}
