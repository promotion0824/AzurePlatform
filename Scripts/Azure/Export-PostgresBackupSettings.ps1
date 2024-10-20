Param
(
    [string]
    $outFile = "export/pgbackupsettings-$($(Get-Date).ToUniversalTime().toString("yyyyMMdd")).csv"
)
$VerbosePreference = "Continue"
$result = az graph query -q "resources | where type =~ 'microsoft.dbforpostgresql/servers' | project name, resourceGroup, subscriptionId, properties.storageProfile.backupRetentionDays, properties.storageProfile.geoRedundantBackup, properties.fullyQualifiedDomainName, location, type, id" | ConvertFrom-Json

Write-Verbose "Total Records $($result.total_records) got $($result.count) records"

if ($result.count -lt $result.total_records){
    throw "Did not retreive all records"
}
$result.data | Export-Csv $outFile  -NoTypeInformation
