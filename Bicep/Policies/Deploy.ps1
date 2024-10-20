
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]
    $managementGroup,

    [Parameter(Mandatory = $true)]
    [string]
    $templateFile,

    [Parameter(Mandatory = $true)]
    [string]
    $parametersFile,

    [Parameter(Mandatory = $true)]
    [string]
    $deploymentName,

    # Where to store the management group deployment
    [Parameter(Mandatory = $false)]
    [string]
    $location = 'AustraliaEast',

    [Parameter(Mandatory = $false)]
    [switch]
    $WhatIf,

    [Parameter(Mandatory = $false)]
    [string]
    $SystemAccessToken

)

function Exec
{
	[CmdletBinding()]
	param(
		[Parameter(Position=0,Mandatory=1)][scriptblock]$cmd
	)
    Write-Host $cmd
	& $cmd
	if ($lastexitcode -ne 0) {
		throw "Exec: $cmd failed"
	}
}


if ($WhatIf) {
    Write-Verbose "Checking the result of deploying $templateFile with $parametersFile to $managementGroup"
    $result = Exec { az deployment mg what-if --management-group-id $managementGroup --location $location --template-file $templateFile --parameters $parametersFile --name $deploymentName --no-prompt true }
    Write-Verbose $($result -join "`r`n")
    $Title = ":mag_right: Template Preview for  $($templateFile.Split('/') | Select-Object -Last 1) with $($parametersFile.Split('/') | Select-Object -Last 1) to $managementGroup :page_with_curl:"
    $markdownComment = @"
Please review the changes to make sure they match intentions of this PR
``````
$($result -join "`r`n")
``````
"@

    try {
        # post to the PR
        . $PSScriptRoot/../../build-templates/GithubHelpers.ps1
        AddOrUpdatePRComment -Title $Title -BodyMarkdown $markdownComment -SystemAccessToken $SystemAccessToken
    }
    catch {
        Write-Warning "Failed to post to azure devops PR"
        Write-Warning $_
    }

}
else {
    Write-Verbose "Deploying $templateFile with $parametersFile to $managementGroup"
    Exec { az deployment mg create --management-group-id $managementGroup --location $location --template-file $templateFile --parameters $parametersFile --name $deploymentName }
}

