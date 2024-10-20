
// Created as a module to allow deployment as part of a managment group initiative creation

param policyDefinitionId string
param assignmentName string
param displayName string
param identityResourceId string

param location string = 'australiaeast'

@allowed([
  'Default'
  'DoNotEnforce'
])
param enforcementMode string

// this should eventuall support assigning to multiple scopes
targetScope = 'subscription'

resource policyAssignment 'Microsoft.Authorization/policyAssignments@2024-04-01' = {
  name: assignmentName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityResourceId}': {}
    }
  }
  properties: {
    displayName: displayName
    enforcementMode: enforcementMode
    policyDefinitionId: policyDefinitionId
  }
}
