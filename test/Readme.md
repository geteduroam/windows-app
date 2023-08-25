# Create test certificates

## non-exportable (server side) certificate
```powershell
$params = @{
    Subject = 'CN=geteduroam test server'
    KeyFriendlyName = 'geteduroam test server'
    CertStoreLocation = 'Cert:\CurrentUser\My'
	NotAfter = (Get-Date).AddMonths(1)
    KeyExportPolicy = 'NonExportable'
}
$serverCertificate = New-SelfSignedCertificate @params

Export-Certificate -Cert $serverCertificate -FilePath "geteduroam-test server.cer"
 
# certificate -> base 64 string, can be used in Eap configurations
$fileContentBytes = get-content 'geteduroam-test server.cer' -AsByteStream
[System.Convert]::ToBase64String($fileContentBytes) 
```

## exportable (client side) certificate
```powershell
$params = @{
    Subject = 'CN=geteduroam test client'
    KeyFriendlyName = 'geteduroam test client'
    CertStoreLocation = 'Cert:\CurrentUser\My'
	NotAfter = (Get-Date).AddMonths(2)
    KeyExportPolicy = 'Exportable'
}
$clientCertificate = New-SelfSignedCertificate @params

$mypwd = ConvertTo-SecureString -String '1VeryHardToRememberPassword:-)' -Force -AsPlainText 
Get-ChildItem -Path "Cert:\CurrentUser\My\$($clientCertificate.ThumbPrint)" | Export-PfxCertificate -FilePath geteduroam-test.pfx -Password $mypwd

# certificate -> base 64 string, can be used in Eap configurations
$fileContentBytes = get-content 'geteduroam-test.pfx' -AsByteStream
[System.Convert]::ToBase64String($fileContentBytes) 
```