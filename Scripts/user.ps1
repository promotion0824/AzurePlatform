$engineers = az ad group member list --group "Engineers_Willow" -o json | ConvertFrom-Json | ForEach-Object { $_.userPrincipalName }


$groups = @(
    "P&E Willow Twin Platform",
    "P&E Twin Lifecycle Management",
    "P&E Willow Twin IoT",
    "P&E Willow Twin Marketplace",
    "P&E Real Estate",
    "P&E Quality Control",
    "P&E Data Science & Engineering",
    "P&E Rail",
    "P&E Mining",
    "P&E Platform Engineering",
    "Willow AppSec Team"
)

$userWithGroups = [System.Collections.ArrayList]::new()

foreach ($group in $groups) {

    $users =  az ad group member list --group $group -o json | ConvertFrom-Json  | ForEach-Object { $_.userPrincipalName }
    $userWithGroups.AddRange($users)
}

$usersWithoutGroups = $engineers | Where-Object { $_ -notin $userWithGroups }
$usersWithoutGroups
