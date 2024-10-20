# Scripts to work with B2C

Requires teh Microsoft.Graph module to be installed

```powershell
      if (Get-Module -ListAvailable -Name Microsoft.Graph) {
        Write-Host "*** Module Microsoft.Graph exists."
      }
      else {
        Write-Host "*** Installing Microsoft.Graph module..."
        Install-Module Microsoft.Graph -AllowClobber -Force
      }
```

# Add-Redirects
Requires an app registration in the B2C directory to be created.

Permissions required are Application ReadWrite to owned application and then to run the AddSpToAppOwner for the app you want to manage.

Auth via certificate can be generated via Generate-Cert connect with Connect-B2c
