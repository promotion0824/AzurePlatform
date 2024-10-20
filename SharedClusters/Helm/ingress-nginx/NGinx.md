# NGinx

## Upgrading NGinx

Get versions available in helm

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm search repo ingress-nginx/ingress-nginx --versions
```

Find latest version and update Deploy-Charts.ps1 as appropriate

## General Troubleshooting

Get pods

```bash
kubectl get pods -n $NGINXNS
```

Add and Pod of interest to ENV variables

```bash
export POD=ingress-nginx-controller-5fbb64fd7d-rxhxq
```

### Get recent events in the namespace

```bash
kubectl get events -n $NGINXNS
```

Get all ingresses in the cluster

```bash
kubectl get ingress -A
```

Describe the service.

```bash
kubectl describe svc -n $NGINXNS ingress-nginx-controller
```

Verify that appropriate Annotations for healthz healthcheck are in place:

`
service.beta.kubernetes.io/azure-load-balancer-health-probe-request-path: /healthz
`

Checking the Events of the ConfigMap Resource

```bash
kubectl describe configmap -n $NGINXNS ingress-nginx-controller
```

contents of a configMap

```bash
kubectl exec $POD -n $NGINXNS -- cat //etc/nginx/nginx.conf
```

NGINX tends to have unhealthy probes when cpu or memory get tight, internal devservices and nonprod clusters should probably stay closer

```yaml
  resources:
    limits:
      cpu: 200m
      memory: 300Mi
    requests:
      cpu: 20m
      memory: 250Mi
```
Production clusters only use nginx for kubecost, which isn't strictly needed, so they can be set lower
