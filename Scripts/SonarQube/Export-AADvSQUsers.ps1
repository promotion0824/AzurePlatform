Param
(
    [string]
    $inFile = "sqDbusers.csv",

    [string]
    $outFile = "combined.csv"
)

$VerbosePreference = "Continue"

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

$usersOut = New-Object System.Collections.ArrayList
$FileContent = Get-Content -Path $inFile

$sqUsers = ConvertFrom-Csv $FileContent


foreach ($user in $sqUsers) {
    if ($user.active){
        $aadUser = Exec { az ad user list --upn $user.external_login -o json } | ConvertFrom-Json

        if ($aadUser.Count -ne 0){
            $usersOut.AddRange(@(@{
                Name=$aadUser.displayName
                ExternalIdSq=$user.external_login
                AADUPN=$aadUser.userPrincipalName
                AADEmail=$aadUser.sipProxyAddress
                aadEnabled=$aadUser.accountEnabled
                AADUPNMatchesCase=$($aadUser.userPrincipalName -cne $user.external_login)
            }))
        }
    }

}
$usersOut
$usersOut | ForEach-Object {  New-Object PSObject -Property $_ } | Export-Csv $outFile -NoTypeInformation
