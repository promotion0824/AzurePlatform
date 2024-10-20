[CmdletBinding(SupportsShouldProcess = $true)]
Param
(
    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "prd")]
    $subscription,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("rg-dev", "rg-prd")]
    $resourceGroup,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com", "https://grafana-prd-eus-c4chfqh8ewb9ezab.eus.grafana.azure.com")]
    $grafanaUrl,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("grafana-dev-eus", "grafana-prd-eus")]
    $grafanaName
)
$ErrorActionPreference = "Stop"
$ContactPointApi = "api/v1/provisioning/contact-points"
$ContactPointUrl = "$grafanaUrl/$ContactPointApi"
$notificationPolicyApi = "api/v1/provisioning/policies"
$notificationPolicyUrl = "$grafanaUrl/$notificationPolicyApi"
$alertRuleApi = "api/v1/provisioning/alert-rules"
$alertRuleBaseUrl = "$grafanaUrl/$alertRuleApi"
write-verbose "ContactPoint URL: $ContactPointUrl"
write-verbose "NotificationPolicy URL: $notificationPolicyUrl"
Write-Verbose "AlertRule URL: $alertRuleBaseUrl"

# get a map of alert rules to files
class AlertRuleMapItem {
    [string]$uid
    [string]$title
    [string]$path
}
$AlertRulesMap = new-object System.Collections.ArrayList

$AlertRuleFiles = Get-ChildItem -Path alerts\AlertRules -Recurse -File -Filter *.json
foreach ($AlertRuleFile in $AlertRuleFiles) {
    $AlertRule = Get-Content -Path $AlertRuleFile.FullName | ConvertFrom-Json
    $mapItem = [AlertRuleMapItem]::new()
    $mapItem.uid = $AlertRule.uid
    $mapItem.title = $AlertRule.title
    $mapItem.path = $AlertRuleFile.FullName
    $AlertRulesMap += $mapItem
}

# create service account if it does not exist
$serviceAccounts = az grafana service-account list -n $grafanaName -g $resourceGroup --subscription $subscription | ConvertFrom-Json
if ($null -eq ($serviceAccounts | Where-Object { $_.name -eq 'GrafanaDevAutomationAdmin' })) {
    az grafana service-account create -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --role Admin
}

# delete the existing service token if it exists
$tokens = (az grafana service-account token list -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin) | ConvertFrom-Json
if ($null -ne ($tokens | Where-Object { $_.name -eq 'admin' })) {
    az grafana service-account token delete -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --token admin
}
$token = (az grafana service-account token create -n $grafanaName -g $resourceGroup --subscription $subscription --service-account GrafanaDevAutomationAdmin --token admin --time-to-live 1d) | ConvertFrom-Json

# add token to authorization header
$Headers = @{Authorization = "Bearer $($token.key)" }

# Get all existing alerts in grafana that aren't in the 'Sandbox' folder
$existingAlertRules = Invoke-RestMethod -Method 'GET' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri $alertRuleBaseUrl
$folders = Invoke-RestMethod -Method 'GET' -Headers $Headers -UseBasicParsing -ContentType "application/json" -uri "$grafanaUrl/api/folders"
$sandboxUid = $folders | Where-Object { $_.title -eq 'Sandbox' } | Select-Object -ExpandProperty uid
$sortedAlertRules = $existingAlertRules | Where-Object { $_.folderUid -ne $sandboxUid } | Sort-Object -Property folderUID, ruleGroup


foreach ($alertRule in $sortedAlertRules) {
    # if the alert exists with the same title, but no uid, delete the file with no uid
    # and write the file out to the correct location
    write-verbose "Checking to see if $($alertRule.title) exists in the correct location"
    $existingFile = $AlertRulesMap | Where-Object { $_.title -eq $alertRule.title -and $_.uid -eq '' }
    if ($null -ne $existingFile) {
        write-verbose "Deleting $($existingFile.path)"
        Remove-Item -Path $existingFile.path
    }

    # if the folder doesn't exist, create it
    $folderName = $folders | Where-Object { $_.uid -eq $alertRule.folderUID } | Select-Object -ExpandProperty title
    $alertPath = "alerts\AlertRules\$folderName\$($alertRule.ruleGroup)"
    if ($false -eq (Test-Path -Path $alertPath)) {
        write-verbose "Creating folder $alertPath"
        new-item -Path $alertPath -ItemType Directory
    }

    # if the alert doesn't exist, create it
    $filename = $alertRule.title -replace '[^A-Za-z0-9-_ \.\[\]]', ''
    $alertFullPath = "$alertPath\$filename.json"
    if ($false -eq (Test-Path -Path $alertFullPath)) {
        write-verbose "Creating alert rule $($alertRule.title) in $alertFullPath"
        $alertRule | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $alertFullPath -Force
    }
}

# git checkout -b "grafanaAlerts"
# git add alerts\AlertRules
# git commit -m "Updating alert rules"
# git push --set-upstream origin grafanaAlerts
# gh pr create --title "Updating alert rules" --body "Updating alert rules" --base "main" --draft

Write-Output $AlertRulesMap
