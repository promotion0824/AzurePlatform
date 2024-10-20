// -----------------------------------------------------------------------
// <copyright file="GrafanaZendeskFieldMapping.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

/// <summary>
/// Represents a mapping between Grafana and Zendesk fields.
/// </summary>
public class GrafanaZendeskFieldMapping
{
    /// <summary>
    /// Gets or sets the name of the Grafana field.
    /// </summary>
    public string GrafanaFieldName { get; set; }

    /// <summary>
    /// Gets or sets the value of the Grafana field.
    /// </summary>
    public string GrafanaFieldValue { get; set; }

    /// <summary>
    /// Gets or sets the tag of the Zendesk ticket field.
    /// </summary>
    public string ZendeskTicketFieldTag { get; set; }

    /// <summary>
    /// Gets or sets the ID of the Zendesk ticket field.
    /// </summary>
    public string ZendeskTicketFieldId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field is a default value.
    /// </summary>
    public bool DefaultValue { get; set; }
}
