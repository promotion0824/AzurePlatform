// pipeline-deployer-dev-spn
param pipelineDeloyerObjectId string = 'bd2c9873-feb2-4948-88c6-257544aecb64'

// azdo-global-spn
param azdoGlobalId string = 'f9624c1d-234a-4527-8fd6-6ca04a8dfa6e'

// Engineers_Willow
param engineersPrincipalId string = 'c791ea50-fa7e-4842-b285-ad230e1dea9c'

@description('Tags for resources.')
param tags object = {}

// Keyvault params
param vaultName string
param vaultAccessPolicies array

// SharedKeyvault params
param sharedVaultName string

// Azure container registry params
param containerRegistryName string

// Grafana params
param grafanaName string
param grafanaZoneRedundancy string

// Zendesk connector params
param zendeskFunctionName string
param zendeskFunctionEnvironment string
param zendeskFunctionAppInsights string
param AppInsightsResourceGroupName string

// Storage account params
param storageAccountName string

// Set the location for all resources to be the same as the resource group they are deployed to
param location string = resourceGroup().location

// Zone redundancy is not supported in all regions. Prd deployment must override the default
param grafanaLocation string = location

@description('The name of the SQL logical server.')
param serverName string = uniqueString('sql', resourceGroup().id)

@description('The name of the WSUP SQL Database.')
param wsupSqlDBName string = 'WSUP'

@description('The AAD administrator group for the SQL server.')
param sqlServerAadAdminGroupName string

@description('The AAD administrator object for the SQL server.')
param sqlServerAadAdminGroupObjectId string

module zendeskFunction './modules/ZendeskFunction.bicep'={
  name: 'zendeskFunctionDeploy'
  params: {
    location: location
    tags: tags
    zendeskFunctionName: zendeskFunctionName
    zendeskFunctionEnvironment: zendeskFunctionEnvironment
    storageAccountName: storageAccountName
    KeyvaultName: vaultName
    AppInsightsName: zendeskFunctionAppInsights
    AppInsightsResourceGroupName: AppInsightsResourceGroupName
  }
}

resource azureManagedGrafana 'Microsoft.Dashboard/grafana@2023-09-01' = {
  name: grafanaName
  sku: {
    name: 'Standard'
  }
  location: grafanaLocation
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    zoneRedundancy: grafanaZoneRedundancy
    publicNetworkAccess: 'Enabled'
    autoGeneratedDomainNameLabelScope: 'TenantReuse'
    apiKey: 'Enabled'
    deterministicOutboundIP: 'Disabled'
    grafanaIntegrations: {
      azureMonitorWorkspaceIntegrations: []
    }
  }
}

// Grant Grafana Admin role to the Azure Managed Grafana Managed Identity
var grafanaAdmin = '22926164-76b3-42b3-bc55-97df8dab3e41'
resource grafanaAdminRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(azdoGlobalId, grafanaAdmin, azureManagedGrafana.id)
  scope: azureManagedGrafana
  properties: {
    principalId: azdoGlobalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', grafanaAdmin)
  }
}

// Standard storage account
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

// Configure blob services
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

// Grant contributor access to the pipeline deployed identity
var storageContributorContributor = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
resource azDoStorageAccountRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(pipelineDeloyerObjectId, storageContributorContributor, storageAccountName_default.id)
  scope: storageAccountName_default
  properties: {
    principalId: pipelineDeloyerObjectId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageContributorContributor)
  }
}

// Create the pulumi blob container
resource storageAccountName_default_pulumi 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: storageAccountName_default
  name: 'pulumi'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
}

// Grant Engineers_Willow reader access to the storage account
var storageReaader = '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
resource pulumiEngineerReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(engineersPrincipalId, storageReaader, storageAccountName_default.id)
  scope: storageAccountName_default_pulumi
  properties: {
    principalId: engineersPrincipalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageReaader)
  }
}

// Create the Shared key vault
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

