
# Debugging
In case of issues with the eduroam WIFI network, 
- Open a Command box (cmd), with Run as administrator
- > netsh wlan show wlanreport

The wlanreport shows details of all (WIFI) networks

[More information](https://learn.microsoft.com/en-us/windows-server/networking/technologies/extensible-authentication-protocol/configure-eap-profiles?tabs=netsh-wifi%2Cpowershell-vpn%2Csettings-wifi%2Cgroup-policy-wifi)

[OneX xml - EAP Configuration](https://learn.microsoft.com/en-us/windows/win32/nativewifi/onexschema-schema)
[EAP Certificate filtering](https://learn.microsoft.com/en-us/windows/client-management/mdm/eap-configuration#eap-certificate-filtering)