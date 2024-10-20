// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.ParseAzureAlertMessage.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using global::GrafanaZendeskIntegration.FunctionApp.Models;

public partial class ZendeskClient : IZendeskClient
{
    public async Task<string> ConstructAzureAlertMessage(AzureMonitorCommonAlertSchema azureAlert)
    {
        // Check the environment and only create Zendesk Tickets for allowed environments
        var environmentShortName = azureAlert.data.customProperties.environment;
        var environmentsToGenerateTicketsFor =
            this.zendeskClientOptions.EnvironmentsToGenerateTicketsFor.ToLower().Split(",").ToList();
        if (environmentShortName == null || !environmentsToGenerateTicketsFor.Contains(environmentShortName.ToLower()))
        {
            return string.Empty;
        }

        // -------------------------------------
        // Construct the Zendesk Ticket Message
        // -------------------------------------
        var zendeskTicketRequester = new ZendeskTicketRequester
        {
            name = this.zendeskClientOptions.ZendeskUserName,
            email = this.zendeskClientOptions.ZendeskUserEmailAddress,
        };

        var alertName = azureAlert.data.essentials.description;

        var zendeskTicketMessageBody = new ZendeskTicketMessageBody
        {
            status = "new",
            subject = "Azure Monitor Alert Fired: " + alertName,
            requester = zendeskTicketRequester,
        };

        var customProperties = azureAlert.data.customProperties;

        // Build a table of custom properties
        var customPropertiesTable =
            @"<table border='1'><tr><th colspan=""2"">Incident Information</th></tr><tr><th>Key</th><th>Value</th></tr>";
        var keys = new[] { "app", "customercode", "environment", "owner", "stamp" };
        foreach (var key in keys)
        {
            if (!string.IsNullOrWhiteSpace(customProperties.GetType().GetProperty(key)
                    ?.GetValue(azureAlert.data.customProperties)?.ToString()))
            {
                customPropertiesTable +=
                    $"<tr><td>{key}</td><td>{customProperties.GetType().GetProperty(key).GetValue(azureAlert.data.customProperties)}</td></tr>";
            }
        }

        customPropertiesTable += "</table>";

        // Get various links
        var tsgUrl = customProperties.troubleshootingGuideUrl ?? string.Empty;
        var azureAlertUrl = $"https://portal.azure.com/#@willowinc.com/resource{azureAlert.data.essentials.alertId}";

        var dashBoardLinksTable = @"
            <table border='1'>
                <tr>
                    <th colspan=""2"">Links</th>
                </tr>
                <tr>
                    <td>Azure Alert</td>
                    <td><a href='" + azureAlertUrl + @"'>" + azureAlertUrl + @"</a></td>
                </tr>
                <tr>
                    <td>Troubleshooting Guide</td>
                    <td><a href='" + tsgUrl + @"'>" + tsgUrl + @"</a></td>
                </tr>
            </table>";

        var htmlBody = "<br><html><body>";
        htmlBody += customPropertiesTable;
        htmlBody += "<br><br>";
        htmlBody += dashBoardLinksTable;
        htmlBody += "</body></html><br>";

        var zendeskTicketComment = new ZendeskTicketComment
        {
            html_body = htmlBody,
        };
        zendeskTicketMessageBody.comment = zendeskTicketComment;

        // ----------------
        // Standard Fields
        // ----------------

        // Priority
        zendeskTicketMessageBody.priority = "normal";

        // GroupId (Resolve Group)
        // https://willowinc.zendesk.com/admin/people/team/groups/
        var zendeskTicketFieldId = "group_id";
        var azureAlertFieldName = "data.customProperties.owner";
        this.UpdateZendeskTicketStandardFieldFromGrafanaAlertCommonLabels(
            azureAlert.data.customProperties.owner,
            azureAlertFieldName,
            zendeskTicketFieldId,
            zendeskTicketMessageBody);

        // --------------
        // Custom Fields
        // --------------
        zendeskTicketMessageBody.custom_fields = new List<ZendeskTicketCustomField>();

        // Severity
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/7231972707215
        zendeskTicketFieldId = "7231972707215";
        azureAlertFieldName = "data.essentials.severity";
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(
            azureAlert.data.essentials.severity,
            azureAlertFieldName,
            zendeskTicketFieldId,
            zendeskTicketMessageBody);

        // Customer Name
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360029547271
        // https://github.com/WillowInc/Infrastructure-and-Application-Deployment/blob/main/Configurations/common/customer.json
        zendeskTicketFieldId = "360029547271";
        azureAlertFieldName = "data.customProperties.customer-code";
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(
            azureAlert.data.customProperties.customercode,
            azureAlertFieldName,
            zendeskTicketFieldId,
            zendeskTicketMessageBody);

        // Environment
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/8001260747407
        azureAlertFieldName = "data.customProperties.environment";
        zendeskTicketFieldId = "8001260747407";
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(
            azureAlert.data.customProperties.environment,
            azureAlertFieldName,
            zendeskTicketFieldId,
            zendeskTicketMessageBody);

        // TODO: Product Area
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360029506771
        azureAlertFieldName = "???";
        zendeskTicketFieldId = "360029506771";

        // Escalation - static value of "no"
        var escalationField = new ZendeskTicketCustomField
        {
            id = 900007747226, // Escalation
            value = "no",
        };
        zendeskTicketMessageBody.custom_fields.Add(escalationField);

        var zendeskTicketMessage = new ZendeskTicketMessage
        {
            ticket = zendeskTicketMessageBody,
        };
        return JsonConvert.SerializeObject(zendeskTicketMessage);
    }
}
