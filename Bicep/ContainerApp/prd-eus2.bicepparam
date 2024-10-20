using 'regional.bicep'

param certificate = readEnvironmentVariable('encodedCert', '')
param containerAppEnvironmentName = 'cae-prd-eus2'
param environment = 'prd'
param certificatePassword = readEnvironmentVariable('certPassword', '')
param subDomain_Suffix = '-prd'
param tags = {
  environment: 'prd'
  'customer-code': 'shared'
  owner: 'cloudops'
  managedby: 'bicep'
  application: 'shared'
  stamp: 'management'
  project: 'https://github.com/WillowInc/Infrastructure-and-Application-Deployment/tree/main/ManagementSubInfrastructure'
}
