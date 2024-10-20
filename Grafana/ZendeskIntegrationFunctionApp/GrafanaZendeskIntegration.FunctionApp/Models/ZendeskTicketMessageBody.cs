// -----------------------------------------------------------------------
// <copyright file="ZendeskTicketMessageBody.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

using System.Collections.Generic;

public class ZendeskTicketMessageBody
{
    /// <summary>
    ///     Gets or sets string or Html formatted comment.
    /// </summary>
    public ZendeskTicketComment comment { get; set; }

    /// <summary>
    ///     Gets or sets subject of the ticket.
    /// </summary>
    public string subject { get; set; }

    /// <summary>
    ///     Gets or sets name of the Zendesk Agent who raised the ticket.
    /// </summary>
    public ZendeskTicketRequester requester { get; set; }

    /// <summary>
    ///     Gets or sets priority of the ticket. "urgent", "high", "normal", "low".
    /// </summary>
    public string priority { get; set; }

    /// <summary>
    ///     Gets or sets status of the ticket.  Default value "new". Possible values: "new", "open", "pending", "hold", "solved", "closed".
    /// </summary>
    public string status { get; set; }

    /// <summary>
    ///     Gets or sets agent group the issue should be assigned to.  May be used to route tickets.
    ///     https://willowinc.zendesk.com/admin/people/team/groups/.
    /// </summary>
    public long group_id { get; set; }

    /// <summary>
    ///     Gets or sets list of custom fields. Customer Name, Customer Site, Environment, Priority Level, Severity Level, Product Area,
    ///     Category.
    /// </summary>
    public List<ZendeskTicketCustomField> custom_fields { get; set; } = new();
}
