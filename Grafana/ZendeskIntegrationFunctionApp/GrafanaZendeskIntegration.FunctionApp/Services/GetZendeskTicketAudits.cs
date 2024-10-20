// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.ShowZendeskTicket.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System.Threading.Tasks;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public partial class ZendeskClient
{
    /// <inheritdoc/>
    public async Task<ZendeskTicketAudits> GetZendeskTicketAudits(string zendeskTicketId)
    {
        string uri = $"tickets/{zendeskTicketId}/audits";
        var audits = await _httpClient.GetAsync(uri);
        if (audits.IsSuccessStatusCode)
        {
            var auditContent = await audits.Content.ReadAsStringAsync();
            log.LogInformation("Successfully retrieved audits: {Audits}", auditContent);
            return JsonConvert.DeserializeObject<ZendeskTicketAudits>(auditContent);
        }
        else
        {
            log.LogError("Failed to get ticket audits from Zendesk. Status code: {StatusCode}.", audits.StatusCode);
            return null;
        }
    }
}
