[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $ProjectName,

    [Parameter(Mandatory = $false)]
    [string]
    $OrgToken = "da4c5e52-ef02-4161-9372-dba0a6a9b0ab",

    [Parameter(Mandatory = $false)]
    [string]
    $ProjectId,

    [Parameter(Mandatory = $false)]
    [string]
    $SystemAccessToken,

    [Parameter(Mandatory = $false)]
    [string]
    $policyRejectionFilePath = ".\whitesource\policyRejectionSummary.json"
)

. $PSScriptRoot/GithubHelpers.ps1

if (!(Test-Path $policyRejectionFilePath))
{
    write-verbose "policy rejection summary not found" -ErrorAction Stop
    # If the policy rejection summary is not found, assume the PR comment is stale
    Remove-PRComment -TextToMatchOn $Title -SystemAccessToken $SystemAccessToken
}


$policyFailuresJson = Get-Content $policyRejectionFilePath

$policyFailures = $policyFailuresJson | convertfrom-json
$policyMsgs = [System.Collections.Arraylist]@()
$policyFailureTotal = [System.Collections.Arraylist]@()
foreach ($policy in $policyFailures.rejectingPolicies)
{
    foreach ($rejectedLibrary in $policy.rejectedLibraries)
    {
        $policyMsgs.Add(("* **{0}** violates policy '{1}'" -f $rejectedLibrary.Name, $policy.policyName)) | out-null
    }
    $policyFailureTotal.Add(("|{0}|{1}|" -f $policy.policyName, $policy.rejectedLibraries.Count)) | out-null
}

$Title = ":unlock: Mend Status for project ($ProjectName) :fire_engine:"

$MendLink = "https://saas.whitesourcesoftware.com/Wss/WSS.html"
if ($ProjectId -ne '' -and $OrgToken -ne '')
{
    $MendLink = $MendLink + "#!project;id=$ProjectId;orgToken=$OrgToken"
}

$markdownComment = @"

"See more details at [Mend]($MendLink)"

$(if ($policyMsgs.Count -gt 0){"## Please fix these issues as they are high severity"} else {""})

$($policyMsgs -join "`r`n")

## All Issues detected by Mend
| Issue Type | Count |
|------------------|----|
$($policyFailureTotal -join "`r`n")
"@

Write-Verbose "Posting PR Comment via AzureDevOps REST API"

# post to the PR
AddOrUpdatePRComment -Title $Title -BodyMarkdown $markdownComment -SystemAccessToken $SystemAccessToken


if ($policyFailures.rejectingPolicies.count -gt 0)
{
    Write-Host "##vso[task.logissue type=error]Failed because of having $($policyFailures.rejectingPolicies.Count) high security vulnerability or policy violation in Mend"
    Write-host "##vso[task.logissue type=error]$MendLink"
    exit 1
}
