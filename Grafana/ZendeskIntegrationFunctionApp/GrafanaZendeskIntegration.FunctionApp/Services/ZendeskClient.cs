// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using GrafanaZendeskIntegration.FunctionApp.Models;

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using System.IO;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Interface for the Zendesk client.
/// </summary>
public interface IZendeskClient
{
    /// <summary>
    ///     Post a Ticket to the Zendesk API.
    /// </summary>
    /// <param name="zendeskTicketMessage">The Zendesk Ticket Message.</param>
    /// <returns>Details of the created ticket.</returns>
    Task<ZendeskTicketDetail> PostZendeskTicket(string zendeskTicketMessage);

    /// <summary>
    ///     Updates the TimesTriggered & Status fields in a Zendesk Ticket.
    /// </summary>
    /// <param name="zendeskTicketId">Zendesk Ticket Id.</param>
    /// <param name="timesTriggered">Number of Times Triggered.</param>
    /// <param name="ticketStatus">Ticket Status: New, Open, Pending, On-hold, Solved.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<HttpStatusCode> UpdateZendeskTicket(string zendeskTicketId, int timesTriggered, string ticketStatus);

    /// <summary>
    ///     Get the details of a Zendesk ticket.
    /// </summary>
    /// <param name="zendeskTicketId">The Zendesk Ticket Number.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<ZendeskTicketResponse> ShowZendeskTicket(string zendeskTicketId);

    /// <summary>
    ///     Constructs a Zendesk ticket message from a Grafana alert.
    /// </summary>
    /// <param name="grafanaAlert">The Grafana alert.</param>
    /// <returns>A Zendesk ticket message.</returns>
    ZendeskTicketMessage ConstructZendeskTicketMessage(GrafanaAlert grafanaAlert);

    /// <summary>
    ///     Determines whether a Grafana alert should be processed.
    /// </summary>
    /// <param name="alert">The Grafana alert.</param>
    /// <returns>True if the alert should be processed, false otherwise.</returns>
    bool ShouldProcessAlert(GrafanaAlert alert);

    /// <summary>
    ///     Calculates the MD5 hash from the custom properties of an Azure Monitor alert.
    /// </summary>
    /// <param name="azureAlert">The Azure alert.</param>
    /// <returns>string representing the MD5 hash.</returns>
    string CalculateAzureAlertCustomPropertiesHash(AzureMonitorCommonAlertSchema azureAlert);

    /// <summary>
    ///     Constructs the Zendesk Ticket Body for an Azure Monitor Alert.
    /// </summary>
    /// <param name="azureAlert">The Azure alert.</param>
    /// <returns>String representing th eZendesk ticket body.</returns>
    Task<string> ConstructAzureAlertMessage(AzureMonitorCommonAlertSchema azureAlert);

    Task<ZendeskTicketAudits> GetZendeskTicketAudits(string zendeskTicketId);

    string GetMergedTicketParent(ZendeskTicketAudits audits);
}

/// <summary>
/// Provides functionality to interact with Zendesk API.
/// </summary>
public partial class ZendeskClient : IZendeskClient
{
    private readonly ZendeskClientOptions zendeskClientOptions;
    private readonly List<GrafanaZendeskFieldMapping> fieldMappings = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZendeskClient"/> class with specified options and storage client.
    /// </summary>
    /// <param name="options">The options for the Zendesk client.</param>
    /// <param name="fieldMappings">List of Field Mappings.</param>
    public ZendeskClient(IOptions<ZendeskClientOptions> options, List<GrafanaZendeskFieldMapping> fieldMappings, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(fieldMappings);

        this.zendeskClientOptions = options.Value;
        this.fieldMappings = fieldMappings;
        this._httpClient = httpClientFactory.CreateClient("ZendeskClient");

        this.log = loggerFactory.CreateLogger<ZendeskClient>();
    }

    /// <summary>
    /// Determines whether a Grafana alert should be processed.
    /// </summary>
    /// <param name="alert">The Grafana alert.</param>
    /// <param name="log">The logger instance.</param>
    /// <returns>True if the alert should be processed, false otherwise.</returns>
    public bool ShouldProcessAlert(GrafanaAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        // Do not process if the alert is a DatasourceNoData or DatasourceError
        if (alert.commonLabels.alertname is "DatasourceNoData" or "DatasourceError")
        {
            log.LogInformation("Ignoring alert {AlertName}", alert.commonLabels.alertname);
            return false;
        }

        // Check the environment and only create Zendesk Tickets for allowed environments
        var environmentShortName = alert.commonLabels.environment ?? null;
        var environmentsToGenerateTicketsFor =
            this.zendeskClientOptions.EnvironmentsToGenerateTicketsFor.ToLower().Split(",").ToList();

        if (environmentShortName != null && environmentsToGenerateTicketsFor.Contains(environmentShortName.ToLower()))
        {
            log.LogInformation("Processing alert {AlertName}, Environment: {EnvironmentShortName}", alert.commonLabels.alertname, environmentShortName);
            return true;
        }

        log.LogInformation("Ignoring alert {AlertName}, Unsupported environment: {EnvironmentShortName}. Supported environments {SupportedEnvironments}", alert.commonLabels.alertname, environmentShortName, this.zendeskClientOptions.EnvironmentsToGenerateTicketsFor.ToLower());
        return false;
    }
}
