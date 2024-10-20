Param
(
    [string]
    $subscription = "249312a0-4c83-4d73-b164-18c5e72bf219",

    [datetime]
    [Parameter(Mandatory = $true)]
    $cleanupDate,

    [string]
    $excludedResourceGroups = "t3-wil-sbx-ccr-aue1-api-rsg,t2-wil-sbx-wco-shr-glb-mgt-rsg,NetworkWatcherRG,test-b2c,deployment-data,wil-ops-bot,t1-wil-plt-sbx-rsg-mgt-aue1,t1-wil-plt-sbx-rsg-shr-aue,t2-wil-sbx-wco-wrt-aue1-mgt-rsg,t2-wil-sbx-wco-wrt-aue1-mgt-rsg-aks,defaultresourcegroup-eau,defaultresourcegroup-wus,defaultresourcegroup-ase,defaultresourcegroup-eus,defaultresourcegroup-sea,defaultresourcegroup-eus2"
)

$rgToSkip = $excludedResourceGroups.split(',')
$date = $cleanupDate.ToString("yyyy-MM-dd")

Write-Verbose "Setting a condemed date of $date for $subscription"


$resourceGroups = az group list --subscription $subscription --output json | ConvertFrom-Json


foreach ($resourceGroup in $resourceGroups) {
    if ($rgToSkip -Contains $resourceGroup.name){
        Write-Verbose "$($resourceGroup.name) was set to skip tagging"
        continue
    }

    if ($null -ne $resourceGroup.tags.Condemned){
        Write-Verbose "RG: $($resourceGroup.name)[$($resourceGroup.id)] already has a Condemned tag with value of $($resource.tags.Condemned)"
    } else {
        Write-Output "RG: $($resourceGroup.name)[$($resourceGroup.id)] will be tagged with a  Condemned value of $($date)"
        az tag update --resource-id $resourceGroup.id --operation merge --tags Condemned=$date --subscription $subscription
    }

    $resources = az resource list -g $resourceGroup.name --subscription $subscription --output json | ConvertFrom-Json
    foreach ($resource in $resources) {
        if ($null -ne $resource.tags.Condemned){
            Write-Verbose "$($resource.id) already has a Condemned tag with value of $($resource.tags.Condemned)"
        } else {
            Write-Debug "$($resource.id) will be tagged with a  Condemned value of $($date)"
            az tag update --resource-id $resource.id --operation merge --tags Condemned=$date --subscription $subscription
        }
        Write-Verbose "Finished setting tags for $($resourceGroup.name)"
    }
}
