Param
(
    [string]
    $outFile = "settings.csv"
)

function Get-AppSettings {
    param (

        [Parameter(Mandatory = $true)]
        [string]$AppName,

        [Parameter(Mandatory = $true)]
        [string]$ResourceGroup,


        [Parameter(Mandatory = $true)]
        [string]$Subscription,

        [Parameter(Mandatory = $false)]
        [Switch]$IsFunctionApp

    )

    if ($IsFunctionApp) {
        $type = "FunctionApp"
        $config = az functionapp config appsettings list --name $AppName --resource-group $ResourceGroup --subscription $Subscription | ConvertFrom-Json
    }
    else {
        $type = "WebApp"
        $config = az webapp config appsettings list --name $AppName --resource-group $ResourceGroup --subscription $Subscription | ConvertFrom-Json
    }

    return $config | ForEach-Object { return @{
            Name          = $_.name
            Value         = $_.value
            App           = $AppName
            ResourceGroup = $ResourceGroup
            Subscription  = $Subscription
            Type          = $type
        }
    }

}

$subscriptions = az account list | ConvertFrom-Json
$result = [System.Collections.ArrayList]::new()
foreach ($subscription in $subscriptions) {
    $webapps = az webapp list --subscription $subscription.name | ConvertFrom-Json
    $functionapps = az functionapp list --subscription $subscription.name | ConvertFrom-Json

    foreach ($webapp in $webapps) {

        $configItems = Get-AppSettings -AppName $webapp.name -ResourceGroup $webapp.resourceGroup -Subscription $subscription.name
        $result.AddRange(@($configItems))
    }
    foreach ($functionapp in $functionapps) {
        $configItems = Get-AppSettings -AppName $functionapp.name -ResourceGroup $functionapp.resourceGroup -Subscription $subscription.name -IsFunctionApp
        $result.AddRange(@($configItems))
    }
}

$result | ForEach-Object { New-Object PSObject -Property $_ } | Export-Csv $outFile  -NoTypeInformation
