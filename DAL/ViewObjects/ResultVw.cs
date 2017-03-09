using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public class MatchResultVw : ViewObjectBase<MatchResultLn>
    {
        protected const string PROPERTY_NAME_TOURNAMENT_GROUP_ID = "TournamentGroupId";
        protected const string PROPERTY_NAME_SPORT_GROUP_ID = "SportGroupId";

        private GroupLn m_tournamentGroup = null;
        private GroupLn m_sportGroup = null;

        protected static SyncDictionary<string, List<string>> m_diLinePropsToViewProps = new SyncDictionary<string, List<string>>()
        {
        };

        public MatchResultVw(MatchResultLn resultLn)
            : base(resultLn)
        {
        }

        protected override SyncDictionary<string, List<string>> LinePropsToViewProps
        {
            get { return m_diLinePropsToViewProps; }
        }

        public string Score
        {
            get { return m_objLine.Score.Value; }
        }

        public override System.Windows.Visibility Visibility
        {
            get { return System.Windows.Visibility.Visible; }
        }

        public override bool IsEnabled
        {
            get { return true; }
        }

        public IGroupVw TournamentView
        {
            get
            {
                if (m_tournamentGroup == null && m_objLine.TournamentGroupId.Value != null)
                {
                    long lTournamentId = (long)m_objLine.TournamentGroupId.Value;
                    m_tournamentGroup = LineSr.Instance.AllObjects.Groups.GetObject(lTournamentId);
                }

                return m_tournamentGroup != null ? m_tournamentGroup.GroupView : null;
            }
        }

        public IGroupVw SportView
        {
            get
            {
                if (m_sportGroup == null && m_objLine.SportGroupId.Value != null)
                {
                    long lSportId = (long)m_objLine.SportGroupId.Value;
                    m_sportGroup = LineSr.Instance.AllObjects.Groups.GetObject(lSportId);
                }

                return m_sportGroup != null ? m_sportGroup.GroupView : null;
            }
        }

        public int LiveMatchMinute
        {
            get
            {
                if (LineObject.MatchLn == null)
                    return 0;
                return LineObject.MatchLn.LiveMatchInfo == null ? -1 : LineObject.MatchLn.LiveMatchInfo.MatchMinute.Value;
            }
        }

        public bool Active
        {
            get
            {
                if (LineObject.MatchLn == null)
                    return false;
                return LineObject.MatchLn.Active.Value;
            }
        }


        public string Name
        {
            get { return HomeCompetitorName + " : " + AwayCompetitorName; }
        }

        public DateTime StartDate
        {
            get { return m_objLine.StartDate.Value.LocalDateTime; }
        }

        private bool _isHeader;
        public bool IsHeader
        {
            get
            {
                return _isHeader;
            }
            set
            {
                if (_isHeader != value)
                {
                    _isHeader = value;
                    AddToChangedPropNames("IsHeader");
                    DoPropertyChanged("IsHeader");
                }
            }
        }

        public string HomeCompetitorName
        {
            get
            {
                return this.LineObject.HomeCompetitor.GetDisplayName(DalStationSettings.Instance.Language);
            }
        }

        public string AwayCompetitorName
        {
            get
            {
                return this.LineObject.AwayCompetitor.GetDisplayName(DalStationSettings.Instance.Language);
            }
        }


        public override int GetHashCode()
        {
            return m_objLine.MatchId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            MatchResultVw mrv = obj as MatchResultVw;
            bool result = false;
            if (mrv == null)
            {
                result = base.Equals(obj);
            }
            else
            {
                result = m_objLine.MatchId.Equals(mrv.m_objLine.MatchId);
            }
            return result;
        }

        protected override void OnPropertyChanged(CommonObjects.ObservablePropertyBase opb)
        {
            if (opb.PropertyName == PROPERTY_NAME_TOURNAMENT_GROUP_ID)
            {
                m_tournamentGroup = null;
            }
            else if (opb.PropertyName == PROPERTY_NAME_SPORT_GROUP_ID)
            {
                m_sportGroup = null;
            }

            base.OnPropertyChanged(opb);
        }
    }
}
