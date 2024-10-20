# Dashboard Creation walkthrough

## Initial Dashboard Creation

1. Navigate to the dev grafana instance
<https://grafana-dev-eus-cwazajhmdzhrhbdt.eus2.grafana.azure.com/>
2. Click on 'Dashboards' in the top left breadcrumbs
3. Hover over the 'Sandbox' folder and click 'Go to folder'
4. Click the 'New' button to the right of the search bar
5. Select 'New Dashboard'
6. Click 'Add Visualization'
7. Click Save, give the Dashboard a name and select Sandbox for the folder.

Items in Sandbox are ignored by all gitops workflows, so they won't be deployed to production, backed up, deleted, etc.

A default/fake visualization will come up.

To create a timeseries chart with the ability to filter by subscription/resource group/region/resource, you will need to add some variables first.

Note: For this walkthrough we will not enable multi-select on our variables, or the 'A' option as it won't work with the metrics we will chart.

## Adding the Variables

1. Click the 'Settings' icon/Dashboard Settings
2. Click 'Variables' (left panel)
3. Click 'Add Variable'

### Add Subscription Variable

1. Set the variable type to 'Query'
2. Name the variable 'Subscription'
3. Change the Data Source to 'Azure Monitor'
4. Set the query type to 'Subscriptions'
5. Verify that a list of Azure subscriptions appears in the Preview
6. Change the Sort order to Alphabetical (asc)
7. Click 'Apply'
8. Click 'Save dashboard'

### Add the rest of the variables

Repeat these steps for ResourceGroups, but when you select Resource Groups for the 'query type' it will prompt you for a Subscription as well. Scroll to the bottom of the list and 'Subscription' will be available under 'Template Variables' since you already created the variable.

Save the dashboard after adding each variable

Repeat the steps for Region

Repeat the steps for a Resource Name and name your variable 'EventHub'. For query type, select 'Resource Names'. Configure the appropriate template variables for Subscription, Resource Group, and Region. For namespace, select 'microsoft.eventhub/namespace'

Click 'Close' after adding the last variable.

Verify that drop down lists exist for each variable.

Select the following:
Subscription = prd-eus2-05
ResourceGroup = rg=prd=eus2=o5=wmt=in1
Region = East US 2
EventHub = evhns-prd-eus2-o5-wmt-in1-xxxxxxx

## Add a metric to the visualization

1. Click on the panel, and hit 'e' on your keyboard to enter the 'Edit' mode
2. below the chart, change the Data source to Azure Monitor
3. for 'Service' select 'Metrics'
4. Click 'Select a resource'

For metrics that are pushed to AppInsights, you could select the AppInsights resource, but since Event Hub Namespace metrics are not in AppInsights, we will use the variables created earlier to configure our data source.

Set the values as:

Subscription = $Subscription

Namespace = 'Microsoft.eventhub/namespaces'

Region = $Region

Resource group = $ResourceGroup

Resource name = $EventHub

The variables should match the variable names created in the last section

5. Select the 'Metric' for 'Incoming Requests'

Note: if you do not see a list of Metrics in the drop down list, you probably mistyped one of your variables in the last step.

At this point you should see some data in the panel.

To add a separate timeseries for each Event Hub under the selected namespace: Under 'Dimensions'; select 'EntityName', but don't select a value.

Change the Panel Title to 'Incoming Requests'

Click 'Save'

In the breadcrumbs select your dashboard name to get out of Edit mode

## Experiment

1. Click/Ctrl-Click on the values in the legend to select/deselect 1 or more eventhubs
2. In the time span selector (next to settings) change the timespan for the dashboard. Unless otherwise configured, this will change the timespan for all visualizations in the dashboard
3. On the panel, drag a box around some time frame of interest to drill down to just the select time. Also, unless otherwise configured, this will change the timespan for all visualizations in the dashboard
4. With the panel selected type 'p d' to duplicate the visualization. Edit the visualization and change the metric to something else interesting
