# Adapted FROM https://codez.deedx.cz/posts/update-redirect-uris-from-azure-devops/
Param
(
    [string]
    $CertificateName = "b2c_dev",

    [string]
    $ExportDirectoryPath = "./"
)


# Based on: https://devblogs.microsoft.com/scripting/generating-a-new-password-with-windows-powershell/
function Get-RandomPassword {
    param(
      [int] $Length
    )

    # Selection of PowerShell/ADO-safe characters for the password.
    $ascii = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz".ToCharArray()

    for ($loop = 1; $loop –le $length; $loop++) {
      $TempPassword += ($ascii | GET-RANDOM)
    }

    return $TempPassword
  }

# Remove spaces from the certificate name and also all file names. Not absolutely necessary, just to get safer values in the pipeline.
$CertificateName = $CertificateName.Replace(" ", "_")

$cerPath = Join-Path -Path $ExportDirectoryPath -ChildPath "$($CertificateName).cer"
$pfxPath = Join-Path -Path $ExportDirectoryPath -ChildPath "$($CertificateName).pfx"

# Certificate expires after 10 years.
$cert = New-SelfSignedCertificate `
  -Subject "CN=$CertificateName" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -KeyExportPolicy Exportable `
  -KeySpec Signature `
  -KeyLength 2048 `
  -KeyAlgorithm RSA `
  -HashAlgorithm SHA256 `
  -NotAfter (Get-Date).AddYears(10)

Export-Certificate `
  -Cert $cert `
  -FilePath $cerPath

# Get-RandomPassword is a custom function defined elsewhere.
$generatedPassword = (Get-RandomPassword -Length 16)
$mypwd = ConvertTo-SecureString -String $generatedPassword -Force -AsPlainText

Export-PfxCertificate `
  -Cert $cert `
  -FilePath $pfxPath `
  -Password $mypwd

Write-Output "Certificate Password: $generatedPassword"

# Cleanup local store.
$certInstore = Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object { $_.Subject -Match $CertificateName } | Select-Object Thumbprint, FriendlyName
Remove-Item -Path Cert:\CurrentUser\My\$($certInstore.Thumbprint) -DeleteKey
