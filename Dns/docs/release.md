# Release
Changes to DNS get deployed by azure devops pipeline [AzurePlatform (DNS)](https://dev.azure.com/willowdev/AzurePlatform/_build?definitionId=541&_a=summary)

## Release Approval
Releases currently need to be approved by a member of the [Dns Managers](https://dev.azure.com/willowdev/AzurePlatform/_settings/teams?subjectDescriptor=vssgp.Uy0xLTktMTU1MTM3NDI0NS0zMjI3NTczNjA0LTI0ODA5MzAzNzQtMjk5NjM2NjQ4Mi0yNjIyNDk5MzIwLTEtOTkwODgzOTE2LTI5Mzg3MzgyNTItMjE5Mjg0Mjg3Ny05Mzk3MDg5NTc) team in Azure Devops approval times out after 4 hours and the preview stage will need to be re-run.

Approvers should validate that what is being changed in the deployment matches the Pull Request description and no manual changes to DNS are going to be overwritten that shouldn't be.

This can be done by checking the output of the preview stage.

![](Preview.png)

## Locks
Currently the DNS zones in Azure have read-only and delete locks applied to them. They need to be removed and re-added after a release to prevent manual changes to DNS by users with PIM elevations (Locks can only be changed by owners and user access administrators).
