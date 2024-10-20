// -----------------------------------------------------------------------
// <copyright file="ZendeskClientOptions.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class ZendeskClientOptions
{
    /// <summary>
    ///     Gets or sets base URL for the Zendesk Tickets API.
    /// </summary>
    public string ZendeskTicketsApiUrl { get; set; }

    /// <summary>
    ///     Gets or sets name of the Zendesk user to create the ticket as.   Registered in Zendesk as a Light Agent.
    /// </summary>
    public string ZendeskUserName { get; set; }

    /// <summary>
    ///     Gets or sets email address of the Zendesk user to create the ticket as.
    /// </summary>
    public string ZendeskUserEmailAddress { get; set; }

    /// <summary>
    ///     Gets or sets aPI Token for the Zendesk user to create the ticket as.
    /// </summary>
    public string ZendeskUserApiToken { get; set; }

    /// <summary>
    ///     Gets or sets comma separated list of the environments to generate Zendesk Tickets for.
    /// </summary>
    public virtual string EnvironmentsToGenerateTicketsFor { get; set; }

    public bool CreateTickets { get; set; }
}
