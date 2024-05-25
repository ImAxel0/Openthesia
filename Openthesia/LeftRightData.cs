using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vanara.PInvoke;

namespace Openthesia;

public class LeftRightData
{
    [XmlIgnore]
    public static List<bool> S_IsRightNote = new();

    [XmlArray("IsRightNote"), XmlArrayItem(typeof(bool))]
    public List<bool> IsRightNote = new();
}
