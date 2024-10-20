# Get all alert manifests
$AlertManifest = Get-ChildItem -Path $PSScriptRoot\AlertManifests -Filter "Avg Memory Working Set.yaml"
foreach ($AlertManifest in $AlertManifest) {

    $AlertManifestPath = $AlertManifest.FullName
    $AlertManifestContent = Get-Content -Path $AlertManifestPath -Raw | ConvertFrom-Yaml
    $ManifestAlertRulesFolder = ("$PSScriptRoot\AlertRules\Single Tenant\HighFrequency\{0}" -f $AlertManifestContent.Title)

    $kqlquery = "Resources | project name, type, location, resourceGroup, id, tags | where type=~ '$($AlertManifestContent.ResourceType)'"

    $skipResult = 0
    $batchSize = 500

    $result = az graph query -q $kqlquery --management-groups WillowTwinDev WillowTwinPrd --skip $skipResult --first $batchSize  | ConvertFrom-Json
    $Resources = $result.data
    write-host "Resources returned: {0}", $Resources.Count

    while ($true) {
        $skipResult += $batchSize

        $moreResults += (az graph query -q $kqlquery --management-groups WillowTwinDev WillowTwinPrd --skip $skipResult --first $batchSize  | ConvertFrom-Json)

        if ($moreResults -eq $null) {
            break
        }

        moreResources = moreResults.data;
        write-host "Resources returned: " $moreResources.Count
        $Resources += $moreResources

        if ($moreResources.Count -lt $batchSize) {
            break
        }
    }

    $filenames = [System.Collections.ArrayList]@()

    foreach ($Resource in $Resources) {

        if ($Resource.resourceGroup -eq $null) {
            continue
        }

        # Set values to be replaced in the template
        $title = "{0} {1} {2}" -f $AlertManifestContent.Title, $Resource.name, $Resource.resourceGroup
        $Id = $Resource.id
        $Rg = $Resource.resourceGroup
        $Location = $Resource.location
        $Tags = $Resource.tags
        $idSplit = $Id.Split('/')
        $subscription = $idSplit[2]

        if ($null -ne $idSplit[10]) {
            $resourceName = "{0}/{1}" -f $idSplit[8], $idSplit[10]
        }
        else {
            $resourceName = $idSplit[8]
        }

        $template = Get-Content "$PSScriptRoot\AlertTemplates\Avg Memory Working Set Template.json" | ConvertFrom-Json

        $template.title = $title
        $template.labels.destination="ZenDesk"
        $template.labels.severity="$($AlertManifestContent.Severity)"
        $template.labels | Add-member -NotePropertyName "region" -NotePropertyValue $Location
        $template.data[0].model.azureMonitor.metricName = $AlertManifestContent.metricName
        $template.data[0].model.azureMonitor.metricNamespace = $AlertManifestContent.metricNamespace
        $template.data[0].model.azureMonitor.region = $Location
        $template.data[0].model.azureMonitor.resources[0].resourceGroup = $Rg
        $template.data[0].model.azureMonitor.resources[0].resourceName = $resourceName
        $template.data[0].model.subscription = $subscription
        $template.data[1].model.azureResourceGraph.query="Resources | where subscriptionId =~ '$subscription' | where resourceGroup =~ '$rg' | where type =~ 'Microsoft.App/containerApps' | where name =~ '$resourceName' | extend containers = todynamic(properties.template.containers) | mv-expand containers | summarize memory = sum(todouble(substring(containers.resources.memory, 0, strlen(containers.resources.memory) - 2)) * 1000) | extend timestamp = now()"
        $template.data[1].model.subscriptions[0] = $subscription
        $template.data[5].model.conditions.evaluator.params[0] = $AlertManifestContent.threshold
        $template.labels | Add-member -NotePropertyName "tsg" -NotePropertyValue $AlertManifestContent.tsg

        # Add all resource tags as labels
        foreach ($property in $Tags.psobject.properties) {
            $template.labels | Add-member -NotePropertyName $property.Name -NotePropertyValue $property.Value
        }

        $template

        if (test-path -Path $ManifestAlertRulesFolder) {
            write-host "Folder $ManifestAlertRulesFolder already exists"
        }
        else {
            write-host "Creating folder $ManifestAlertRulesFolder"
            new-item -Path $ManifestAlertRulesFolder -ItemType Directory
        }
        $FileName = "{0}-{1}.json" -f $Resource.name, $Resource.resourceGroup
        $template | ConvertTo-Json -Depth 100 | Set-Content -Path ("{0}\{1}" -f $ManifestAlertRulesFolder, $FileName) -Force
        $filenames.Add($FileName)
    }

    # delete any alert rules which no longer have a corresponding resource
    $alertRules = Get-ChildItem -Path $ManifestAlertRulesFolder -Filter *.json -Recurse

    foreach ($alertRuleFile in $alertRules) {
        if ($alertRuleFile.name -notin $filenames) {
            write-host "Removing $($alertRuleFile.FullName)"
            remove-item -LiteralPath $alertRuleFile.FullName
        }
    }
}
