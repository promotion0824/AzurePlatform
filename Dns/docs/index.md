# DNSControl


We use [DNSControl](https://stackexchange.github.io/dnscontrol/) to manage our DNS entries across multiple zones and systems. It follows a [GitOps](https://www.redhat.com/en/topics/devops/what-is-gitops) style to manage changes and configurations as well as enforcing a set of [opinions](https://stackexchange.github.io/dnscontrol/opinions) and checks to help reduce the risk of making DNS changes.

## Demo
A demo of using dns control can be found [here](https://ridleyco-my.sharepoint.com/:v:/g/personal/ssaraswati_willowinc_com/ERcGLfhYeXZBh2_t3KStoH0BAeXLbiNOiuzEe0Q_kgrQJA?e=ZYc9BK)
## Updating DNS records

Dns records are recorded in the files src/zones/willowInc.ts and src/zones/willowRail.ts.

Details docs of the file functions can be found here https://stackexchange.github.io/dnscontrol/js.

Once a change is made make a Pull Request to incorporate your changes. The pull request will trigger a preview of the changes to let you validate that what is being changed matches your expectations.



## Cloudflare vs Azure
DNS entires exist in both Azure and Cloudflare for the same zone.

Cloudflare is used to provide Cloudflare proxy and DDOS protection. Azure DNS sets a CNAME to point to cloudlfare to delegate the record to cloudflare. The cloudflare proxy will additionally setup SSL termination with the willow wildcard certificates.

```

+----------+     +-----------+     +------------+
|          |     |           |     |            |
| Azure    +---->| Cloudflare+---->| Endpoint   |
|          |     |           |     |            |
+----------+     +-----------+     +------------+



┌──────────┐                       ┌────────────┐
│          │                       │            │
│ Azure    ├──────────────────────►│ Endpoint   │
│          │                       │            │
└──────────┘                       └────────────┘


```

```typescript
D("willowrail.com!cf", RegNone,  DnsProvider(CloudflareDns), NoNs,
    CNAME('portal', 'wil-wrt-prd-agw-portal-weu.trafficmanager.net.', CfProxyOn)
)

...

D("willowrail.com!az", RegNone,  DnsProvider(AzureWillowRail),
  CNAME('portal', 'portal.willowrail.com.cdn.cloudflare.net.', TTL(300))
)


```
