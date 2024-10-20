[CmdletBinding(SupportsShouldProcess=$true)]
Param
(
    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev","prd")]
    $subscription,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("rg-dev","rg-prd")]
    $resourceGroup,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com","https://grafana-prd-eus-c4chfqh8ewb9ezab.eus.grafana.azure.com")]
    $grafanaUrl,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("grafana-dev-eus","grafana-prd-eus")]
    $grafanaName,

    [string]
    [Parameter(Mandatory = $true)]
    $zendeskWebhookUrl
)
$ErrorActionPreference = "Stop"
$ContactPointApi = "api/v1/provisioning/contact-points"
$ContactPointUrl = "$grafanaUrl/$ContactPointApi"
$notificationPolicyApi = "api/v1/provisioning/policies"
$notificationPolicyUrl = "$grafanaUrl/$notificationPolicyApi"
$alertRuleApi = "api/v1/provisioning/alert-rules"
$alertRuleBaseUrl = "$grafanaUrl/$alertRuleApi"
$folderApi = "api/folders"
$folderUrl = "$grafanaUrl/$folderApi"
write-verbose "ContactPoint URL: $ContactPointUrl"
write-verbose "NotificationPolicy URL: $notificationPolicyUrl"
Write-Verbose "AlertRule URL: $alertRuleBaseUrl"
Write-Verbose "Folder URL: $folderUrl"

# create service account if it does not exist
$serviceAccounts = az grafana service-account list -n $grafanaName -g $resourceGroup --subscription $subscription | ConvertFrom-Json
if ($null -eq ($serviceAccounts | Where-Object {$_.name -eq 'GrafanaDevAutomationAdmin'}))
{
    az grafana service-account create -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --role Admin
}

# delete the existing service token if it exists
$tokens = (az grafana service-account token list -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin) | ConvertFrom-Json
if ($null -ne ($tokens | Where-Object {$_.name -eq 'admin'}))
{
    az grafana service-account token delete -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --token admin
}
$token =  (az grafana service-account token create -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --token admin --time-to-live 1d) | ConvertFrom-Json

# add token to authorization header
$Headers = @{Authorization="Bearer $($token.key)"}
$Headers['X-Disable-Provenance'] = "api"

$contactPoints = Invoke-RestMethod -Method 'Get' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $ContactPointUrl

# deploy contact points
Get-ChildItem -Path $PSScriptRoot\alerts\ContactPoints\$subscription -Filter *.json -Recurse | ForEach-Object {

    $ContactPointUrl = "$grafanaUrl/$ContactPointApi"
    $contactPoint = get-content -Raw -Path $_.FullName
    Write-Verbose "Processing $_"

    # if Contact point exists, PUT instead of POST
    $method = "POST"
    if ($null -ne ($contactPoints | Where-Object {$_.uid -eq ($contactPoint | ConvertFrom-Json).uid }))
    {
        write-verbose Found
        $method = "PUT"
        $ContactPointUrl = $ContactPointUrl + "/" + ($contactPoint | ConvertFrom-Json).uid
    }

    # create/update contact point
    write-verbose "Attempting $method to $ContactPointUrl with content: $contactPoint"

    $result = Invoke-RestMethod -Method $method -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $ContactPointUrl -Body $contactPoint.Replace("{{zendeskWebhookUrl}}",$zendeskWebhookUrl)
    write-verbose $result
    # check if contact point was created/updated successfully
    if (($method -eq "POST" -and $result.uid -eq ($contactPoint | ConvertFrom-Json).uid) -or ($method -eq "PUT" -and $result.message -eq "contactpoint updated"))
    {
        Write-Host "ContactPoint $($_.Name) created/updated successfully"
    }
    else
    {
        Write-Host "ContactPoint $($_.Name) failed to create/update"
    }
}
$ContactPointUrl = "$grafanaUrl/$ContactPointApi"
$contactPoints = Invoke-RestMethod -Method 'Get' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $ContactPointUrl

# deploy notification policies
$policies = Get-Content -Path $PSScriptRoot\alerts\NotificationPolicies\$subscription\NotificationPolicies.json -Raw
$result = Invoke-RestMethod -Method "PUT" -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $notificationPolicyUrl -Body $policies

# check if Notification policy was updated successfully
if ($result.message -eq "policies updated")
{
    Write-Host "Notification policies created/updated successfully"
}
else
{
    Write-Host "Notification policies failed to create/update"
}

# deploy alert rules
$existingAlertRules = Invoke-RestMethod -Method 'GET' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $alertRuleBaseUrl
$maxExistingAlertId = $ExistingAlertRules.id | sort-object -Descending | Select-Object -First 1

