

@description('Name of the workspace where the data will be stored.')
param workspaceName string = 'myWorkspace'

@description('Name of the application insights resource.')
param applicationInsightsName string = 'myApplicationInsights'

@description('Location for all resources.')
param location string = resourceGroup().location

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
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
  tags: {
    PipelineId: '744'
    PipelineName: 'SharedClusters.Pulumi'
    app: 'PlatformShared'
    company: 'willow'
    created: '10/20/2021 04:31:58 +00:00'
    customer: 'shared'
    environment: 'nonprod'
    managedby: 'pulumi'
    project: 'https://github.com/WillowInc/AzurePlatform'
    stack: 'nonprod.platformshared'
    team: 'azurePlatform'
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: applicationInsightsName
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
  }
  tags: {
    PipelineId: '744'
    PipelineName: 'SharedClusters.Pulumi'
    app: 'PlatformShared'
    company: 'willow'
    created: '01/31/2023 19:13:52 +00:00'
    customer: 'shared'
    environment: 'nonprod'
    managedby: 'pulumi'
    project: 'https://github.com/WillowInc/AzurePlatform'
    stack: 'nonprod.platformshared'
    team: 'azurePlatform'
  }
}


