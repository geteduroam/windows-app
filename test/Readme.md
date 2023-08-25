# Test Eap configurations
This folder contains 3 Eap configuration templates. These templates can be used to test the following connection flows
- User has to enter username/password to connect (template - credentials.eap-config)
- User has to enter a certificate password (template - passphrase.eap-config)
- User has to select a client certificate (*.pfx) and the certificate's password (template - clientcertificate and passphrase.eap-config)

The templates contains certificates in base64 format. Self signed certificates can be used for testing purposes. Below the Powershell commands to create server/client side certificates.


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