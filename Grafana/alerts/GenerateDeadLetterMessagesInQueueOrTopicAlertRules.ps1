# Get all alert manifests
$AlertManifest = Get-ChildItem -Path $PSScriptRoot\AlertManifests -Filter "Dead Letter Messages in Queue or Topic.yaml"
foreach ($AlertManifest in $AlertManifest) {

    $AlertManifestPath = $AlertManifest.FullName
    $AlertManifestContent = Get-Content -Path $AlertManifestPath -Raw | ConvertFrom-Yaml
    $ManifestAlertRulesFolder = ("$PSScriptRoot\AlertRules\Single Tenant\HighFrequency\{0}" -f $AlertManifestContent.Title)

    $Resources = (az graph query -q "Resources | project name, type, location, resourceGroup, id, tags | where type=~ '$($AlertManifestContent.ResourceType)'" --management-groups WillowTwinDev WillowTwinPrd | ConvertFrom-Json).data
    foreach ($Resource in $Resources) {

        # Set values to be replaced in the template
        $title = "{0} {1}" -f $AlertManifestContent.Title, $Resource.name
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

        $template = Get-Content "$PSScriptRoot\AlertTemplates\Dead Letter Messages in Queue or Topic Template.json" | ConvertFrom-Json

        $template.title = $title
        $template.labels.destination="ZenDesk"
        $template.labels.severity="$($AlertManifestContent.Severity)"
        $template.labels | Add-member -NotePropertyName "region" -NotePropertyValue $Location
        $template.data[0].model.azureMonitor.aggregation = $AlertManifestContent.aggregation
        $template.data[0].model.azureMonitor.metricName = $AlertManifestContent.metricName
        $template.data[0].model.azureMonitor.metricNamespace = $AlertManifestContent.metricNamespace
        $template.data[0].model.azureMonitor.region = $Location
        $template.data[0].model.azureMonitor.resources[0].metricNamespace = $AlertManifestContent.metricNamespace
        $template.data[0].model.azureMonitor.resources[0].resourceGroup = $Rg
        $template.data[0].model.azureMonitor.resources[0].resourceName = $resourceName
        $template.data[0].model.azureMonitor.resources[0].subscription = $subscription
        $template.data[0].model.azureMonitor.resources[0].region = $Location
        $template.data[2].model.conditions.evaluator.params[0] = $AlertManifestContent.threshold
        $template.data[0].model.subscription = $subscription
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

        $template | ConvertTo-Json -Depth 100 | Set-Content -Path ("{0}\{1}.json" -f $ManifestAlertRulesFolder, $Resource.name) -Force
    }

    # delete any alert rules which no longer have a corresponding resource
    $alertRules = Get-ChildItem -Path $ManifestAlertRulesFolder -Filter *.json -Recurse
    $filenames = [System.Collections.ArrayList]@()
    foreach ($resource in $resources) {
        $filenames.Add("{0}.json" -f $Resource.name)
    }

    foreach ($alertRuleFile in $alertRules) {
        if ($alertRuleFile.name -notin $filenames) {
            write-host "Removing $($alertRuleFile.FullName)"
            remove-item -LiteralPath $alertRuleFile.FullName
        }
    }
}
