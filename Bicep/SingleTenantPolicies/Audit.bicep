targetScope = 'managementGroup'

param policyAssignmentName string = 'Enable Audit'
param policyDefinitionID string = '/providers/Microsoft.Authorization/policyDefinitions/187242f4-89c6-4c43-9a4e-188c0efacc5f'

resource assignment 'Microsoft.Authorization/policyAssignments@2024-04-01' = {
    name: policyAssignmentName
    scope: managementGroup()
    properties: {
        policyDefinitionId: policyDefinitionID
    }
}

output assignmentId string = assignment.id
