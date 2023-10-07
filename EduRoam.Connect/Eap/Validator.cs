using System.IO;
using System.Xml;


namespace EduRoam.Connect.Eap
{
    internal class Validator
    {

        public static bool ValidateXml(string xmlContent, Stream xsdContent)
        {
            var isValid = true;

            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.CheckCharacters = true;
            settings.Schemas.Add(null, XmlReader.Create(xsdContent));
            settings.ValidationEventHandler += (sender, e) =>
            {
                isValid = false;
            };



            using (var reader = XmlReader.Create(new StringReader(xmlContent), settings))
            {
                while (reader.Read()) { }
            }

            return isValid;
        }

    }
}
