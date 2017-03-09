using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("CountrySr")]
    public class CountrySr
    {
        [XmlElement(ElementName = "m1")]
        public long CountryID { get; set; }
        [XmlElement(ElementName = "m2")]
        public string ISO2 { get; set; }
        [XmlElement(ElementName = "m3")]
        public string ISO3 { get; set; }
        [XmlElement(ElementName = "m4", IsNullable = true)]
        public long? MultiStringID { get; set; }
        [XmlElement(ElementName = "m5")]
        public string DefaultName { get; set; }
        [XmlElement(ElementName = "m6")]
        public System.DateTime LastModified { get; set; }
    }
}