// Give the Azdo_global_spn Admin access to the key Shared vault
param keyVaultAdministratorRoleDefinitionId string = '00482a5a-887f-4fb3-b363-3b7fe8e74483'
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vaultName_resource.id, keyVaultAdministratorRoleDefinitionId, azdoGlobalId)
  scope: vaultName_resource
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      keyVaultAdministratorRoleDefinitionId
    )
    principalId: azdoGlobalId
    principalType: 'ServicePrincipal'
  }
}

// Give the Azdo_Global_spn contributor access on the storage account
param storageAccountContributorRoleDefinitionId string = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Account Contributor
resource storageAccountRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountName_resource.id, storageAccountContributorRoleDefinitionId, azdoGlobalId)
  scope: storageAccountName_resource
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageAccountContributorRoleDefinitionId
    )
    principalId: azdoGlobalId
    principalType: 'ServicePrincipal'
  }
}

// Assign the Azdo_Global_spn in a keyvault policy to access all secrets
resource azdoSecretAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: vaultName_resource
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: azdoGlobalId
        permissions: {
          secrets: [
            'all'
          ]
        }
        tenantId: 'd43166d1-c2a1-4f26-a213-f620dba13ab8'
      }
    ]
  }
}

// Create the Global key vault
resource sharedVaultName_resource 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: sharedVaultName
  tags: tags
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: 'd43166d1-c2a1-4f26-a213-f620dba13ab8'
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: true
    enablePurgeProtection: true
    provisioningState: 'Succeeded'
  }
}

// Create the Pulumi key in the Shared keyvault
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

resource teamsConnection 'Microsoft.Web/connections@2016-06-01' = if (tags.environment == 'dev') {
  name: 'teams'
  location: 'eastus'
  properties: {
    displayName: 'iotservicebot@willowinc.com'
    api: {
      name: 'teams'
      displayName: 'Microsoft Teams'
      description: 'Microsoft Teams enables you to get all your content, tools and conversations in the Team workspace with Microsoft 365.'
      iconUri: 'https://connectoricons-prod.azureedge.net/releases/v1.0.1690/1.0.1690.3719/teams/icon.png'
      id: 'subscriptions/${subscription().subscriptionId}/providers/Microsoft.Web/locations/eastus/managedApis/teams'
      brandColor: '#4B53BC'
      type: 'Microsoft.Web/locations/managedApis'
    }
  }
}

