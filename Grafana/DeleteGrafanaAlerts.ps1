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
    $grafanaName
)
$ErrorActionPreference = "Stop"
$alertRuleApi = "api/v1/provisioning/alert-rules"
$alertRuleBaseUrl = "$grafanaUrl/$alertRuleApi"
Write-Verbose "AlertRule URL: $alertRuleBaseUrl"

# delete the existing service token if it exists
$tokens = (az grafana service-account token list -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin) | ConvertFrom-Json
if ($null -ne ($tokens | Where-Object {$_.name -eq 'admin'}))
{
    az grafana service-account token delete -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --token admin
}
$token =  (az grafana service-account token create -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --token admin --time-to-live 1d) | ConvertFrom-Json

# add token to authorization header
$Headers = @{Authorization="Bearer $($token.key)"}

# delete alert rules
$existingAlertRules = Invoke-RestMethod -Method 'GET' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $alertRuleBaseUrl
$maxExistingAlertId = $ExistingAlertRules.id | sort-object -Descending | Select-Object -First 1

# disable provenance so that alerts can be edited in the Grafana UI if needed
$Headers['X-Disable-Provenance']="api"

# Alerts generated in grafana-dev need to have their folder updated to the appropriate folder uid for grafana prod
$folders = Invoke-RestMethod -Method 'GET' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri "$grafanaUrl/api/folders"
$pathToAlerts = "{0}\alerts\AlertRules\Single Tenant\HighFrequency\Avg Memory Working Set" -f $PSScriptRoot

write-Host "Folders: $pathToAlerts"

Get-ChildItem -Path $pathToAlerts -Filter *.json -Recurse | ForEach-Object {
    $alertRule = get-content -Raw -Path $_.FullName | ConvertFrom-json
    $alertRuleUrl = $alertRuleBaseUrl
    $foldersSplit = $_.DirectoryName.Split("\")
    $FolderIndex = $foldersSplit.IndexOf("alerts") + 2
    Write-Verbose $FolderIndex
    $folderName = $foldersSplit[$FolderIndex]
    Write-Verbose $folderName
    $folderUid = $folders | Where-Object { $_.title -eq $folderName } | Select-Object -ExpandProperty uid
    Write-Verbose $folderUid
    write-Host "Processing $_"

    $method = "DELETE"
    
    # Delete alert rule
    $alertRuleUrl = $alertRuleBaseUrl + "/" + $alertRule.uid
    write-Host "Attempting $method to $alertRuleUrl"

    $result = Invoke-RestMethod -Method $method -Headers $Headers -UseBasicParsing -uri $alertRuleUrl
    write-verbose $result
}
