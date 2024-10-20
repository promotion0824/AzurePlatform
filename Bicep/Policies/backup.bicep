targetScope = 'managementGroup'

@description('Target Management Group')
param targetMG string

@description('Subscriptions to assign the policy to')
param subscriptionAssignments array
var mgScope = tenantResourceId('Microsoft.Management/managementGroups', targetMG)

var initiativeDisplayName = 'Resources Should Have Backups Enabled'
// Currently no inbuilt backup policy for azure sql servers for short term backups
resource shortTermSqlServerBackups 'Microsoft.Authorization/policyDefinitions@2023-04-01' = {
  name: 'SqlServerShortTermGeoBackups'
  properties: {
    description: 'Sql servers should have geo redundant backup enabled on them'
    displayName: 'Short-term geo-redundant backup should be enabled for Azure SQL Databases'
    policyType: 'Custom'
    mode: 'All'
    metadata: {
      version: '1.0.0'
      category: 'SQL'
    }
    parameters: {
      effect: {
        allowedValues: [
          'AuditIfNotExists'
          'Disabled'
        ]
        defaultValue: 'AuditIfNotExists'
        metadata: {
          additionalProperties: null
          assignPermissions: null
          description: 'Enable or disable the execution of the policy'
          displayName: 'Effect'
          strongType: null
        }
        type: 'String'
      }
    }
    policyRule: {
      if: {
        allOf: [
          {
            equals: 'Microsoft.Sql/servers/databases'
            field: 'type'
          }
          {
            field: 'name'
            notEquals: 'master'
          }
        ]
      }
      then: {
        details: {
          existenceCondition: {
            anyOf: [
              {
                field: 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies/retentionDays'
                notEquals: 'PT0S'
              }
            ]
          }
          name: 'default'
          type: 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies'
        }
        effect: '[parameters(\'effect\')]'
      }
    }
  }
}


resource localPostgresBackups 'Microsoft.Authorization/policyDefinitions@2023-04-01' = {
  name: 'PostgresServerBackups'
  properties: {
    description: 'Postgres servers should have geo redundant backup enabled on them'
    displayName: 'Backup should be enabled for Postgres servers'
    policyType: 'Custom'
    mode: 'Indexed'
    metadata: {
      version: '1.0.0'
      category: 'SQL'
    }
    parameters: {
      effect: {
        allowedValues: [
          'Audit'
          'Disabled'
        ]
        defaultValue: 'Audit'
        metadata: {
          description: 'Enable or disable the execution of the policy'
          displayName: 'Effect'
        }
        type: 'String'
      }
    }
    policyRule: {
      if: {
        allOf: [
          {
            equals: 'Microsoft.DBforPostgreSQL/servers'
            field: 'type'
          }
          {
            greater: 0
            field: 'Microsoft.DBforPostgreSQL/servers/storageProfile.backupRetentionDays'
          }
        ]
      }
      then: {
        effect: '[parameters(\'effect\')]'
      }
    }
  }
}


resource policySetName 'Microsoft.Authorization/policySetDefinitions@2023-04-01' = {
  name: 'WillowBackupInitiative'
  properties: {
    displayName: initiativeDisplayName
    description: initiativeDisplayName
    policyType: 'Custom'
    metadata: {
      category: 'Backup'
    }
    parameters: {
    }
    policyDefinitions: [
      {
        policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/0ec47710-77ff-4a3d-9181-6aa50af424d0'
        policyDefinitionReferenceId: 'Geo-redundant backup should be enabled for Azure Database for MariaDB_1'
      }
      {
        policyDefinitionId: extensionResourceId(mgScope, 'Microsoft.Authorization/policyDefinitions', localPostgresBackups.name)
        policyDefinitionReferenceId: localPostgresBackups.properties.displayName
      }
      {
        policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/82339799-d096-41ae-8538-b108becf0970'
        policyDefinitionReferenceId: 'Geo-redundant backup should be enabled for Azure Database for MySQL_1'
      }
      {
        policyDefinitionId: extensionResourceId(mgScope, 'Microsoft.Authorization/policyDefinitions', shortTermSqlServerBackups.name)
        policyDefinitionReferenceId: shortTermSqlServerBackups.properties.displayName
      }
    ]
  }
}


module assignmentModule './modules/policyAssignment.bicep' = [for subscriptionAssignment in subscriptionAssignments: {
  name: 'Backup${subscriptionAssignment.name}'
  scope: subscription(subscriptionAssignment.id)
  params: {
    assignmentName: 'Backup${subscriptionAssignment.name}'
    displayName: '${subscriptionAssignment.name} ${initiativeDisplayName}'
    policyDefinitionId: extensionResourceId(mgScope, 'Microsoft.Authorization/policySetDefinitions', policySetName.name)
    enforcementMode: 'Default'
  }
}]
