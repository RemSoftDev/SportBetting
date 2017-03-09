using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public class MatchResultLn : ObjectBase, ILineObjectWithId<MatchResultLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("MatchResult", false, "MatchId");
        private bool _homeTeamWon;
        private bool _awayTeamWon;
        private string _teamWon;

        public CompetitorLn HomeCompetitor { get; protected set; }
        public CompetitorLn AwayCompetitor { get; protected set; }

        public long MatchId { get; set; }
        public long BtrMatchId { get; set; }
        public ObservableProperty<long?> TournamentGroupId { get; set; }
        public ObservableProperty<long?> SportGroupId { get; set; }
        public ObservableProperty<DateTimeSr> StartDate { get; set; }
        public ObservableProperty<bool> IsLiveBet { get; set; }
        public ObservableProperty<long> HomeCompetitorId { get; set; }
        public ObservableProperty<long> AwayCompetitorId { get; set; }
        public ObservableProperty<string> Score { get; set; }
        public ObservableProperty<string> ExtendedState { get; set; }
        public ObservableProperty<long?> CategoryGroupId { get; set; }
        public MatchLn MatchLn
        {
            get { return LineSr.Instance.AllObjects.GetObject<MatchLn>(MatchId); }

        }

        public MatchResultLn()
            : base(true)
        {

        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.MatchId = DbConvert.ToInt64(dr, "MatchId");
            this.BtrMatchId = DbConvert.ToInt64(dr, "BtrMatchId");
            this.TournamentGroupId.Value = DbConvert.ToNullableInt64(dr, "TournamentGroupId");
            this.CategoryGroupId.Value = DbConvert.ToNullableInt64(dr, "CategoryGroupId");
            this.SportGroupId.Value = DbConvert.ToNullableInt64(dr, "SportGroupId");
            this.StartDate.Value = DbConvert.ToDateTimeSr(dr, "StartDate");
            this.IsLiveBet.Value = DbConvert.ToInt32(dr, "IsLiveBet") > 0;
            this.HomeCompetitorId.Value = DbConvert.ToInt64(dr, "HomeCompetitorId");
            this.AwayCompetitorId.Value = DbConvert.ToInt64(dr, "AwayCompetitorId");
            this.Score.Value = DbConvert.ToString(dr, "Score");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateID");
            this.TeamWon = DbConvert.ToString(dr, "TeamWon");
            this.ExtendedState.Value = DbConvert.ToString(dr, "ExtendedState");
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["MatchId"] = this.MatchId;
            dr["BtrMatchId"] = this.BtrMatchId;
            dr["TournamentGroupId"] = this.TournamentGroupId.Value ?? (object)DBNull.Value;
            dr["CategoryGroupId"] = this.CategoryGroupId.Value ?? (object)DBNull.Value;
            dr["SportGroupId"] = this.SportGroupId.Value ?? (object)DBNull.Value;
            dr["StartDate"] = this.StartDate.Value.LocalDateTime;
            dr["IsLiveBet"] = this.IsLiveBet.Value ? 1 : 0;
            dr["HomeCompetitorId"] = this.HomeCompetitorId.Value;
            dr["AwayCompetitorId"] = this.AwayCompetitorId.Value;
            dr["Score"] = this.Score.Value;
            dr["UpdateID"] = this.UpdateId;
            dr["TeamWon"] = this.TeamWon;
            dr["ExtendedState"] = this.ExtendedState.Value;

            return dr;
        }

        public long Id
        {
            get { return this.MatchId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.MatchResults.ContainsKey(this.MatchId); }
        }

        public void MergeFrom(MatchResultLn objSource)
        {
            Debug.Assert(this.MatchId == objSource.MatchId);
            Debug.Assert(this.BtrMatchId == objSource.BtrMatchId);

            this.TournamentGroupId.Value = objSource.TournamentGroupId.Value;
            this.CategoryGroupId.Value = objSource.CategoryGroupId.Value;
            this.SportGroupId.Value = objSource.SportGroupId.Value;
            this.StartDate.Value = objSource.StartDate.Value;
            this.IsLiveBet.Value = objSource.IsLiveBet.Value;
            this.HomeCompetitorId.Value = objSource.HomeCompetitorId.Value;
            this.AwayCompetitorId.Value = objSource.AwayCompetitorId.Value;
            this.Score.Value = objSource.Score.Value;
            this.ExtendedState.Value = objSource.ExtendedState.Value;

            SetRelations();
        }

        // View
        public MatchResultVw MatchResultView
        {
            get
            {
                if (m_objView == null)
                {
                    m_objView = new MatchResultVw(this);
                }

                return m_objView as MatchResultVw;
            }
        }

        public bool HomeTeamWon
        {
            get { return _teamWon == "1"; }
        }

        public bool AwayTeamWon
        {
            get { return _teamWon == "2"; }
        }

        public string TeamWon
        {
            get { return _teamWon; }
            set
            {
                _teamWon = value;
                //Debug.Assert(!string.IsNullOrEmpty(_teamWon));
                //Debug.Assert(_teamWon == "2" || _teamWon == "1");
            }
        }


        public override void SetRelations()
        {
            base.SetRelations();

            HashSet<string> hsPropertyNames = this.ChangedProps.GetPropertyNames();

            if (this.HomeCompetitor == null || hsPropertyNames.Contains("HomeCompetitorId"))
            {
                this.HomeCompetitor = LineSr.Instance.AllObjects.Competitors.GetObject(this.HomeCompetitorId.Value);
                ExcpHelper.ThrowIf(this.HomeCompetitor == null, "MatchResultLn.MergeFrom() ERROR. Cannot get Home Competitor for {0}", this);
            }

            if (this.AwayCompetitor == null || hsPropertyNames.Contains("AwayCompetitorId"))
            {
                this.AwayCompetitor = LineSr.Instance.AllObjects.Competitors.GetObject(this.AwayCompetitorId.Value);
                ExcpHelper.ThrowIf(this.AwayCompetitor == null, "MatchResultLn.MergeFrom() ERROR. Cannot get Away Competitor for {0}", this);
            }
        }

        public override string ToString()
        {
            string sLang = DalStationSettings.Instance.Language;

            string sHomeCompetitorName = this.HomeCompetitor != null ? this.HomeCompetitor.GetDisplayName(sLang) : string.Empty;
            string sAwayCompetitorName = this.AwayCompetitor != null ? this.AwayCompetitor.GetDisplayName(sLang) : string.Empty;

            string sCompetitors = string.Format("'{0}'-'{1}'", sHomeCompetitorName, sAwayCompetitorName);

            return string.Format("MatchResultLn {{MatchId={0}, BtrMatchId={1}, Competitors({2})={3}, IsLiveBet={4}, IsNew={5}, ChangedProps={6}}}",
                this.MatchId, this.BtrMatchId, sLang, sCompetitors, this.IsLiveBet.Value, this.IsNew, this.ChangedProps.Count);
        }
    }

    public class MatchResultDictionary : LineObjectDictionaryByIdBase<MatchResultLn>
    {
    }
}
