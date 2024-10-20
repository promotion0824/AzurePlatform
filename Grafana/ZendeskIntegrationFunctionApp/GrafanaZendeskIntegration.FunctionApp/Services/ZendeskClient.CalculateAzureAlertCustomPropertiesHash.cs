// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.CalculateAzureAlertCustomPropertiesHash.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services;

using Newtonsoft.Json;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using System.Security.Cryptography;
using System.Text;

public partial class ZendeskClient : IZendeskClient
{
    public string CalculateAzureAlertCustomPropertiesHash(AzureMonitorCommonAlertSchema azureAlert)
    {
        // Calculate the MD5 hash of the Custom Properties
        if (azureAlert.data.customProperties == null)
        {
            return string.Empty;
        }

        string customPropertiesString = JsonConvert.SerializeObject(azureAlert.data.customProperties);
        return this.CalculateMD5Hash(customPropertiesString);
    }

    private string CalculateMD5Hash(string input)
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new();
        foreach (byte t in hashBytes)
        {
            _ = sb.Append(t.ToString("X2"));
        }

        return sb.ToString();
    }
}
