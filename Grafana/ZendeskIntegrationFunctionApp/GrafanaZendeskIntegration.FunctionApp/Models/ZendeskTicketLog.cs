// -----------------------------------------------------------------------
// <copyright file="ZendeskTicketLog.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

using System;

public class ZendeskTicketLog
{
    public string AlertHash { get; set; } // Row Key

    public DateTime UpdatedAt { get; set; }

    public string ZendeskTicketId { get; set; }

    public int TimesTriggered { get; set; }

    public string TicketStatus { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            $"AlertHash: {this.AlertHash}, UpdatedAt {this.UpdatedAt}, ZendeskTicketId: {this.ZendeskTicketId}, TimesTriggered: {this.TimesTriggered}, TicketStatus: {this.TicketStatus}";
    }
}
