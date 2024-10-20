// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.ParseGrafanaAlertMessage.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System;
using System.Linq;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using Microsoft.Extensions.Logging;

public partial class ZendeskClient : IZendeskClient
{
    /// <summary>
    /// Constructs a Zendesk ticket message from a Grafana alert.
    /// </summary>
    /// <param name="grafanaAlert">The Grafana alert to construct the message from.</param>
    /// <param name="log">The logger instance for logging.</param>
    /// <returns>A <see cref="ZendeskTicketMessage"/> object that represents the Zendesk ticket message.</returns>
    public ZendeskTicketMessage ConstructZendeskTicketMessage(GrafanaAlert grafanaAlert)
    {
        // -------------------------------------
        // Construct the Zendesk Ticket Message
        // -------------------------------------
        var zendeskTicketRequester = new ZendeskTicketRequester
        {
            name = this.zendeskClientOptions.ZendeskUserName,
            email = this.zendeskClientOptions.ZendeskUserEmailAddress,
        };

        var title =
            $"Alert: {grafanaAlert.commonLabels.alertname} Folder {grafanaAlert.commonLabels.grafana_folder} Status: {grafanaAlert.status}";
        log.LogInformation("Create Zendesk html for {Title}", title);
        var zendeskTicketMessageBody = new ZendeskTicketMessageBody
        {
            status = "new",
            subject = grafanaAlert.commonLabels.alertname + " " + (grafanaAlert.commonLabels.connectorname ?? string.Empty),
            requester = zendeskTicketRequester,
        };

        var commonLabelsTable = GenerateCommonLabelsTable(grafanaAlert, title);

        log.LogInformation("Adding dashboard links html table");
        var dashboardUrl = grafanaAlert.alerts?[0].dashboardURL ?? string.Empty;
        var tsgUrl = grafanaAlert.commonLabels?.tsg ?? string.Empty;
        var panelUrl = grafanaAlert.alerts?[0].panelURL ?? string.Empty;

        zendeskTicketMessageBody.custom_fields.Add(
            new ZendeskTicketCustomField { id = 9249847891343, value = panelUrl });
        zendeskTicketMessageBody.custom_fields.Add(new ZendeskTicketCustomField
        { id = 9249848448911, value = dashboardUrl });
        zendeskTicketMessageBody.custom_fields.Add(new ZendeskTicketCustomField { id = 9249847581199, value = tsgUrl });

        // Construct the dashboard links table including only the rows that are not empty.
        var dashBoardLinksTable = @"
        <table border='1'>
        <tr>
            <th colspan=""2"">Links</th>
        </tr>";

        if (!string.IsNullOrEmpty(dashboardUrl))
        {
            dashBoardLinksTable += @"
        <tr>
            <td>Grafana Dashboard</td>
            <td><a href='" + dashboardUrl + @"'>" + dashboardUrl + @"</a></td>
        </tr>";
        }

        if (!string.IsNullOrEmpty(panelUrl))
        {
            dashBoardLinksTable += @"
        <tr>
            <td>Grafana Panel</td>
            <td><a href='" + panelUrl + @"'>" + panelUrl + @"</a></td>
        </tr>";
        }

        if (!string.IsNullOrEmpty(tsgUrl))
        {
            dashBoardLinksTable += @"
        <tr>
            <td>Troubleshooting Guide</td>
            <td><a href='" + tsgUrl + @"'>" + tsgUrl + @"</a></td>
        </tr>";
        }

        dashBoardLinksTable += "</table>";

        var htmlBody = "<br><html><body>";
        htmlBody += commonLabelsTable;
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

        // group_id (Resolve Group)
        // See also custom field "Product Area" below, which is also set using the "Owner" common label
        // https://willowinc.zendesk.com/admin/people/team/groups/
        // commonLabels.Owner used to select the group_id
        var zendeskTicketFieldId = "group_id";
        var grafanaFieldName = "commonLabels.Owner";
        var owner = grafanaAlert.commonLabels.owner;
        log.LogDebug("replacing Resolver Group / group_id: {Owner}", owner);
        this.UpdateZendeskTicketStandardFieldFromGrafanaAlertCommonLabels(owner, grafanaFieldName, zendeskTicketFieldId, zendeskTicketMessageBody);
        log.LogDebug("Replaced '{Owner}' with '{Group_id}'", owner, zendeskTicketMessageBody.group_id);

        // --------------
        // Custom Fields
        // --------------

        // Customer Site
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/4413864107161
        zendeskTicketFieldId = "4413864107161";
        if (!string.IsNullOrWhiteSpace(grafanaAlert.commonLabels.buildings))
        {
            var building = grafanaAlert.commonLabels.buildings.Split(',')[0];
            if (!string.IsNullOrWhiteSpace(building))
            {
                zendeskTicketMessageBody.custom_fields.Add(new ZendeskTicketCustomField
                {
                    id = long.Parse(zendeskTicketFieldId),
                    value = building,
                });
                log.LogDebug("Added custom field '{Building}'", building);
            }
            else
            {
                log.LogWarning("Unable to determine the first building in {Buildings}", grafanaAlert.commonLabels.buildings);
            }
        }
        else
        {
            log.LogDebug("Alert does not contain a building that needs to be replaced");
        }

        // Severity
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/7231972707215
        zendeskTicketFieldId = "7231972707215";
        grafanaFieldName = "commonLabels.Severity";
        var severity = grafanaAlert.commonLabels.severity;
        log.LogDebug("Replacing Severity: {Severity}", severity);
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(severity, grafanaFieldName, zendeskTicketFieldId, zendeskTicketMessageBody);
        var customFieldValue = zendeskTicketMessageBody.custom_fields
            .FirstOrDefault(x => x.id == long.Parse(zendeskTicketFieldId))
            ?.value;
        log.LogDebug("Replaced '{Severity}' with '{CustomFieldValue}'", severity, customFieldValue);

        // Customer Name
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360029547271
        // https://github.com/WillowInc/Infrastructure-and-Application-Deployment/blob/main/Configurations/common/customer.json
        zendeskTicketFieldId = "360029547271";
        grafanaFieldName = "commonLabels.customer-code";
        var customerCode = grafanaAlert.commonLabels.customercode;

        // if common label customer-code not present, parse it from common label "fullcustomerinstancename"
        if (string.IsNullOrEmpty(customerCode))
        {
            var fullCustomerInstanceName = grafanaAlert.commonLabels.fullcustomerinstancename;

            if (!string.IsNullOrEmpty(fullCustomerInstanceName))
            {
                string[] nameParts = fullCustomerInstanceName.Split(new string[] { ":" }, StringSplitOptions.None);
                if (nameParts.Length >= 4)
                {
                    string fourthPart = nameParts[3];
                    int dashIndex = fourthPart.IndexOf('-');
                    if (dashIndex != -1)
                    {
                        customerCode = fourthPart[..dashIndex];
                    }
                }
            }
        }

        log.LogDebug("Replacing Customer Code: {CustomerCode}", customerCode);
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(customerCode, grafanaFieldName, zendeskTicketFieldId, zendeskTicketMessageBody);
        customFieldValue = zendeskTicketMessageBody.custom_fields
            .FirstOrDefault(x => x.id == long.Parse(zendeskTicketFieldId))
            ?.value;
        log.LogDebug("Replaced '{CustomerCode}' with '{CustomFieldValue}'", customerCode, customFieldValue);

        // Environment
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/8001260747407
        grafanaFieldName = "commonLabels.Environment";
        zendeskTicketFieldId = "8001260747407";
        var environment = grafanaAlert.commonLabels.environment;
        log.LogDebug($"replacing Environment: {environment}");
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(environment, grafanaFieldName, zendeskTicketFieldId, zendeskTicketMessageBody);
        customFieldValue = zendeskTicketMessageBody.custom_fields
            .FirstOrDefault(x => x.id == long.Parse(zendeskTicketFieldId))
            ?.value;
        log.LogDebug($"Replaced '{environment}' with '{customFieldValue}'");

        // Product Area
        // See Also standard field "group_id" above (also set using commonLabels.Owner)
        // https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360029506771
        grafanaFieldName = "commonLabels.Owner";
        zendeskTicketFieldId = "360029506771";
        owner = grafanaAlert.commonLabels.owner;
        log.LogDebug($"replacing Resolver Group / group_id: {owner}");
        this.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(owner, grafanaFieldName, zendeskTicketFieldId, zendeskTicketMessageBody);
        customFieldValue = zendeskTicketMessageBody.custom_fields
            .FirstOrDefault(x => x.id == long.Parse(zendeskTicketFieldId))
            ?.value;
        log.LogDebug($"Replaced '{owner}' with '{customFieldValue}'");

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
        log.LogInformation("Zendesk message created successfully");
        return zendeskTicketMessage;
    }

