# Get all alert manifests
$AlertManifest = Get-ChildItem -Path $PSScriptRoot\AlertManifests -Filter "Server Request Duration by Customer Instance App Role.yaml"
foreach ($AlertManifest in $AlertManifest) {

    $AlertManifestPath = $AlertManifest.FullName
    $AlertManifestContent = Get-Content -Path $AlertManifestPath -Raw | ConvertFrom-Yaml
    $ManifestAlertRulesFolder = ("$PSScriptRoot\AlertRules\Single Tenant\HighFrequency\{0}" -f $AlertManifestContent.Title)

    $Resources = (az graph query -q "Resources | project name, type, location, resourceGroup, id, tags | where type=~ '$($AlertManifestContent.ResourceType)' and name !contains 'sandbox' and name !contains 'audit'" --management-groups WillowTwinDev WillowTwinPrd | ConvertFrom-Json).data
    $filenames = [System.Collections.ArrayList]@()

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

        $template = Get-Content "$PSScriptRoot\AlertTemplates\Server Request Duration By Customer Instance App Role Template.json" | ConvertFrom-Json

        $template.title = $title
        $template.labels.destination="ZenDesk"
        $template.labels.severity="$($AlertManifestContent.Severity)"
        $template.labels | Add-member -NotePropertyName "region" -NotePropertyValue $Location
        $template.data[0].model.azureMonitor.region = $Location
        $template.data[0].model.azureMonitor.resources[0].region = $Location
        $template.data[0].model.azureMonitor.resources[0].resourceGroup = $Rg
        $template.data[0].model.azureMonitor.resources[0].resourceName = $resourceName
        $template.data[0].model.azureMonitor.resources[0].subscription = $subscription
        $template.data[0].model.subscription = $subscription
        $template.data[2].model.conditions[0].evaluator.params[0] = $AlertManifestContent.threshold
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

        $FileName = "{0}.json" -f $Resource.name
        $template | ConvertTo-Json -Depth 100 | Set-Content -Path ("{0}\{1}" -f $ManifestAlertRulesFolder, $FileName) -Force
        $filenames.Add($FileName)
    }

    # delete any alert rules which no longer have a corresponding resource
    $alertRules = Get-ChildItem -Path $ManifestAlertRulesFolder -Filter *.json -Recurse
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

