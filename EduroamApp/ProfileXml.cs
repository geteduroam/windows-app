using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EduroamApp
{
    class ProfileXml
    {           
        // Properties
        public string Ssid { get; set; }
        public string ProfileName { get; set; }        
        public List<string> Thumbprints { get; set; }
        public ProfileXml.Eap EapType { get; set; }
        public enum Eap
        {
            TLS = 13,
            PEAP_MSCHAPv2 = 25
        }

        // Namespaces
        private XNamespace nsWLAN = "http://www.microsoft.com/networking/WLAN/profile/v1";
        private XNamespace nsOneX = "http://www.microsoft.com/networking/OneX/v1";
        private XNamespace nsEHC = "http://www.microsoft.com/provisioning/EapHostConfig";
        private XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
        private XNamespace nsBECP = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
        // TLS specific
        private XNamespace nsETCPv1 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1";
        private XNamespace nsETCPv2 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV2";
        // MSCHAPv2 specific
        private XNamespace nsMPCPv1 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1";
        private XNamespace nsMPCPv2 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2";
        private XNamespace nsMPCPv3 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3";
        private XNamespace nsMCCP = "http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1";


        public string CreateProfileXml(string sName, string pName, int eapType, List<string> tPrints)
        {
            //XElement newProfile =
            //    new XElement();

            return "";
        }


        // takes in parameters as properties: ssid, profilename, EAP type, thumbprints

        // methods: depending on EAP type, build XML document

        /*
        // loads the XML file from its file path
        XDocument doc = XDocument.Load(xmlFile);

        // shortens namespaces from XML file for easier typing
        XNamespace ns1 = "http://www.microsoft.com/networking/WLAN/profile/v1";
        XNamespace ns2 = "http://www.microsoft.com/networking/OneX/v1";
        XNamespace ns3 = "http://www.microsoft.com/provisioning/EapHostConfig";
        XNamespace ns4 = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
        // namespace changes depending on EAP-type
        XNamespace ns5 = (cboMethod.SelectedIndex == 0 ? "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1" : "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1");

        // gets elements to edit
        XElement profileName = doc.Root.Element(ns1 + "name");

        XElement ssidName = doc.Root.Element(ns1 + "SSIDConfig")
                                .Element(ns1 + "SSID")
                                .Element(ns1 + "name");

        XElement serverValidationElement = doc.Root.Element(ns1 + "MSM")
                                 .Element(ns1 + "security")
                                 .Element(ns2 + "OneX")
                                 .Element(ns2 + "EAPConfig")
                                 .Element(ns3 + "EapHostConfig")
                                 .Element(ns3 + "Config")
                                 .Element(ns4 + "Eap")
                                 .Element(ns5 + "EapType")
                                 .Element(ns5 + "ServerValidation");
        //.Element(ns5 + "TrustedRootCA");

        // sets elements to desired values
        profileName.Value = ssid;
            ssidName.Value = ssid;
            
            foreach (string thumb in thumbprints)
            {
                serverValidationElement.Add(new XElement(ns5 + "TrustedRootCA", thumb));
            }

    // adds the xml declaration to the top of the document and converts it to string
    string wDeclaration = doc.Declaration.ToString() + Environment.NewLine + doc.ToString();
    */

}

}
