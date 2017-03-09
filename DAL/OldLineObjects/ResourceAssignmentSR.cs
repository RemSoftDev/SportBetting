using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("BRA")]
    public class ResourceAssignmentSr
    {
        [XmlElement(ElementName = "F1")]
        public long ResourceId { get; set; }
        [XmlElement(ElementName = "F2")]
        public string ObjectClass { get; set; }
        [XmlElement(ElementName = "F3")]
        public long ObjectId { get; set; }
        [XmlElement(ElementName = "F4")]
        public bool IsActive { get; set; }
    }
}
