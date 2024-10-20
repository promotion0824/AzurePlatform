
import { RegNone } from '../registers/none'
import { CloudflareDns, CfProxyOn, NoNs } from '../providers/cloudflare'

console.log('Zone: willowinc.com!cf - Cloudflare');

D("willowinc.com!cf", RegNone,  DnsProvider(CloudflareDns), NoNs,
    DefaultTTL(1),
    //SOA('willowinc.com', 'willowinc.com.willowinc.com.', 'root.willowinc.com.willowinc.com.', 2038265008, 7200, 3600, 86400, 3600, TTL(3600)),

    // Customer facing portal/api
    CNAME('buildapi-dev', 'wil-bld-dev-wap-aue-buildapi.azurewebsites.net.', CfProxyOn),

    CNAME('command-admin', 'wp-admin.azurefd.net.', CfProxyOn),
    CNAME('command-mobile', 'wp-mobile.azurefd.net.'),
    CNAME('command', 'command.azurefd.net.', CfProxyOn),

    CNAME('launch', 'wil-bld-prd-wap-aue-register.azurewebsites.net.', CfProxyOn),

    CNAME('login-dev', 'wil-bld-dev-wap-aue-idp.azurewebsites.net.', CfProxyOn),
    CNAME('login-uat', 'wil-bld-uat-wap-aue-idp.azurewebsites.net.', CfProxyOn),
    CNAME('login', 'wil-bld-prd-wap-aue-idp.azurewebsites.net.', CfProxyOn),

    CNAME('portal', 'wil-bld-prd-wap-aue-portal.azurewebsites.net.', CfProxyOn),

    // Product
    CNAME('roadmap', 'portal.productboard.com.', CfProxyOn),

    A('@', '141.193.213.10'),
    A('@', '141.193.213.11'),
    CNAME('www', '@'),
    CNAME('www2', 'go.pardot.com.'),
    TXT('_cf-custom-hostname', '5fbd58c1-f597-4fc4-9b17-c4173d32e39f'),

    // Email related
    MX('@', 10, 'au-smtp-inbound-1.mimecast.com.', TTL(1800)),
    MX('@', 10, 'au-smtp-inbound-2.mimecast.com.', TTL(1800)),
    CNAME('7322574', 'sendgrid.net.'),
    CNAME('s1._domainkey', 's1.domainkey.u7322574.wl075.sendgrid.net.'),
    CNAME('s2._domainkey', 's2.domainkey.u7322574.wl075.sendgrid.net.'),
    CNAME('em6491','u7322574.wl075.sendgrid.net.'),
    CNAME('url2057', 'sendgrid.net.'),
    CNAME('url6910', 'sendgrid.net.'),
    CNAME('url6557', 'sendgrid.net.'),
    CNAME('url800', 'sendgrid.net.'),
    CNAME('url8413', 'sendgrid.net.'),
    CNAME('autotask._domainkey', 'dkim.autotask.net.'),
    TXT('mimecast20220124._domainkey', 'v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC4wbbxqM/sVHVZoi2u7uUANJn6gEP35xusFrbCdEnf0hqLcc3dwLMU2jwOZHTG6KklQDg97hhflJe2WMxZ5h1eIZNnsGi1fNobvveUrH3nNsb22f/QdHD1kNIioVoBSuSl0NVX+5zl7txeb0D+McumRCgbpRuq4ULOm48nEh2snwIDAQAB'),
    CNAME('zendesk1._domainkey', 'zendesk1._domainkey.zendesk.com.'),
    CNAME('zendesk2._domainkey', 'zendesk2._domainkey.zendesk.com.'),
    TXT('zendeskverification', '05e24581c78874ea'),
    CNAME('salesforce._domainkey', 'salesforce.uwobve.custdkim.salesforce.com.'),
    CNAME('salesforce1._domainkey', 'salesforce1.jjteqt.custdkim.salesforce.com.'),

    //Employment Innovations - start
    CNAME('kp._domainkey','kp.domainkey.u111712.wl213.sendgrid.net.'),
    CNAME('kp2._domainkey','kp2.domainkey.u111712.wl213.sendgrid.net.'),
    CNAME('em6796','u111712.wl213.sendgrid.net.'),
    //Employment Innovations - end

    // Validation records
    TXT('@', 'willow-website.azurewebsites.net', TTL(60)),
    TXT('@', 'vlnkmoni2pjnu5r8e4ohbj6j4c', TTL(60)),
    TXT('@', 'MS=ms74116313', TTL(60)),
    TXT('@', 'v=spf1 include:spf.protection.outlook.com include:et._spf.pardot.com include:au._netblocks.mimecast.com include:spf.au.exclaimer.net include:_spf.salesforce.com include:autotask.net include:mail.zendesk.com -all', TTL(60)),
    TXT('@', 'pardot658313=1b30bc4d6372170d492d340e9cc93668c83759fc6410467b1a87c7bc65902409', TTL(60)),
    TXT('@', 'MS=ms54049416', TTL(60)),
    TXT('@', 'pardot658313=85ed464d71d23e446095dbd81ada87f62454e7d446e7e2b7089843f5b7f8aeea', TTL(60)),
    TXT('@', 'stf3e43mg34j6vevm49dtd1afb', TTL(60)),
    TXT('@', '93vluns21uoha9gpinlcev3dip', TTL(60)),
    TXT('@', 'g5s8vr45tcp38c1usqm4uro771', TTL(60)),
    TXT('@', 'i7be5mqi7hp30b3mcp4t6osbl2', TTL(60)),
    TXT('@', 'it0ftldggupvslh99dlr5ss6cv', TTL(60)),
    TXT('@', 'atlassian-domain-verification=FbL7ijknPv5aqq9GvrR2CETnSmkKyGnxY08FCHnadzgkaialjEXCG2C0M5KUBAi7', TTL(60)),
    TXT('@', 'apple-domain-verification=oupiljhpjWInX5af', TTL(60)),
    TXT('@', 'status-page-domain-verification=m6sdh097xvr3', TTL(60)),
    TXT('@', 'miro-verification=3d5053f81e6b14e80d16e9041824042bdf68f43f', TTL(300)),
    TXT('_acme-challenge', 'Fnwr0ZMnkA0Vo9roL6klBwQPoKScPxD665bp_dtI8pA', TTL(60)),
    TXT('_dmarc', 'v=DMARC1; p=reject; rua=mailto:6b8eb375142b4cd0898be425bad5e828@dmarc-reports.cloudflare.net,mailto:security@willowinc.com; ruf=mailto:security@willowinc.com; fo=1'),
    TXT('_domainkey', 't=y; o=~;'),
    TXT('200608._domainkey', 'k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDGoQCNwAQdJBy23MrShs1EuHqK/dtDC33QrTqgWd9CJmtM3CK2ZiTYugkhcxnkEtGbzg+IJqcDRNkZHyoRezTf6QbinBB2dbyANEuwKI5DVRBFowQOj9zvM3IvxAEboMlb0szUjAoML94HOkKuGuCkdZ1gbVEi3GcVwrIQphal1QIDAQAB;'),
    TXT('_github-challenge-willowinc', 'd7870d8668', TTL(300)),
    TXT('@', 'ca3-521e3d2e259c4f00aef817a30d7f9c8f'),
    TXT('@', 'facebook-domain-verification=fcrjslv85z7lmthoowgze53k3qmtg1'),
    SRV('_sipfederationtls._tcp', 100, 1, 5061, 'sipfed.online.lync.com.'),
    SRV('_sip._tls', 100, 1, 443, 'sipdir.online.lync.com.'),

    CNAME('_5a857124e959d256fee40027c619de68', '631479cf43b9ea2b923b157fcaef51bf.181d659ea9417886f2b3bba0ede1eec5.63d21cd4249ec.comodoca.com.'),

    CNAME('selector1._domainkey', 'selector1-willowinc-com._domainkey.ridleyco.onmicrosoft.com.'),
    CNAME('selector2._domainkey', 'selector2-willowinc-com._domainkey.ridleyco.onmicrosoft.com.'),

    CAA('@', 'issue', 'letsencrypt.org'),
    CAA('@', 'iodef', 'mailto:willowcloud@willowinc.com'),
    CAA('@', 'issue', 'godaddy.com'),
    CAA('@', 'issue', 'sectigo.com'),

    // This was incorreclty set as isuewild a lovely typo
    CAA('@', 'issuewild', 'sectigo.com'),
    CAA('@', 'issue', 'digicert.com'),

    CNAME('autodiscover', 'autodiscover.outlook.com.'),
    CNAME('buildapi', 'wil-bld-prd-wap-aue-buildapi.azurewebsites.net.', TTL(300)),
    CNAME('buildapi-uat', 'wil-bld-uat-wap-aue-buildapi.azurewebsites.net.'),

    CNAME('developers', 'ssl.redoc.ly.', TTL(300)),
    A('o1.ptr9386.email', '168.245.14.95'),
    CNAME('enterpriseenrollment', 'enterpriseenrollment.manage.microsoft.com.'),
    CNAME('enterpriseregistration', 'enterpriseregistration.windows.net.'),
    TXT('fsverification-servicedesk', 'dfe3896c261da29048f13484332d2dfb830d8f8a77a8a82aa9320bb5b8e902de', TTL(300)),
    A('infoexchange', '13.75.175.120', TTL(1800)),
    CNAME('intranet', 'ridleyco.sharepoint.com.'),

    CNAME('lyncdiscover', 'webdir.online.lync.com.'),
    CNAME('model', 'model.azureedge.net.', TTL(300)),
    CNAME('modelaudit', 'modelauditing.azurewebsites.net.'),
    CNAME('mp-demo', 'mp-web-prd-aue.azurewebsites.net.'),
    CNAME('msoid', 'clientconfig.microsoftonline-p.net.'),
    CNAME('sip', 'sipdir.online.lync.com.'),

    TXT('_acme-challenge.sq', 'uJnETn11OHTHoOcRAHkIrl1hIwoTDHHm0UcetfF04_w', TTL(60)),
    CNAME('support', 'willowinc.zendesk.com.'),
    A('sydmgtvpn', '103.145.236.6'),
    TXT('p450863p.teams', 'MS=ms28371185'),
    CNAME('thevine', 'ridleyco.sharepoint.com.'),

    A('ux-dev-webapp', '13.75.224.18'),
    A('ux-uat-webapp', '104.210.81.42'),
    A('vpn', '203.87.85.154'),
    A('man-vpn', '203.82.39.70'),
    A('vpn.ads', '20.213.156.2'), // Fortigate VPN Azure VM
    A('syd-iotlab.ads', '123.103.222.37'), //Syd IOT Testlab
    TXT('@', 'testdnschange', TTL(60)),

    // SaaSify
    CNAME('purchase', 'saasify.trafficmanager.net.'),

    // Kubernetes Clusters
    A('*.prodaue', '20.70.228.186', TTL(300)), // Ngnix Ingress
    A('*.prodeu2', '52.247.81.42', TTL(300)), // Ngnix Ingress
    A('*.prodweu', '20.126.171.205', TTL(300)), // Ngnix Ingress
    A('app-aue.sbx', '20.53.175.5', TTL(300)), // Gloo Edge
    A('app-eu2.sbx', '20.53.175.5', TTL(300)), // Gloo Edge
    A('app-aue.dev', '20.70.228.166', TTL(300)), // Gloo Edge
    A('app-eu2.dev', '20.70.228.166', TTL(300)), // Gloo Edge
    A('app-aue.nonprod', '20.92.212.156', TTL(300)), // Gloo Edge
    A('app-eu2.nonprod', '20.92.212.156', TTL(300)), // Gloo Edge
    A('app-aue.prod', '20.92.215.116', TTL(300)), // Gloo Edge
    A('app-eu2.prod', '52.247.87.100', TTL(300)), // Gloo Edge
    A('app-weu.prod', '20.93.214.181', TTL(300)), // Gloo Edge
    A('*.nonprod', '20.92.221.202', TTL(300)), // Ngnix Ingress
    A('*.devservices', '20.92.217.79', TTL(300)), // Ngnix Ingress

    // Frontdoors
    //CNAME('api', 'wp-api.azurefd.net.', TTL(300)),
    //CNAME('command-mobile', 'wp-mobile.azurefd.net.', TTL(300)),
    CNAME('commerce', 'commerce.azurefd.net.', TTL(300)),
    CNAME('experience', 'experience.azurefd.net.', TTL(300)),
    CNAME('experience-dev', 'wil-dev-exp-shr-glb-experience.azurefd.net.'),
    CNAME('experience-uat', 'wil-uat-exp-shr-glb-experience.azurefd.net.'),
    CNAME('myexperience', 'wil-uxa-prd-aue1.azurefd.net.', TTL(300)),
    CNAME('myexperience-dev', 'wil-dev-exp-shr-glb-customers.azurefd.net.'),
    CNAME('myexperience-uat', 'wil-uat-exp-shr-glb-customers.azurefd.net.'),
    CNAME('command-uat', 'wil-uat-lda-shr-glb-command.azurefd.net.', CfProxyOn),
    CNAME('command-dev', 'wil-dev-lda-shr-glb-command.azurefd.net.', CfProxyOn),

    // Other Zones
    NS('success', 'ns1-02.azure-dns.com.'),
    NS('success', 'ns2-02.azure-dns.net.'),
    NS('success', 'ns3-02.azure-dns.org.'),
    NS('success', 'ns4-02.azure-dns.info.'),

    // Speckle POC
    A('speckle', '20.11.58.168'),

    //Security Services- Jira
    A('security', '192.0.2.1', CfProxyOn),

    CNAME('afdverify.command-uat', 'afdverify.wil-uat-lda-shr-glb-command.azurefd.net.'),
    CNAME('afdverify.command-dev', 'afdverify.wil-dev-lda-shr-glb-command.azurefd.net.'),

    // GitHub Pages for Storybook UI Documentation
    TXT('_github-pages-challenge-WillowInc', '227040804ac76849c2572d1c5743ee', TTL(60)),
    // Github Pages server by WillowInc/StorybookReleases repo
    CNAME('storybook', 'willowinc.github.io.'),
    // Github Pages server by WillowInc/TwinPlatform repo
    CNAME('storybook-dev', 'willowinc.github.io.'),

    // Willow GPT Copilot
    TXT('asuid.willowgpt', '324BD715EEE4C29559F9838B39A8545ED9433F7306C46CF1720413C3A2CF3054'),
    A('willowgpt', '20.119.0.45', CfProxyOn),



    // Single tenant subscription scoped records
    // SandboxShared sbx
    TXT('asuid.app-sbx', '44E182F437C82D4E1EFA560F9296D67A201CFE6AAC063CE9B59550605AEA4F90'),
    // dev-eus-00 ephemeral
    TXT('asuid.app-dev', '8A9CF22971560DBDA335ADBBCE64023812183C8DB3FD9499C99D45A2C6CB925C'),
    // dev-eus-01 ci
    TXT('asuid.app-dev', 'BAFD20CD55D1D9402AE75BD5E3B96A86D265309BC698A4483EE904EC6CAF529B'),
    // wil-prd-mkp-aue2-imagehub-dev
    TXT('asuid.app-dev', 'E883C6EC42A2BC2D838A7CA053CF4F0691F1C29CBACDFE7B1C86AD38AE434A25'),
    // wil-prd-mkp-aue2-imagehub-prd
    TXT('asuid.app', 'E883C6EC42A2BC2D838A7CA053CF4F0691F1C29CBACDFE7B1C86AD38AE434A25'),
    // wil-uat-mkp-aue2-imagehub-dev
    TXT('asuid.app-dev', '6ECFD378559FF5EAD508E4DF87AFF3FE2391508F21F90CA7A0BAAFD04E0CB63C'),
    // wil-uat-mkp-aue2-imagehub-prd
    TXT('asuid.app', '6ECFD378559FF5EAD508E4DF87AFF3FE2391508F21F90CA7A0BAAFD04E0CB63C'),
    // dev-eus-02 nbci
    TXT('asuid.app-dev', '37C0B291CEF5B53DB5AA9BFC3BD71846817069B02DEF686EC018DE4A3B5750D7'),
    TXT('asuid.newbuild', '37C0B291CEF5B53DB5AA9BFC3BD71846817069B02DEF686EC018DE4A3B5750D7'),
    // prd-aue-01 public
    TXT('asuid.app', '8ABF6CE4CA2D4A16FC84B10EFA26376EC4B0C0475CDEF05C646B213E2FBB3A86'),
    // prd-aue-02 public
    TXT('asuid.app', 'A621FBEF4E0B38D41B0BDCBE948FB1A4168AC02BB9B188BEB9B3C0AC08E9AA4F'),
    // prd-aue-03 pilot
    TXT('asuid.app', 'FC9D495781DE5D7294A53676246EB18DBC7A41CC9E2F4AE5AD6EFA0CD81C4F9C'),
    TXT('asuid.newbuild', 'FC9D495781DE5D7294A53676246EB18DBC7A41CC9E2F4AE5AD6EFA0CD81C4F9C'),
    // prd-aue-04 pilot
    TXT('asuid.app', '8FBC7350E625B851448332C876131D2E60405C0A276092E8D2EA0B161928C41D'),
    TXT('asuid.newbuild', '8FBC7350E625B851448332C876131D2E60405C0A276092E8D2EA0B161928C41D'),
    // prd-aue-05 pilot
    TXT('asuid.app', '8648F28FC76315B8C7F483FC5ED7194887E89C6EE8D5A9B8209F718144B3095C'),
    TXT('asuid.newbuild', '8648F28FC76315B8C7F483FC5ED7194887E89C6EE8D5A9B8209F718144B3095C'),
    // prd-aue-06 pilot
    TXT('asuid.app', '6ADDFDFCD435F24FAAA6912FEE713D3A0FBF76087E5995A915CC474836EB1305'),
    TXT('asuid.newbuild', '6ADDFDFCD435F24FAAA6912FEE713D3A0FBF76087E5995A915CC474836EB1305'),
    // prd-aue-07 pilot
    TXT('asuid.app', 'D1522D259AECD5BE63E66F0A7224F315D0974FD75EB1FAEBAF5648BDBBA16F66'),
    TXT('asuid.newbuild', 'D1522D259AECD5BE63E66F0A7224F315D0974FD75EB1FAEBAF5648BDBBA16F66'),
    // prd-aue-08 pilot
    TXT('asuid.app', '50A39D0402A21E83EFD1C2170BE1A7D676FD50B70C098245FC0049F3BA9BE105'),
    TXT('asuid.newbuild', '50A39D0402A21E83EFD1C2170BE1A7D676FD50B70C098245FC0049F3BA9BE105'),
    // prd-aue-09 pilot
    TXT('asuid.app', '22867E2AF4CF5D232DD47ADE4B9EF264C3BBC00200D2CCAD36C695214963CAED'),
    TXT('asuid.newbuild', '22867E2AF4CF5D232DD47ADE4B9EF264C3BBC00200D2CCAD36C695214963CAED'),
    // prd-eus2-01 ppe
    TXT('asuid.app', 'A6DBB01ECC8406119D820FE33BEBCA4DBAF4357F3CA0EE723F4587663BDC7FA5'),
    // prd-eus2-02 pilot
    TXT('asuid.app', '52925999D4C54F6D275A7ACC48B6FBED6C624769B683C11465076E90FF6F50C9'),
    // prd-eus2-03 pilot
    TXT('asuid.app', 'D5DCA0F9AA9C61AA6F07826C406D65A3427197D4B7C380606945774DDF2C43B1'),
    // prd-eus2-04 pilot
    TXT('asuid.app', '0607288C263400360BB5610C5160654BA89796F0E20B722588A39E5C67F292EF'),
    // prd-eus2-05 public
    TXT('asuid.app', '8D78642FB71C6BC9A9B66D49DA92F087E6EEC0AD8BE4B0AA91159A03A0CB3ED2'),
    // prd-eus2-06 medium
    TXT('asuid.app', 'B6C1F1A88548A89D436E7E5997AC9504A47D8715AD564C6D8F504364EFC02294'),
    // prd-eus2-07 heavy
    TXT('asuid.app', '6085838C12639AE58A5DB2C9CD78796FA5D2F47F3A40C979CADA0BEC01D93821'),
    // prd-eus2-08 public
    TXT('asuid.app', '51627A85E1E930E316BCDE122DB8624A90978361E46BE4393B7D3FD4256AA940'),
    // prd-eus2-09 public
    TXT('asuid.app', '97A819B2A3DD22B13DB00909800322C30C4F02C615B09895C5CEBF50770B2AEB'),
    // prd-eus2-10 public
    TXT('asuid.app', '020F707B6A65D1513A0FF85264C3DF40E6DA36391FF443E03F4F81BBC5EB8410'),
    // prd-eus2-11 public
    TXT('asuid.app', 'A8CA6E9DA7BCC7DDD77F1566FCA6FA88BCA614FBFFB1E2E97E49F75F700E7123'),
    // prd-eus2-12 public
    TXT('asuid.app', '0A85A12315AD3A06D55B5215564686D6A7497915CA9B4E4C8613F18F092CC5C4'),
    // prd-eus2-13 public
    TXT('asuid.app', 'B663825E8937E904B186772D9706A043450432BA4F94F9D28585EE4799AD7C0C'),
    // prd-eus2-14 public
    TXT('asuid.app', 'A5B9C1C39A4015E494940B8FB3B40559548FD930180B50A646CAA70B05C5C840'),
    // prd-eus2-15 public
    TXT('asuid.app', '743C29FD06E3E07F209F0BF4AFDF2B7F3B56ED49D264748A38399952EFDBE9B8'),
    // prd-eus2-16 public
    TXT('asuid.app', '7A7B5C8B4D2E31A26679759B3611597189D8635A3891BA9B7BB621F82752FDC2'),
    // prd-eus2-17 public
    TXT('asuid.app', '602C1864158E4AFFBE71854C4F600FF563B0524F4A96B85CDB4ACDA7856A5923'),
    // prd-eus2-18 public
    TXT('asuid.app', '4525519AE053BDADEEA3522ECF6FD837456C3102690545923603B2A353B0054D'),
    // prd-weu-01 public
    TXT('asuid.app', '2842C71C1BA0F5497B73F0EF65A0938FACB1C0E1C38BE4590B1F374FB0F41C97'),
    // prd-weu-02 public
    TXT('asuid.app', '69BCDB9BED21360BB86040E9FD2A52A85C113920B163B3FA9AC0661518871729'),

    //sbx
    CNAME('sample.app-sbx', 'iad-ingress.bravesand-860afe31.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // wsup-eus
    TXT('asuid', '09A8F54EFE87C6B91776D28DB213DD238CE2DE471B3F14334F87E930BE8B20F6'),
    //TXT('asuid.wsup', '09A8F54EFE87C6B91776D28DB213DD238CE2DE471B3F14334F87E930BE8B20F6'),
    //TXT('asuid.wsupapi', '09A8F54EFE87C6B91776D28DB213DD238CE2DE471B3F14334F87E930BE8B20F6'),
    TXT('asuid.wsup-dev', '09A8F54EFE87C6B91776D28DB213DD238CE2DE471B3F14334F87E930BE8B20F6'),
    TXT('asuid.wsupapi-dev', '09A8F54EFE87C6B91776D28DB213DD238CE2DE471B3F14334F87E930BE8B20F6'),

    TXT('asuid.wsup', '1BB96A66829FE274D893DDA04D427C86EC8E4CA90256A8F76AD98F4792970281'),
    TXT('asuid.wsupapi', '1BB96A66829FE274D893DDA04D427C86EC8E4CA90256A8F76AD98F4792970281'),
    TXT('asuid.wsup-prd', '1BB96A66829FE274D893DDA04D427C86EC8E4CA90256A8F76AD98F4792970281'),
    TXT('asuid.wsupapi-prd', '1BB96A66829FE274D893DDA04D427C86EC8E4CA90256A8F76AD98F4792970281'),

    CNAME('wsup-dev', 'wsup.proudflower-a1fe127e.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('wsupapi-dev', 'wsupapi.proudflower-a1fe127e.eastus.azurecontainerapps.io.', CfProxyOn),

    CNAME('wsup', 'wsup.happygrass-c717c637.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('wsupapi', 'wsupapi.happygrass-c717c637.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('wsup-prd', 'wsup.happygrass-c717c637.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('wsupapi-prd', 'wsupapi.happygrass-c717c637.eastus2.azurecontainerapps.io.', CfProxyOn),

    // New build CNAME section
    // dev-eus-02 nbci
    CNAME('dev-eus-02-wil-nbci-new-build.app-dev', 'iad-ingress.kindwave-ca167914.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('nbci.app-dev', 'iad-ingress.kindwave-ca167914.eastus.azurecontainerapps.io.', CfProxyOn),

    // Single tenant CNAME section
    // dev-eus-00 dev
    CNAME('dev-eus-00-wil-in1.app-dev', 'iad-ingress.yellowsea-9d14124e.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('willow.app-dev', 'iad-ingress.yellowsea-9d14124e.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('willow-dev.app-dev', 'iad-ingress.yellowsea-9d14124e.eastus.azurecontainerapps.io.', CfProxyOn),

    // dev-eus-01 ci
    CNAME('dev-eus-01-wil-in1.app-dev', 'iad-ingress.victoriouscliff-e430d605.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('ci.app-dev', 'iad-ingress.victoriouscliff-e430d605.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('ci-mobile.app-dev', 'iad-ingress.victoriouscliff-e430d605.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('willow-ci.app-dev', 'iad-ingress.victoriouscliff-e430d605.eastus.azurecontainerapps.io.', CfProxyOn),

    // dev-eus-01 lt
    CNAME('dev-eus-01-wil-lt.app-dev', 'iad-ingress.whitesmoke-43cc7ac9.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('lt.app-dev', 'iad-ingress.whitesmoke-43cc7ac9.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('willow-lt.app-dev', 'iad-ingress.whitesmoke-43cc7ac9.eastus.azurecontainerapps.io.', CfProxyOn),

    // dev-eus-01 fga
    CNAME('dev-eus-01-wil-fga.app-dev', 'iad-ingress.proudwave-c11c89a8.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('fga.app-dev', 'iad-ingress.proudwave-c11c89a8.eastus.azurecontainerapps.io.', CfProxyOn),
    CNAME('willow-fga.app-dev', 'iad-ingress.proudwave-c11c89a8.eastus.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-01 ppe
    CNAME('prd-eus2-01-wil-in1.app', 'iad-ingress.braveriver-150d7429.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('ppe.app', 'iad-ingress.braveriver-150d7429.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('willow-ppe.app', 'iad-ingress.braveriver-150d7429.eastus2.azurecontainerapps.io.', CfProxyOn),

    // jll
    // prd-eus2-02 jll
    CNAME('prd-eus2-03-jll-in1.app', 'iad-ingress.prouddune-d604fc28.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('jll.app', 'iad-ingress.prouddune-d604fc28.eastus2.azurecontainerapps.io.', CfProxyOn),

    // ddk
    // prd-eus2-03 ddk
    CNAME('prd-eus2-03-ddk-in1.app', 'iad-ingress.thankfulflower-ce66bef6.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('ddk.app', 'iad-ingress.thankfulflower-ce66bef6.eastus2.azurecontainerapps.io.', CfProxyOn),

    // wmt
    // prd-eus2-04 wmt uat
    CNAME('prd-eus2-04-wmt-uat.app', 'iad-ingress.happyground-600de3aa.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('walmart-uat.app', 'iad-ingress.happyground-600de3aa.eastus2.azurecontainerapps.io.', CfProxyOn),
    // prd-eus2-05 wmt prd
    CNAME('prd-eus2-05-wmt-in1.app', 'iad-ingress.whitebay-c11cf2c3.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('walmart.app', 'iad-ingress.whitebay-c11cf2c3.eastus2.azurecontainerapps.io.', CfProxyOn),

    // durst
    // prd-eus2-06 dur prd
    CNAME('prd-eus2-06-dur-in1.app', 'iad-ingress.mangograss-fd27505d.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('durst.app', 'iad-ingress.mangograss-fd27505d.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-07 tur prd
    CNAME('prd-eus2-07-tur-in1.app', 'iad-ingress.jollywater-7d88decc.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('turner.app', 'iad-ingress.jollywater-7d88decc.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-08 msft prd
    CNAME('prd-eus2-08-msft-in1.app', 'iad-ingress.reddune-3291d9c6.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('msft.app', 'iad-ingress.reddune-3291d9c6.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-09 san prd
    CNAME('prd-eus2-09-san-in1.app', 'iad-ingress.graysky-8f940129.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('sanofi.app', 'iad-ingress.graysky-8f940129.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-10 hwp uat
    CNAME('hollywoodpark-uat.app', 'iad-ingress.mangodune-e0e7fcde.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('rg-prd-eus2-10-hwp-uat.app', 'iad-ingress.mangodune-e0e7fcde.eastus2.azurecontainerapps.io.', CfProxyOn),
    // prd-eus2-10 hwp prd
    CNAME('hollywoodpark.app', 'iad-ingress.thankfulsand-014f1c78.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-10-hwp-in1.app', 'iad-ingress.thankfulsand-014f1c78.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-11 brk uat
    CNAME('brookfield-uat.app', 'iad-ingress.politewave-a19cb9bf.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('brookfield-uat-mobile.app', 'iad-ingress.politewave-a19cb9bf.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-11-brk-uat.app', 'iad-ingress.politewave-a19cb9bf.eastus2.azurecontainerapps.io.', CfProxyOn),
    // prd-eus2-11 brk prd
    CNAME('brookfield.app', 'iad-ingress.livelydune-f2613cc8.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('brookfield-mobile.app', 'iad-ingress.livelydune-f2613cc8.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-11-brk-in1.app', 'iad-ingress.livelydune-f2613cc8.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-12 dfw uat
    CNAME('dfw-uat.app', 'iad-ingress.ashydune-7e335ef9.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-12-dfw-uat.app', 'iad-ingress.ashydune-7e335ef9.eastus2.azurecontainerapps.io.', CfProxyOn),
    // prd-eus2-12 dfw prd
    CNAME('dfw.app', 'iad-ingress.salmonsea-73791804.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-12-dfw-in1', 'iad-ingress.salmonsea-73791804.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-13 bp
    CNAME('bp.app', 'iad-ingress.nicewater-163e9741.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-13-bp-in1.app', 'iad-ingress.nicewater-163e9741.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-14 wmk in1
    CNAME('watermanclark.app', 'iad-ingress.gentlewater-fb385dcb.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-14-wmk-in1.app', 'iad-ingress.gentlewater-fb385dcb.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-15 wmr uat
    CNAME('walmartretail-uat.app', 'iad-ingress.yellowcoast-9b7cce4b.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('walmartretail-uat-mobile.app', 'iad-ingress.yellowcoast-9b7cce4b.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-15-wmr-uat.app', 'iad-ingress.yellowcoast-9b7cce4b.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-15 wmr prd
    CNAME('walmartretail.app', 'iad-ingress.calmground-058871b8.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('walmartretail-mobile.app', 'iad-ingress.calmground-058871b8.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-15-wmr-in1.app', 'iad-ingress.calmground-058871b8.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-16 jpmc prd
    CNAME('jpmc.app', 'iad-ingress.gentlecliff-b17162c0.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-16-jpmc-in1.app', 'iad-ingress.gentlecliff-b17162c0.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-17 nau prd
    CNAME('northernarizonauniversity.app', 'iad-ingress.jollytree-66af29c0.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-17-nau-in1.app', 'iad-ingress.jollytree-66af29c0.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-eus2-18 oxf prd
    CNAME('oxford.app', 'iad-ingress.braveflower-508c3fe3.eastus2.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-eus2-18-oxf-in1.app', 'iad-ingress.braveflower-508c3fe3.eastus2.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-01 inv uat
    CNAME('investa-uat.app', 'iad-ingress.bravebay-3dbbe7d6.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-01-inv-uat.app', 'iad-ingress.bravebay-3dbbe7d6.australiaeast.azurecontainerapps.io.', CfProxyOn),
    // prd-aue-01 inv prd
    CNAME('investa.app', 'iad-ingress.calmpond-8052fcb1.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-01-inv-in1', 'iad-ingress.calmpond-8052fcb1.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-02 jhg prd
    CNAME('jhg.app', 'iad-ingress.thankfulhill-fe6f63ec.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-02-jgh-in1', 'iad-ingress.thankfulhill-fe6f63ec.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-03 wbg prd
    CNAME('webuild.newbuild', 'iad-ingress.whitesea-2eb8a165.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-03-wbg-nb1', 'iad-ingress.whitesea-2eb8a165.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-04 mlpx prd
    CNAME('multiplex.newbuild', 'iad-ingress.purplesmoke-5c6cc99d.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-04-mlpx-nb1', 'iad-ingress.purplesmoke-5c6cc99d.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-05 cyp prd
    CNAME('cyp.newbuild', 'iad-ingress.whitesmoke-e8943a32.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-05-cyp-nb1', 'iad-ingress.whitesmoke-e8943a32.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-06 inv prd
    CNAME('investa.newbuild', 'iad-ingress.whitedune-7c146cf2.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-06-inv-nb1', 'iad-ingress.whitedune-7c146cf2.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-07 srl prd
    CNAME('srl.newbuild', 'iad-ingress.yellowpebble-4a87c85e.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-07-srl-nb1', 'iad-ingress.yellowpebble-4a87c85e.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-08 mac prd
    CNAME('macquarie.newbuild', 'iad-ingress.lemonpond-bbc19146.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-08-mac-nb1', 'iad-ingress.lemonpond-bbc19146.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-aue-09 cpb prd
    CNAME('cpb.newbuild', 'iad-ingress.thankfulsand-70c45d95.australiaeast.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-aue-09-cpb-nb1', 'iad-ingress.thankfulsand-70c45d95.australiaeast.azurecontainerapps.io.', CfProxyOn),

    // prd-weu-01 axa prd
    CNAME('axa.app', 'iad-ingress.blackflower-307f34d3.westeurope.azurecontainerapps.io.', CfProxyOn),
    CNAME('prd-weu-01-axa-in1.app', 'iad-ingress.blackflower-307f34d3.westeurope.azurecontainerapps.io.', CfProxyOn),

    // bnp
    // prd-weu-02 bnp-nuv
    CNAME('prd-weu-02-bnp-nuv.app', 'iad-ingress.victoriouscliff-fb50e1d0.westeurope.azurecontainerapps.io.', CfProxyOn),
    CNAME('bnp-nuv.app', 'iad-ingress.victoriouscliff-fb50e1d0.westeurope.azurecontainerapps.io.', CfProxyOn),
)
