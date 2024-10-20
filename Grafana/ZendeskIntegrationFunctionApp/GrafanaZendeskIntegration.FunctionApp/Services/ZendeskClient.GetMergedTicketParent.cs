// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.GetMergedTicketParent.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using global::GrafanaZendeskIntegration.FunctionApp.Models;

public partial class ZendeskClient
{
    /// <inheritdoc/>
    public string GetMergedTicketParent(ZendeskTicketAudits audits)
    {
        var auditEvent = audits.audits.FirstOrDefault(a => a.via.source.rel == "merge");
        return auditEvent != null ? auditEvent.via.source.from.ticket_id : string.Empty;
    }
}