resource iotServicesAlertWorkflow 'Microsoft.Logic/workflows@2019-05-01' = if (tags.environment == 'dev') {
  name: 'iot-service-alert-workflow'
  location: 'eastus'
  tags: {
    app: 'shared'
  }
  properties: {
    state: 'Enabled'
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      staticResults: {
        Post_message_in_a_chat_or_channel0: {
          status: 'Succeeded'
          outputs: {
            statusCode: 'OK'
          }
        }
      }
      triggers: {
        AlertRequest: {
          type: 'Request'
          kind: 'Http'
          inputs: {
            schema: {
              type: 'object'
              properties: {
                schemaId: {
                  type: 'string'
                }
                data: {
                  type: 'object'
                  properties: {
                    essentials: {
                      type: 'object'
                      properties: {
                        alertId: {
                          type: 'string'
                        }
                        alertRule: {
                          type: 'string'
                        }
                        severity: {
                          type: 'string'
                        }
                        signalType: {
                          type: 'string'
                        }
                        monitorCondition: {
                          type: 'string'
                        }
                        monitoringService: {
                          type: 'string'
                        }
                        alertTargetIDs: {
                          type: 'array'
                          items: {
                            type: 'string'
                          }
                        }
                        configurationItems: {
                          type: 'array'
                          items: {
                            type: 'string'
                          }
                        }
                        originAlertId: {
                          type: 'string'
                        }
                        firedDateTime: {
                          type: 'string'
                        }
                        resolvedDateTime: {
                          type: 'string'
                        }
                        description: {
                          type: 'string'
                        }
                        essentialsVersion: {
                          type: 'string'
                        }
                        alertContextVersion: {
                          type: 'string'
                        }
                      }
                    }
                    alertContext: {
                      type: 'object'
                      properties: {
                        properties: {}
                        conditionType: {
                          type: 'string'
                        }
                        condition: {
                          type: 'object'
                          properties: {
                            windowSize: {
                              type: 'string'
                            }
                            allOf: {
                              type: 'array'
                              items: {
                                type: 'object'
                                properties: {
                                  metricName: {
                                    type: 'string'
                                  }
                                  metricNamespace: {
                                    type: 'string'
                                  }
                                  operator: {
                                    type: 'string'
                                  }
                                  threshold: {
                                    type: 'string'
                                  }
                                  timeAggregation: {
                                    type: 'string'
                                  }
                                  dimensions: {
                                    type: 'array'
                                    items: {
                                      type: 'object'
                                      properties: {
                                        name: {
                                          type: 'string'
                                        }
                                        value: {
                                          type: 'string'
                                        }
                                      }
                                      required: [
                                        'name'
                                        'value'
                                      ]
                                    }
                                  }
                                  metricValue: {
                                    type: 'number'
                                  }
                                }
                                required: [
                                  'metricName'
                                  'metricNamespace'
                                  'operator'
                                  'threshold'
                                  'timeAggregation'
                                  'dimensions'
                                  'metricValue'
                                ]
                              }
                            }
                          }
                        }
                      }
                    }
                    customProperties: {
                      type: 'object'
                      properties: {
                        containerName: {
                          type: 'string'
                        }
                        app: {
                          type: 'string'
                        }
                        resourceGroup: {
                          type: 'string'
                        }
                        resourceId: {
                          type: 'string'
                        }
                        'customer-code': {
                          type: 'string'
                        }
                        environment: {
                          type: 'string'
                        }
                        stamp: {
                          type: 'string'
                        }
                        owner: {
                          type: 'string'
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      actions: {
        For_each: {
          foreach: '@triggerOutputs()?[\'body\']?[\'data\']?[\'alertContext\']?[\'condition\']?[\'allOf\']'
          actions: {
            Check_metric_value: {
              actions: {
                Post_message_in_a_chat_or_channel: {
                  type: 'ApiConnection'
                  inputs: {
                    host: {
                      connection: {
                        name: '@parameters(\'$connections\')[\'teams\'][\'connectionId\']'
                      }
                    }
                    method: 'post'
                    body: {
                      recipient: {
                        groupId: '94e54565-09ff-4786-8c26-68d4902e9d13'
                        channelId: '19:becfe255c5d44342949bfb2e9f56b14d@thread.skype'
                      }
                      messageBody: '<h1><strong>@{triggerBody()?[\'data\']?[\'essentials\']?[\'alertRule\']}</strong></h1>\n<br>\n\n<p>@{triggerBody()?[\'data\']?[\'essentials\']?[\'description\']}</p>\n<a href="https://portal.azure.com/#@willowinc.com/resource@{triggerBody()?[\'data\']?[\'customProperties\']?[\'resourceId\']}">Link to resource</a>\n\nResource Group name @{triggerBody()?[\'data\']?[\'customProperties\']?[\'resourceGroup\']}\n<br>\n<table>\n<tbody>\n<tr>\n<td>&nbsp;Metric Name</td>\n<td>&nbsp;@{item()?[\'metricName\']}</td>\n</tr>\n<tr>\n<td>&nbsp;Value</td>\n<td>&nbsp;@{item()?[\'metricValue\']}</td>\n</tr>\n<tr>\n<td>&nbsp;Condition</td>\n<td>&nbsp;@{item()?[\'operator\']} @{item()?[\'threshold\']}</td>\n</tr>\n<tr>\n<td>&nbsp;Severity</td>\n<td>&nbsp;@{triggerBody()?[\'data\']?[\'essentials\']?[\'severity\']}</td>\n</tr>\n<tr>\n<td>&nbsp;Occurred On</td>\n<td>&nbsp;@{triggerBody()?[\'data\']?[\'essentials\']?[\'firedDateTime\']}</td>\n</tr>\n</tbody>\n</table>\n<br>\n<table>\n<tbody>\n<tr>\n<td>Container Name</td>\n<td>@{triggerBody()?[\'data\']?[\'customProperties\']?[\'containerName\']}</td>\n</tr>\n<tr>\n<td>App Name</td>\n<td>@{triggerBody()?[\'data\']?[\'customProperties\']?[\'app\']}</td>\n</tr>\n<tr>\n<td>Environment</td>\n<td>@{triggerBody()?[\'data\']?[\'customProperties\']?[\'environment\']}</td>\n</tr>\n<tr>\n<td>Stamp</td>\n<td>@{triggerBody()?[\'data\']?[\'customProperties\']?[\'stamp\']}</td>\n</tr>\n<tr>\n<td>Customer Code</td>\n<td>@{triggerBody()?[\'data\']?[\'customProperties\']?[\'customer-code\']}</td>\n</tr>\n<tr>\n<td>Owner</td>\n<td>@{triggerBody()?[\'data\']?[\'customProperties\']?[\'owner\']}</td>\n</tr>\n</tbody>\n</table>'
                    }
                    path: '/beta/teams/conversation/message/poster/Flow bot/location/@{encodeURIComponent(\'Channel\')}'
                  }
                  runtimeConfiguration: {
                    staticResult: {
                      staticResultOptions: 'Disabled'
                      name: 'Post_message_in_a_chat_or_channel0'
                    }
                  }
                }
              }
              else: {
                actions: {}
              }
              expression: {
                or: [
                  {
                    and: [
                      {
                        equals: [
                          '@item()?[\'operator\']'
                          'GreaterThan'
                        ]
                      }
                      {
                        greater: [
                          '@item()?[\'metricValue\']'
                          '@int(item()?[\'threshold\'])'
                        ]
                      }
                    ]
                  }
                  {
                    and: [
                      {
                        equals: [
                          '@item()?[\'operator\']'
                          'LessThan'
                        ]
                      }
                      {
                        less: [
                          '@item()?[\'metricValue\']'
                          '@int(item()?[\'threshold\'])'
                        ]
                      }
                    ]
                  }
                  {
                    and: [
                      {
                        equals: [
                          '@item()?[\'operator\']'
                          'Equals'
                        ]
                      }
                      {
                        equals: [
                          '@item()?[\'metricValue\']'
                          '@int(item()?[\'threshold\'])'
                        ]
                      }
                    ]
                  }
                ]
              }
              type: 'If'
            }
          }
          runAfter: {}
          type: 'Foreach'
        }
      }
      outputs: {}
    }
    parameters: {
      '$connections': {
        value: {
          teams: {
            id: 'subscriptions/${subscription().subscriptionId}/providers/Microsoft.Web/locations/eastus/managedApis/teams'
            connectionId: teamsConnection.id
            connectionName: teamsConnection.name
          }
        }
      }
    }
  }
}

resource actionGroupIotServiceAlert 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'ag-iot-services-alert'
  location: 'Global'
  properties: {
    enabled: true
    groupShortName: 'iotservices'
    logicAppReceivers: [
      {
        name: 'IoTServiceTeamWebhook'
        resourceId: iotServicesAlertWorkflow.id
        callbackUrl: listCallbackURL('${iotServicesAlertWorkflow.id}/triggers/AlertRequest', '2017-07-01').value
        useCommonAlertSchema: true
      }
    ]
  }
}
var logicAppContributor = '87a39d53-fc1b-424a-814c-f7e04687dc9e'
resource engineerApiConnection 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(engineersPrincipalId, logicAppContributor, teamsConnection.id)
  scope: teamsConnection
  properties: {
    principalId: engineersPrincipalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', logicAppContributor)
  }
}

@description('Generated from /subscriptions/178b67d7-b6fd-46db-b4a3-b57f8a6b045f/resourceGroups/nonprod-platformshared/providers/Microsoft.ContainerRegistry/registries/nonprodplatformsharedcr')
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  sku: {
    name: 'Premium'
  }
  name: containerRegistryName
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

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    version: '12.0'
    minimalTlsVersion: 'None'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: sqlServerAadAdminGroupName
      sid: sqlServerAadAdminGroupObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource sqlDB 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: wsupSqlDBName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

// Allow Azure Service and Resources to access this server
resource allowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  name: 'AllowAllWindowsAzureIps' // don't change the name
  parent: sqlServer
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

output key object = vaultName_pulumi
output grafana object = azureManagedGrafana
