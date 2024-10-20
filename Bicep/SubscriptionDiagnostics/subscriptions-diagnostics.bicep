targetScope = 'managementGroup'
param workspaceId string = '/subscriptions/178b67d7-b6fd-46db-b4a3-b57f8a6b045f/resourceGroups/nonprod-platformshared/providers/Microsoft.OperationalInsights/workspaces/nonprodplatformshared-aue-log'

@description('Subscriptions to assign the export to')
param subscriptionAssignments array

@description('Unused in template but needed to use the same parameters file as other deployments')
#disable-next-line no-unused-params
param targetMG string = 'none'


module assignmentModuleRules './modules/subscriptionActivityDiagnostics.bicep' = [for subscriptionAssignment in subscriptionAssignments: {
  name: 'Platform-ActivityExport-${subscriptionAssignment.name}'
  scope: subscription(subscriptionAssignment.id)
  params: {
    subscriptionName: subscriptionAssignment.name
    workspaceId: workspaceId
  }
}]
