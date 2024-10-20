# Zendesk Integration Function App

This folder contains an Azure Function App used to integrate our customer support ticketing system Zendesk with Willow applications.

The Function App has 2 endpoints:

1) A HTTP POST endpoint which is used as a [Webhook Contact Point](https://grafana.com/docs/grafana/latest/alerting/fundamentals/contact-points/) by the [Grafana Alert Manager](https://grafana.com/docs/grafana/latest/alerting/fundamentals/alertmanager/).  
2) A HTTP POST endpoint which is used to receive information about new or updated Customers & Sites


## Grafana Contact Point Webhook

| Key | Value  | 
|-------|-----|
| URL | {base-url}/api/grafana/alerts |
| Authentication | shared key in x-functions-key header | 

### Description

This endpoint performs the following functions:
* Converts Grafana formatted Alert payload into the format required to create a ticket in Zendesk.
* Infers Customer Name, Issue Severity, Environment and Zendesk Action Group (responders) from the details in the Grafana Alert
* De-duplicates multiple Alert messages from Grafana in a 12 hour time period using the 'commonLabels' in the Grafana Alert Message 
* Provides the option to specify which Environments to create tickets for (DEV, UAT, PRD).   

### Grafana to Zendesk Field Mappings
The following table summarises how [fields in the Alert message from Grafana](https://grafana.com/docs/grafana-cloud/alerting-and-irm/alerting/alerting-rules/manage-contact-points/integrations/webhook-notifier/) are converted to [fields in ticket message sent to Zendesk](https://developer.zendesk.com/api-reference/ticketing/tickets/tickets/).

| Implimented | Description | Grafana Alert message field | Zendesk ticket field Id | Example Zendesk ticket field value(s) | Zendesk documentation |
|-------|------|------|------|------|------|
| X | Environment | commonLabels.Environment OR commonLabels.environmentshortname | 8001260747407 | dev, uat, prd, ppe | https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/8001260747407 |
| | Group | commonLabels.Container | group_id |  | https://willowinc.zendesk.com/admin/people/team/groups/ |
| | Priority |  |  | | https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360017763331 |
| | Severity | commonLabels.Severity | 7231972707215 | | https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/7231972707215 |
| X | Customer Name | commonLabels.Instance | 360029547271 | |https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360029547271 |
| | Customer Site | | | | |
| | Product Area | | | | https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/360029506771 |
| | Corrective Action | | | | |
| X | Escallation | | 900007747226 | no | https://willowinc.zendesk.com/admin/objects-rules/tickets/ticket-fields/900007747226 |
| | | | | | |

## Customer & Site Details Webhook
| Key | Value  | 
|-------|-----|
| URL | {base-url}/api/zendesk/customer-site-details |
| Authentication | shared key in x-functions-key header | 

This endpoint is yet to be specified.  See Azure Dev Ops [User Story 115147](https://dev.azure.com/willowdev/Unified/_workitems/edit/115147)


# Who do I contact for help?
You can contact the author `mjanos@willowinc.com` for help.

# Function App Settings

The Function App requires the following Settings.

| Key | Description  | Example Value(s)  |
|-------|-----|-----|
| StorageClientOptions:SaveGrafanaAlertsMessages | Boolean indicating whethert to save Alert Messages from Grafana to Blob storage (for testing) | true, false  |
| StorageClientOptions:StorageAccountName  | The name of the Storage Account used to save Grafana Alert messages | zendeskintegrationstest |
| StorageClientOptions:GrafanaAlertsStorageContainerName | The name of the Container used to save Grafana Alert messages |                      grafana-alert-messages |
| ZendeskClientOptions:ZendeskTicketsApiUrl | The URL of the Zendesk Tickets API | https://willowinc.zendesk.com/api/v2/tickets |
| ZendeskClientOptions:ZendeskEmailAddress | Email address of the account used to generate the Zendesk API Key | grafana-alerts@willowinc.com |
| ZendeskClientOptions:ZendeskApiToken | Zendesk API Key | aVGb225HfaSNFFcd77GbJa88Gfd5mnsa82x |


# Infastructure Configuration

## Function App Permissions
The Function App requires the System Assigned Managed Identity enabled for integration with Storage Account and KeyVault.  

The required permissions on the storage account are: `Storage Blob Data Contributor` 

The required permissions on the KeyVault are:  `Key Vault Secrets User` 


## Storage Account
A storage account is used to save copies of Grafana Alerts & updated customer information for the purposes of developing and debugging the Integration.


# Zendesk Configuration
Each time a new Customer or Site is configured in Willow Twin it must also be manually configured in Zendesk.  

This manual configuration will eventually be automated by the "Customer & Site Webhook" but for now we reference the [customers.json file](https://github.com/WillowInc/Infrastructure-and-Application-Deployment/blob/main/Configurations/common/customer.json) to get the list of Customers.






