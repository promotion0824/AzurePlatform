param ([string] $appUser, [string] $sqlServer, [string]$sqlDatabase)

Install-Module -Name SqlServer -Force
Import-Module -Name SqlServer

$sqlScript = "IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name = '<placeholder-for-userid>')
BEGIN
      CREATE USER [<placeholder-for-userid>] FROM EXTERNAL PROVIDER
      ALTER ROLE db_datareader ADD MEMBER [<placeholder-for-userid>]
      ALTER ROLE db_datawriter ADD MEMBER [<placeholder-for-userid>]
      ALTER ROLE db_ddladmin ADD MEMBER [<placeholder-for-userid>]
END"

$updateScript = $sqlScript.Replace('<placeholder-for-userid>', '${{ parameters.appUser }}')

Write-Host "Server: " + $sqlServerConnection
Write-Host "Executing SQL script: $updateScript"

Invoke-Sqlcmd -ServerInstance $sqlServer -Database $sqlDatabase -Query $updateScript
