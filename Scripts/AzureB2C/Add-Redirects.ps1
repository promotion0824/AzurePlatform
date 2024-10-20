Param
(
    [string]
    $AppObjectId = "5d038cb6-e52a-4f13-8fd4-b5f73d810a9b",

    [string]
    $WebRedirectUrls
)

$webUrls = $WebRedirectUrls.split(',')

$app = Get-MgApplication -ApplicationId $AppObjectId

foreach ($url in $webUrls) {
    if ($url -notlike "https://*"){
        Write-Error "Requires HTTPS url's for redirect $url"
    } elseif ($app.Web.RedirectUris -contains $url) {
        Write-Verbose "Url: $url already exists as a redirect skipping addition"
    }else {
        $app.Web.RedirectUris += $url
    }
}

Update-MgApplication -ApplicationId $AppObjectId -Web $app.Web
