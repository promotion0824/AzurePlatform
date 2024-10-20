# Editing an Alert

Note: Alerts that are created and edited in the Sandbox folder will not be deployed to dev/prd Grafana. You can use the sandbox folder to experiment with alerts.

At a high level, the process is:

1. Navigate to the [dev alert list](https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com/alerting/list)
1. Find the alert you want to modify
1. Make any needed changes
1. Make note of the guid in the url (for example)

    ```https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com/alerting/c78f0a75-fb7b-4987-b80f-bca6f651096e/edit```
1. Obtain the JSON representation of the alert by navigating to the API and appending the guid (for example)

    ```https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com/api/v1/provisioning/alert-rules/c78f0a75-fb7b-4987-b80f-bca6f651096e```
1. Locate the Alert Rule in the [Azure Platform repo](https://github.com/WillowInc/AzurePlatform/tree/main/Grafana/alerts/AlertRules)
    1. Alerts are in a folder structure based on their folder/ruleGroup in the [Grafana alert list](<https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com/alerting/list>)
1. Using your normal Github branch/PR process, replace the content of the file in github with the json content from the API, then get your PR approved.

Once approved, the Github workflow will deploy back to dev Grafana. If the alert looks correct in dev Grafana, you can contact anyone on the [PlatformEngineering team](https://github.com/orgs/WillowInc/teams/platform-engineering) to approve the release to production
