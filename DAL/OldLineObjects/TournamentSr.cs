using System;
using System.Data;
using System.Windows;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("TournamentSr")]
    public class TournamentSr : DatabaseBase
    {
        public const string MS_TAG_SOCCER = "TOURN_SOCCER_LIVE_TAG";
        public const string MS_TAG_TENNIS = "TOURN_TENNIS_LIVE_TAG";
        public const string MS_TAG_BASKETBALL = "TOURN_BASKETBALL_LIVE_TAG";
        public const string MS_TAG_ICE_HOCKEY = "TOURN_ICE_HOCKEY_LIVE_TAG";

        public const string LS_TEXT_SOCCER = "Live-Fußball";
        public const string LS_TEXT_TENNIS = "Live-Tennis";
        public const string LS_TEXT_BASKETBALL = "Live-Basketball";
        public const string LS_TEXT_ICE_HOCKEY = "Live-Ice-Hockey";

        public override void FillFromDataRow(DataRow dr)
        {
            this.TournamentID = DbConvert.ToInt64(dr, "TournamentID");
            this.SvrTournamentID = DbConvert.ToInt64(dr, "SvrTournamentID");
            this.DefaultName = DbConvert.ToString(dr, "DefaultName");
            this.BtrTournamentID = DbConvert.ToNullableInt64(dr, "BtrTournamentID");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.SportID = DbConvert.ToInt64(dr, "SportID");
            this.CategoryID = DbConvert.ToNullableInt64(dr, "CategoryID");
            this.MaxStakeLigaLimit = DbConvert.ToNullableDecimal(dr, "MaxStakeLigaLimit");
            this.MaxStakeTipLimit = DbConvert.ToNullableDecimal(dr, "MaxStakeTipLimit");
            this.MinCombination = DbConvert.ToNullableInt32(dr, "MinCombination");
            this.Active = DbConvert.ToBool(dr, "Active");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
            this.TennisSet = DbConvert.ToNullableInt32(dr, "TennisSets");
            this.ShowOnOddSheet = DbConvert.ToBool(dr, "ShowOnOddSheet");
            this.Info = DbConvert.ToString(dr, "Info");
            this.OutrightType = DbConvert.ToInt32(dr, "OutrightTyp");
            this.Sort = DbConvert.ToInt32(dr, "Sort");
            this.IsLiveBet = DbConvert.ToBool(dr, "IsLiveBet");
            this.IsLocked = DbConvert.ToBool(dr, "IsLocked");
            this.LockWithAllOtherTournaments = DbConvert.ToBool(dr, "LockWithAllOtherTournaments");
        }

        public static TournamentSr CreateFromDataRow(LineBase lb, DataRow dr)
        {
            TournamentSr ts = new TournamentSr();

            ts.FillFromDataRow(dr);

            return ts;
        }

        [XmlElement(ElementName = "m1")]
        public long TournamentID { get; set; }
        [XmlElement(ElementName = "m2")]
        public string DefaultName { get; set; }
        [XmlElement(ElementName = "m3")]
        public long? BtrTournamentID { get; set; }
        [XmlElement(ElementName = "m4")]
        public long MultiStringID { get; set; }
        [XmlElement(ElementName = "m5")]
        public long SportID { get; set; }
        [XmlElement(ElementName = "m6", IsNullable = true)]
        public long? CategoryID { get; set; }
        [XmlElement(ElementName = "m7")]
        public decimal? MaxStakeLigaLimit { get; set; }
        [XmlElement(ElementName = "m8")]
        public decimal? MaxStakeTipLimit { get; set; }
        [XmlElement(ElementName = "m9")]
        public int? MinCombination { get; set; }
        [XmlElement(ElementName = "m10")]
        public bool Active { get; set; }
        [XmlElement(ElementName = "m11")]
        public System.DateTime LastModified { get; set; }
        [XmlElement(ElementName = "m12")]
        public int? TennisSet { get; set; }
        [XmlElement(ElementName = "m13")]
        public bool ShowOnOddSheet { get; set; }
        [XmlElement(ElementName = "m14")]
        public string Info { get; set; }
        [XmlElement(ElementName = "m15")]
        public int OutrightType { get; set; }
        [XmlElement(ElementName = "m16")]
        public int Sort { get; set; }
        [XmlElement(ElementName = "m17")]
        public bool IsLiveBet { get; set; }
        [XmlElement(ElementName = "m18")]
        public bool IsLocked { get; set; }
        [XmlElement(ElementName = "m19")]
        public bool LockWithAllOtherTournaments { get; set; }
        [XmlElement(ElementName = "m20", IsNullable = true)]
        public long? CountryID { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.TournamentID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrTournamentID; } set { this.SvrTournamentID = value; } }
        [XmlIgnore]
        public long SvrTournamentID { get; set; }

        public override string ToString()
        {
            return string.Format("Tournament {{TournamentID = {0}, SvrTournamentID = {1}, Name = '{2}', IsLiveBet = {3}}}",
                                 this.TournamentID,
                                 this.SvrTournamentID,
                                 this.DefaultName,
                                 this.IsLiveBet);
        }
    }
}