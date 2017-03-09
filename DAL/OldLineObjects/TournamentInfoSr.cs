using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("TournamentInfo")]
    public class TournamentInfoSr
    {
        [XmlElement(ElementName = "m1")]
        public long TournamentInfoId { get; set; }
        [XmlElement(ElementName = "m2")]
        public SportRadarLineContainer CompetitorInfoCollections { get; set; }
        [XmlElement(ElementName = "m3", IsNullable = true)]
        public string LastModifiedString { get; set; }
    }
}