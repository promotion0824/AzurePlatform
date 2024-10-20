@description('Name of the workspace where the data will be stored.')
param workspaceName string = 'myWorkspace'

@description('Name of the audit workspace where the audit logs will be stored.')
param auditWorkspaceName string = 'myWorkspace'

@description('Name of the application insights resource.')
param auditApplicationInsightsName string = 'myAuditApplicationInsights'

@description('Name of the application insights resource.')
param applicationInsightsName string = 'myApplicationInsights'

@description('Name of the API management service.')
param apimName string = 'myApim'

@description('Name of the kusto cluster resource.')
param kustoClusterName string = 'myKustoCluster'

@description('SP AppId to be assigned the database admin role')
param kustoClusterDatabaseAdminPrincipalAppId string = ''

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Tags for resources.')
param tags object = {}

@description('Should a regional Edge IOT ACR be deployed')
param deployEdgeACR bool = false

@description('Name of the container registry for IOT Edge.')
param edgeContainerRegistryName string = 'myEdgeContainerRegistry'

@description('Configuration details for Databricks Workspace.')
param databricksWorkspaceInfo object = {
  deployDatabricks: false
  workspaceName: ''
  pricingTier: 'premium'
  accessConnectorName: ''
  catalogType: ''
  ucmetastoreName: ''
}

// Log Analytics workspace for general logs
resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  tags: tags
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      legacy: 0
      searchVersion: 1
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: json('-1.0')
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Log Analytics workspace for Audit logs
resource auditWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: auditWorkspaceName
  tags: tags
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      legacy: 0
      searchVersion: 1
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: json('-1.0')
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Application Insights for Audit metrics
resource auditApplicationInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: auditApplicationInsightsName
  tags: tags
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: auditWorkspace.id

    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    CustomMetricsOptedInType: 'WithDimensions'
  }
}

// Application Insights for Application metrics
resource applicationInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: applicationInsightsName
  tags: tags
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id

    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    CustomMetricsOptedInType: 'WithDimensions'
  }
}

// Shared regional ADX cluster
resource kustoCluster 'Microsoft.Kusto/clusters@2023-08-15' = {
  name: kustoClusterName
  tags: tags
  location: location
  sku: {
    name: 'Standard_L8s_v3'
    capacity: 3
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    optimizedAutoscale: {
      isEnabled: true
      maximum: 4
      minimum: 2
      version: 1
    }
    enableAutoStop: false
    engineType: 'V3'
    enableStreamingIngest: true
    enableDiskEncryption: true
    enablePurge: true
    enableDoubleEncryption: true
    trustedExternalTenants: [
      {
        value: '*'
      }
    ]
  }
}

resource kustoClusterDatabaseAdminRoleAssignment 'Microsoft.Kusto/clusters/principalAssignments@2023-08-15' = {
  name: guid(kustoCluster.name)
  parent: kustoCluster
  properties: {
    principalId: kustoClusterDatabaseAdminPrincipalAppId
    principalType: 'App'
    role: 'AllDatabasesAdmin'
    tenantId: 'd43166d1-c2a1-4f26-a213-f620dba13ab8'
  }
}

resource kustoClusterDiagnosticSetting 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diags-${kustoClusterName}'
  scope: kustoCluster
  properties: {
    logAnalyticsDestinationType: 'Dedicated'
    logs: [
      {
        enabled: true
        categoryGroup: 'allLogs'
      }
      {
        enabled: true
        categoryGroup: 'audit'
      }
    ]
    metrics: [
      {
        enabled: true
        category: 'AllMetrics'
      }
    ]
    workspaceId: workspace.id
  }
}

resource apiManagementService 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  name: apimName
  location: location
  sku: {
    name: tags.environment == 'dev' ? 'Developer' : 'Standard'
    capacity: 1
  }
  tags: tags
  properties: {
    publisherEmail: 'developers@willowinc.com'
    publisherName: 'Willow'
  }
}

// Regional container regiestries for IOT Edge
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = if (deployEdgeACR) {
  sku: {
    name: 'Premium'
  }
  name: edgeContainerRegistryName
  location: location
  tags: tags
  properties: {
    adminUserEnabled: false
    networkRuleSet: {
      defaultAction: 'Allow'
      ipRules: []
    }
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 7
        status: 'disabled'
      }
      exportPolicy: {
        status: 'enabled'
      }
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
      softDeletePolicy: {
        retentionDays: 14
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: 'Disabled'
    anonymousPullEnabled: false
  }
}

var managedResourceGroupName = 'rg-${databricksWorkspaceInfo.workspaceName}-${uniqueString(databricksWorkspaceInfo.workspaceName, resourceGroup().id)}'
var trimmedMRGName = substring(managedResourceGroupName, 0, min(length(managedResourceGroupName), 90))
var managedResourceGroupId = '${subscription().id}/resourceGroups/${trimmedMRGName}'

// https://learn.microsoft.com/en-us/azure/templates/microsoft.databricks/accessconnectors?pivots=deployment-language-bicep
// resource accessConnector 'Microsoft.Databricks/accessConnectors@2024-05-01' = if (databricksWorkspaceInfo.deployDatabricks) {
//   name: databricksWorkspaceInfo.accessConnectorName
//   location: location
//   identity: {
//     type: 'SystemAssigned'
//   }
// }

// https://learn.microsoft.com/en-us/azure/templates/microsoft.databricks/workspaces?pivots=deployment-language-bicep
resource databricksWorkspace 'Microsoft.Databricks/workspaces@2024-05-01' = if (databricksWorkspaceInfo.deployDatabricks) {
  name: databricksWorkspaceInfo.workspaceName
  location: location
  sku: {
    name: databricksWorkspaceInfo.pricingTier
  }
  properties: {
    // accessConnector: {
    //   id: accessConnector.id
    //   identityType: 'SystemAssigned'
    // }
    managedResourceGroupId: managedResourceGroupId
    defaultCatalog: {
      initialType: databricksWorkspaceInfo.catalogType
    }
    parameters: {
      enableNoPublicIp: {
        value: false
      }
    }
  }
}

// https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts?pivots=deployment-language-bicep
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = if (databricksWorkspaceInfo.deployDatabricks) {
  name: databricksWorkspaceInfo.ucmetastoreName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    allowCrossTenantReplication: false
    isNfsV3Enabled: false
    minimumTlsVersion: 'TLS1_0'
    allowBlobPublicAccess: false
    isHnsEnabled: true
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

output workspace object = workspace