    /// <summary>
    /// Updates a standard field in the Zendesk ticket message body based on the Grafana alert's common labels.
    /// </summary>
    /// <param name="grafanaFieldValue">The value of the field from the Grafana alert.</param>
    /// <param name="grafanaFieldName">The name of the field in the Grafana alert.</param>
    /// <param name="zendeskTicketFieldId">The ID of the corresponding field in the Zendesk ticket.</param>
    /// <param name="zendeskTicketMessageBody">The Zendesk ticket message body to update.</param>
    public void UpdateZendeskTicketStandardFieldFromGrafanaAlertCommonLabels(
        string grafanaFieldValue,
        string grafanaFieldName,
        string zendeskTicketFieldId,
        ZendeskTicketMessageBody zendeskTicketMessageBody)
    {
        // Get the Grafana-Zendesk field mappings
        var grafanaZendeskFieldMappings = this.fieldMappings.Where(x =>
            x.GrafanaFieldName == grafanaFieldName && x.ZendeskTicketFieldId == zendeskTicketFieldId).ToList();

        // Find the matching Zendesk ticket field tag
        var zendeskTicketFieldTag = grafanaZendeskFieldMappings
            .Find(x => string.Equals(x.GrafanaFieldValue, grafanaFieldValue, StringComparison.OrdinalIgnoreCase))
            ?.ZendeskTicketFieldTag;

        // If no matching tag is found, use the default value
        if (string.IsNullOrEmpty(zendeskTicketFieldTag))
        {
            zendeskTicketFieldTag = grafanaZendeskFieldMappings.Find(x => x.DefaultValue)?.ZendeskTicketFieldTag;
        }

        if (string.IsNullOrEmpty(zendeskTicketFieldTag))
        {
            return;
        }

        // If the tag is not null or empty, assign its value to the appropriate field in zendeskTicketMessageBody
        var propertyInfo = typeof(ZendeskTicketMessageBody).GetProperty(zendeskTicketFieldId);

        if (propertyInfo.PropertyType == typeof(long))
        {
            propertyInfo.SetValue(zendeskTicketMessageBody, Convert.ToInt64(zendeskTicketFieldTag));
        }
        else
        {
            propertyInfo.SetValue(zendeskTicketMessageBody, zendeskTicketFieldTag);
        }
    }


