using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("BetDomainSr")]
    public class BetDomainSr
    {
        public const string LIVEBET_DISCRIMINATOR_PREFIX = "LB_";
        public const string DISCRIMINATOR_BASE_BET_DOMAIN = "BaseBetDomain";
        public const string DISCRIMINATOR_THREE_WAY_BET_DOMAIN = "ThreeWayBaseBet";
        public const string DISCRIMINATOR_DOUBLE_CHANCE_BET_DOMAIN = "DoubleChanceBetDomain";
        public const string DISCRIMINATOR_UNDER_OVER_BET_DOMAIN = "UnderOverBetDomain";
        public const string DISCRIMINATOR_WINNER_REST_OF_MATCH = "WinnerRestOfMatchBetDomain";
        public const string DISCRIMINATOR_NEXT_GOAL = "NextGoalBetDomain";
        public const string DISCRIMINATOR_WHO_WINS_TENNIS_MATCH = "LiveTennisMatchBetDomain";
        public const string DISCRIMINATOR_WINNER_REST_OF_MATCH_OT = "WinnerRestOfMatch_OT_BetDomain";
        public const string DISCRIMINATOR_LIVE_WHO_WINS_THE_SET = "LiveTennisSetBetDomain";
        //        public const string DISCRIMINATOR_UNDER_OVER_HT = "UnderOverHTBetDomain";

        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainSr));

        public const int STATUS_VISIBLE = 0;
        //ist ausgeblendet
        public const int STATUS_HIDDEN = 1; //now used for BetDomains which should be enabled again, as soon as LiveBetServer is connected again GMU 2010-11-24

        //ist vom System ausgeblendet - darf nicht angezeigt werden
        public const int STATUS_FORBIDDEN = 2;//used to remove betDomains if 

        //inactive, but still displayed in gray
        public const int STATUS_INACTIVE = 3;//Used to disable matches on BetStop

        //System should activate LiveBet as soon as possible
        public const int STATUS_ACTIVATE_ASAP = 4;//not used GMU 2010-05-10

        //System should deactivate LiveBet as soon as possible
        public const int STATUS_DEACTIVATE_ASAP = 5;//not used GMU 2010-05-10

        public const int STATUS_RESULT_ENTERED = 8;

        public const int STATUS_CALCULATED = 6;
        public const int STATUS_CALCULATED_ERROR = 7;

        private MatchSr m_ParentMatch = null;

        private List<OddSr> m_lOdds = new List<OddSr>();

        public BetDomainSr()
        {
            this.BetDomainNumber = -1;
            this.Sort = -1;
        }

        public BetDomainSr(MatchSr match)
        {
            m_ParentMatch = match;
        }

        [XmlElement(ElementName = "m1")]
        public long BetDomainID { get; set; }
        [XmlElement(ElementName = "m4")]
        public int Status { get; set; }
        [XmlElement(ElementName = "m5")]
        public int BetDomainNumber { get; set; }
        [XmlElement(ElementName = "m6")]
        public string Discriminator { get; set; }
        [XmlElement(ElementName = "m7", IsNullable = true)]
        public long? MultiStringID { get; set; }
        [XmlElement(ElementName = "m8", IsNullable = true)]
        public long? MultiStringID2 { get; set; }
        [XmlElement(ElementName = "m9", IsNullable = true)]
        public int? ScoreFrequency { get; set; }
        [XmlElement(ElementName = "m10", IsNullable = true)]
        public long? MatchID { get; set; }
        [XmlElement(ElementName = "m11", IsNullable = true)]
        public int? Sort { get; set; }
        [XmlElement(ElementName = "m12", IsNullable = true)]
        public int? HomeHandicap { get; set; }
        [XmlElement(ElementName = "m13", IsNullable = true)]
        public int? AwayHandicap { get; set; }
        [XmlElement(ElementName = "m14")]
        public System.DateTime LastModified { get; set; }
        [XmlElement(ElementName = "m15", IsNullable = true)]
        public int? Set { get; set; }
        [XmlElement(ElementName = "m16", IsNullable = true)]
        public decimal? OverAllScore { get; set; }
        [XmlElement(ElementName = "m17")]
        public int MinCombination { get; set; }
        [XmlElement(ElementName = "m18", IsNullable = true)]
        public bool? Calculated { get; set; }
        [XmlElement(ElementName = "m19")]
        public bool IsLiveBet { get; set; }
        [XmlElement(ElementName = "m20", IsNullable = true)]
        public bool? IsManualLiveBetDomain { get; set; }
        [XmlElement(ElementName = "m21")]
        public string SpecialOddValue { get; set; }
        [XmlElement(ElementName = "m22")]
        public string SpecialLiveOddValue_Full { get; set; }
        [XmlElement(ElementName = "m23", IsNullable = true)]
        public string BetDomainNumber_External { get; set; }
        [XmlElement(ElementName = "m24")]
        public long BtrLiveBetID { get; set; }

        public static string GetStatusString(int iStatus)
        {
            switch (iStatus)
            {
                case STATUS_VISIBLE: return "VISIBLE";
                case STATUS_HIDDEN: return "HIDDEN";
                case STATUS_FORBIDDEN: return "FORBIDDEN";
                case STATUS_INACTIVE: return "INACTIVE";
                // case STATUS_ACTIVATE_ASAP: return "ACTIVATE_ASAP";
                // case STATUS_DEACTIVATE_ASAP: return "DEACTIVATE_ASAP";
                case STATUS_RESULT_ENTERED: return "RESULT_ENTERED";
                case STATUS_CALCULATED: return "CALCULATED";
                case STATUS_CALCULATED_ERROR: return "CALCULATED_ERROR";
            }

            return string.Format("Unknown ({0})", iStatus);
        }

        public override string ToString()
        {
            return string.Format("BetDomainSr {{BetDomainId={0}, BtrLiveBetID={1}, Discriminator='{2}', BetDomainNumber={3}, SpecialOddValue='{4}'}}",
                                 this.BetDomainID, this.BtrLiveBetID, this.Discriminator, this.BetDomainNumber, this.SpecialOddValue);
        }
    }
}