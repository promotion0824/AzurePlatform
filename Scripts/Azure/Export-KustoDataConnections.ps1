Param
(
    [string]
    $outFile = "export/kustoDataConnections-$($(Get-Date).ToUniversalTime().toString("yyyyMMdd")).csv"
)
$VerbosePreference = "Continue"

$connections = New-Object System.Collections.ArrayList


$clusters = az graph query -q "resources | where type =~ 'microsoft.kusto/clusters' and properties.state != 'Stopped' | project name, resourceGroup, subscriptionId" | ConvertFrom-Json


foreach ($cluster in $clusters.data) {
    $databases = az kusto database list --cluster-name $cluster.name --resource-group $cluster.resourceGroup --subscription $cluster.subscriptionId -o json | ConvertFrom-Json

    foreach ($database in $databases) {
        $dbName = $database.name -replace "$($cluster.name)/", ""
        $dataConnections = az kusto data-connection list --cluster-name $cluster.name --database-name $dbName  --resource-group $cluster.resourceGroup --subscription $cluster.subscriptionId -o json | ConvertFrom-Json
        foreach ($connection in $dataConnections) {
            $connections.AddRange(@(@{
                cluster=$cluster.name
                resourceGroup=$cluster.resourceGroup
                subscription=$connection.subscriptionId
                database=$connection.dbName
                connectionName=$connection.name
                consumerGroup=$connection.consumerGroup
                tableName=$connection.tableName
                eventHubResourceId=$connection.eventHubResourceId
            }))
        }
    }
}

$connections

$connections | ForEach-Object { New-Object PSObject -Property $_ } | Export-Csv $outFile  -NoTypeInformation
