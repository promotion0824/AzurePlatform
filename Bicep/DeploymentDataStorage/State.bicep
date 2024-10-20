param azDoPrincipalId string = 'f9624c1d-234a-4527-8fd6-6ca04a8dfa6e'
param engineersPrincipalId string = 'c791ea50-fa7e-4842-b285-ad230e1dea9c'
param vaultName string = 'K8sInternalDeployKeys'
param vaultAccessPolicies array = [
  {
    tenantId: 'd43166d1-c2a1-4f26-a213-f620dba13ab8'
    objectId: 'dd7ecc03-af56-40a9-b591-ef46d887ee5f'
    permissions: {
      keys: [
        'Encrypt'
        'Decrypt'
      ]
      secrets: []
      certificates: []
    }
  }
]
param storageAccountName string = 'k8sintdeploydata'

@description('Tags for resources.')
param tags object = {}

// Set the location for all resources to be the same as the resource group they are deployed to
param location string = resourceGroup().location

resource vaultName_resource 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: vaultName
  tags: tags
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: 'd43166d1-c2a1-4f26-a213-f620dba13ab8'
    accessPolicies: vaultAccessPolicies
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: false
    enablePurgeProtection: true
    provisioningState: 'Succeeded'
  }
}

resource storageAccountName_resource 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  tags: tags
  location: location
  sku: {
    name: 'Standard_RAGRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true
    allowSharedKeyAccess: true
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource vaultName_pulumi 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  parent: vaultName_resource
  name: 'pulumi'
  tags: tags
  properties: {
    kty: 'RSA'
    keySize: 2048
    attributes: {
      enabled: true
    }
  }
}

resource storageAccountName_default 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccountName_resource
  name: 'default'
  properties: {
    changeFeed: {
      enabled: false
    }
    restorePolicy: {
      enabled: false
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    isVersioningEnabled: false
  }
}
var storageContributorContributor = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
resource azDoStorageAccountRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(azDoPrincipalId, storageContributorContributor, storageAccountName_default.id)
  scope: storageAccountName_default
  properties: {
    principalId: azDoPrincipalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageContributorContributor)
  }
}

resource storageAccountName_default_pulumi 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: storageAccountName_default
  name: 'pulumi'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
}

resource storageAccountName_default_terraform_state 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: storageAccountName_default
  name: 'terraform-state'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
}

var storageReaader = '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
resource pulumiEngineerReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(engineersPrincipalId, storageReaader, storageAccountName_default.id)
  scope: storageAccountName_default_pulumi
  properties: {
    principalId: engineersPrincipalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageReaader)
  }
}

output key object = reference(vaultName_pulumi.id, '2019-09-01', 'Full')
