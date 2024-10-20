Param
(
    [string]
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [ValidateSet("internal-devservices-aks")]
    $ClusterName,

    [string]
    $certManagerVersion = "v1.8.0",

    [string]
    $ingressNginxVersion = "4.4.2",

    [string]
    $kedaVersion = "2.7.2",

    [string]
    $csiVaultVersion = "1.0.1",

    [string]
    $podIdentityVersion = "4.1.12",

    [string]
    $reloaderVersion = "v0.0.110",

    [string]
    $glooVersion = "1.11.11"

)
$ErrorActionPreference = "Stop"
function RunCliCommand {
    param (
        $CliCommand
    )
    Write-Verbose "Running: $CliCommand"
    Invoke-Expression $CliCommand
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Command $CliCommand resulted in exit code $LASTEXITCODE"
    }

}

RunCliCommand "kubectl apply -f $PSScriptRoot/namespaces.yaml"


RunCliCommand "helm repo add jetstack https://charts.jetstack.io"
RunCliCommand "helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx"
RunCliCommand "helm repo add kedacore https://kedacore.github.io/charts"
RunCliCommand "helm repo add csi-secrets-store-provider-azure https://azure.github.io/secrets-store-csi-driver-provider-azure/charts"
RunCliCommand "helm repo add aad-pod-identity https://raw.githubusercontent.com/Azure/aad-pod-identity/master/charts"
RunCliCommand "helm repo add stakater https://stakater.github.io/stakater-charts"
RunCliCommand "helm repo add gloo https://storage.googleapis.com/solo-public-helm"


RunCliCommand "helm repo update"

## Pod Id
RunCliCommand "helm upgrade --install aad-pod-identity aad-pod-identity/aad-pod-identity --namespace cluster-pod-id --version $podIdentityVersion -f $PSScriptRoot/pod-identity/willow-values.yaml -f $PSScriptRoot/pod-identity/values-$ClusterName.yaml"

## Keyvault Provider ##
RunCliCommand "helm upgrade --install csi csi-secrets-store-provider-azure/csi-secrets-store-provider-azure --namespace kube-system --version $csiVaultVersion -f $PSScriptRoot/csi-secrets/willow-values.yaml"

## Reloader ##
RunCliCommand "helm upgrade --install reloader stakater/reloader --namespace cluster-reloader --version $reloaderVersion -f $PSScriptRoot/stakater-reloader/willow-values.yaml"

## Keda ##
RunCliCommand "helm upgrade --install keda kedacore/keda  --namespace cluster-keda --version $kedaVersion -f $PSScriptRoot/keda/willow-values.yaml"

## Gloo ##
if ($ClusterName -ne "internal-devservices-aks"){## We Don't currently need gloo on the dev service cluster
    if($ClusterName -eq "nonprod-platformapps-aks"){ ## An additional values to accomodate both dev and nonprod environments in a single cluster.
        RunCliCommand "helm upgrade --install gloo gloo/gloo  --namespace cluster-gloo --version $glooVersion -f $PSScriptRoot/gloo-edge/willow-values.yaml -f $PSScriptRoot/gloo-edge/values-nonprod-platformapps-aks.yaml"
    } else {
        RunCliCommand "helm upgrade --install gloo gloo/gloo  --namespace cluster-gloo --version $glooVersion -f $PSScriptRoot/gloo-edge/willow-values.yaml"
    }
    RunCliCommand "kubectl apply -f $PSScriptRoot/gloo-edge/customer-auth-gateway-upstream-$ClusterName.yaml"
}


## Ingress ##
RunCliCommand "helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx  --namespace cluster-ingress --version $ingressNginxVersion -f $PSScriptRoot/ingress-nginx/willow-values.yaml -f $PSScriptRoot/ingress-nginx/values-$ClusterName.yaml"

## Cert Manager ##
# Apply CRDs seperately so we can uninstall cert manager without losing certificate requests
RunCliCommand "kubectl apply -f https://github.com/jetstack/cert-manager/releases/download/$certManagerVersion/cert-manager.crds.yaml"
RunCliCommand "helm upgrade --install cert-manager jetstack/cert-manager --namespace cluster-cert-manager --version $certManagerVersion -f $PSScriptRoot/cert-manager/willow-values.yaml"
RunCliCommand "kubectl apply -f $PSScriptRoot/cert-manager/production_issuer.yaml"
RunCliCommand "kubectl apply -f $PSScriptRoot/cert-manager/staging_issuer.yaml"
