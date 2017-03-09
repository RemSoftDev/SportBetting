using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("BR")]
    public class ResourceRepositorySR
    {
        [XmlElement(ElementName = "F1")]
        public long ResourceId { get; set; }
        [XmlElement(ElementName = "F2")]
        public string ResourceType { get; set; }
        [XmlElement(ElementName = "F3")]
        public string MimeType { get; set; }
        [XmlElement(ElementName = "F4")]
        public string Data { get; set; }
    }
}
