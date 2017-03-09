using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("T")]
    public class ActiveTournamentSr
    {
        [XmlElement(ElementName = "F1")]
        public string Id { get; set; }

        [XmlElement(ElementName = "F2")]
        public string OddIncreaseDecrease { get; set; }

        [XmlElement(ElementName = "F3")]
        public string VisibleMarkets { get; set; }
    }
}
