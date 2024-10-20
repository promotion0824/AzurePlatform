// -----------------------------------------------------------------------
// <copyright file="StorageClientOptions.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class StorageClientOptions
{
    public string StorageAccountName { get; set; }

    public bool SaveCustomerSiteMessages { get; set; }

    public string CustomerSiteStorageContainerName { get; set; }

    public bool SaveAlertsMessages { get; set; }

    public string AlertMessagesContainerName { get; set; }

    public bool SaveZendeskTicketMessages { get; set; }

    public string ZendeskMessagesContainerName { get; set; }

    public string ZendeskMessageLogTableName { get; set; }
}
