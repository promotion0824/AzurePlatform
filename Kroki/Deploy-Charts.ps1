Param
(
)
$ErrorActionPreference = "Stop"

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
Exec { kubectl apply -f "$PSScriptRoot/namespaces.yaml" }

Exec { helm upgrade --install kroki --namespace kroki "$PSScriptRoot/charts/kroki" }
