
[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    $subscriptionFileFilter = "*.yaml",

    [Parameter(Mandatory = $false)]
    $resourceGroupFilter = "rg-twin-willow-",

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
        [string]$Scope,

        [Parameter(Mandatory = $true)]
        [string]$Subscription
    )

    if ($PSCmdlet.ShouldProcess("Assign $Assignee a role of $Role for $Scope in $Subscription")) {
        try {
            Write-Output "Assigning $Assignee a role of $Role for $Scope in $Subscription"
            az role assignment create --role $Role --scope $Scope --subscription $Subscription --assignee $Assignee --description "Role assignment from AzurePlatform/AzureRoles"
        }
        catch {
            Write-Warning "Could not assign $Assignee a role of $Role for $Scope in $Subscription check that the resource and object id still exist"
        }

    }
    else {
        $WhatIfPreference = $false
        "Would assign $Assignee a role of $Role for $Scope in $Subscription" | Out-File -FilePath ./assignments.txt -Append -Encoding utf8
        $WhatIfPreference = $true
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
        $matchingRole = $existingRoles | Where-Object { $_.principalId -eq $roleToAdd.principalId -and ($_.roleDefinitionId -eq $roleToAdd.role -or $_.roleDefinitionName -eq $roleToAdd.role) -and $_.scope -eq $($roleToAdd.scope) }
        if ($null -ne $matchingRole -or $matchingRole.Count -gt 0) {
            Write-Debug "Role has already been assigned to scope for Assignee:$($roleToAdd.principalId) Role:$($roleToAdd.role) Scope:$($roleToAdd.scope) Subscription:$($roleToAdd.subscriptionId)"
        }
        else {
            Add-Role -Assignee $roleToAdd.principalId -Role $roleToAdd.role -Scope $roleToAdd.scope -Subscription $roleToAdd.subscriptionId -WhatIf:$(-not $PSCmdlet.ShouldProcess("$($roleToAdd.principalId) Role:$($roleToAdd.role) Scope:$($roleToAdd.scope) Subscription:$($roleToAdd.subscriptionId)"))
        }
    }
}

function Assert-NoMissingRole {

    param (
        [Parameter(Mandatory = $false)]
        [string]$subscriptionId,

        [Parameter(Mandatory = $false)]
        [System.Collections.ArrayList]$existingRoles,

        [Parameter(Mandatory = $true)]
        [System.Collections.ArrayList]$rolesinFile,


        [Parameter(Mandatory = $false)]
        $resourceGroupFilter
    )

    foreach ($existingRole in $existingRoles) {
        $matchingRole = $rolesinFile | Where-Object { $existingRole.principalId -eq $_.principalId -and $existingRole.roleDefinitionId -eq $_.role -and $existingRole.scope -eq $_.scope }
        $parentScope =  $existingRole.scope -eq "/" -or $existingRole.scope -match "/providers/Microsoft.Management/managementGroups/*"
        if ($($null -eq $matchingRole -or $matchingRole.Count -eq 0) -and -not $parentScope) {

            if ($existingRole.scope -match "/subscriptions/$($subscriptionId)/resourceGroups/$($resourceGroupFilter)*"){
                Write-Debug "Existing role ignored due to resoure group filter `n`r         Assignee:$($existingRole.principalId)/$($existingRole.principalName) `n`r         Role:$($existingRole.roleDefinitionId) `n`r         Scope:$($existingRole.scope)"
            }
            elseif ($existingRole.scope -eq "/subscriptions/$($subscriptionId)") {
                # Subsciption level roles should be recorded in this file
                # One day theses should be cleaned up automatically
                Write-Output "`n`r
  - role: $($existingRole.roleDefinitionId) # $($existingRole.roleDefinitionName)
    scope: $($existingRole.scope)
    assignments:
    - $($existingRole.principalId) #$($existingRole.principalName)"
            }
            elseif ($existingRole.scope -match "/subscriptions/$($subscriptionId)/*" -and $($existingRole.principalType -eq "User" -or $existingRole.principalType -eq "Group")) {
                # Like subscription level roles roles for users or groups to specific resources should be recorded in this file
                # One day theses should be cleaned up automatically
                Write-Output "`n`r
  - role: $($existingRole.roleDefinitionId) # $($existingRole.roleDefinitionName)
    scope: $($existingRole.scope)
    assignments:
        - $($existingRole.principalId) #$($existingRole.principalName)"
            }
            elseif ($existingRole.scope -match "/subscriptions/$($subscriptionId)/*") {
                # Most of these roles are probably created by apps its not feasible to manage this by this role approach when using managed identities
                Write-Debug "Resource Role at resource scope doesn't exist in file `n`r         Assignee:$($existingRole.principalId)/$($existingRole.principalName) `n`r         Role:$($existingRole.roleDefinitionId) `n`r         Scope:$($existingRole.scope)"
            }
            else {
                # These roles are generally managment group roles and not handled here
                Write-Warning "Role at other scope doesn't `n`r         Assignee:$($existingRole.principalId)/$($existingRole.principalName) `n`r         Role:$($existingRole.roleDefinitionId) `n`r         Scope:$($existingRole.scope)"
            }
        } elseif ($parentScope){
            Write-Debug "Exists at a parent scope `n`r         Assignee:$($existingRole.principalId)/$($existingRole.principalName) `n`r         Role:$($existingRole.roleDefinitionId) `n`r         Scope:$($existingRole.scope)"
        }
        else {
            Write-Debug "Exists in file and subscription `n`r         Assignee:$($existingRole.principalId)/$($existingRole.principalName) `n`r         Role:$($existingRole.roleDefinitionId) `n`r         Scope:$($existingRole.scope)"
        }
    }
}
# Work
$assignmentFiles = Get-ChildItem -Path $PSScriptRoot -Filter $subscriptionFileFilter

