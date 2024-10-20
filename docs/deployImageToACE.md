# Deploying an image to ACE

To deploy an image to ACE through AZ CLI you will need to:

1. Find a suitable image
    1. Useful for networking tools:

        ```bash
        docker pull wbitt/network-multitool
        ```

1. Pull that image to your local docker desktop
1. Tag that image with an appropriate tag to be able to push it to an ACR

    ```bash
    docker tag wbitt/network-multitool:latest nonprodplatformsharedcr.azurecr.io/troubleshooting/networktools
    ```

1. Push to the ACR (You may need to be granted the AcrPush RBAC role on the ACR)

    ```bash
    docker push nonprodplatformsharedcr.azurecr.io/troubleshooting/networktools:latest
    ```

1. The ACR needs to be enabled for an [Admin User](https://portal.azure.com/?feature.msaljs=true#@willowinc.com/resource/subscriptions/178b67d7-b6fd-46db-b4a3-b57f8a6b045f/resourceGroups/nonprod-platformshared/providers/Microsoft.ContainerRegistry/registries/nonprodplatformsharedcr/accessKey)
1. AZ CLI command like the following

    ```bash
    az containerapp create --name nettools --resource-group rg-dev-eus-01-wil-pen --environment cae-dev-eus-01-wil-pen-eb72f0d8 --image nonprodplatformsharedcr.azurecr.io/troubleshooting/networktools --target-port 1180 --ingress 'external' --registry-server nonprodplatformsharedcr.azurecr.io --query properties.configuration.ingress.fqdn --registry-username nonprodplatformsharedcr --registry-password 'The password from the admin user access keys'
    ```
