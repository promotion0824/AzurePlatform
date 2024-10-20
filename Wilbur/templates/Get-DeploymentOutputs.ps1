[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $releaseName,

    [Parameter()]
    [string]
    $resourceGroup
)

$outputs = az deployment group show -g $resourceGroup -n $releaseName --query properties.outputs | ConvertFrom-Json

$outputs.PSObject.Properties | ForEach-Object {
    Write-Host "##vso[task.setvariable variable=$($_.name)]$($_.value.value)"
}
