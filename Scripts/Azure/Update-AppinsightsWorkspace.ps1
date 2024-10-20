Param
(
    [string]
    $inFile = "export/appInsightsInstances.csv",

    [Parameter(Mandatory = $false)]
    [switch]
    $WhatIf
)

$VerbosePreference = "Continue"

function RunCliCommand {
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $CliCommand,

        [Parameter(Mandatory = $false)]
        [switch]
        $WhatIf
    )

    if ($WhatIf) {
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

$FileContent = Get-Content -Path $inFile

$appInsights = ConvertFrom-Csv $FileContent

Write-Verbose "Total Records $($appInsights.count) with $($($appInsights | Where-Object { $_.RequiresUpdate}).count) requiring updates"


foreach ($item in $appInsights) {
    if ([bool]::Parse($item.RequiresUpdate)){
        RunCliCommand -WhatIf:$WhatIf -CliCommand "az monitor app-insights component update --app $($item.name)  -g $($item.resourceGroup) --workspace $($item.TargetWorkspace) --subscription $($item.subscriptionId)"
    }
}
