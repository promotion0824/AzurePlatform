// Create the ZenDesk connector function app
// Zendesk connector params
param zendeskFunctionName string
param zendeskFunctionEnvironment string

@description('Tags for resources.')
param tags object = {}

// Set the location for all resources to be the same as the resource group they are deployed to
param location string = resourceGroup().location

// pipeline-deployer-dev-spn
param pipelineDeloyerObjectId string = 'bd2c9873-feb2-4948-88c6-257544aecb64'

// Names of dependencies
param KeyvaultName string
param AppInsightsName string
param AppInsightsResourceGroupName string
param storageAccountName string

// Get references to resources that the function depends on
resource Keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: KeyvaultName
}

resource AppInsights  'Microsoft.Insights/components@2020-02-02' existing = {
  name: AppInsightsName
  scope: resourceGroup(AppInsightsResourceGroupName)
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

// Create the Zendesk connector function app
resource zendeskconnector 'Microsoft.Web/sites@2023-12-01' = {
  name: zendeskFunctionName
  kind: 'functionapp'
  location: location
  tags: tags
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: 'fnzendeskconnectordev.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: 'fnzendeskconnectordev.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    reserved: false
    isXenon: false
    hyperV: false
    vnetRouteAllEnabled: false
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: ''
      acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: false
      functionAppScaleLimit: 200
      minimumElasticInstanceCount: 0
      appSettings: [
        {name: 'AzureWebJobsStorage__accountName', value: storageAccountName}
        {name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4'}
        {name: 'AZURE_FUNCTIONS_ENVIRONMENT', value: zendeskFunctionEnvironment}
        {name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated'}
        {name: 'APPINSIGHTS_INSTRUMENTATIONKEY', value: AppInsights.properties.InstrumentationKey}
        {name: 'APPINSIGHTS_CONNECTION_STRING', value: AppInsights.properties.ConnectionString}
        {name: 'ApplicationInsightsAgent_EXTENSION_VERSION', value: '~2'}
      ]
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: false
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    customDomainVerificationId: '44E182F437C82D4E1EFA560F9296D67A201CFE6AAC063CE9B59550605AEA4F90'
    containerSize: 1536
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    redundancyMode: 'None'
    publicNetworkAccess: 'Enabled'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Grant Zendesk Managed Identity Blob Owner access to the storage account
var storageBlobDataOwner = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
resource zendeskBlobOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(pipelineDeloyerObjectId, storageBlobDataOwner, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: zendeskconnector.identity.principalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwner)
  }
}

// Grant Zendesk Managed Identity Table Data Contributor access to the storage account
var tableDataContributor = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
resource zendeskTableDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(pipelineDeloyerObjectId, tableDataContributor, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: zendeskconnector.identity.principalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', tableDataContributor)
  }
}

// Add a key vault access policy to allow the Zendesk connector to read secrets
resource accessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: Keyvault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: zendeskconnector.identity.principalId
        permissions: {
          keys: []
          secrets: ['Get', 'List']
          certificates: []
        }
      }
    ]
  }
}

// Create the action group for the Zendesk connector
var functionKey = listkeys('${zendeskconnector.id}/host/default', '2021-03-01').masterKey
resource symbolicname 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'ag-zendesk-webhook'
  location: 'Global'
  properties: {
    enabled: true
    groupShortName: 'zendesk'
    azureFunctionReceivers: [
      {
        name: 'zendesk-azure-function'
        functionAppResourceId: zendeskconnector.id
        functionName: 'AzureAlertZendeskIntegration'
        useCommonAlertSchema: true
        httpTriggerUrl: 'https://${zendeskconnector.properties.defaultHostName}/api/azure/alerts?code=${functionKey}'
      }
    ]
  }
}
