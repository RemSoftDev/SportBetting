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
    [XmlType("MatchInfo")]
    public class MatchInfoSr
    {
        [XmlElement(ElementName = "m1")]
        public long MatchInfoId { get; set; }
        [XmlArray("m2")]
        [XmlArrayItem("f")]
        public SyncList<StatisticValueSr> StatisticValues { get; set; }
        [XmlElement(ElementName = "m3", IsNullable = true)]
        public string LastModifiedString { get; set; }
    }
}