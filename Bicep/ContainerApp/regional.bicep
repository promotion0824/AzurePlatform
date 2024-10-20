@description('Name of the kusto cluster resource.')
param containerAppEnvironmentName string = 'myContainerAppEnvironmentName'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Certificate')
param certificate string = ''

@description('Certificate secret')
@secure()
param certificatePassword string = ''

@description('Tags for resources.')
param tags object = {}

@description('Name of environment')
param environment string = 'dev'

@description('Custom Domain')
param subDomain_Suffix string = ''

// pipeline-deployer-dev-spn
param pipelineDeloyerDevObjectId string = '561ecd96-300b-4525-94e1-e8b86efb649e'
// pipeline-deployer-prd-spn
param pipelineDeloyerPrdObjectId string = '66758847-bffc-4ded-941f-14c688be7cc0'

var devAppsettings = loadJsonContent('appsettings.json')
var prdAppsettings = loadJsonContent('appsettings.prd.json')

param acrPullRoleDefinitionId string = '7f951dda-4ed3-4680-a7ca-43fe172d538d' // AcrPull

var regionMapping = {
  eastus2: 'eus2'
  eastus: 'eus'
  australiaeast: 'aue'
}

var prdConfig = {
  environmentResourceGroup: 'rg-prd'
  appsettings: prdAppsettings
  deployerObjectId: pipelineDeloyerPrdObjectId
  logAnalytics: 'logs-prd-${regionMapping[location]}'
  applicationInsights: 'ain-prd-${regionMapping[location]}'
  containerRegistry: 'crwilprdshared01'
  sqlServerConnection: 'Server=tcp:sqlwilprdshared01.database.windows.net;Initial Catalog=wsup;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication="Active Directory Managed Identity";User Id="f8626118-53c3-453f-86ce-8c2c45a55d73";'
  sqlServer: 'sqlwilprdshared01.database.windows.net'
  WsupDB: 'wsup'
  wsupApiUrl: 'https://wsupapi-prd.willowinc.com'
  wsupApiScope: 'api://04bea7fc-bee1-401e-ae41-0eebecb750e9/.default'
  authority: 'https://login.microsoftonline.com/d43166d1-c2a1-4f26-a213-f620dba13ab8'
  clientId: '47928fe4-4809-4079-a327-981b0c7f5d6a'
  userAssignedId: 'f8626118-53c3-453f-86ce-8c2c45a55d73'
  keyVaultName: 'kvwilprdglobal01'
}

var devConfig = {
  environmentResourceGroup: 'rg-dev'
  appsettings: devAppsettings
  deployerObjectId: pipelineDeloyerDevObjectId
  logAnalytics: 'logs-dev-${regionMapping[location]}'
  applicationInsights: 'ain-dev-${regionMapping[location]}'
  containerRegistry: 'crwildevshared01'
  sqlServerConnection: 'Server=tcp:sqlwildevshared01.database.windows.net;Initial Catalog=wsup;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication="Active Directory Managed Identity";User Id="d442fa1a-e212-4b60-a90a-28b57b5017a6";'
  sqlServer: 'sqlwilprdshared01.database.windows.net'
  WsupDB: 'wsup'
  wsupApiUrl: 'https://wsupapi-dev.willowinc.com'
  wsupApiScope: 'api://04bea7fc-bee1-401e-ae41-0eebecb750e9/.default'
  authority: 'https://login.microsoftonline.com/d43166d1-c2a1-4f26-a213-f620dba13ab8'
  clientId: '47928fe4-4809-4079-a327-981b0c7f5d6a'
  userAssignedId: 'd442fa1a-e212-4b60-a90a-28b57b5017a6'
  keyVaultName: 'kvwildevglobal01'
}

var config = environment == 'dev' ? devConfig : prdConfig

resource applicationInsights 'Microsoft.Insights/components@2020-02-02-preview' existing = {
  name: config.applicationInsights
}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: config.logAnalytics
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppEnvironmentName
  tags: tags
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: workspace.properties.customerId
        sharedKey: workspace.listKeys().primarySharedKey
      }
    }
    zoneRedundant: false
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: config.containerRegistry
  scope: resourceGroup(config.environmentResourceGroup)
}

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${containerAppEnvironment.name}-${uniqueString(subscription().subscriptionId, containerAppEnvironment.name)}-id'
  location: location
}

