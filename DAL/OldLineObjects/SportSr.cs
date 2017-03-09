using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    public enum eLiveBetSportType
    {
        SPORT_TYPE_NONE = 0,
        SPORT_TYPE_NOT_SUPPORTED = 1,
        SPORT_TYPE_SOCCER = 2,
        SPORT_TYPE_BASKETBALL = 3,
        SPORT_TYPE_ICE_HOCKEY = 4,
        SPORT_TYPE_TENNIS = 5,
    }

    [XmlType("SportSr")]
    public class SportSr : DatabaseBase
    {
        public const string SOCCER_DEFAULTNAME = "Soccer";
        public const string TENNIS_DEFAULTNAME = "Tennis";
        public const string BASKETBALL_DEFAULTNAME = "Basketball";
        public const string ICE_HOCKEY_DEFAULTNAME = "Ice Hockey";

        public const string SOCCER_DEFAULTICON = "soccer";
        public const string TENNIS_DEFAULTICON = "tennis";
        public const string BASKETBALL_DEFAULTICON = "basketball";
        public const string ICE_HOCKEY_DEFAULTICON = "icehockey";

        public const string SOCCER_SPRT_MST = "SOCCER_SPRT_MST";
        public const string BASKETBALL_SPRT_MST = "BASKETBALL_SPRT_MST";
        public const string ICE_HOCKEY_SPRT_MST = "ICE_HOCKEY_SPRT_MST";
        public const string TENNIS_SPRT_MST = "TENNIS_SPRT_MST";
        public const string RUGBY_SPRT_MST = "RUGBY_SPRT_MST";
        public const string VOLLEYBALL_SPRT_MST = "VOLLEYBALL_SPRT_MST";

        public const int SOCCER_SORT = 1;
        public const int TENNIS_SORT = 2;
        public const int BASKETBALL_SORT = 3;
        public const int ICE_HOCKEY_SORT = 4;
        public const int RUGBY_SORT = 5;
        public const int VOLLEYBALL_SORT = 6;

        public const string SPORT_DESCRIPTOR_SOCCER       = "SPRT_SOCCER";
        public const string SPORT_DESCRIPTOR_BASKETBALL   = "SPRT_BASKETBALL";
        public const string SPORT_DESCRIPTOR_BASEBALL     = "SPRT_BASEBALL";
        public const string SPORT_DESCRIPTOR_MIXED        = "SPRT_MIXED";
        public const string SPORT_DESCRIPTOR_ICE_HOCKEY   = "SPRT_ICE_HOCKEY";
        public const string SPORT_DESCRIPTOR_TENNIS       = "SPRT_TENNIS";
        public const string SPORT_DESCRIPTOR_RUGBY        = "SPRT_RUGBY";
        public const string SPORT_DESCRIPTOR_HANDBALL     = "SPRT_HANDBALL";
        public const string SPORT_DESCRIPTOR_VOLLEYBALL   = "SPRT_VOLLEYBALL";
        public const string SPORT_DESCRIPTOR_FOOTBALL     = "SPRT_FOOTBALL";
        public const string SPORT_DESCRIPTOR_MOTOSPORT    = "SPRT_MOTOR_SPORT";
        public const string SPORT_DESCRIPTOR_WINTERSPORTS = "SPRT_WINTER_SPORTS";//"SPRT_WINTER_SPORTS"
        public const string SPORT_DESCRIPTOR_DARTS        = "SPRT_DARTS";
        public const string SPORT_DESCRIPTOR_OLYMPICS     = "SPRT_OLYMPICS";
        public const string SPORT_DESCRIPTOR_SNOOKER      = "SPRT_SNOOKER";
        public const string ALL_SPORTS                    = "ALL_SPORTS";
        
        public override void FillFromDataRow(DataRow dr)
        {
            this.SportID = DbConvert.ToInt64(dr, "SportID");
            this.SvrSportID = DbConvert.ToInt64(dr, "SvrSportID");
            this.BtrSportID = DbConvert.ToNullableInt64(dr, "BtrSportID");
            this.DefaultName = DbConvert.ToString(dr, "DefaultName");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
        }

        public static SportSr CreateFromDataRow(LineBase lb, DataRow dr)
        {
            SportSr sport = new SportSr();

            sport.FillFromDataRow(dr);

            return sport;
        }

        [XmlElement(ElementName = "m1")]
        public long SportID { get; set; }
        [XmlElement(ElementName = "m2")]
        public long? BtrSportID { get; set; }
        [XmlElement(ElementName = "m3")]
        public string DefaultName { get; set; }
        [XmlElement(ElementName = "m4")]
        public long MultiStringID { get; set; }
        [XmlElement(ElementName = "m5")]
        public System.DateTime LastModified { get; set; }
        [XmlElement(ElementName = "m6", IsNullable = true)]
        public string Tag { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.SportID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrSportID; } set { this.SvrSportID = value; } }
        [XmlIgnore]
        public long SvrSportID { get; set; }

        /*
        // UI Props & Methods
        public static string GetMultiStringTagBySportType(int iSportType)
        {
            switch (iSportType)
            {
                case SPORT_TYPE_SOCCER: return SOCCER_SPRT_MST;
                case SPORT_TYPE_ICE_HOCKEY: return ICE_HOCKEY_SPRT_MST;
                case SPORT_TYPE_BASKETBALL: return BASKETBALL_SPRT_MST;
                case SPORT_TYPE_TENNIS: return TENNIS_SPRT_MST;
            }

            return string.Empty;
        }
        */
    }
}