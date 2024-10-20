Param
(
    [string]
    $outFile = "export/appInsightsInstances-$($(Get-Date).ToUniversalTime().toString("yyyyMMdd")).csv"
)

function Get-WorkspaceForProdLocation {
    param (

        [Parameter(Mandatory = $true)]
        [string]$Location

    )

    $workspace = switch -Wildcard ( $Location )
        {
            'CentralUs*' { "/subscriptions/d4746c7a-19cb-47ac-82b5-069b17cb99de/resourceGroups/prod-platformshared/providers/Microsoft.OperationalInsights/workspaces/prodplatformshared-eu2-log"    }
            'WestUs*' { "/subscriptions/d4746c7a-19cb-47ac-82b5-069b17cb99de/resourceGroups/prod-platformshared/providers/Microsoft.OperationalInsights/workspaces/prodplatformshared-eu2-log"    }
            'EastUs*' { "/subscriptions/d4746c7a-19cb-47ac-82b5-069b17cb99de/resourceGroups/prod-platformshared/providers/Microsoft.OperationalInsights/workspaces/prodplatformshared-eu2-log"    }
            '*Europe*' { "/subscriptions/d4746c7a-19cb-47ac-82b5-069b17cb99de/resourceGroups/prod-platformshared/providers/Microsoft.OperationalInsights/workspaces/prodplatformshared-weu-log"   }
            '*Australia*' { "/subscriptions/d4746c7a-19cb-47ac-82b5-069b17cb99de/resourceGroups/prod-platformshared/providers/Microsoft.OperationalInsights/workspaces/prodplatformshared-aue-log"  }
            default { "/subscriptions/d4746c7a-19cb-47ac-82b5-069b17cb99de/resourceGroups/prod-platformshared/providers/Microsoft.OperationalInsights/workspaces/prodplatformshared-aue-log" }
        }
    return $workspace
}

function Get-WorkspaceLocation {
    param (

        [Parameter(Mandatory = $true)]
        [string]$Location,

        [Parameter(Mandatory = $true)]
        [string]$Subscription

    )

    $workspace = switch ( $Subscription )
        {
            'bdd7479b-ce38-47cf-b22f-285337c1e951' { Get-WorkspaceForProdLocation -Location $Location } # Build-PRD
            '16c3dd19-5016-4bf8-85e2-8c7e88607b7a' { Get-WorkspaceForProdLocation -Location $Location } # Data-PRD
            '3ab44a28-5c8d-4f57-9d5e-b4830331e5db' { Get-WorkspaceForProdLocation -Location $Location } # Experience-PRD
            'e878a98a-20ec-4516-a59d-f393429fe07c' { Get-WorkspaceForProdLocation -Location $Location } # Platform-PRD
            'ee06cff9-d2d7-405a-8ce9-cf82fe78fbf6' { Get-WorkspaceForProdLocation -Location $Location } # Rail-PRD
            'd4746c7a-19cb-47ac-82b5-069b17cb99de' { Get-WorkspaceForProdLocation -Location $Location } # Products-Shared
            '5f077d49-cd08-48b4-a26b-59d708d7847b' { Get-WorkspaceForProdLocation -Location $Location } # Products-Shared
            '0659c76d-fcbf-4e58-8bce-960b9e0cd100' { Get-WorkspaceForProdLocation -Location $Location } # Products-Shared
            default { "/subscriptions/178b67d7-b6fd-46db-b4a3-b57f8a6b045f/resourceGroups/nonprod-platformshared/providers/Microsoft.OperationalInsights/workspaces/nonprodplatformshared-aue-log"}
        }
    return $workspace
}

$VerbosePreference = "Continue"
$result = az graph query -q "resources| where type == 'microsoft.insights/components' | join kind = leftouter (resourcecontainers | where type == 'microsoft.resources/subscriptions' | extend SubscriptionName=name) on subscriptionId| project name, resourceGroup, subscriptionId, location, CurrentWorkspace=properties.WorkspaceResourceId, SubscriptionName" --first 1000 | ConvertFrom-Json

Write-Verbose "Total Records $($result.total_records) got $($result.count) records"

if ($result.count -lt $result.total_records){
    throw "Did not retreive all records"
}

$result.data | ForEach-Object {
    $TargetWorkspace = $(Get-WorkspaceLocation -Location $_.location -Subscription $_.subscriptionId)
    Add-Member -InputObject $_ -NotePropertyName TargetWorkspace -NotePropertyValue $TargetWorkspace
    Add-Member -InputObject $_ -NotePropertyName RequiresUpdate -NotePropertyValue $($TargetWorkspace -ne $_.CurrentWorkspace)
}
$result.data | Export-Csv $outFile -NoTypeInformation