# disable provenance so that alerts can be edited in the Grafana UI if needed
$Headers['X-Disable-Provenance']="api"

# Alerts generated in grafana-dev need to have their folder updated to the appropriate folder uid for grafana prod
$folders = Invoke-RestMethod -Method 'GET' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $folderUrl

$files = Get-ChildItem -Path $PSScriptRoot\alerts\AlertRules -Filter *.json -Recurse
if ($files.Count -eq 0) {
    Write-Host "There are no alert rules to deploy"    <# Action to perform if the condition is true #>
}
write-verbose "Processing $($files.Count) alert rules"
$titles = New-Object System.Collections.ArrayList($null)
$files | ForEach-Object {
    $alertRule = get-content -Raw -Path $_.FullName | ConvertFrom-json
    $titles.Add($alertRule.title) | Out-Null
    $alertRuleUrl = $alertRuleBaseUrl
    Write-Verbose "Directory: $($_.DirectoryName)"

    $delimter = "/"
    if ($Env:OS -eq "Windows_NT")
    {
        $delimter = "\"
    }
    $foldersSplit = $_.DirectoryName.Split($delimter)
    $FolderIndex = $foldersSplit.IndexOf("alerts") + 2
    Write-Verbose "Folder Index: $FolderIndex"
    $folderName = $foldersSplit[$FolderIndex]
    Write-Verbose "Folder Name: $folderName"
    $folderUid = $folders | Where-Object { $_.title -eq $folderName } | Select-Object -ExpandProperty uid
    Write-Verbose "Folder UID: $folderUid"
    Write-Verbose "Processing $_"

    # IF the folder is not found, create it
    if ($null -eq $folderUid)
    {
        $folder = @{
            title = $folderName
        }
        $folderJson = $folder | ConvertTo-Json -depth 100
        if ($WhatIfPreference) {
            Write-Verbose "WhatIf: $method to $folderUrl with content: $folderJson"
        }
        else {
            $result = Invoke-RestMethod -Method "POST" -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $folderUrl -Body $folderJson
            $folderUid = $result.uid
        }
    }

    # set default to POST to create a new alert. Change to PUT if alert already exists and needs to be updated
    $method = "POST"
    $existingAlertRule = $existingAlertRules | Where-Object {$_.uid -eq $alertRule.uid -or $_.title -eq $alertRule.title}
    if ($null -ne $existingAlertRule)
    {
        write-verbose Found
        $method = "PUT"
        $alertRule.uid = $existingAlertRule.uid
        $alertRule.id = $existingAlertRule.id
        $alertRuleUrl = $alertRuleBaseUrl + "/" + $alertRule.uid
        $alertRule.folderUid = $folderUid
    }
    else {
        $alertRule.id = $maxExistingAlertId + 1
        $alertRule.folderUid = $folderUid
    }

    # create/update alert rule
    if ($WhatIfPreference) {
        Write-Verbose "WhatIf: $method to $alertRuleUrl with content: $alertRule"
    }
    else {
        write-verbose "Attempting $method to $alertRuleUrl with content: $alertRule"
        $result = Invoke-RestMethod -Method $method -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $alertRuleUrl -Body ($alertRule | ConvertTo-Json -depth 100)
        write-verbose $result
        # check if alert rule was created/updated successfully
        if ($method -eq "POST" -and ($null -ne $result.uid)) {
            Write-Host "Alert Rule $($_.Name) created successfully"
            $maxExistingAlertId ++
        }
        elseif ($method -eq "PUT" -and ($null -ne $result.uid)) {
            Write-Host "Alert Rule $($_.Name) updated successfully"
        }
        else {
            Write-Host "Alert Rule $($_.Name) failed to create/update"
        }
    }
}

# Delete any alerts in Grafana that don't have a corresponding alert rule in the repo

foreach ($existingAlertRule in $existingAlertRules)
{
    $existingAlertRuleName = $existingAlertRule.title
    $existingAlertRuleUid = $existingAlertRule.uid

    if ($titles.Contains($existingAlertRuleName) -eq $false)
    {
        Write-Host "Deleting alert $existingAlertRuleName"
        $alertRuleUrl = $alertRuleBaseUrl + "/" + $existingAlertRuleUid
        if ($WhatIfPreference) {
            Write-Verbose "WhatIf: Delete to $existingAlertRuleName at $alertRuleUrl"
        }
        else {
            $result = Invoke-RestMethod -Method "DELETE" -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $alertRuleUrl
        }
    }
}
