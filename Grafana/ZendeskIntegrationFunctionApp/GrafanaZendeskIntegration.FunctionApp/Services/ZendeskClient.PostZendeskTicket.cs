// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.PostZendeskTicket.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using GrafanaZendeskIntegration.FunctionApp.Models;

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class ZendeskClient : IZendeskClient
{
    /// <inheritdoc/>
    public virtual async Task<ZendeskTicketDetail> PostZendeskTicket(string zendeskTicketMessage)
    {
        if (!this.zendeskClientOptions.CreateTickets)
        {
            return new ZendeskTicketDetail
            {
                StatusCode = System.Net.HttpStatusCode.NotModified,
                ResponseBody = string.Empty,
                TicketId = string.Empty,
            };
        }

        var httpContent = new StringContent(zendeskTicketMessage, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(this.zendeskClientOptions.ZendeskTicketsApiUrl, httpContent);

        var responseStatusCode = response.StatusCode;

        if (!response.IsSuccessStatusCode)
        {
            return new ZendeskTicketDetail
            {
                StatusCode = responseStatusCode,
                ResponseBody = string.Empty,
                TicketId = string.Empty,
            };
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<JObject>(responseContent);
        var ticketId = responseObject["ticket"]["id"].ToString();

        return new ZendeskTicketDetail
        {
            StatusCode = responseStatusCode,
            ResponseBody = responseContent,
            TicketId = ticketId,
        };
    }
}