$subscriptionAssignments = $assignmentFiles | ForEach-Object {
    $content = Get-Content -Path $_.FullName, .\AzureRoles\assignments\variables.yml | ConvertFrom-Yaml
    $hasClusterRoles = $null -ne $content.clusterRoles
    $hasSubscriptionRoles = $null -ne $content.subscriptionRoles
    $hasSubscriptionAppRoles = $null -ne $content.subscriptionAppRoles
    Write-Verbose "Found $($_.FullName) which handles $($content.subscriptionName)-$($content.subscriptionId) `n`r         with SubscriptionAppRole: $hasSubscriptionAppRoles, SubscriptionRole: $hasSubscriptionRoles, ClusterRoles: $hasClusterRoles"
    return $content
}

foreach ($subscription in $subscriptionAssignments) {
    $existing = az role assignment list --subscription $subscription.subscriptionId --all -o json | ConvertFrom-Json
    $inFile = New-Object System.Collections.ArrayList
    # Roles that are generally managed by other systems don't get included in the assignment audit and should be left out of this list
    $controlledRoles = New-Object System.Collections.ArrayList

    foreach ($cluster in $subscription.clusterRoles) {
        $clusterId = "/subscriptions/$($subscription.subscriptionId)/resourceGroups/$($cluster.resourceGroup)/providers/Microsoft.ContainerService/managedClusters/$($cluster.name)"
        foreach ($namespace in $cluster.namespaces) {
            $scope = "$clusterId/namespaces/$($namespace.namespace)"
            foreach ($assignment in $namespace.assignments) {
                $roles = @(@{
                    principalId    = $assignment
                    role           = $namespace.role
                    scope          = $scope.TrimEnd('/')
                    subscriptionId = $subscription.subscriptionId
                })
                $inFile.AddRange($roles)
                $controlledRoles.AddRange($roles)
            }
        }
    }

    foreach ($role in $subscription.subscriptionRoles) {
        $scope = if ([string]::IsNullOrWhiteSpace($role.scope.TrimEnd('/'))) {"/subscriptions/$($subscription.subscriptionId)"} else {$role.scope}
        foreach ($assignment in $role.assignments) {
            $roles = @(@{
                principalId    = $assignment
                role           = $role.role
                scope          = $scope.TrimEnd('/')
                subscriptionId = $subscription.subscriptionId
            })
            $inFile.AddRange($roles)
            $controlledRoles.AddRange($roles)
        }
    }

    foreach ($role in $subscription.subscriptionAppRoles) {
        $scope = if ([string]::IsNullOrWhiteSpace($role.scope.TrimEnd('/'))) {"/subscriptions/$($subscription.subscriptionId)"} else {$role.scope}
        foreach ($assignment in $role.assignments) {
            $roles = @(@{
                principalId    = $assignment
                role           = $role.role
                scope          = $scope.TrimEnd('/')
                subscriptionId = $subscription.subscriptionId
            })
            $inFile.AddRange($roles)
            $controlledRoles.AddRange($roles)
        }
    }

    if ($inFile.Count -ne 0) {
        Add-NewRole -existingRoles $existing -rolesinFile $inFile -WhatIf:$WhatIf
    }
    write-verbose "Checking for roles missing from files in $($subscription.subscriptionId)"
    Assert-NoMissingRole -existingRoles $existing -rolesinFile $controlledRoles -subscriptionId $subscription.subscriptionId -resourceGroupFilter $resourceGroupFilter
}

