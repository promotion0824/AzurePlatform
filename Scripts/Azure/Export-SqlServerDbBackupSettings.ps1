Param
(
    [string]
    $outFile = "export/sqldbbackupsettings-$($(Get-Date).ToUniversalTime().toString("yyyyMMdd")).csv"
)
$VerbosePreference = "Continue"

function DaysOfBackupAvailable {
    param (
        [string]
        $dateString,

        [string]
        $replicationType
    )
    if ([string]::IsNullOrWhiteSpace($dateString) -and $replicationType -eq "Geo"){
        return "GeoReplica"
    }
    if ([string]::IsNullOrWhiteSpace($dateString)){
        return "NONE"
    }

    return $($(Get-Date).ToUniversalTime() - $([DateTime]$dateString)).TotalDays
}

function  ServerName {
    param (
        [string]
        $dbId
    )
    $match = Select-string "servers/(.*)/databases" -InputObject $dbId
    return $match.Matches[0].Groups[1].Value
}


$result = az graph query -q "resources | where type =~ 'Microsoft.Sql/servers/databases'| where name != 'main' | project name, resourceGroup, subscriptionId, properties.defaultSecondaryLocation, properties.earliestRestoreDate, properties.storageAccountType, properties.zoneRedundant, properties.secondaryType, location, type, id" --first 1000 | ConvertFrom-Json

Write-Verbose "Total Records $($result.total_records) got $($result.count) records"

if ($result.count -lt $result.total_records){
    throw "Did not retreive all records"
}

$result.data | ForEach-Object {
    Add-Member -NotePropertyName BackupDays -NotePropertyValue $(DaysOfBackupAvailable -dateString $($_.properties_earliestRestoreDate) -replicationType $($_.properties_secondaryType)) -force -InputObject $_
    Add-Member -NotePropertyName DbServer -NotePropertyValue $(ServerName -dbId $($_.id)) -force -InputObject $_

}

$result.data | Export-Csv $outFile  -NoTypeInformation