module registryRoleAssignment 'container-registry-role-assignment.bicep' = {
  name: 'container-registry-role-assignment'
  scope: resourceGroup(config.environmentResourceGroup)
  params: {
    roleId: acrPullRoleDefinitionId
    principalId: identity.properties.principalId
    registryName: containerRegistry.name
  }
}

resource managedEnvironmentCertificate 'Microsoft.App/managedEnvironments/certificates@2024-03-01' = {
  parent: containerAppEnvironment
  name: 'cf-certificate'
  location: location
  tags: tags
  properties: {
    password: certificatePassword
    value: certificate
  }
}

// Todo: Add deployment script to assign user role
// resource deploymentScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
//   name: 'inlinePowershell'
//   location: location
//   kind: 'AzurePowerShell'
//   properties: {
//     azPowerShellVersion: '11.4'
//     arguments: '-appUser ${identity.id} -sqlServer ${config.sqlServer} -sqlDatabase ${config.WsupDB}'
//     scriptContent: loadTextContent('assign-user-role.ps1')
//     cleanupPreference: 'OnExpiration'
//     retentionInterval: 'PT1H'
//   }
// }

resource wsupContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'wsup'
  tags: tags
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    environmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        external: true
        customDomains: [
          {
            name: 'wsup.willowinc.com'
            bindingType: 'SniEnabled'
            certificateId: managedEnvironmentCertificate.id
          }, {
            name: 'wsup${subDomain_Suffix}.willowinc.com'
            bindingType: 'SniEnabled'
            certificateId: managedEnvironmentCertificate.id
          }
        ]
        targetPort: 5000
        exposedPort: 0
        transport: 'Auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: identity.id
        }
      ]
      secrets: [
        {
          name: 'application-insight-connection-string'
          value: applicationInsights.properties.ConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: config.appsettings.Containers.Wsup.Name
          image: '${containerRegistry.properties.loginServer}/${config.appsettings.Containers.Wsup.Image}'
          env: [
            {
              name: 'ApplicationInsights__ConnectionString'
              secretRef: 'application-insight-connection-string'
            }
            {
              name: 'WsupApiUrl'
              value: config.wsupApiUrl
            }
            {
              name: 'WsupApiScope'
              value: config.wsupApiScope
            }
            {
              name: 'Authority'
              value: config.authority
            }
            {
              name: 'ClientId'
              value: config.clientId
            }
            {
              name: 'UserAssignedId'
              value: config.userAssignedId
            }
            {
              name: 'Azure__KeyVault__KeyVaultName'
              value: config.keyVaultName
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: config.appsettings.AspNet.EnvironmentName
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                port: 5000
                path: '/livez'
              }
              initialDelaySeconds: 7
              periodSeconds: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                port: 5000
                path: '/readyz'
              }
              initialDelaySeconds: 7
              periodSeconds: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource wsupApiContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'wsupapi'
  tags: tags
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    environmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        external: true
        customDomains: [
          {
            name: 'wsupapi.willowinc.com'
            bindingType: 'SniEnabled'
            certificateId: managedEnvironmentCertificate.id
          }, {
            name: 'wsupapi${subDomain_Suffix}.willowinc.com'
            bindingType: 'SniEnabled'
            certificateId: managedEnvironmentCertificate.id
          }
        ]
        targetPort: 5001
        exposedPort: 0
        transport: 'Auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: identity.id
        }
      ]
      secrets: [
        {
          name: 'application-insight-connection-string'
          value: applicationInsights.properties.ConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: config.appsettings.Containers.Wsupapi.Name
          image: '${containerRegistry.properties.loginServer}/${config.appsettings.Containers.Wsupapi.Image}'
          env: [
            {
              name: 'ApplicationInsights__ConnectionString'
              secretRef: 'application-insight-connection-string'
            }
            {
              name: 'DBServerConnectionString'
              value: config.sqlServerConnection
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: config.appsettings.AspNet.EnvironmentName
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                port: 5001
                path: '/livez'
              }
              initialDelaySeconds: 7
              periodSeconds: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                port: 5001
                path: '/readyz'
              }
              initialDelaySeconds: 7
              periodSeconds: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}
