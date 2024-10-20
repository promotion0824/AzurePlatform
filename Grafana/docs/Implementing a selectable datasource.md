# Implementing A Selectable DataSource

## Purpose

Occasionally it is nice to have a single dashboard configured to look at multiple instances of an application (UAT/PRD for instance).
This is possible in Grafana by using a drop-down list with manually populated variable options.

## Adding the Variables

1. Click the 'Settings' icon/Dashboard Settings
2. Click 'Variables' (left panel)
3. Click 'Add Variable'

### Add Subscription Variable

1. Set the variable type to 'Custom'
2. Name the variable 'dataSource'
3. In 'Custom Options', the values need to be a comma separated list in the format:

    label : value, label : value

    for example to add ain-prd-eus2 and ain-prd-eus:

    ain-prd-eus2 : /subscriptions/fd259995-1de7-4ae8-8431-0d150dcca6f4/resourceGroups/rg-prd-eus2/providers/Microsoft.Insights/components/ain-prd-eus2, ain-dev-eus : /subscriptions/48a16780-c719-4528-a0f2-4e7640a9c850/resourceGroups/rg-dev-eus/providers/Microsoft.Insights/components/ain-dev-eus

    In this case it is "label : azure resource identifier"
4. Click 'Apply'
5. Click 'Save dashboard'

### Add the dataSource variable to visualizations

1. Edit the panel
2. Select 'Azure Monitor' for the Data source
3. Select 'Logs' for the Service
4. Select a resource
5. In the resource selector, select 'Advanced'
6. Set the Resource URI to '${dataSource} (same value as the name of your variable above)

The above may also work for Metrics rather than Logs, but I have not tested it.

Note: KQL queries may need to be modified to work with different data sources, so make sure to modify any queries as needed, for example, cloud_RoleName might be different so you will need to filter using startswith instead of equals.
