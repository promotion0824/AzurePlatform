targetScope = 'managementGroup'

@description('Target Management Group')
param targetMG string

@description('Subscriptions to assign the policy to')
param subscriptionAssignments array
var mgScope = tenantResourceId('Microsoft.Management/managementGroups', targetMG)

var tagRemediationId = '/subscriptions/178b67d7-b6fd-46db-b4a3-b57f8a6b045f/resourceGroups/policy-resources/providers/Microsoft.ManagedIdentity/userAssignedIdentities/az-tag-policy-remediation-id'

// https://github.com/WillowInc/rfcs/discussions/6
var tags = [
  'environment'
  'customer-code'
  'application'
  'stamp'
  'owner'
]

module tagPolicies './modules/requireTags.bicep' = [for tag in tags: {
  name: 'TagsPolicy_${tag}'
  params: {
    tagName: tag
  }
}]

var initiativeDisplayName = 'Resources Should have Tags set'

resource rulesPolicySetName 'Microsoft.Authorization/policySetDefinitions@2023-04-01' = {
  name: 'WillowTagsInitiative'
  properties: {
    displayName: initiativeDisplayName
    description: initiativeDisplayName
    policyType: 'Custom'
    metadata: {
      category: 'Tags'
    }
    parameters: {
    }
    policyDefinitionGroups: [
    ]
    policyDefinitions: [for (tag, i) in tags: {
      policyDefinitionId: extensionResourceId(mgScope, 'Microsoft.Authorization/policyDefinitions', 'RequireTagOnResources${tag}')
      policyDefinitionReferenceId: 'RequireTag_${tag}'
    }]
  }
}
var remediateDisplayName = 'Inherit tags from resource groups'

resource remediationPolicySetName 'Microsoft.Authorization/policySetDefinitions@2023-04-01' = {
  name: 'WillowTagsRemediation'
  properties: {
    displayName: remediateDisplayName
    description: 'Adds the specified tag with its value from the parent resource group when any resource missing this tag is created or updated. Existing resources can be remediated by triggering a remediation task. If the tag exists with a different value it will not be changed.'
    policyType: 'Custom'
    metadata: {
      category: 'Tags'
    }
    parameters: {
    }
    policyDefinitionGroups: [
    ]
    policyDefinitions: [for (tag, i) in tags: {
      policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/ea3f2387-9b95-492a-a190-fcdc54f7b070'
      policyDefinitionReferenceId: 'InheritTag_${tag}'
      parameters:{
        tagName: {
          value: tag
        }
      }
    }]
  }
}

module assignmentModuleRules './modules/policyAssignment.bicep' = [for subscriptionAssignment in subscriptionAssignments: {
  name: 'Tags${subscriptionAssignment.name}'
  scope: subscription(subscriptionAssignment.id)
  params: {
    assignmentName: 'RequireTags${subscriptionAssignment.name}'
    displayName: '${subscriptionAssignment.name} ${initiativeDisplayName}'
    policyDefinitionId: extensionResourceId(mgScope, 'Microsoft.Authorization/policySetDefinitions', rulesPolicySetName.name)
    enforcementMode: subscriptionAssignment.enforceTags ? 'Default' : 'DoNotEnforce'
  }
}]

module assignmentModuleRemediation './modules/policyAssignmentWithIdentity.bicep' = [for subscriptionAssignment in subscriptionAssignments: if (subscriptionAssignment.remediateTags) {
  name: 'InheritTags${subscriptionAssignment.name}'
  scope: subscription(subscriptionAssignment.id)
  params: {
    identityResourceId: tagRemediationId
    location: 'australiaeast'
    assignmentName: 'InheritTags${subscriptionAssignment.name}'
    displayName: '${subscriptionAssignment.name} ${remediateDisplayName}'
    policyDefinitionId: extensionResourceId(mgScope, 'Microsoft.Authorization/policySetDefinitions', remediationPolicySetName.name)
    enforcementMode: 'Default'
  }
}]
