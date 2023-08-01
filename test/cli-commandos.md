Voor een overzicht van beschikbare commando's
> geteduroam-cli -?

# [Debug in Visual Studio](../EduRoam.CLI/Properties/launchSettings.json)
-> "commandLineArgs"

"status"
"list"
"list -i \"Moreelsepark College\""
"show -i \"Moreelsepark College\" -p \"Mijn Moreelsepark\""
"show -i \"uninett\" -p Ansatt"
"show -i \"eduroam USA\" -p \"eduroam USA\""
"configure -i \"Moreelsepark College\" -p \"Mijn Moreelsepark\""
"configure -i \"uninett\" -p Ansatt"
"configure -i \"uninett\" -p \"geteduroam (sertifikat)\""
"configure -i \"eduroam USA\" -p \"eduroam USA\" \cp \"C:\\Temp\\geteduroam\\geteduroam-test-cert.pfx\""
"connect"
"uninstall"


cert + passphrase => eduroam USA