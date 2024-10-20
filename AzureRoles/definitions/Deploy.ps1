Param
(
    [string]
    $subscription,

    [string]
    $roleFile,

    [Parameter(Mandatory = $false)]
    [switch]
    $WhatIf
)

$details = Get-Content $roleFile | ConvertFrom-Json
$existing = az role definition list --name $details.roleName --subscription $subscription -o json | ConvertFrom-Json

if ($WhatIf){
    if ($null -eq $existing -or $existing.Count -eq 0){
        Write-Verbose "Would attempt Creating role from $roleFile in $subscription"
    } else {
        Write-Verbose "Would attempt Updating role from $roleFile in $subscription"
    }

} else {
    if ($null -eq $existing -or $existing.Count -eq 0){
        Write-Verbose "Creating role from $roleFile in $subscription"
        az role definition create --role-definition $roleFile --subscription $subscription
    } else {
        Write-Verbose "Updating role from $roleFile in $subscription"
        az role definition update --role-definition $roleFile --subscription $subscription
    }
}
