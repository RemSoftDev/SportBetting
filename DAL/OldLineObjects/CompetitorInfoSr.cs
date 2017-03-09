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
    [XmlType("CompetitorInfo")]
    public class CompetitorInfoSr
    {
        [XmlElement(ElementName = "m1")]
        public long CompetitorInfoId { get; set; }
        [XmlArray("m2")]
        [XmlArrayItem("f")]
        public SyncList<StatisticValueSr> StatisticValues { get; set; }
        [XmlElement(ElementName = "m3")]
        public string TshirtHome { get; set; }
        [XmlElement(ElementName = "m4")]
        public string TshirtAway { get; set; }
        [XmlElement(ElementName = "m5", IsNullable = true)]
        public string LastModifiedString { get; set; }
    }
}