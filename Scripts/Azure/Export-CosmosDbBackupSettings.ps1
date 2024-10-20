Param
(
    [string]
    $outFile = "export/cosmosbackupsettings-$($(Get-Date).ToUniversalTime().toString("yyyyMMdd")).csv"
)
$VerbosePreference = "Continue"
$result = az graph query -q "resources | where type =~ 'Microsoft.DocumentDB/databaseAccounts' | project name, resourceGroup, subscriptionId, properties.backupPolicy.type, properties.backupPolicy.periodicModeProperties.backupIntervalInMinutes, properties.backupPolicy.periodicModeProperties.backupRetentionIntervalInHours, properties.backupPolicy.periodicModeProperties.backupStorageRedundancy, location, type, id" | ConvertFrom-Json

Write-Verbose "Total Records $($result.total_records) got $($result.count) records"

if ($result.count -lt $result.total_records){
    throw "Did not retreive all records"
}
$result.data | Export-Csv $outFile  -NoTypeInformation
