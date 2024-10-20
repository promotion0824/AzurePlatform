using 'regional.bicep'

param certificate = readEnvironmentVariable('encodedCert', '')
param containerAppEnvironmentName = 'cae-dev-eus'
param environment = 'dev'
param certificatePassword = readEnvironmentVariable('certPassword', '')
param subDomain_Suffix = '-dev'
param tags = {
  environment: 'dev'
  'customer-code': 'shared'
  owner: 'cloudops'
  managedby: 'bicep'
  application: 'shared'
  stamp: 'management'
  project: 'https://github.com/WillowInc/Infrastructure-and-Application-Deployment/tree/main/ManagementSubInfrastructure'
}
