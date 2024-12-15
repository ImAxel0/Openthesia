using System.Xml.Serialization;

namespace Openthesia.Core;

public class LeftRightData
{
    [XmlIgnore]
    public static List<bool> S_IsRightNote = new();

    [XmlArray("IsRightNote"), XmlArrayItem(typeof(bool))]
    public List<bool> IsRightNote = new();
}
