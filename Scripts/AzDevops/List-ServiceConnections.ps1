Param
(
    [string]
    $org = "https://dev.azure.com/willowdev/"
)

$projects = az devops project list --org $org -o json | ConvertFrom-Json | Select-Object -ExpandProperty value

$allEndpoints = New-Object System.Collections.ArrayList
foreach ($project in $projects) {
    $endpoints = az devops service-endpoint list --org $org --project $project.id -o json | ConvertFrom-Json

    foreach ($endpoint in $endpoints) {
        $allEndpoints.AddRange(@(@{
            project=$project
            endpoint=$endpoint}))
    }
}
return $allEndpoints
