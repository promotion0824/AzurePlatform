
[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    $subscriptionFileFilter = "*.yaml",

    [Parameter(Mandatory = $false)]
    [switch]
    $WhatIf
)

$InformationPreference = "Continue"
if (Get-Module -ListAvailable -Name psyml) {
    Write-Debug "psyml Already Installed"
}
else {
    try {
        Install-Module -Name psyml -Scope CurrentUser -Confirm:$False -Force
    }
    catch [Exception] {
        $_.message
        exit
    }
}

## Functions
function Add-Role {
    [CmdletBinding(SupportsShouldProcess)]
    param (
        [Parameter(Mandatory = $true)]
        [string]$Assignee,

        [Parameter(Mandatory = $true)]
        [string]$Role,

        [Parameter(Mandatory = $true)]
        [string]$Scope
    )

    if ($PSCmdlet.ShouldProcess("Assign $Assignee a role of $Role for $Scope")) {
        try {
            Write-Output "Assigning $Assignee a role of $Role for $Scope"
            az role assignment create --assignee $Assignee --role $Role --scope $Scope
        }
        catch {
            Write-Warning "Could not assign $Assignee a role of $Role for $Scope check that the resource and object id still exist"
        }
    }
    else {
        Write-Output "Would assign $Assignee a role of $Role for $Scope"
    }
}

function Add-NewRole {
    [CmdletBinding(SupportsShouldProcess)]
    param (
        [Parameter(Mandatory = $true)]
        [System.Collections.ArrayList]$existingRoles,

        [Parameter(Mandatory = $true)]
        [System.Collections.ArrayList]$rolesinFile
    )

    foreach ($roleToAdd in $rolesinFile) {
        # $roleToAdd | Format-Table
        $matchingRole = $existingRoles | Where-Object { $_.principalId -eq $roleToAdd.principalId -and $_.roleDefinitionName -eq $roleToAdd.role -and $_.scope -eq $($roleToAdd.scope) }

        if ($null -ne $matchingRole -or $matchingRole.Count -gt 0) {
            Write-Debug "Role has already been assigned to scope for Assignee:$($roleToAdd.principalId) Role:$($roleToAdd.role) Scope:$($roleToAdd.scope) Subscription:$($roleToAdd.subscriptionId)"
        }
        else {
            Add-Role -Assignee $roleToAdd.principalId -Role $roleToAdd.role -Scope $roleToAdd.scope -WhatIf:$(-not $PSCmdlet.ShouldProcess("$($roleToAdd.principalId) Role:$($roleToAdd.role) Scope:$($roleToAdd.scope) Subscription:$($roleToAdd.subscriptionId)"))
        }
    }
}

function Assert-NoMissingRole {

    param (
        [Parameter(Mandatory = $false)]
        [System.Collections.ArrayList]$existingRoles,

        [Parameter(Mandatory = $true)]
        [System.Collections.ArrayList]$rolesinFile
    )

    foreach ($existingRole in $existingRoles) {
        $matchingRole = $rolesinFile | Where-Object { $existingRole.principalId -eq $_.principalId -and $existingRole.roleDefinitionName -eq $_.role -and $existingRole.scope -eq $_.scope }
        if ($null -eq $matchingRole -or $matchingRole.Count -eq 0) {
            Write-Warning "Role is assigned but not in the file Assignee:$($existingRole.principalId) Role:$($existingRole.roleDefinitionName) Scope:$($existingRole.scope)"
        }
    }
}
# Work
$assignmentFiles = Get-ChildItem -Path $PSScriptRoot -Filter $subscriptionFileFilter

$managementGroupAssignments = $assignmentFiles | ForEach-Object {
    $content = Get-Content -Path $_.FullName | ConvertFrom-Yaml
    $hasManagementGroupRoles = $null -ne $content.managementGroupRoles
    Write-Verbose "Found $($_.FullName) which handles $($content.managementGroupName)`n`r         with managementGroupRoles: $hasManagementGroupRoles"
    return $content
}

foreach ($managementGroup in $managementGroupAssignments) {
    $existing = az role assignment list --scope $managementGroup.variables.scopes -o json | ConvertFrom-Json
    $inFile = New-Object System.Collections.ArrayList
    # Roles that are generally managed by other systems don't get included in the assignment audit and should be left out of this list
    $controlledRoles = New-Object System.Collections.ArrayList

    foreach ($role in $managementGroup.managementGroupRoles) {
        foreach ($assignment in $role.assignments) {
            $roles = @(@{
                principalId    = $assignment
                role           = $role.role
                scope          = $role.scope
            })
            $inFile.AddRange($roles)
            $controlledRoles.AddRange($roles)
        }
    }
    Add-NewRole -existingRoles $existing -rolesinFile $inFile -WhatIf:$WhatIf
    Write-Verbose "Asserting that no roles are missing in management group $($managementGroup.managementGroupName)"
    Assert-NoMissingRole -existingRoles $existing -rolesinFile $controlledRoles
}

