// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.ShowZendeskTicket.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Net;
using System.Threading.Tasks;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public partial class ZendeskClient
{
    /// <inheritdoc/>
    public async Task<ZendeskTicketResponse> ShowZendeskTicket(string zendeskTicketId)
    {
        try
        {
            string uri = $"tickets/{zendeskTicketId}";
            var response = await this._httpClient.GetAsync(uri);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.LogError("Failed to get ticket details from Zendesk. Status code: {StatusCode}. Response: {Content}", response.StatusCode, responseContent);
            }

            var zendeskTicketResponse = ZendeskTicketResponse.CreateInstance(responseContent);
            return zendeskTicketResponse;
        }
        catch (Exception e)
        {
            log.LogError(e, "Failed to get ticket details from Zendesk.");
            return null;
        }
    }
}
