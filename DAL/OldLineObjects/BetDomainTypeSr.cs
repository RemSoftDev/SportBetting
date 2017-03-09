using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("BetDomainTypeLn")]
    public class BetDomainTypeSr
    {
        [XmlElement(ElementName = "tag")]
        public string Tag { get; set; }
        [XmlElement(ElementName = "mappingcode")]
        public string MappingCode { get; set; }
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "bettypetag")]
        public string BetTypeTag { get; set; }
        [XmlElement(ElementName = "scoretypetag")]
        public string ScoreTypeTag { get; set; }
        [XmlElement(ElementName = "timetypetag")]
        public string TimeTypeTag { get; set; }
        [XmlElement(ElementName = "sort")]
        public int Sort { get; set; }
        [XmlElement(ElementName = "active")]
        public bool Active { get; set; }
        [XmlElement(ElementName = "externalstate")]
        public string ExternalState { get; set; }
    }
}
