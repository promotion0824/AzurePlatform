[CmdletBinding(SupportsShouldProcess)]
Param
(
    [Parameter(Mandatory = $true)]
    [string]
    $managementGroupName,

    [Parameter(Mandatory = $true)]
    [string]
    $managementSubscriptionName
)

$VerbosePreference = "Continue"

function RunCliCommand {
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $CliCommand
    )

    if ($WhatIfPreference) {
        Write-Verbose "Would Run:"
        Write-Output "$CliCommand"

    } else {
        Write-Verbose "Running: $CliCommand"
        Invoke-Expression $CliCommand
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Command $CliCommand resulted in exit code $LASTEXITCODE"
        }
    }
}

$supportedTypes = @(
    "microsoft.agfoodplatform/farmbeats",
    "microsoft.apimanagement/service",
    "microsoft.appconfiguration/configurationstores",
    "microsoft.attestation/attestationproviders",
    "microsoft.automation/automationaccounts",
    "microsoft.avs/privateclouds",
    "microsoft.cache/redis",
    "microsoft.cdn/profiles",
    "microsoft.cognitiveservices/accounts",
    "microsoft.containerregistry/registries",
    "microsoft.devices/iothubs",
    "microsoft.eventgrid/topics",
    "microsoft.eventgrid/domains",
    "microsoft.eventgrid/partnernamespaces",
    "microsoft.eventhub/namespaces",
    "microsoft.keyvault/vaults",
    "microsoft.keyvault/managedhsms",
    "microsoft.machinelearningservices/workspaces",
    "microsoft.media/mediaservices",
    "microsoft.media/videoanalyzers",
    "microsoft.netapp/netappaccounts/capacitypools/volumes",
    "microsoft.network/publicipaddresses",
    "microsoft.network/virtualnetworkgateways",
    "microsoft.network/p2svpngateways",
    "microsoft.network/frontdoors",
    "microsoft.network/bastionhosts",
    "microsoft.operationalinsights/workspaces",
    "microsoft.purview/accounts",
    "microsoft.servicebus/namespaces",
    "microsoft.signalrservice/signalr",
    "microsoft.signalrservice/webpubsub",
    # "microsoft.sql/servers/databases", don't add audit settings at the db level. Add it at the server level
    "microsoft.sql/managedinstances"
    )

# Get valid workspaces from the management subscription
az account set --subscription $managementSubscriptionName

$resources = az resource list --query "[].{resourceGroup:resourceGroup, resourceType:type, resourceName:name, resourceId:id, location:location}" | ConvertFrom-Json

$aueWorkspace = $resources | Where-Object { $_.resourceType -eq "microsoft.operationalinsights/workspaces" -and $_.resourceName -eq ("logs-{0}-aue" -f $managementSubscriptionName) }
$eusWorkspace = $resources | Where-Object { $_.resourceType -eq "microsoft.operationalinsights/workspaces" -and $_.resourceName -eq ("logs-{0}-eus" -f $managementSubscriptionName) }
$weuWorkspace = $resources | Where-Object { $_.resourceType -eq "microsoft.operationalinsights/workspaces" -and $_.resourceName -eq ("logs-{0}-weu" -f $managementSubscriptionName) }
write-verbose "aueWorkspace: $($aueWorkspace.resourceId)"
write-verbose "eusWorkspace: $($eusWorkspace.resourceId)"
write-verbose "weuWorkspace: $($weuWorkspace.resourceId)"

# Get subscriptions under the management group
$subscriptions = az account management-group subscription show-sub-under-mg --name $managementGroupName | ConvertFrom-Json

# Loop through subscriptions and set the subscription context
foreach ($subscription in $subscriptions) {
    Write-Verbose "Setting subscription context to $($subscription.displayName)"
    az account set --subscription $subscription.name

    # Register microsoft.insights provider
    $provider = (az provider list | ConvertFrom-Json) | Where-Object {$_.namespace -eq 'microsoft.insights'}
    if ($provider.registrationState -ne 'Registered') {
        az provider register --namespace microsoft.insights
    }

    # Get resources in the subscription
    $resources = az resource list --query "[].{resourceGroup:resourceGroup, resourceType:type, resourceName:name, resourceId:id, location:location}" | ConvertFrom-Json
    foreach ($resource in $resources) {
        switch ($resource.location) {
            "eastus" { $workspace = $eusWorkspace.resourceId }
            "eastus2" { $workspace = $eusWorkspace.resourceId }
            "westeurope" { $workspace = $weuWorkspace.resourceId }
            "australiaeast" { $workspace = $aueWorkspace.resourceId }

            Default { $workspace = $eusWorkspace.resourceId }
        }
        if ($supportedTypes -contains $resource.resourceType) {

            # Determine if auditing is already enabled. Do not update if it is.
            $diagSettings = az monitor diagnostic-settings list --resource $resource.resourceId | ConvertFrom-Json
            $auditGroup = $diagSettings.logs | Where-Object {$_.categoryGroup -eq 'audit'}

            if ($auditGroup.enabled -eq $true) {
                Write-Verbose "Diagnostic settings already already enabled for $($resource.resourceId)"
            }
            else {
                Write-Verbose "Creating diagnostic settings for $($resource.resourceId)"
                RunCliCommand -CliCommand "az monitor diagnostic-settings create --resource $($resource.resourceId) -n AuditLogs --logs '[{categoryGroup:audit,enabled:true}]' --workspace $workspace"
            }
        } elseif ('Microsoft.Sql/servers' -eq $resource.resourceType) {
            RunCliCommand -CliCommand "az sql server audit-policy update --ids $($resource.resourceId) --state Enabled --lats Enabled --lawri $workspace"
        }
        # } elseif ('microsoft.sql/servers/databases' -eq $resource.resourceType -and $auditGroup.enabled -ne $true) {
        #     # Remove the audit settings for the database so that we can add it at the server level
        #     Write-Verbose "Removing audit settings for $($resource.resourceId)"
        #     RunCliCommand -CliCommand "az monitor diagnostic-settings delete --resource $($resource.resourceId) -n AuditLogs"
        # }
    }
}