    /// <summary>
    /// Updates a custom field in the Zendesk ticket message body based on the Grafana alert's common labels.
    /// </summary>
    /// <param name="grafanaFieldValue">The value of the field from the Grafana alert.</param>
    /// <param name="grafanaFieldName">The name of the field in the Grafana alert.</param>
    /// <param name="zendeskTicketFieldId">The ID of the corresponding custom field in the Zendesk ticket.</param>
    /// <param name="zendeskTicketMessageBody">The Zendesk ticket message body to update.</param>
    /// <param name="log">The logger instance for logging.</param>
    public void UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(
        string grafanaFieldValue,
        string grafanaFieldName,
        string zendeskTicketFieldId,
        ZendeskTicketMessageBody zendeskTicketMessageBody)
    {
        // Get the Grafana-Zendesk field mappings
        var grafanaZendeskFieldMappings = this.fieldMappings.Where(x =>
            x.GrafanaFieldName == grafanaFieldName && x.ZendeskTicketFieldId == zendeskTicketFieldId).ToList();

        log.LogDebug(
            "Found {Count} for grafanaFieldName '{FieldName}' and zendeskTicketFieldId '{TicketFieldId}'", grafanaZendeskFieldMappings.Count, grafanaFieldName, zendeskTicketFieldId);

        // Find the matching Zendesk ticket field tag
        var zendeskTicketFieldTag = grafanaZendeskFieldMappings
            .Find(x => string.Equals(x.GrafanaFieldValue, grafanaFieldValue, StringComparison.OrdinalIgnoreCase))
            ?.ZendeskTicketFieldTag;

        log.LogDebug("Found zendeskTicketFieldTag '{TicketFieldTag}'", zendeskTicketFieldTag);

        // If no matching tag is found, use the default value
        if (string.IsNullOrEmpty(zendeskTicketFieldTag))
        {
            zendeskTicketFieldTag = grafanaZendeskFieldMappings.Find(x => x.DefaultValue)?.ZendeskTicketFieldTag;
        }

        // If the tag is not null or empty add a custom field to zendeskTicketMessageBody
        if (!string.IsNullOrEmpty(zendeskTicketFieldTag))
        {
            zendeskTicketMessageBody.custom_fields.Add(new ZendeskTicketCustomField
            {
                id = Convert.ToInt64(zendeskTicketFieldId),
                value = zendeskTicketFieldTag,
            });
        }
    }

    /// <summary>
    /// Generate a table of common labels for the Zendesk Ticket.
    /// </summary>
    /// <param name="grafanaAlert">The grafana alert JSON message.</param>
    /// <param name="title">Title of the Alert.</param>
    /// <returns>HTML table of Common Labels.</returns>
    internal static string GenerateCommonLabelsTable(GrafanaAlert grafanaAlert, string title)
    {
        var commonLabelsTable =
            @"<table border='1'><tr><th colspan=""2"">Incident Information</th></tr><tr><th>Key</th><th>Value</th></tr>";
        commonLabelsTable += $"<tr><td>Title</td><td>{title}</td></tr>";
        Commonlabels commonLabels = grafanaAlert.commonLabels;

        var keys = new[] { "alertname", "fullcustomerinstancename", "connectorname", "app", "customercode", "EntityName", "environment", "owner", "region", "severity", "stamp", "buildings" };
        foreach (string key in keys)
        {
            var propertyInfo = commonLabels.GetType().GetProperty(key);
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(commonLabels, null)?.ToString();

                // if the key is "severity" replace the integer value with the string value
                if (key == "severity")
                {
                    value = value switch
                    {
                        "1" => "Critical",
                        "2" => "Medium",
                        "3" => "Low",
                        "4" => "Informational",
                        _ => value,
                    };
                }

                if (!string.IsNullOrEmpty(value))
                {
                    commonLabelsTable += $"<tr><td>{key}</td><td>{value}</td></tr>";
                }
            }
        }

        commonLabelsTable += "</table>";
        return commonLabelsTable;
    }
}
