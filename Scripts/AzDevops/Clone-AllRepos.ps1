Param
(
    [string]
    $userName,

    [string]
    $pat,

    [string]
    $org = "willowdev",

    [string]
    $exportDirectory = "Export"
)
function Exec
{
	[CmdletBinding()]
	param(
		[Parameter(Position=0,Mandatory=1)][scriptblock]$cmd
	)
	& $cmd
	if ($lastexitcode -ne 0) {
		throw "Exec: $cmd failed"
	}
}

function CreateDirectory
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory = $True)]
        [String] $DirectoryToCreate)

    if (-not (Test-Path -LiteralPath $DirectoryToCreate)) {

        try {
            New-Item -Path $DirectoryToCreate -ItemType Directory -ErrorAction Stop | Out-Null #-Force
        }
        catch {
            Write-Error -Message "Unable to create directory '$DirectoryToCreate'. Error was: $_" -ErrorAction Stop
        }
        "Successfully created directory '$DirectoryToCreate'."

    }
    else {
        throw "Directory already exists $DirectoryToCreate"
    }
}

$url = "https://dev.azure.com/$org"

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $userName,$pat)))
$headers = @{
    "Authorization" = "Basic $base64AuthInfo"
    "Accept" = "application/json"
}
$failedClones = New-Object System.Collections.ArrayList

$projectsResponse = Invoke-WebRequest -Headers $headers -Uri "$url/_apis/projects?api-version=6.0" -UseBasicParsing

$projects = $projectsResponse.Content | ConvertFrom-Json

CreateDirectory -DirectoryToCreate $exportDirectory

foreach ($project in $projects.value) {
    CreateDirectory -DirectoryToCreate "$exportDirectory/$($project.name)"
    $reposResponse = Invoke-WebRequest -Headers $headers -Uri "$url/$($project.name)/_apis/git/repositories?api-version=6.0" -UseBasicParsing
    $repos = $reposResponse.Content | ConvertFrom-Json

    foreach ($repo in $repos.value){
        try {
            Exec { git clone --mirror -c http.extraHeader="Authorization: Basic $base64AuthInfo" $repo.remoteUrl "$exportDirectory/$($project.name)/$($repo.name)" }
        }
        catch {
            $failedClones.AddRange(@($repo))
        }
    }
}

if ($failedClones.Count -ne 0) {
    "Some repositories failed to clone"
    $failedClones
    throw "Several repositories failed to clone"

}
