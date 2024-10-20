Param
(
    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("rg-dev","rg-prd")]
    $resourceGroup,

    [string]
    [Parameter(Mandatory = $true)]
    [ValidateSet("grafana-dev-eus","grafana-prd-eus")]
    $grafanaName
)
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"
$ErrorActionPreference = "SilentlyContinue"

# create folder to hold Azure Managed Grafana dashboards
New-Item -ItemType Directory -Force -Path $PSScriptRoot\grafana\dashboards\amg

# Get desired folder structure from willow-values.yaml
if (Test-Path ./Grafana/AzureManagedGrafanaFolders.yaml) {
    write-verbose "Found AzureManagedGrafanaFolders.yaml"
} else {
    write-verbose "AzureManagedGrafanaFolders.yaml not found"
    exit
}

$folders = get-content ./Grafana/AzureManagedGrafanaFolders.yaml -Raw | ConvertFrom-StringData

# Create any folders that don't already exist
write-verbose "Creating missing folders"
$existingFolders = az grafana folder list --resource-group $resourceGroup --name $grafanaName --output json | ConvertFrom-Json
foreach ($folder in ($folders.Values | sort-object | get-unique | Where-Object {$_ -ne 'root'})) {
    if ($null -eq ($existingFolders | Where-Object {$_.title -eq $folder})) {
        write-host "creating folder $folder"
        az grafana folder create --resource-group $resourceGroup --name $grafanaName --title $folder
    }
}
$existingFolders = az grafana folder list --resource-group $resourceGroup --name $grafanaName --output json | ConvertFrom-Json

# Get all dashboards from the dashboards folder
Get-ChildItem -Path $PSScriptRoot\dashboards -Filter *.json -Recurse | Select-Object -First 100 | ForEach-Object {

    # The existing dashboards contain datasource information that is different in azure managed grafana. It needs to be replaced.
    $dashboard = get-content -Raw -Path $_.FullName | ConvertFrom-Json
    Write-Verbose "Processing $_"

    # Update datasources with new uid
    foreach ($ds in $dashboard.panels.datasource | Where-Object {$_.type -eq 'grafana-azure-monitor-datasource'}) {$ds.uid = 'azure-monitor-oob'}
    foreach ($ds in $dashboard.panels.panels.datasource | Where-Object {$_.type -eq 'grafana-azure-monitor-datasource'}) {$ds.uid = 'azure-monitor-oob'}
    foreach ($ds in $dashboard.panels.panels.targets.datasource | Where-Object {$_.type -eq 'grafana-azure-monitor-datasource'}) {$ds.uid = 'azure-monitor-oob'}
    foreach ($ds in $dashboard.panels.Targets.datasource | Where-Object {$_.type -eq 'grafana-azure-monitor-datasource'}) {$ds.uid = 'azure-monitor-oob'}
    foreach ($ds in $dashboard.templating.list.datasource | Where-Object {$_.type -eq 'grafana-azure-monitor-datasource'}) {$ds.uid = 'azure-monitor-oob'}

    # Remove the id field as it will prevent importing the dashboard into Azure Managed Grafana
    if ($null -ne $dashboard.id){ $dashboard.id = $null }

    # write the updated dashboard to a file
    $path = Join-Path -Path $PSScriptRoot\grafana\dashboards\amg -ChildPath $_.Name

    # Convert the dashboard to json and replace some characters that are not supported by ConvertTo-Json on some flavors of PowerShell
    $json = ($dashboard | ConvertTo-Json -Depth 100) -replace '\\u0026', '&' -replace '\\u0027', "'" -replace '\\u003c', '<' -replace '\\u003e', '>'
    Set-Content -Path $path -Value $json

    # determine which folder the dashboard belongs in
    $folderId = '0'
    $folderName = $folders[$_.Name]
    write-verbose "folderName: $folderName"
    Write-Verbose $_.Name
    if ($folderName -and $folderName -ne 'root') {
        $folderId = ($existingFolders | Where-Object {$_.title -eq $folderName}).id
        Write-Verbose "Assigning $_ to folder id: $folderId"
        az grafana dashboard import --definition $path --resource-group $resourceGroup --name $grafanaName --folder $folderId --overwrite
    }

    if ('0' -eq $folderId) {
        Write-Verbose "Assigning $_.Name to folder: 'general'"
         az grafana dashboard import --definition $path --resource-group $resourceGroup --name $grafanaName --overwrite
    }
}
