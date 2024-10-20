function AddOrUpdatePRComment {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        [ValidateNotNullOrEmpty()]
        $Title,

        [Parameter(Mandatory = $true)]
        [string]
        [ValidateNotNullOrEmpty()]
        $BodyMarkdown,

        [Parameter(Mandatory = $true)]
        [string]
        [ValidateNotNullOrEmpty()]
        $SystemAccessToken
    )
    Write-Verbose "Posting PR Comment via Github REST API"

    try {

        $headers = @{Authorization = "Bearer $SystemAccessToken"; Accept = "application/vnd.github+json" }
        $commentBody = @"
$Title
$BodyMarkdown
"@
        $newBody = @{body = $commentBody} | ConvertTo-Json
        $urls = Get-PRUrls

        $existingComments = Invoke-RestMethod -Uri $urls.comments -Method GET -Headers $headers -ContentType application/json

        $matchingComment = $existingComments | Where-Object {$_.body -like "*$Title*"}

        if (($null -ne $matchingComment) -and ($matchingComment.Count -gt 0)){
            $url = "$($urls.commentsFragment)/$($matchingComment.id)"
            Write-Verbose "Updating comment @ $url"
            $update = Invoke-RestMethod -Uri $url -Method PATCH -Headers $headers -ContentType application/json -Body $newBody
            Write-Verbose "Updated existing comment"
        } else {
            $url = "$($urls.comments)"
            Write-Verbose "Posting comment @ $url"
            $update = Invoke-RestMethod -Uri $url -Method POST -Headers $headers -ContentType application/json -Body $newBody
            Write-Verbose "Posted new comment"
        }

        $update
    }
    catch {
        # Don't break builds for failing to post a comment
        Write-Output $_
        Write-Output $_.Exception.Message
    }

}

function PostFileContentToPRComment {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $Title,
        [Parameter(Mandatory = $true)]
        [string]
        $FilePath,
        [Parameter(Mandatory = $true)]
        [string]
        $SystemAccessToken
    )
    Write-Verbose "Posting PR Comment via AzureDevOps REST API"
    $content = Get-Content -path $FilePath
    $BodyMarkdown = @"
``````
$($content -join "`r`n")
``````
"@

    AddOrUpdatePRComment -Title $Title -BodyMarkdown $BodyMarkdown -SystemAccessToken $SystemAccessToken
}



function Get-PRUrls {

    $baseUrl = "https://api.github.com/repos"

    $repository = $env:BUILD_REPOSITORY_NAME ## WillowInc/AzurePlatform

    $prNumber = $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER ## 8


    return @{
        ## https://api.github.com/repos/WillowInc/AzurePlatform/issues/8/comments
        comments = "$baseUrl/$repository/issues/$prNumber/comments"
        commentsFragment = "$baseUrl/$repository/issues/comments"
    }
}

function Get-PR {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [string]
        $SystemAccessToken
    )
    if ([string]::IsNullOrWhiteSpace($SystemAccessToken)) {
        Write-Verbose "No access token"
    }
    elseif ($null -eq $Env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
        Write-Verbose "Not Part of a PR"
    }
    else {
        Write-Verbose "Getting PR details"

        try {
            $headers = @{Authorization = "Bearer $SystemAccessToken"; Accept = "application/vnd.github+json" }
            $baseUrl = "https://api.github.com/repos"
            $repository = $env:BUILD_REPOSITORY_NAME
            $prNumber = $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
            $uri = "$baseUrl/$repository/pulls/$prNumber"
            Write-Verbose "Constructed URL: $uri"

            $pr = Invoke-RestMethod -Uri $uri -Method GET -Headers $headers -ContentType application/json
            return $pr

        }
        catch {
            Write-Host "##vso[task.logissue type=error]$($_.Exception.Message)"
            Write-Error $_
        }
    }
}
