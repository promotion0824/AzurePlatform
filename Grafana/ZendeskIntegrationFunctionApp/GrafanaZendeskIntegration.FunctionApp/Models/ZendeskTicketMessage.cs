// -----------------------------------------------------------------------
// <copyright file="ZendeskTicketMessage.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class ZendeskTicketMessage
{
    public ZendeskTicketMessageBody ticket { get; set; }
}
