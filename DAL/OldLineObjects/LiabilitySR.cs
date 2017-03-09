using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("CF")]
    public class LiabilitySR
    {
        [XmlElement(ElementName = "m1")]
        public string LiabilityID { get; set; }
        [XmlElement("m2")]
        public string LiabilityType { get; set; }
        [XmlElement(ElementName = "m3")]
        public string LiabilityValue { get; set; }
    }
}
