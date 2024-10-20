targetScope = 'managementGroup'

param tagName string

// Similar implementation to /providers/Microsoft.Authorization/policyDefinitions/871b6d14-10aa-478d-b590-94f262ecfa99 but with a customisable name and applied to both rg and resources
resource tagOnResourcePolicy 'Microsoft.Authorization/policyDefinitions@2023-04-01' = {
  name: 'RequireTagOnResources${tagName}'
  properties: {
    description: 'Enforces existence of a tag ${tagName} on resources'
    displayName: 'Require a tag on resources: ${tagName}'
    policyType: 'Custom'
    mode: 'All'
    policyRule: {
      if:         {
          field: 'tags[${tagName}]'
          exists: 'false'
        }

      then: {
        effect: 'deny'
      }
    }
  }
}

output policyId string = tagOnResourcePolicy.id
output policyName string = tagOnResourcePolicy.name
