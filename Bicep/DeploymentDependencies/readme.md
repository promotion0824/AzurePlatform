# Deploy Single Tenant Management subscription shared resources to dev and prd subscriptions

See this
[RFC](https://willow.atlassian.net/wiki/spaces/RFC/pages/2405007382/New+Subscription+Resource+Group+Role+Assignments+Policies#Infrastructure-%26-Application-Deployer-(Pulumi)-Requirements)
for details

## Non-Regional Resources

The following resources are deployed once to the management subscription

### Keyvault

Pulumi encryption key and secrets storage

### Storage

Pulumi state files

### Container Registry

Azure Container Registry for the environment

## Regional Resources

The following resources are deployed regionally and are meant to be shared by all stamps with resources in the region and environment

### Application Insights

### Log Analytics Workspace

### Kusto Cluster

### Databricks Workspace
