using System.Xml.Serialization;

namespace Openthesia;

public class LeftRightData
{
    [XmlIgnore]
    public static List<bool> S_IsRightNote = new();

    [XmlArray("IsRightNote"), XmlArrayItem(typeof(bool))]
    public List<bool> IsRightNote = new();
}
