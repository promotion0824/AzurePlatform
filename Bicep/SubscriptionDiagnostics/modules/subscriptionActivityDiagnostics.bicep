
// Created as a module to allow deployment to target multiple subscription
param workspaceId string
param subscriptionName string


targetScope = 'subscription'

resource settingName_resource 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  name: 'ActivityExport-${subscriptionName}'
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'Administrative'
        enabled: true
      }
      {
        category: 'Security'
        enabled: true
      }
      {
        category: 'ServiceHealth'
        enabled: true
      }
      {
        category: 'Alert'
        enabled: true
      }
      {
        category: 'Recommendation'
        enabled: true
      }
      {
        category: 'Policy'
        enabled: true
      }
      {
        category: 'Autoscale'
        enabled: true
      }
      {
        category: 'ResourceHealth'
        enabled: true
      }
    ]
  }
}
