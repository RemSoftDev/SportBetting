using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public class GroupVw : ViewObjectBase<GroupLn> ,IGroupVw
    {
        protected static SyncDictionary<string, List<string>> m_diLinePropsToViewProps = new SyncDictionary<string, List<string>>()
        {
            {"Active", new List<string>(){"Active"}},
            {"ExternalState", new List<string>(){"TournamentSportView", "TournamentCountryView"}},
        };

        public IGroupVw TournamentSportView
        {
            get
            {
                if (m_objLine.GroupTournament != null)
                {
                    var sportGroup = LineSr.Instance.AllObjects.Groups.GetObject(m_objLine.GroupTournament.SportGroupId);

                    return sportGroup != null ? sportGroup.GroupView : null;
                }

                return null;
            }
        }

        public IGroupVw TournamentCountryView
        {
            get
            {
                if (m_objLine.GroupTournament != null && m_objLine.GroupTournament.CountryGroupId != null)
                {
                    var countryGroup = LineSr.Instance.AllObjects.Groups.GetObject((long)m_objLine.GroupTournament.CountryGroupId);

                    return countryGroup != null ? countryGroup.GroupView : null;
                }

                return null;
            }
        }

        public int Sort { get; set; }

        public int TournamentMinCombination
        {
            get
            {
                return m_objLine.GroupTournament != null && m_objLine.GroupTournament.MinCombination != null ? (int)m_objLine.GroupTournament.MinCombination : 0;
            }
        }

        protected override SyncDictionary<string, List<string>> LinePropsToViewProps
        {
            get { return m_diLinePropsToViewProps; }
        }

        public string DisplayName
        {
            get
            {
                return m_objLine.GetDisplayName(DalStationSettings.Instance.Language);
            }
        }

        public bool Active
        {
            get { return m_objLine.Active.Value; }
        }

        public GroupVw(GroupLn groupLn) : base(groupLn)
        {
        }

        public GroupVw ParentGroupView
        {
            get { return m_objLine.ParentGroup != null ? m_objLine.ParentGroup.GroupView : null; }
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public override System.Windows.Visibility Visibility
        {
            get { return Visibility.Visible; }
        }

        public override bool IsEnabled
        {
            get { return true; }
        }

        public override int GetHashCode()
        {
            return m_objLine.GroupId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var gv = obj as GroupVw;

            return gv != null ? m_objLine.GroupId.Equals(gv.m_objLine.GroupId) : base.Equals(obj);
        }
    }
}
