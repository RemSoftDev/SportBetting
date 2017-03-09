using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{

    public enum eLivePeriodInfo
    {
        ///Live-Bet Period Info Common:
        Undefined = 0,
        NotStarted = 1, //match has not started yet
        Started = 2, // match started
        Paused = 3, // Match Pause
        Stopped = 4, // BetStop received
        Interrupted = 5,
        Penalty = 40, // Penalty
        OverTime = 39, // OverTime

        ///Live-Bet Period Info for Soccer
        Soccer_1st_Period = 10, // first period
        Soccer_2nd_Period = 20, // second period
        Soccer_1st_PeriodOverTime = 11, // OverTime first period
        Soccer_2nd_PeriodOverTime = 22, // OverTime second period

        ///Live-Bet Period Info for Tennis
        Tennis_1st_Set = 31, //1st set
        Tennis_2nd_Set = 32, //2nd set
        Tennis_3rd_Set = 33, //3rd set
        Tennis_4th_Set = 34, //4th set
        Tennis_5th_Set = 35, //5th set
        Tennis_WALKOVER = 36, //won on walkover
        Tennis_RETIRED = 37, //ended because a player has retired
        Tennis_DELAYED = 38, //start delayed

        //Live-Bet Period Info for Ice Hockey
        IceHockey_1st_Third = 41, // First third
        IceHockey_2nd_Third = 42, // Second third
        IceHockey_3rd_Third = 43, // Third third

        //Live-Bet Period Info for Basketball
        Basketball_Pause1 = 52, // First period pause
        Basketball_Pause2 = 54, // Secound period pause
        Basketball_Pause3 = 56, // Third period pause
        Basketball_1st_Quarter = 50, // first period
        Basketball_2nd_Quarter = 51, // secound period
        Basketball_3rd_Quarter = 55, // Third period
        Basketball_4th_Quarter = 57, // fourth period
        Basketball_OverTime = 58, // overtime

        //5P
        FifthPeriod = 60, // fived period
    }


    public enum eMatchStatus
    {
        Undefined = 0,
        Started = 15,
        Stopped = 16,
        Ended = 17,
        NotStarted = 19,
        Hanged = 29,
    }

    public class LiveMatchInfoLn : ObjectBase, ILineObjectWithId<LiveMatchInfoLn>, IRemovableLineObject<LiveMatchInfoLn> 
    {
        public static readonly int STATUS_ENDED = (int) eMatchStatus.Ended;

        protected MatchLn m_match = null;

        private static ILog m_logger = LogFactory.CreateLog(typeof(LiveMatchInfoLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("LiveMatchInfo", false, "MatchId");

        public long Id { get { return this.MatchId; } }
        public long RemoveId { get { return this.MatchId; } }

        public LiveMatchInfoLn() : base(false)
        {
            this.ExpiryDate = new ObservableProperty<DateTimeSr>(this, m_lChangedProps, "ExpiryDate");
            this.MatchMinute = new ObservableProperty<int>(this, m_lChangedProps, "MatchMinute");
            this.PeriodInfo = new ObservableProperty<eLivePeriodInfo>(this, m_lChangedProps, "PeriodInfo");
            this.Status = new ObservableProperty<eMatchStatus>(this, m_lChangedProps, "Status");
            this.Score = new ObservableProperty<string>(this, m_lChangedProps, "Score");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["MatchId"] = this.MatchId;
            dr["ExpiryDate"] = this.ExpiryDate.Value.LocalDateTime;
            dr["UpdateId"] = this.UpdateId;
            dr["MatchMinute"] = this.MatchMinute.Value;
            dr["PeriodInfo"] = (int)this.PeriodInfo.Value;
            dr["SportType"] = 0;
            dr["Status"] = (int)this.Status.Value;
            dr["Score"] = this.Score.Value;

            return dr;
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.MatchId = DbConvert.ToInt64(dr, "MatchId");
            this.ExpiryDate.Value = DbConvert.ToDateTimeSr(dr, "ExpiryDate");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
            this.MatchMinute.Value = DbConvert.ToInt32(dr, "MatchMinute");
            this.PeriodInfo.Value = IntToPeriodInfo(DbConvert.ToInt32(dr, "PeriodInfo"));
            this.Status.Value = IntToMatchStatus(DbConvert.ToInt32(dr, "Status"));
            this.Score.Value = DbConvert.ToString(dr, "Score");
        }

        public long MatchId { get; set; }
        public ObservableProperty<DateTimeSr> ExpiryDate { get; set; }
        public ObservableProperty<int>  MatchMinute { get; set; }
        public ObservableProperty<eLivePeriodInfo> PeriodInfo { get; set; } // LB_PeriodInfo
        //public ObservableProperty<string> SportDescriptor { get; set; } 
        public ObservableProperty<eMatchStatus> Status { get; set; } // LiveBetStatus
        public ObservableProperty<string> Score { get; set; } // LiveBetStatus

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.LiveMatchInfos.ContainsKey(this.MatchId); }
        }

        public void MergeFrom(LiveMatchInfoLn objSource)
        {
            Debug.Assert(this.MatchId == objSource.MatchId);

            this.ExpiryDate.Value = objSource.ExpiryDate.Value;
            this.MatchMinute.Value = objSource.MatchMinute.Value;
            this.PeriodInfo.Value = objSource.PeriodInfo.Value;
            this.Status.Value = objSource.Status.Value;
            this.Score.Value = objSource.Score.Value;

            SetRelations();
        }

        public override void SetRelations()
        {
            if (this.ChangedProps.Contains(this.PeriodInfo) || this.ChangedProps.Contains(this.Status))
            {
                try
                {
                    this.Match.SetActiveChanged();
                }
                catch (Exception excp)
                {
                    ExcpHelper.ThrowUp<RelatedLineObjectNotFoundException>(excp, "LiveMatchInfo.SetRelations() ERROR. Parent Match not found for {0}", this);             
                }
            }
        }

        public static eLivePeriodInfo IntToPeriodInfo(int iValue)
        {
            try
            {
                return (eLivePeriodInfo)iValue;
            }
            catch
            {
            }

            return eLivePeriodInfo.Undefined;
        }

        public static eMatchStatus IntToMatchStatus(int iValue)
        {
            try
            {
                return (eMatchStatus)iValue;
            }
            catch
            {
            }

            return eMatchStatus.Undefined;
        }

        public MatchLn Match
        {
            get
            {
                if (m_match == null)
                {
                    m_match = LineSr.Instance.AllObjects.Matches.GetObject(this.MatchId);

                    // Debug.Assert(m_match != null); DK - Probably this is normal - When we delete Match from line and loaded (from Database) LiveMatchInfo suddenly cannot get Match because it is removed from line (Cache)
                }

                return m_match;
            }
        }

        // View
        public override void NotifyPropertiesChanged()
        {
            if (this.MatchView != null)
            {
                this.MatchView.RaisePropertiesChanged<LiveMatchInfoLn>(this);
            }
        }

        public MatchVw MatchView
        {
            get
            {
                if (m_objView == null)
                {
                    m_objView = this.Match != null ? (MatchVw)this.Match.MatchView : null;
                }

                return m_objView as MatchVw;
            }
        }

        public static int LivePeriodInfoToTimeType (eLivePeriodInfo lpi)
        {
            switch (lpi)
            {
                //Live-Bet Period Info Common: 
                case eLivePeriodInfo.OverTime: return BetDomainMapItem.PART_OVERTIME;

                //Live-Bet Period Info for Soccer
                case eLivePeriodInfo.Soccer_1st_Period: return 1;
                case eLivePeriodInfo.Soccer_2nd_Period: return 2;
                case eLivePeriodInfo.Soccer_1st_PeriodOverTime:
                case eLivePeriodInfo.Soccer_2nd_PeriodOverTime: return BetDomainMapItem.PART_OVERTIME;

                //Live-Bet Period Info for Tennis
                case eLivePeriodInfo.Tennis_1st_Set: return 1;
                case eLivePeriodInfo.Tennis_2nd_Set: return 2;
                case eLivePeriodInfo.Tennis_3rd_Set: return 3;
                case eLivePeriodInfo.Tennis_4th_Set: return 4;
                case eLivePeriodInfo.Tennis_5th_Set: return 5;

                //Live-Bet Period Info for Ice Hockey
                case eLivePeriodInfo.IceHockey_1st_Third: return 1;
                case eLivePeriodInfo.IceHockey_2nd_Third: return 2;
                case eLivePeriodInfo.IceHockey_3rd_Third: return 3;

                //Live-Bet Period Info for Basketball
                case eLivePeriodInfo.Basketball_1st_Quarter: return 1;
                case eLivePeriodInfo.Basketball_2nd_Quarter: return 2;
                case eLivePeriodInfo.Basketball_3rd_Quarter: return 3;
                case eLivePeriodInfo.Basketball_4th_Quarter: return 4;
                case eLivePeriodInfo.Basketball_OverTime: return BetDomainMapItem.PART_OVERTIME;
            }

            return BetDomainMapItem.PART_UNKNOWN;
        }

        public override ISerializableObject Serialize()
        {
            dynamic so = new SerializableObject(this.GetType());

            so.MatchId = this.MatchId;
            so.ExpiryDate = this.ExpiryDate.Value;
            so.MatchMinute = this.MatchMinute.Value;
            so.PeriodInfo = this.PeriodInfo.Value;
            so.Status = this.Status.Value;
            so.Score = this.Score.Value;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.MatchId = dso.MatchId.Value;
            this.ExpiryDate.Value = dso.ExpiryDate.Value;
            this.MatchMinute.Value = dso.MatchMinute.Value;
            this.PeriodInfo.Value = dso.PeriodInfo.Value;
            this.Status.Value = dso.Status.Value;
            this.Score.Value = dso.Score.Value;
        }

        public override string ToString()
        {
            return string.Format("LiveMatchInfoLn {{MatchId={0}, ExpiryDate='{1}', Status={2}, PeriodInfo={3}, Score='{4}', MatchMinute={5}, IsNew={6}}}",
                this.MatchId, this.ExpiryDate.Value, this.Status.Value, this.PeriodInfo.Value, this.Score.Value, this.MatchMinute.Value, this.IsNew);
        }
    }

    public class LiveMatchInfoDictionary : LineObjectDictionaryByIdBase<LiveMatchInfoLn>
    {

    }
}
