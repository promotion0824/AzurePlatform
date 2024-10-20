// -----------------------------------------------------------------------
// <copyright file="ZendeskTicketCustomField.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class ZendeskTicketCustomField
{
    public long id { get; set; }

    public string value { get; set; }
}
