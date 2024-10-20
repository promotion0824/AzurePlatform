

Param
(
    [string]
    $TenantId = "a80618f8-f5e9-43bf-a98f-107dd8f54aa9",

    [string]
    $ClientID = "871e1da2-3955-4936-b8a7-5d556bae68d1",

    [string]
    $CertificatePath = "$PWD/b2c_dev.pfx",

    [SecureString]
    [Parameter(Mandatory = $true)]
    $CertificatePassword
)

$certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($CertificatePath, $CertificatePassword)
Connect-MgGraph -ClientID $ClientID  -TenantId $TenantId -Certificate $certificate -ForceRefresh
