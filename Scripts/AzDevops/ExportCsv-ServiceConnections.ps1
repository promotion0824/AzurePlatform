Param
(
    [string]
    $org = "https://dev.azure.com/willowdev/",

    [string]
    $outFile = "connections.csv"
)


$serviceConnections = .\List-ServiceConnections.ps1 -org $org


$reportData = New-Object System.Collections.ArrayList


foreach ($serviceConnection in $serviceConnections) {
    $auth=$serviceConnection.endpoint.authorization
    $data=$serviceConnection.endpoint.data

    if ($null -ne $auth){
        $sp = $auth.parameters.serviceprincipalid
        $authType = $auth.parameters.authenticationType
        $loginServer = $auth.parameters.loginServer
        $username = $auth.parameters.username
    }
    if ($null -ne $data){
        $subscriptionId = $data.subscriptionId
        $subscriptionName = $data.subscriptionName
        $clusterId = $data.clusterId
        $namespace = $data.namespace
        $appObjectId = $data.appObjectId
        $azureSpnRoleAssignmentId = $data.azureSpnRoleAssignmentId
    }
    $reportData.AddRange(@(@{
        projectId=$serviceConnection.project.id
        projectName=$serviceConnection.project.name
        endpointId=$serviceConnection.endpoint.id
        type=$serviceConnection.endpoint.type
        shared=$serviceConnection.endpoint.shared
        ready=$serviceConnection.endpoint.ready
        sp=$sp
        authType=$authType
        loginServer = $loginServer
        username = $username
        subscriptionId = $subscriptionId
        subscriptionName = $subscriptionName
        clusterId = $clusterId
        namespace = $namespace
        appObjectId = $appObjectId
        azureSpnRoleAssignmentId = $azureSpnRoleAssignmentId
    }))

    $auth= $null
    $sp = $null
    $authType = $null
    $loginServer = $null
    $username = $null
    $subscriptionId = $null
    $subscriptionName = $null
    $clusterId = $null
    $namespace = $null
    $appObjectId = $null
    $azureSpnRoleAssignmentId = $null
}

$reportData | ForEach-Object {  New-Object PSObject -Property $_ } | Export-Csv $outFile  -NoTypeInformation
