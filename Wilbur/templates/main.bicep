param botPrefix string
param msAppId string
param resourceTags object

@description('Displayname of the bot')
param botDisplayName string

@description('The address of the hosting for the bot')
param botEndpoint string

@description('The azure ad that has access to keyvault')
param keyVaultOwner string


@description('The azure ad the bot runs under')
param podIdentityPrincipalId string

@description('The azure application insights key for the bot framework')
param applicationInsightsKey string

resource botId_kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${botPrefix}-kv'
  location: resourceGroup().location
  tags: resourceTags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        permissions: {
          keys: []
          secrets: [
            'get'
            'list'
          ]
          certificates: []
        }
        tenantId: subscription().tenantId
        objectId: podIdentityPrincipalId
      }
      {
        permissions: {
          keys: []
          secrets: [
            'get'
            'list'
            'set'
          ]
          certificates: []
        }
        tenantId: subscription().tenantId
        objectId: keyVaultOwner
      }
    ]
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: false
    enablePurgeProtection: true
  }
}

resource botId_resource 'Microsoft.BotService/botServices@2022-09-15' = {
  name: '${botPrefix}-bot'
  kind: 'bot'
  location: 'global'
  sku: {
    name: 'F0'
  }
  tags: resourceTags
  properties: {
    displayName: botDisplayName
    endpoint: botEndpoint
    msaAppId: msAppId
    developerAppInsightKey:applicationInsightsKey
  }
  dependsOn: []
}

resource botTeamsChannel_resource 'Microsoft.BotService/botServices/channels@2022-09-15' = {
  name: '${botId_resource.name}/MsTeamsChannel'
  tags: resourceTags
  location: 'global'
  properties: {
    channelName: 'MsTeamsChannel'
    properties:{
      enableCalling: false
      isEnabled: true
    }
  }
}

output keyVaultUri string = botId_kv.properties.vaultUri
