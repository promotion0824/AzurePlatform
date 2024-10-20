// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.UpdateZendeskTicket.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public partial class ZendeskClient : IZendeskClient
{
    /// <summary>
    /// Updates the TimesTriggered & Status fields in a Zendesk Ticket.
    /// </summary>
    /// <param name="zendeskTicketId">Zendesk Ticket Id.</param>
    /// <param name="timesTriggered">Number of Times Triggered.</param>
    /// <param name="ticketStatus">Ticket Status: New, Open, Pending, On-hold, Solved.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<HttpStatusCode> UpdateZendeskTicket(string zendeskTicketId, int timesTriggered, string ticketStatus)
    {
        if (!this.zendeskClientOptions.CreateTickets)
        {
            return HttpStatusCode.NotModified;
        }

        // 8855585604367 is the fieldId for the custom TimesTriggered field
        var zendeskTicketMessage =
            $"{{\"ticket\": {{\"status\": \"{ticketStatus}\",  \"custom_fields\": [{{\"id\": 8855585604367,\"value\": {timesTriggered}}}]}}}}";

        var httpContent = new StringContent(zendeskTicketMessage, Encoding.UTF8, "application/json");
        var updateTicketUrl = this.zendeskClientOptions.ZendeskTicketsApiUrl + "/" + zendeskTicketId;
        var response = await this._httpClient.PutAsync(updateTicketUrl, httpContent);

        return response.StatusCode;
    }
}
