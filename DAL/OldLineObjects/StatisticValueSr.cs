using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

namespace SportRadar.DAL.OldLineObjects
{
    public class StatisticValueSr
    {
        [XmlAttribute(AttributeName = "n")]
        public string Name { get; set; }
        [XmlText()]
        public string Value { get; set; }

    }
}