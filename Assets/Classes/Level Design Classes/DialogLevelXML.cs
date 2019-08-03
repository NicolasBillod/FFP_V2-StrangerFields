using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot("Dialogs")]
public class DialogLevelXML
{
    public List<DialogClassXML> dialogsList = new List<DialogClassXML>();

    private DialogLevelXML() { }

    public static DialogLevelXML LoadFromFile(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(DialogLevelXML));
        using (FileStream stream = new FileStream(filePath, FileMode.Open))
        {
            return serializer.Deserialize(stream) as DialogLevelXML;
        }
    }
}
