
# Debugging
In case of issues with the eduroam WIFI network, 
- Open a Command box (cmd), with Run as administrator
- > netsh wlan show wlanreport

The wlanreport shows details of all (WIFI) networks

# Further reading
- [802.1X Authenticated Wireless Deployment Guide](https://learn.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/dd283093(v=ws.10))
- [Configure EAP profiles and settings in Windows](https://learn.microsoft.com/en-us/windows-server/networking/technologies/extensible-authentication-protocol/configure-eap-profiles?tabs=netsh-wifi%2Cpowershell-vpn%2Csettings-wifi%2Cgroup-policy-wifi)
- [WLAN Profile schema](https://learn.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-elements)
  - [OneX xml - EAP Configuration](https://learn.microsoft.com/en-us/windows/win32/nativewifi/onexschema-schema)
- [EAP Certificate filtering](https://learn.microsoft.com/en-us/windows/client-management/mdm/eap-configuration#eap-certificate-filtering)
- [eaphostusercredentials Schema](https://learn.microsoft.com/en-us/windows/win32/eaphost/eaphostusercredentialsschema-schema)