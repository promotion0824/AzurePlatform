# Azure Roles

Allows the assignment of roles as well as creation of custom roles

# Custom Roles

If you can't find a suitable role from the existing azure roles https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles or need to assign permissions that would typically need multiple roles a custom role may be the best option.

To create add a new role definition as a Json file into AzureRoles/Definitions/ and register the role in the deployment in AzureRoles/azure-pipelines.yaml

```yaml
parameters:
...
  - name: roles
    type: object
    default:
      - definitionFile: rate-card-role.json
        defaultSubscription: 178b67d7-b6fd-46db-b4a3-b57f8a6b045f # K8S-INTERNAL
```

You can find what resource operations are available from this page https://docs.microsoft.com/en-us/azure/role-based-access-control/resource-provider-operations

```json
{
  "Name": "RateCardBillingRole",
  "roleName": "RateCardBillingRole",
  "IsCustom": true,
  "Description": "Rate Card query role",
  "Actions": [
      "Microsoft.Compute/virtualMachines/vmSizes/read",
      "Microsoft.Resources/subscriptions/locations/read",
      "Microsoft.Resources/providers/read",
      "Microsoft.ContainerService/containerServices/read",
      "Microsoft.Commerce/RateCard/read"
  ],
  "AssignableScopes": [
    "/subscriptions/249312a0-4c83-4d73-b164-18c5e72bf219",
    "/providers/Microsoft.Management/managementGroups/Product"
  ]
}

```


# Assigning Roles

To assign a role update the yaml files in assignments that corresponds to the subscription for the role. If you are onboarding a new subscription run the Deploy script with the WhatIf switch to get a list of existing roles that should be included in the assignment file.

## Finding the PrincipalId to use for assignments
Role assignments needs the PrincipalId (objectid) of an entity as the assignment target. You can find this from [Active directory in the azure portal](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview) by searching by name

![](group.png)
![](user.png)


Or via azure cli

```bash
az ad user list --display-name "Richard" --query "[].[objectId,userPrincipalName,displayName]"
az ad group list --display-name "Platform" --query "[].[objectId,displayName]"
```

## Finding the RoleId
These [docs](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-definitions-list) explain how to find a role id.

Role ids are specific to a subscription but have the same names

```bash
az role definition list --name "SqlFirewallAdmin" --subscription "K8S-Internal"
#     "id": "/subscriptions/249312a0-4c83-4d73-b164-18c5e72bf219/providers/Microsoft.Authorization/roleDefinitions/4c116243-4efd-471e-862c-bf1791010154",


az role definition list --name "SqlFirewallAdmin" --subscription "K8S-Internal"


#     "id": "/subscriptions/178b67d7-b6fd-46db-b4a3-b57f8a6b045f/providers/Microsoft.Authorization/roleDefinitions/4c116243-4efd-471e-862c-bf1791010154",

```


## What to check when reviewing or making an update

Ensure the change to roles matches your intention by running a preview of the assignments with the WhatIf flag set on Deploy.ps1

Include a comment in the yaml describing the role and assignee
