# Grafana

Grafana has been upgraded to Azure Managed Grafana.

## Links

[Prod instance](https://grafana-prd-eus-c4chfqh8ewb9ezab.eus.grafana.azure.com/?orgId=1)

[Dev Instance](https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com/?orgId=1)

## Access

The AAD group AzureManagedGrafanaUsers has been given Editor access to the dev instance and Viewer access to the Prod instance.

## Custom Dashboards

Dashboards and alert configuration is persistent in Azure Managed Grafana

To create a dashboard, create or import a dashboard within the Sandbox folder in the dev instance.

To have your dashboard deployed to dev/prod Grafana, please add it to the [Dashboards folder](https://github.com/WillowInc/AzurePlatform/tree/main/Grafana) and update the [AzureManagedGrafanaFolders.yaml](../AzureManagedGrafanaFolders.yaml) file to specify the name of the file and the name of the dashboards folder in Grafana. Please do not use 'Sandbox' for deploying.

```yaml
az-adx-clusters.json = Azure ADX
az-adx-resources.json = Azure ADX
az-adx-usage.json = Azure ADX
delivery-adt-adx.json = Delivery
iot-monitoring-building-axa.json = IoT Monitoring AXA
iot-monitoring-portfolio-axa.json = IoT Monitoring AXA
```
The json files may be arranged into folders within the github repo for better organization.
