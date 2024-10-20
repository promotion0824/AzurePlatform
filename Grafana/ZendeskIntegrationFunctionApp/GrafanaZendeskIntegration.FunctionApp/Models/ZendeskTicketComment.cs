// -----------------------------------------------------------------------
// <copyright file="ZendeskTicketComment.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class ZendeskTicketComment
{
    public string body { get; set; }

    public string html_body { get; set; }
}
