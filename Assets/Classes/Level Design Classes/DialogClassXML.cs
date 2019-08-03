using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;


public class DialogClassXML
{
    [XmlArrayItem("Dialog")]
    [XmlAttribute("speakerName")]
    public string speakerName;
    [XmlAttribute("expression")]
    public string expression;
    [XmlAttribute("dialogText")]
    public string dialogText;
}
