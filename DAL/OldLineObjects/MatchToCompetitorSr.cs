using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("MatchToCompetitorSr")]
    public class MatchToCompetitorSr : DatabaseBase
    {
        [XmlElement(ElementName = "m1")]
        public long MatchToCompetitorID { get; set; }
        [XmlElement(ElementName = "m2")]
        public int HomeTeam { get; set; }
        [XmlElement(ElementName = "m3")]
        public long CompetitorID { get; set; }
        [XmlElement(ElementName = "m4")]
        public long MatchID { get; set; }
        [XmlElement(ElementName = "m5")]
        public bool IsLiveBet { get; set; }

        public override void FillFromDataRow(DataRow dr)
        {
            throw new System.NotImplementedException();
        }

        [XmlIgnore]
        public override long ORMID { get { return this.MatchToCompetitorID; } }
    }
}