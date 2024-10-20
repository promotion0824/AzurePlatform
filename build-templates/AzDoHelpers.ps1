function Add-PRComment {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $Body,

        [Parameter(Mandatory = $false)]
        [string]
        $SystemAccessToken
    )
    if ([string]::IsNullOrWhiteSpace($SystemAccessToken)) {
        Write-Verbose "No access token can't post a comment"
    }
    elseif ($null -eq $Env:SYSTEM_PULLREQUEST_PULLREQUESTID) {
        Write-Verbose "Not Part of a PR can't add comments"
    }
    else {
        Write-Verbose "Posting PR Comment via AzureDevOps REST API"
        $prUri = "$($Env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$Env:SYSTEM_TEAMPROJECTID/_apis/git/repositories/$($Env:BUILD_REPOSITORY_NAME)/pullRequests/$($Env:SYSTEM_PULLREQUEST_PULLREQUESTID)/"

        try {
            $uri = "$prUri/threads?api-version=6.0"
            Write-Verbose "Constructed URL: $uri"

            $response = Invoke-RestMethod -Uri $uri -Method POST -Headers @{Authorization = "Bearer $SystemAccessToken" } -Body $Body -ContentType application/json

            if ($null -eq $response) {
                Write-Verbose "Rest API posted OK"
            }
        }
        catch {
            Write-Output $_
            Write-Output $_.Exception.Message
        }
    }
}

function Remove-PRComment {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $TextToMatchOn,

        [Parameter(Mandatory = $false)]
        [string]
        $SystemAccessToken
    )
    if ([string]::IsNullOrWhiteSpace($SystemAccessToken)) {
        Write-Verbose "No access token can't post a comment"
    }
    elseif ($null -eq $Env:SYSTEM_PULLREQUEST_PULLREQUESTID) {
        Write-Verbose "Not Part of a PR can't delete comments"
    }
    else {
        Write-Verbose "Posting PR Comment via AzureDevOps REST API"

        $prUri = "$($Env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$Env:SYSTEM_TEAMPROJECTID/_apis/git/repositories/$($Env:BUILD_REPOSITORY_NAME)/pullRequests/$($Env:SYSTEM_PULLREQUEST_PULLREQUESTID)"
        try {
            $uri = "$prUri/threads?api-version=6.0"
            Write-Verbose "Constructed URL: $uri"

            $list = Invoke-RestMethod -Uri $uri -Method GET -Headers @{Authorization = "Bearer $SystemAccessToken" } -ContentType application/json
            $matchingThreads = $list.value | Where-Object {$_.isDeleted -eq $false -and $_.comments.Count -gt 0 -and $_.comments[0].content -like "*$TextToMatchOn*"}

            foreach ($thread in $matchingThreads) {
                $commentUri = "$prUri/threads/$($thread.id)/comments/1?api-version=6.0"
                Write-Verbose "Constructed URL of comment to delete: $commentUri"
                $list = Invoke-RestMethod -Uri $commentUri -Method DELETE -Headers @{Authorization = "Bearer $SystemAccessToken" } -ContentType application/json

            }

        }
        catch {
            Write-Output $_
            Write-Output $_.Exception.Message
        }
    }
}

function PostFileContentTo-PRComment {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $Title,
        [Parameter(Mandatory = $true)]
        [string]
        $FilePath,
        [Parameter(Mandatory = $false)]
        [string]
        $SystemAccessToken
    )
    Write-Verbose "Posting PR Comment via AzureDevOps REST API"
    $content = Get-Content -path $FilePath
    $content
    $markdownComment = @"
$Title
``````
$($content -join "`r`n")
``````
"@
    $body = @{
        "comments" = @(
            @{"parentCommentId" = 0;
                "commentType"   = 1;
                "content"       = $markdownComment
            }
        );
        "status"   = "active"
    } | ConvertTo-Json
    # post to the PR
    Remove-PRComment -TextToMatchOn $Title -SystemAccessToken $SystemAccessToken
    Add-PRComment -Body $body -SystemAccessToken $SystemAccessToken
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
    elseif ($null -eq $Env:SYSTEM_PULLREQUEST_PULLREQUESTID) {
        Write-Verbose "Not Part of a PR"
    }
    else {
        Write-Verbose "Getting PR details"

        try {
            $uri = "$($Env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$Env:SYSTEM_TEAMPROJECTID/_apis/git/repositories/$($Env:BUILD_REPOSITORY_NAME)/pullRequests/$($Env:SYSTEM_PULLREQUEST_PULLREQUESTID)?api-version=7.1-preview.1"
            Write-Verbose "Constructed URL: $uri"

            $pr = Invoke-RestMethod -Uri $uri -Method GET -Headers @{Authorization = "Bearer $SystemAccessToken" } -ContentType application/json
            return $pr

        }
        catch {
            Write-Host "##vso[task.logissue type=error]$($_.Exception.Message)"
            Write-Error $_
        }
    }
}

function Get-PRLabels {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [string]
        $SystemAccessToken
    )
    if ([string]::IsNullOrWhiteSpace($SystemAccessToken)) {
        Write-Verbose "No access token"
    }
    elseif ($null -eq $Env:SYSTEM_PULLREQUEST_PULLREQUESTID) {
        Write-Verbose "Not Part of a PR"
    }
    else {
        Write-Verbose "Getting PR Labels"

        try {
            $uri = "$($Env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$Env:SYSTEM_TEAMPROJECTID/_apis/git/repositories/$($Env:BUILD_REPOSITORY_NAME)/pullRequests/$($Env:SYSTEM_PULLREQUEST_PULLREQUESTID)/labels?api-version=7.1-preview.1"
            Write-Verbose "Constructed URL: $uri"

            $pr = Invoke-RestMethod -Uri $uri -Method GET -Headers @{Authorization = "Bearer $SystemAccessToken" } -ContentType application/json
            return $pr.value

        }
        catch {
            Write-Host "##vso[task.logissue type=error]$($_.Exception.Message)"
            Write-Error $_
        }
    }
}
