using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;
using SportRadar.Common.Windows;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.OldLineObjects
{
    public enum eServerSourceType
    {
        BtrPre = 0,
        BtrLive = 1,
        BtrVfl = 2,
        BtrVhc = 3
    }

    [XmlType("MatchSr")]
    public class MatchSr
    {
        public const int HOME_TEAM = 1;
        public const int AWAY_TEAM = 2;

        public const string MATCHRESULT_Team1Won = "1";
        public const string MATCHRESULT_Team2Won = "2";
        public const string MATCHRESULT_Draw = "X";
        public const string MATCHRESULT_CANCELED = "ABGESAGT";
        public const string MATCHRESULT_NO_RESULT = "KEIN RESULTAT";
        public const int MATCHRESULT_NO_SCORE = -10000;
        public const string MATCHRESULT_WON = "SIEG";
        public const string MATCHRESULT_PLACE = "PLATZ";
        public const string MATCHRESULT_LOST = "VERLOREN";
        public const string MATCHRESULT_UNDER = "U";
        public const string MATCHRESULT_OVER = "O";

        public const string TIPRESULT_Team1Won = MATCHRESULT_Team1Won;
        public const string TIPRESULT_Team2Won = MATCHRESULT_Team2Won;
        public const string TIPRESULT_Draw = MATCHRESULT_Draw;
        public const string TIPRESULT_Team1WonOrDraw = "1X";
        public const string TIPRESULT_Team1WonOrTeam2Won = "12";
        public const string TIPRESULT_DrawOrTeam2Won = "X2";

        public const int MATCH_STATE_CREATED = 1;               //Imported from BetRadar
        public const int MATCH_STATE_CREATIONACCEPTED = 2;		//Admin user accepted this match and its odds
        public const int MATCH_STATE_ACTIVE = 3;				//Bets can be placed on this match
        public const int MATCH_STATE_CANCELED = 4;				//all Odds are set to 1 and won
        public const int MATCH_STATE_OPEN = 5;					//Match ended but not calculated yet
        public const int MATCH_STATE_CALCULATED = 6;			//All tips concerning this match are calculated
        public const int MATCH_STATE_CALCULATIONACCEPTED = 7;	//All tips concerning this match are calculated and accepted by Admin//Not used
        public const int MATCH_STATE_RESULTENTERED = 8;		    //match result is entered and save from Admin to database
        public const int MATCH_STATE_CALCULATION_RUNNING = 9;	//match calculation is running

        //Live-Bet NBT
        public const int MATCH_STATE_LIVEBET_REGISTERED = 11; //registered for odds from betradar
        public const int MATCH_STATE_LIVEBET_UNREGISTERED = 22; //unregistered to get no odds from betradar
        public const int MATCH_STATE_LIVEBET_SELECTED = 13; //manually selected, not registered yet
        public const int MATCH_STATE_LIVEBET_DESELECTED = 14; //deselected by NbtUser, not unregistered yet
        public const int MATCH_STATE_LIVEBET_STARTED = 15;
        public const int MATCH_STATE_LIVEBET_STOPPED = 16;
        public const int MATCH_STATE_LIVEBET_ENDED = 17; //match has ended
        //GMA 01.03.2010
        public const int MATCH_STATE_LIVEBET_UNCHANGED = 18; //new imported match by Livebet
        public const int MATCH_STATE_LIVEBET_NOT_STARTED = 19; //new imported match by Livebet

        ///Live-Bet Period Info Common: 
        public const int LB_PERIOD_INFO_INVALID = 0; //This is a sportBet
        public const int LB_PERIOD_INFO_NOT_STARTED = 1; //match has not started yet
        public const int LB_PERIOD_INFO_STARTED = 2; //match has not started yet
        public const int LB_PERIOD_INFO_PAUSED = 3; // Match Pause
        public const int LB_PERIOD_INFO_STOP = 4; // BetStop received
        public const int LB_PERIOD_INFO_INTERRUPTED = 5;
        public const int LB_PERIOD_INFO_PENALTY = 40; // penalty
        public const int LB_PERIOD_INFO_OT = 39; // OverTime

        ///Live-Bet Period Info for Soccer
        public const int LB_PERIOD_INFO_SOCCER_1P = 10; // first period
        public const int LB_PERIOD_INFO_SOCCER_2P = 20; // second period
        public const int LB_PERIOD_INFO_SOCCER_1P_OT = 11; // OverTime first period
        public const int LB_PERIOD_INFO_SOCCER_2P_OT = 22; // OverTime second period

        ///Live-Bet Period Info for Tennis
        public const int LB_PERIOD_INFO_TENNIS_1SET = 31; //1st set
        public const int LB_PERIOD_INFO_TENNIS_2SET = 32; //2nd set
        public const int LB_PERIOD_INFO_TENNIS_3SET = 33; //3rd set
        public const int LB_PERIOD_INFO_TENNIS_4SET = 34; //4th set
        public const int LB_PERIOD_INFO_TENNIS_5SET = 35; //5th set
        public const int LB_PERIOD_INFO_TENNIS_WALKOVER = 36; //won on walkover
        public const int LB_PERIOD_INFO_TENNIS_RETIRED = 37; //ended because a player has retired
        public const int LB_PERIOD_INFO_TENNIS_DELAYED = 38; //start delayed

        //Live-Bet Period Info for Ice Hockey
        public const int LB_PERIOD_INFO_IH_1T = 41; // First third
        public const int LB_PERIOD_INFO_IH_2T = 42; // First third
        public const int LB_PERIOD_INFO_IH_3T = 43; // Third third

        //Live-Bet Period Info for Basketball
        public const int LB_PERIOD_INFO_BASKET_PAUSE1 = 52; // First period pause
        public const int LB_PERIOD_INFO_BASKET_PAUSE2 = 54; // Secound period pause
        public const int LB_PERIOD_INFO_BASKET_PAUSE3 = 56; // Third period pause
        public const int LB_PERIOD_INFO_BASKET_1P = 50; // first period
        public const int LB_PERIOD_INFO_BASKET_2P = 51; // secound period
        public const int LB_PERIOD_INFO_BASKET_3P = 55; // Third period
        public const int LB_PERIOD_INFO_BASKET_4P = 57; // fourth period
        public const int LB_PERIOD_INFO_BASKET_OT = 58; // overtime

        //5P
        public const int LB_PERIOD_INFO_5P = 60; // fived period

        public const int ERROR_MATCH_CODE = -1;
        public const int MATCH_CODE_FACTOR = 100000;

        [XmlElement(ElementName = "m1")]
        public long MatchID { get; set; }

        [XmlElement(ElementName = "m3")]
        public bool ActivateAfterDBSync { get; set; }

        [XmlElement(ElementName = "m4")]
        public System.DateTime StartDate { get; set; }
        [XmlElement(ElementName = "m5")]
        public System.DateTime? EndDate { get; set; }
        [XmlElement(ElementName = "m6")]
        public System.DateTime? ExpiryDate { get; set; }


        [XmlElement(ElementName = "m104", IsNullable = true)]
        public DateTimeSr StartDateOffset { get; set; }
        [XmlElement(ElementName = "m105", IsNullable = true)]
        public DateTimeSr EndDateOffset { get; set; }
        [XmlElement(ElementName = "m106", IsNullable = true)]
        public DateTimeSr ExpiryDateOffset { get; set; }

        [XmlElement(ElementName = "m7")]
        public int? MinCombination { get; set; }
        [XmlElement(ElementName = "m8")]
        public string TeamWon { get; set; }
        [XmlElement(ElementName = "m9")]
        public int? PointsTeamHome { get; set; }
        [XmlElement(ElementName = "m10")]
        public int? PointsTeamAway { get; set; }
        [XmlElement(ElementName = "m11")]
        public int? PointsTeamHomeHalf1 { get; set; }
        [XmlElement(ElementName = "m12")]
        public int? PointsTeamAwayHalf1 { get; set; }
        [XmlElement(ElementName = "m13")]
        public long? BtrMatchID { get; set; }
        [XmlElement(ElementName = "m14")]
        public int State { get; set; }
        [XmlElement(ElementName = "m15")]
        public long? BetDomainID { get; set; }
        [XmlElement(ElementName = "m16")]
        public long? TournamentID { get; set; }
        [XmlElement(ElementName = "m17")]
        public bool Active { get; set; }
        [XmlElement(ElementName = "m18")]
        public int WeekOfYear { get; set; }
        [XmlElement(ElementName = "m19")]
        public System.DateTime LastModified { get; set; }
        [XmlElement(ElementName = "m20")]
        public bool IsLiveBet { get; set; }
        [XmlElement(ElementName = "m21")]
        public int MatchMinute { get; set; }
        [XmlElement(ElementName = "m22")]
        public int LB_PeriodInfo { get; set; }
        [XmlElement(ElementName = "m23")]
        public int LB_SportType { get; set; }
        [XmlElement(ElementName = "m24")]
        public int LiveBetStatus { get; set; }
        [XmlElement(ElementName = "m25")]
        public string Info { get; set; }
        [XmlElement(ElementName = "m26")]
        public int CardsTeam1 { get; set; }
        [XmlElement(ElementName = "m27")]
        public int CardsTeam2 { get; set; }
        [XmlElement(ElementName = "m28")]
        public int Sort { get; set; }
        [XmlElement(ElementName = "m29")]
        public string Code_External { get; set; }
        [XmlElement(ElementName = "m30")]
        public string MatchIcon { get; set; }
        [XmlElement(ElementName = "m31")]
        public string MatchScore { get; set; }
        [XmlElement(ElementName = "m32", IsNullable = true)]
        public int? SourceType { get; set; }
        [XmlElement(ElementName = "m33", IsNullable = true)]
        public int? OutrightType { get; set; }
        [XmlElement(ElementName = "m34", IsNullable = true)]
        public int? InfoMultiStringId{ get; set; }

        [XmlIgnore]
        public eServerSourceType ServerSourceType
        {
            get
            {
                try
                {
                    if (SourceType == null)
                        return eServerSourceType.BtrPre;
                    return (eServerSourceType)this.SourceType;
                }
                catch
                {
                    return eServerSourceType.BtrPre;
                }
            }
            set
            {
                this.SourceType = (int)value;
            }
        }

        [XmlIgnore]
        public int Code
        {
            get
            {
                return (int)(this.MatchID % MATCH_CODE_FACTOR);
            }
        }

        [XmlIgnore]
        public eMatchStatus LiveBetStatusEx
        {
            get
            {
                try
                {
                    return this.LiveBetStatus == null ? eMatchStatus.Undefined : (eMatchStatus)this.LiveBetStatus;
                }
                catch
                {
                    return eMatchStatus.Undefined;
                }
            }
        }

        public string FormatResult(string sSportId)
        {
            if (sSportId == SportSr.SPORT_DESCRIPTOR_SOCCER) //soccer
            {
                return string.Format("{0}:{1} ({2}:{3})", this.PointsTeamHome, this.PointsTeamAway, this.PointsTeamHomeHalf1, this.PointsTeamAwayHalf1);
            }

            return string.Format("{0}:{1}", this.PointsTeamHome, this.PointsTeamAway);
        }

        public override string ToString()
        {
            return
                string.Format(
                    "MatchSr {{MatchId = {0}, BtrMatchId = {1}, IsLiveBet = {2}, Active={3}, StartDate = {4}, ExpiryDate = {5}, LastModified = {6}}}",
                    this.MatchID,
                    this.BtrMatchID,
                    this.IsLiveBet,
                    this.Active,
                    this.StartDate,
                    this.ExpiryDate,
                    this.LastModified);
        }
    }
}