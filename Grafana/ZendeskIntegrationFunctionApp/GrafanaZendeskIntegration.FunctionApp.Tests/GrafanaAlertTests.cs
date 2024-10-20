// -----------------------------------------------------------------------
// <copyright file="GrafanaAlertTests.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1600 // Elements should be documented

using GrafanaZendeskIntegration.FunctionApp.Models;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using System.Diagnostics.CodeAnalysis;

[TestClass]
[ExcludeFromCodeCoverage]
public class GrafanaAlertTests
{
    [TestMethod]
    public void GrafanaAlert_CommonLabelsString_Should_Only_Include_Properties_With_IncludeInHash_Attribute()
    {
        var grafanaAlert = new GrafanaAlert
        {
            commonLabels = new Commonlabels
            {
                EntityName = "EntityName",
                alertname = "alertname",
                app = "app",
                buildings = "building1, building2",
                connectorname = "connectorname",
                destination = "destination",
            },
        };

        var grafanaAlertCommonLabelsString = grafanaAlert.commonLabels.ToString();

        Assert.IsFalse(grafanaAlertCommonLabelsString.Contains("destination"));
        Assert.IsTrue(grafanaAlertCommonLabelsString.Contains("building"));
    }

    [TestMethod]
    public void GrafanaAlert_CommonLabelsString_Should_Not_Include_Properties_With_Null_Values()
    {
        var grafanaAlert = new GrafanaAlert
        {
            commonLabels = new Commonlabels
            {
                EntityName = "EntityName",
                alertname = "alertname",
                app = "app",
                buildings = "building",
                connectorname = "connectorname",
                destination = "destination",
            },
        };

        var grafanaAlertCommonLabelsString = grafanaAlert.commonLabels.ToString();

        Assert.IsFalse(grafanaAlertCommonLabelsString.Contains("severity"));
    }
}
