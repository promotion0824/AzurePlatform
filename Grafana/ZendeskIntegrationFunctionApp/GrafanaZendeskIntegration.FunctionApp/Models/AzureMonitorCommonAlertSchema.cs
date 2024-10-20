// -----------------------------------------------------------------------
// <copyright file="AzureMonitorCommonAlertSchema.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1402 // File may only contain a single type
namespace GrafanaZendeskIntegration.FunctionApp.Models;

using System;
using Newtonsoft.Json;

[JsonObject]

public class AzureMonitorCommonAlertSchema
{
    public string schemaId { get; set; } = string.Empty;

    public Data data { get; set; } = new();
}

public class Data
{
    public Essentials essentials { get; set; } = new();

    public Alertcontext alertContext { get; set; } = new();

    public Customproperties customProperties { get; set; } = new();
}

public class Essentials
{
    public string alertId { get; set; } = string.Empty;

    public string alertRule { get; set; } = string.Empty;

    public string severity { get; set; } = string.Empty;

    public string signalType { get; set; } = string.Empty;

    public string monitorCondition { get; set; } = string.Empty;

    public string monitoringService { get; set; } = string.Empty;

    public string[] alertTargetIDs { get; set; }

    public string[] configurationItems { get; set; }

    public string originAlertId { get; set; } = string.Empty;

    public DateTime firedDateTime { get; set; }

    public string description { get; set; } = string.Empty;

    public string essentialsVersion { get; set; } = string.Empty;

    public string alertContextVersion { get; set; } = string.Empty;
}

public class Alertcontext
{
    public Properties properties { get; set; }

    public string conditionType { get; set; }

    public Condition condition { get; set; }
}

public class Properties
{
    public string app { get; set; } = string.Empty;

    public string customercode { get; set; } = string.Empty;

    public string environment { get; set; } = string.Empty;

    public string owner { get; set; } = string.Empty;

    public string stamp { get; set; } = string.Empty;
}

public class Condition
{
    public string windowSize { get; set; }

    public Allof[] allOf { get; set; }

    public DateTime windowStartTime { get; set; }

    public DateTime windowEndTime { get; set; }
}

public class Allof
{
    public string metricName { get; set; }

    public string metricNamespace { get; set; }

    public string _operator { get; set; }

    public string threshold { get; set; }

    public string timeAggregation { get; set; }

    public Dimension[] dimensions { get; set; }

    public float metricValue { get; set; }

    public object webTestName { get; set; }
}

public class Dimension
{
    public string name { get; set; }

    public string value { get; set; }
}

public class Customproperties
{
    public string app { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "customer-code")]
    public string customercode { get; set; } = string.Empty;

    public string environment { get; set; } = string.Empty;

    public string owner { get; set; } = string.Empty;

    public string stamp { get; set; } = string.Empty;

    public string troubleshootingGuideUrl { get; set; } = string.Empty;
}
