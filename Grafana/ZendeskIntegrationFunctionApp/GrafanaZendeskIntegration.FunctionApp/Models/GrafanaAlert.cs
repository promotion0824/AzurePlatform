// -----------------------------------------------------------------------
// <copyright file="GrafanaAlert.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1402 // File may only contain a single type
namespace GrafanaZendeskIntegration.FunctionApp.Models;

using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class GrafanaAlert
{
    public string receiver { get; set; }

    public string status { get; set; }

    public Alert[] alerts { get; set; }

    public Commonlabels commonLabels { get; set; } = new();

    public string externalURL { get; set; }

    public string version { get; set; }

    public string groupKey { get; set; }

    public int truncatedAlerts { get; set; }

    public string CommonLabelHash => this.commonLabels.CalculateGrafanaAlertCommonLabelsHash();

    public static GrafanaAlert CreateInstance(string grafanaAlertString)
    {
        var grafanaAlert = JsonConvert.DeserializeObject<GrafanaAlert>(grafanaAlertString);
        return grafanaAlert;
    }
}

public class Commonlabels
{
    [IncludeInHash]
    public string EntityName { get; set; }

    [IncludeInHash]
    public string alertname { get; set; }

    public string app { get; set; }

    [IncludeInHash]
    public string buildings { get; set; }

    [IncludeInHash]
    public string connectorname { get; set; }

    public string destination { get; set; }

    [IncludeInHash]
    public string environment { get; set; }

    [IncludeInHash]
    public string fullcustomerinstancename { get; set; }

    public string grafana_folder { get; set; }

    [IncludeInHash]
    public string severity { get; set; }

    [JsonProperty(PropertyName = "customer-code")]
    public string customercode { get; set; }

    [IncludeInHash]
    public string owner { get; set; }

    [IncludeInHash]
    public string region { get; set; }

    [IncludeInHash]
    public string stamp { get; set; }

    public string tsg { get; set; }

    public override string ToString()
    {
        var stringProperties = this.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop => Attribute.IsDefined(prop, typeof(IncludeInHash)))
            .Where(p => p.PropertyType == typeof(string))
            .ToDictionary(p => p.Name, p => (string)p.GetValue(this));

        foreach (var property in stringProperties.Where(property => property.Value == null))
        {
            stringProperties.Remove(property.Key);
        }

        return JsonConvert.SerializeObject(stringProperties);
    }

    internal string CalculateGrafanaAlertCommonLabelsHash()
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(this.ToString());
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new();
        foreach (byte t in hashBytes)
        {
            _ = sb.Append(t.ToString("X2"));
        }

        return sb.ToString();
    }
}

public class Alert
{
    public string status { get; set; }

    public DateTime startsAt { get; set; }

    public object endsAt { get; set; }

    public string generatorURL { get; set; }

    public string fingerprint { get; set; }

    public string silenceURL { get; set; }

    public string dashboardURL { get; set; }

    public string panelURL { get; set; }
}
