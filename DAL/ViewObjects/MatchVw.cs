using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using System.Configuration;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace SportRadar.DAL.ViewObjects
{
    public class MatchVw : ViewObjectBase<MatchLn>, IMatchVw
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(MatchVw));

        protected static SyncDictionary<string, List<string>> m_diLinePropsToViewProps = new SyncDictionary<string, List<string>>()
        {
            {"Active", new List<string>(){"Active", "IsEnabled","LiveColor","LiveGradientColor"}},
            {"IsSelected", new List<string>(){"IsSelected"}},
            {"StartDate", new List<string>(){"StartDate"}},
            {"Code", new List<string>(){"Code"}},
            {"MatchMinute", new List<string>(){"LiveMatchMinute", "LiveMatchMinuteEx", "LiveMinuteToShow"}},
            {"ExpiryDate", new List<string>(){"ExpiryDate"}},
            {"PeriodInfo", new List<string>(){"LivePeriodInfo", "LivePeriodInfoString", "BottomSpecialBetDomain", "BaseBetDomainView", "UnderOverBetDomain", "BottomUnderOverBetdomain", "SecondMatchButtonRowText", "ShomMinutes", "InverseShomMinutes", "LiveMInuteVisibility", "InversedLiveMInuteVisibility","LiveColor","LiveGradientColor"}},
            {"Status", new List<string>(){"LiveBetStatus", "Visibility", "IsEnabled","LiveColor","LiveGradientColor"}},
            {"Score", new List<string>(){"LiveScore", "GoalsTextTest"}},
            {"ChangedCount", new List<string>(){"Visibility", "IsEnabled", "LiveBetStatus","LiveColor","LiveGradientColor"}},
            {"CardsTeam1", new List<string>(){"HomeTeamRedCards", "IsVisibleHomeTeamRedCards"}},
            {"CardsTeam2", new List<string>(){"AwayTeamRedCards", "IsVisibleAwayTeamRedCards"}},
        };

        protected static SyncDictionary<string, string> m_diLinePeriodsToViewPeriods = new SyncDictionary<string, string>()
        {
            {"Soccer_1st_Period", "TERMINAL_1HT"},
            {"Soccer_2nd_Period", "TERMINAL_2HT"},
            {"Soccer_1st_PeriodOverTime", "TERMINAL_1ST_PERIOD_OVERTIME"},
            {"Soccer_2nd_PeriodOverTime", "TERMINAL_2ND_PERIOD_OVERTIME"},
            {"Tennis_1st_Set", "TERMINAL_1_SET_SHORT"},
            {"Tennis_2nd_Set", "TERMINAL_2_SET_SHORT"},
            {"Tennis_3rd_Set", "TERMINAL_3_SET_SHORT"},
            {"Tennis_4th_Set", "TERMINAL_4_SET_SHORT"},
            {"Tennis_5th_Set", "TERMINAL_5_SET_SHORT"},
            {"Tennis_WALKOVER", "TERMINAL_WALKOVER"}, 
            {"Tennis_RETIRED", "TERMINAL_RETIRED"},
            {"Tennis_DELAYED", "TERMINAL_DELAYED"},
            {"IceHockey_1st_Third", "TERMINAL_1_THIRD"},
            {"IceHockey_2nd_Third", "TERMINAL_2_THIRD"},
            {"IceHockey_3rd_Third", "TERMINAL_3_THIRD"},
            {"Basketball_Pause1", "TERMINAL_PAUSE"},
            {"Basketball_Pause2", "TERMINAL_PAUSE"},
            {"Basketball_Pause3", "TERMINAL_PAUSE"},
            {"Basketball_1st_Quarter", "TERMINAL_1_QUARTER"},
            {"Basketball_2nd_Quarter", "TERMINAL_2_QUARTER"},
            {"Basketball_3rd_Quarter", "TERMINAL_3_QUARTER"},
            {"Basketball_4th_Quarter", "TERMINAL_4_QUARTER"},
            {"Penalty", "TERMINAL_PENALTY"},
            {"NotStarted", "TERMINAL_NOTSTARTED"},
            {"Started", "TERMINAL_STARTED"},
            {"Paused", "TERMINAL_PAUSE"},
            {"Stopped", "TERMINAL_STOPPED"},
            {"Interrupted", "TERMINAL_INTERRUPTED"},
            {"OverTime", "TERMINAL_OVERTIME"},
        };

        protected GroupLn m_tournament = null;
        protected GroupLn m_sport = null;
        protected GroupLn m_country = null;


        protected int m_iAllBetDomainCount = 0;
        protected int m_iVisibleBetDomainCount = 0;
        protected int m_iAllVisibleOddCount = 0;
        protected int m_iVisibleOddCount = 0;
        protected int moreBetdomainCount = 0;
        protected int m_iAllEnabledOddCount = 0;
        protected int m_iEnabledOddCount = 0;

        protected int m_iBasketOddCount = 0;

        protected string m_sSportDescriptor = null;

        protected IBetDomainVw m_baseBetDomainView = BetDomainLn.EmptyBetDomain.BetDomainView;
        protected IBetDomainVw m_underOverBetDomainView = BetDomainLn.EmptyBetDomain.BetDomainView;
        protected IBetDomainVw m_bottomUnderOverBetdomainView = BetDomainLn.EmptyBetDomain.BetDomainView;
        protected IBetDomainVw m_bottomSpecialBetDomainView = BetDomainLn.EmptyBetDomain.BetDomainView;

        protected SyncList<IBetDomainVw> m_lVisibleBetDomains = new SyncList<IBetDomainVw>();
        protected SyncObservableCollection<IBetDomainVw> m_socVisibleBetDomainViews = null;
        protected SyncObservableCollection<IBetDomainVw> allBetDomainViews = new SyncObservableCollection<IBetDomainVw>();

        protected int m_iLiveMatchMinuteEx = 0;

        protected override SyncDictionary<string, List<string>> LinePropsToViewProps
        {
            get { return m_diLinePropsToViewProps; }
        }

        public MatchVw(MatchLn matchLn)
            : base(matchLn)
        {
        }

        public string SecondMatchButtonRowText
        {
            get
            {
                string result = "";

                int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

                if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
                {
                    if (period == 1 || this.LiveBetStatus == eMatchStatus.NotStarted)
                    {
                        if (this.BottomSpecialBetDomain != BetDomainLn.EmptyBetDomain.BetDomainView || this.BottomUnderOverBetdomain != BetDomainLn.EmptyBetDomain.BetDomainView)
                        {
                            result = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely("TERMINAL_1ST_HALF_BETS", DalStationSettings.Instance.Language);

                            if (result == "")
                                result = "TERMINAL_1ST_HALF_BETS";
                        }
                    }
                }

                return result;
            }
        }

        //Tennis or ice hokkey does not have live minute, so block should be hidden
        public Visibility LiveMInuteVisibility
        {
            get
            {
                return this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        //Tennis or ice hokkey does not have live minute, so block should be hidden
        public Visibility InversedLiveMInuteVisibility
        {
            get
            {
                return this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ShomMinutes
        {
            get
            {
                if (LiveMinuteToShow != "")
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public SyncObservableCollection<IBetDomainVw> AllBetdomains
        {
            get { return new SyncObservableCollection<IBetDomainVw>(allBetDomainViews.ToSyncList()); }
        }

        public Visibility InverseShomMinutes
        {
            get
            {
                if (LiveMinuteToShow != "")
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public string LiveColor
        {
            get
            {

                bool isMatchEnabled;
                eLivePeriodInfo srPeriodInfo;
                int OddCount;
                string color;



                isMatchEnabled = IsEnabled;
                srPeriodInfo = LivePeriodInfo;
                OddCount = AllEnabledOddCount;

                if (isMatchEnabled && OddCount > 0)
                {

                    switch (srPeriodInfo)
                    {
                        case eLivePeriodInfo.Paused:
                            color = "#AA7470c0";
                            break;
                        case eLivePeriodInfo.Basketball_1st_Quarter:
                        case eLivePeriodInfo.Basketball_2nd_Quarter:
                        case eLivePeriodInfo.Basketball_3rd_Quarter:
                        case eLivePeriodInfo.Basketball_4th_Quarter:
                        case eLivePeriodInfo.IceHockey_1st_Third:
                        case eLivePeriodInfo.IceHockey_2nd_Third:
                        case eLivePeriodInfo.IceHockey_3rd_Third:
                        case eLivePeriodInfo.Soccer_1st_Period:
                        case eLivePeriodInfo.Tennis_1st_Set:
                        case eLivePeriodInfo.Tennis_2nd_Set:
                        case eLivePeriodInfo.Tennis_3rd_Set:
                        case eLivePeriodInfo.Tennis_4th_Set:
                        case eLivePeriodInfo.Tennis_5th_Set:
                            color = "#AA67b457";
                            break;
                        case eLivePeriodInfo.Soccer_2nd_Period:
                            color = "#AAffa800";
                            break;
                        case eLivePeriodInfo.Basketball_OverTime:
                            color = "Gray";
                            break;
                        case eLivePeriodInfo.Basketball_Pause1:
                        case eLivePeriodInfo.Basketball_Pause2:
                        case eLivePeriodInfo.Basketball_Pause3:
                            color = "#AA7470c0";
                            break;
                        case eLivePeriodInfo.Soccer_1st_PeriodOverTime:
                        case eLivePeriodInfo.Soccer_2nd_PeriodOverTime:
                        case eLivePeriodInfo.Penalty:
                        case eLivePeriodInfo.OverTime:
                            color = "#AAFF8211";
                            break;
                        default:
                            return "Transparent";
                    }

                }
                else if (srPeriodInfo == eLivePeriodInfo.Interrupted)
                {
                    color = "Transparent";
                }
                else
                {
                    color = "#AAd01f00";
                }




                return color;
            }
        }
        public LinearGradientBrush LiveGradientColor
        {
            get
            {

                bool isMatchEnabled;
                eLivePeriodInfo srPeriodInfo;
                int OddCount;
                LinearGradientBrush color = new LinearGradientBrush();
                color.StartPoint = new Point(1, 0.5);
                color.EndPoint = new Point(0, 0.5);



                isMatchEnabled = IsEnabled;
                srPeriodInfo = LivePeriodInfo;
                OddCount = AllEnabledOddCount;

                if (isMatchEnabled && OddCount > 0)
                {

                    switch (srPeriodInfo)
                    {
                        case eLivePeriodInfo.Paused:
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#777470c0"), 1));//purple
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00FFFFFF"), 0));
                            break;
                        case eLivePeriodInfo.Basketball_1st_Quarter:
                        case eLivePeriodInfo.Basketball_2nd_Quarter:
                        case eLivePeriodInfo.Basketball_3rd_Quarter:
                        case eLivePeriodInfo.Basketball_4th_Quarter:
                        case eLivePeriodInfo.IceHockey_1st_Third:
                        case eLivePeriodInfo.IceHockey_2nd_Third:
                        case eLivePeriodInfo.IceHockey_3rd_Third:
                        case eLivePeriodInfo.Soccer_1st_Period:
                        case eLivePeriodInfo.Tennis_1st_Set:
                        case eLivePeriodInfo.Tennis_2nd_Set:
                        case eLivePeriodInfo.Tennis_3rd_Set:
                        case eLivePeriodInfo.Tennis_4th_Set:
                        case eLivePeriodInfo.Tennis_5th_Set:
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#7767b457"), 1));//green
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00FFFFFF"), 0));
                            break;
                        case eLivePeriodInfo.Soccer_2nd_Period:
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#77ffa800"), 1));//orange
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00FFFFFF"), 0));
                            break;
                        case eLivePeriodInfo.Basketball_OverTime:
                            //gray
                            break;
                        case eLivePeriodInfo.Basketball_Pause1:
                        case eLivePeriodInfo.Basketball_Pause2:
                        case eLivePeriodInfo.Basketball_Pause3:
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#777470c0"), 1));//purple
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00FFFFFF"), 0));
                            break;
                        case eLivePeriodInfo.Penalty:
                        case eLivePeriodInfo.Soccer_1st_PeriodOverTime:
                        case eLivePeriodInfo.Soccer_2nd_PeriodOverTime:
                        case eLivePeriodInfo.OverTime:
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#77FF8211"), 1));//orange_overtime
                            color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00FFFFFF"), 0));
                            break;
                        default:
                            break;
                    }

                }
                else if (srPeriodInfo == eLivePeriodInfo.Interrupted)
                {

                }
                else
                {
                    color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#77d01f00"), 1));//reg
                    color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00FFFFFF"), 0));
                }




                return color;
            }
        }

        public IGroupVw CategoryView
        {
            get { return this.TournamentView != null ? this.TournamentView.ParentGroupView : null; }
        }

        protected GroupLn GetParentGroupByType(string sGroupType)
        {
            if (m_objLine.ParentGroups != null)
            {
                var lGroups = m_objLine.ParentGroups.Clone();

                foreach (var group in lGroups)
                {
                    if (group.Type == sGroupType)
                    {
                        return group;
                    }
                }
            }

            return null;
        }

        public string SportDescriptor
        {
            get
            {
                if (string.IsNullOrEmpty(m_sSportDescriptor))
                {
                    Debug.Assert(this.Sport != null && this.Sport.GroupSport != null);

                    m_sSportDescriptor = this.Sport.GroupSport.SportDescriptor;

                    Debug.Assert(!string.IsNullOrEmpty(m_sSportDescriptor));
                }

                return m_sSportDescriptor;
            }
        }

        protected GroupLn Tournament
        {
            get
            {
                if (m_tournament == null)
                {
                    m_tournament = this.GetParentGroupByType(GroupLn.GROUP_TYPE_GROUP_T);
                }

                return m_tournament;
            }
        }

        protected GroupLn Sport
        {
            get
            {
                if (m_sport == null)
                {
                    m_sport = this.GetParentGroupByType(GroupLn.GROUP_TYPE_SPORT);
                }

                return m_sport;
            }
        }

        protected GroupLn Country
        {
            get
            {
                if (m_country == null)
                {
                    m_country = this.GetParentGroupByType(GroupLn.GROUP_TYPE_COUNTRY);
                }

                return m_country;
            }
        }

        public IGroupVw TournamentView
        {
            get
            {
                return this.Tournament != null ? this.Tournament.GroupView : null;
            }
        }

        public IGroupVw SportView
        {
            get
            {
                return this.Sport != null ? this.Sport.GroupView : null;
            }
        }

        public IGroupVw CountryView
        {
            get
            {
                return this.Country != null ? this.Country.GroupView : null;
            }
        }

        public int Code
        {
            get { return m_objLine.Code.Value; }
        }

        public string Name
        {
            get
            {
                if (m_objLine.outright_type == eOutrightType.None)
                    return HomeCompetitorName + " : " + AwayCompetitorName;

                string sTranslation = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(m_objLine.NameTag.Value, DalStationSettings.Instance.Language);

                return sTranslation;
            }
        }

        public string HomeCompetitorName
        {
            get
            {
                return m_objLine.outright_type == eOutrightType.None ? m_objLine.HomeCompetitor.GetDisplayName(DalStationSettings.Instance.Language) : string.Empty;
            }
        }

        public string AwayCompetitorName
        {
            get { return m_objLine.outright_type == eOutrightType.None ? m_objLine.AwayCompetitor.GetDisplayName(DalStationSettings.Instance.Language) : string.Empty; }
        }

        public string OutrightDisplayName
        {
            get { return m_objLine.GetOutrightDisplayName(DalStationSettings.Instance.Language); }
        }

        public SyncObservableCollection<OutrightCompetitorVw> OutrightCompetitors
        {
            get
            {
                try
                {
                    SyncObservableCollection<OutrightCompetitorVw> lCompetitors = new SyncObservableCollection<OutrightCompetitorVw>();

                    if (m_objLine.OutrightCompetitors != null)
                    {
                        foreach (CompetitorToOutrightLn cto in m_objLine.OutrightCompetitors.ToSyncList())
                        {
                            lCompetitors.Add(new OutrightCompetitorVw(cto));
                        }
                    }

                    lCompetitors.Sort(delegate(OutrightCompetitorVw o1, OutrightCompetitorVw o2)
                    {
                        return o1.Position.CompareTo(o2.Position);
                    });

                    return lCompetitors;
                }
                catch (Exception excp)
                {
                    ExcpHelper.ThrowUp(excp, "OutrightCompetitors_get() ERROR for {0}", this);
                }

                return null;
            }
        }

        SyncObservableCollection<OutrightCompetitorVw> lCompetitorsVHC = new SyncObservableCollection<OutrightCompetitorVw>();
        public SyncObservableCollection<OutrightCompetitorVw> OutrightCompetitorsVHC
        {
            get
            {
                try
                {
                    if (m_objLine.OutrightCompetitors != null)
                    {
                        foreach (CompetitorToOutrightLn cto in m_objLine.OutrightCompetitors.ToSyncList())
                        {
                            OutrightCompetitorVw outr = new OutrightCompetitorVw(cto);
                            bool contains = false;

                            foreach (OutrightCompetitorVw orc in lCompetitorsVHC)
                            {
                                if (orc.Position == outr.Position && orc.Name == outr.Name && orc.Competitor == outr.Competitor && orc.ToString() == outr.ToString())
                                    contains = true;
                            }

                            if (!contains)
                                lCompetitorsVHC.Add(outr);
                        }
                    }

                    lCompetitorsVHC.Sort(delegate(OutrightCompetitorVw o1, OutrightCompetitorVw o2)
                    {
                        return o1.Position.CompareTo(o2.Position);
                    });

                    return lCompetitorsVHC;
                }
                catch (Exception excp)
                {
                    ExcpHelper.ThrowUp(excp, "OutrightCompetitorsVHC_get() ERROR for {0}", this);
                }

                return null;
            }
        }

        public bool Active
        {
            get { return m_objLine.Active.Value; }
        }

        public bool IsLiveBet
        {
            get { return m_objLine.IsLiveBet.Value; }
        }

        public bool IsOutright
        {
            get { return m_objLine.outright_type != eOutrightType.None; }
        }

        public bool IsSelected
        {
            get { return m_objLine.IsSelected; }
        }

        public DateTime StartDate
        {
            get
            {
                if (this.IsOutright && this.LineObject.SourceType == eServerSourceType.BtrPre)
                    return m_objLine.EndDate.Value.LocalDateTime;
                else
                    return m_objLine.StartDate.Value.LocalDateTime;
            }
        }

        public DateTime ExpiryDate
        {
            get { return m_objLine.LiveMatchInfo == null ? m_objLine.ExpiryDate.Value.LocalDateTime : m_objLine.LiveMatchInfo.ExpiryDate.Value.LocalDateTime; }
        }

        public int CardsTeam1
        {
            get
            {
                return m_objLine.CardsTeam1.Value;
            }
        }

        public SyncObservableCollection<int> HomeTeamRedCards
        {
            get
            {
                SyncObservableCollection<int> cards = new SyncObservableCollection<int>();

                int cardsCount = CardsTeam1;

                for (int i = 0; i < cardsCount; i++)
                {
                    cards.Add(0);
                }

                return cards;
            }
        }

        public Boolean IsVisibleHomeTeamRedCards { get { return CardsTeam1 > 0; } }
        public Boolean IsVisibleAwayTeamRedCards { get { return CardsTeam2 > 0; } }

        public SyncObservableCollection<int> AwayTeamRedCards
        {
            get
            {
                SyncObservableCollection<int> cards = new SyncObservableCollection<int>();

                int cardsCount = CardsTeam2;

                for (int i = 0; i < cardsCount; i++)
                {
                    cards.Add(0);
                }

                return cards;
            }
        }

        public int CardsTeam2
        {
            get
            {
                return m_objLine.CardsTeam2.Value;
            }
        }

        public int MinCombination
        {
            get
            {
                return m_objLine.MatchExternalState != null && m_objLine.MatchExternalState.MinCombination != null ? (int)m_objLine.MatchExternalState.MinCombination : 0;
            }
        }


        public int VirtualSeason
        {
            get
            {
                return m_objLine.MatchExternalState != null && m_objLine.MatchExternalState.VirtualSeason != null ? (int)m_objLine.MatchExternalState.VirtualSeason : 0;
            }
        }

        public int VirtualDay
        {
            get
            {
                return m_objLine.MatchExternalState != null && m_objLine.MatchExternalState.VirtualDay != null ? (int)m_objLine.MatchExternalState.VirtualDay : 0;
            }
        }

        public int LiveMatchMinute
        {
            get { return m_objLine.LiveMatchInfo == null ? -1 : m_objLine.LiveMatchInfo.MatchMinute.Value; }
        }

        public int LiveMatchMinuteEx
        {
            get
            {
                return m_iLiveMatchMinuteEx;
            }
        }

        public Visibility LivePrefixVisibility
        {
            get
            {
                if (this.LineObject.SourceType == eServerSourceType.BtrLive)
                    return Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public string LiveMinuteToShow
        {
            get
            {
                if (LiveMatchMinute == -1)
                {
                    return string.Empty;
                }

                if (this.LivePeriodInfo == eLivePeriodInfo.Paused || this.LivePeriodInfo == eLivePeriodInfo.Interrupted || this.LineObject.SourceType != eServerSourceType.BtrLive)
                {
                    return string.Empty;
                }

                if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                {
                    return string.Empty;
                }

                return this.LiveMatchMinute.ToString("G");
            }
        }

        public eLivePeriodInfo LivePeriodInfo
        {
            get { return m_objLine.LiveMatchInfo == null ? eLivePeriodInfo.Undefined : m_objLine.LiveMatchInfo.PeriodInfo.Value; }
        }

        public eLivePeriodInfo LivePreviousPeriodInfo
        {
            get { return m_objLine.LiveMatchInfo == null ? eLivePeriodInfo.Undefined : m_objLine.LiveMatchInfo.PeriodInfo.PreviousValue; }
        }

        public string LivePeriodInfoString
        {
            get
            {
                if (m_objLine.LiveMatchInfo == null)
                    return null;
                string translatedString = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(m_diLinePeriodsToViewPeriods.SafelyGetValue(m_objLine.LiveMatchInfo.PeriodInfo.Value.ToString()), DalStationSettings.Instance.Language);
                if (this.LivePeriodInfo == eLivePeriodInfo.Interrupted)
                {
                    return "";
                }

                return string.IsNullOrEmpty(translatedString) ? m_diLinePeriodsToViewPeriods.SafelyGetValue(m_objLine.LiveMatchInfo.PeriodInfo.Value.ToString()) : translatedString;

                //if (m_objLine.LiveMatchInfo.PeriodInfo.Value.ToString() == "Soccer_1st_Period")
                //{
                //    return LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely("TERMINAL_1HT",
                //                                                                    DalStationSettings.Instance.Language);
                //} else if (m_objLine.LiveMatchInfo.PeriodInfo.Value.ToString() == "Soccer_2nd_Period")
                //{
                //    return LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely("TERMINAL_2HT",
                //                                                                    DalStationSettings.Instance.Language);
                //}
                //else
                //{
                //    string translatedString = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(m_objLine.LiveMatchInfo.PeriodInfo.Value.ToString(), DalStationSettings.Instance.Language);
                //    return string.IsNullOrEmpty(translatedString) ? eLivePeriodInfo.Undefined.ToString() : translatedString;
                //}

            }
        }

        public eMatchStatus LiveBetStatus
        {
            get { return m_objLine.LiveMatchInfo == null ? eMatchStatus.Undefined : m_objLine.LiveMatchInfo.Status.Value; }
        }

        public int AllBetDomainCount { get { return m_iAllBetDomainCount; } }
        public int VisibleBetDomainCount { get { return m_iVisibleBetDomainCount; } }
        public int AllVisibleOddCount { get { return m_iAllVisibleOddCount; } }
        public int VisibleOddCount { get { return m_iVisibleOddCount; } }
        public int MoreBetdomainCount { get { return moreBetdomainCount; } }
        public int AllEnabledOddCount { get { return m_iAllEnabledOddCount; } }
        public eServerSourceType? MatchSource { get { return LineObject != null ? LineObject.SourceType : null as eServerSourceType?; } }

        public int BasketOddCount
        {
            get
            {
                int selectedOddsCount = 0;
                foreach (IBetDomainVw bdmn in this.VisibleBetDomains)
                {
                    if (bdmn.LineObject.Odds.Any(x => x.IsSelected))
                        selectedOddsCount += 1;
                }
                return VisibleBetDomainCount - selectedOddsCount;
            }
        }

        /*
        public BetDomainVw BaseBetDomainView
        {
            get
            {
                long SportId = this.SportView.LineObject.SvrGroupId;
                int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

                SyncList<BetDomainLn> lBetDomains = this.LineObject.BetDomains.Clone();

                if (SportId == 1) //soccer
                {
                    foreach (BetDomainLn bd in lBetDomains)
                    {
                        if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                        {
                            continue;
                        }

                        if (bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINFTR)//WINFTR
                            return bd.BetDomainView;

                        if (!this.IsLiveBet && bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINFT)
                            return bd.BetDomainView;
                    }
                }
                else if (SportId == 2) //basketball
                {
                    foreach (BetDomainLn bd in lBetDomains)
                    {
                        if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                        {
                            continue;
                        }

                        if (bd.BetTag.Value == "WINGAM")
                            return bd.BetDomainView;

                        if (!this.IsLiveBet && bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINFTR)
                            return bd.BetDomainView;

                        if (!this.IsLiveBet && bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINFT)
                            return bd.BetDomainView;
                    }
                }
                else
                {
                    foreach (BetDomainLn bd in lBetDomains)
                    {
                        if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                        {
                            continue;
                        }

                        if (bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINFT)
                            return bd.BetDomainView;

                        if (bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINFTR)
                            return bd.BetDomainView;
                    }
                }

                return BetDomainLn.EmptyBetDomain.BetDomainView;
            }
        }
        */

        public IBetDomainVw BaseBetDomainView { get { return m_baseBetDomainView; } }

        public bool IsBaseBetDomain(BetDomainLn bd)
        {
            if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER) //soccer
            {
                if (bd.BetTag == "WINOTR" + "_L" && bd.BetDomainView.IsEnabled && (this.LivePeriodInfo == eLivePeriodInfo.OverTime || this.LivePeriodInfo == eLivePeriodInfo.Soccer_1st_PeriodOverTime || this.LivePeriodInfo == eLivePeriodInfo.Soccer_2nd_PeriodOverTime || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_1st_PeriodOverTime || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_2nd_PeriodOverTime || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_2nd_Period))
                {
                    return true;
                } //WINOTR

                if (IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR + "_L") //WINFTR
                {
                    return true;
                }
                if (bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR) //WINFTR
                {
                    return true;
                }

                if ((!this.IsLiveBet || this.LineObject.SourceType == eServerSourceType.BtrVfl) && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT)
                {
                    return true;
                }

                if (this.LineObject.SourceType == eServerSourceType.BtrVfl && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT + "_L")
                    return true;

                if (bd.BetTag == "WINAP_L" && this.LivePeriodInfo == eLivePeriodInfo.Penalty && this.LineObject.SourceType == eServerSourceType.BtrLive)
                    return true;
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL) //basketball
            {

                if (bd.BetTag == "WINGAM")
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "WINGAM" + "_L")
                {
                    return true;
                }

                if (!this.IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR)
                {
                    return true;
                }

                if (!this.IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT)
                {
                    return true;
                }

            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_RUGBY) //basketball
            {
                if (bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT + "_L" || bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT)
                {
                    return true;
                }

            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
            {
                if (bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT + "_L")
                {
                    return true;
                }

                if (bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR + "_L")
                {
                    return true;
                }
                if (IsLiveBet && this.LineObject.LiveMatchInfo.PeriodInfo.Value == eLivePeriodInfo.OverTime && bd.BetTag == "WINGAMOTAP_L")
                {
                    return true;
                }
                if (bd.BetTag == "WINAP_L" && this.LivePeriodInfo == eLivePeriodInfo.Penalty && this.LineObject.SourceType == eServerSourceType.BtrLive)
                    return true;
            }
            else
            {
                if (bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFT + "_L")
                {
                    return true;
                }

                if (bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == BetDomainTypeLn.BET_TAG_WINFTR + "_L")
                {
                    return true;
                }
            }

            return false;
        }

        /*
        public BetDomainVw UnderOverBetDomain
        {
            get
            {
                SyncList<BetDomainLn> lBetDomains = this.LineObject.BetDomains.Clone();
                long SportId = this.SportView.LineObject.SvrGroupId;
                int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

                switch (SportId)
                {
                    case 1: //soccer
                        foreach (BetDomainLn bd in lBetDomains)
                        {
                            if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                            {
                                continue;
                            }

                            if ((period == 1 || this.LivePeriodInfo == eLivePeriodInfo.NotStarted || !this.IsLiveBet || period == 2) && bd.BetTag.Value == "TTLFT" && bd.BetDomainView.IsEnabled)
                                return bd.BetDomainView;

                            //if (period == 2 && bd.BetTag.Value == "SCR1OT" && bd.BetDomainView.IsEnabled)
                            //    return bd.BetDomainView;
                        }
                        break;
                    case 4: //ice hockey
                        foreach (BetDomainLn bd in lBetDomains)
                        {
                            if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                            {
                                continue;
                            }

                            if (bd.BetTag.Value == "TTLFT" && bd.BetDomainView.IsEnabled)
                                return bd.BetDomainView;
                        }
                        break;
                    case 2: //bascetball TTLGAM
                        foreach (BetDomainLn bd in lBetDomains)
                        {
                            if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                            {
                                continue;
                            }

                            if (bd.BetTag.Value == "TTLGAM" && bd.BetDomainView.IsEnabled)
                                return bd.BetDomainView;
                        }
                        break;
                }


                return BetDomainLn.EmptyBetDomain.BetDomainView;

            }
        }
        */

        public IBetDomainVw UnderOverBetDomain { get { return m_underOverBetDomainView; } }

        public bool IsUnderOverBetDomain(BetDomainLn bd)
        {
            int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

            if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
            {
                if ((period == 1 || this.LivePeriodInfo == eLivePeriodInfo.NotStarted || !this.IsLiveBet || period == 2 || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_1st_Period) && (bd.BetTag == "TTLFT" + "_L" || bd.BetTag == "TTLFT") && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }

                //if ((period == 2 || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_1st_Period) && bd.BetTag.Value == "SCR1OT" && bd.BetDomainView.IsEnabled)
                //{
                //    return true;
                //}

                if (bd.BetTag == "TTLOT" + "_L" && bd.BetDomainView.IsEnabled && (this.LivePeriodInfo == eLivePeriodInfo.OverTime || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_2nd_Period || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_1st_PeriodOverTime || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_1st_PeriodOverTime || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_2nd_PeriodOverTime))
                {
                    return true;
                }
                //if (period == 2 && bd.BetTag.Value == "SCR1OT" && bd.BetDomainView.IsEnabled)
                //    return bd.BetDomainView;
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
            {
                if (bd.BetTag == "TTLFT" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "TTLFT" + "_L" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
            {
                if (bd.BetTag == "TTLGAM" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "TTLGAM" + "_L" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
                if (bd.BetTag == "TTLFT" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "TTLFT" + "_L" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
            {
                if (bd.BetTag == "TTLFT" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "TTLFT" + "_L" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_RUGBY)
            {
                if (bd.BetTag == "TTLFT" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "TTLFT" + "_L" && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        /*
        public BetDomainVw BottomSpecialBetDomain
        {
            get
            {
                SyncList<BetDomainLn> lBetDomains = this.LineObject.BetDomains.Clone();
                long SportId = this.SportView.LineObject.SvrGroupId;
                int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

                switch (SportId)
                {
                    case 1: //soccer
                        foreach (BetDomainLn bd in lBetDomains)
                        {
                            if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                            {
                                continue;
                            }

                            if ((period == 2 || this.LivePeriodInfo == eLivePeriodInfo.Paused) && bd.BetTag.Value == "SCR1FTR")
                                return bd.BetDomainView;

                            if ((period == 1 || period == BetDomainMapItem.PART_UNKNOWN) && bd.BetTag.Value == BetDomainTypeLn.BET_TAG_WINHFT)
                                return bd.BetDomainView;
                        }

                        break;
                    case 5: //tennis


                        lBetDomains.Sort(delegate(BetDomainLn bdmn1, BetDomainLn bdmn2)
                        {
                            return bdmn1.BetDomainType.Part.CompareTo(bdmn2.BetDomainType.Part);
                        });

                        foreach (BetDomainLn bd in lBetDomains)
                        {
                            if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                            {
                                continue;
                            }

                            //string sBetTag = string.Format("WINS{0}", period);

                            if (BetDomainTypeLn.ARR_SET_WINNERS.Contains(bd.BetDomainType.Tag))
                                return bd.BetDomainView;
                        }
                        break;
                    case 4: //ice hockey
                    case 2: //basketball

                        lBetDomains.Sort(delegate(BetDomainLn bdmn1, BetDomainLn bdmn2)
                        {
                            return bdmn1.BetDomainType.Part.CompareTo(bdmn2.BetDomainType.Part);
                        });

                        foreach (BetDomainLn bd in lBetDomains)
                        {
                            if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                            {
                                continue;
                            }

                            if (period == 1 && bd.BetTag.Value == "DRAWQ1")
                                return bd.BetDomainView;
                            if (period == 2 && bd.BetTag.Value == "DRAWQ2")
                                return bd.BetDomainView;
                            if (period == 3 && bd.BetTag.Value == "DRAWQ3")
                                return bd.BetDomainView;
                            if (period == 4 && bd.BetTag.Value == "DRAWQ4")
                                return bd.BetDomainView;
                        }

                        break;
                    default:
                        return BetDomainLn.EmptyBetDomain.BetDomainView;
                }

                return BetDomainLn.EmptyBetDomain.BetDomainView;
            }
        }
        */

        public string TournamentNameToShow
        {
            get
            {
                return this.IsLiveBet && this.LineObject.SourceType == eServerSourceType.BtrLive ? LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely("TERMINAL_LIVE_NOTIFIER", DalStationSettings.Instance.Language) + ": " + this.TournamentView.DisplayName : this.TournamentView.DisplayName;
            }
        }

        public IBetDomainVw BottomSpecialBetDomain { get { return m_bottomSpecialBetDomainView; } }

        public bool IsBottomSpecialBetDomainView(BetDomainLn bd)
        {
            if (this.LineObject.SourceType == eServerSourceType.BtrVfl || this.LineObject.SourceType == eServerSourceType.BtrVhc)
                return false;

            int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

            if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
            {
                if ((this.LivePeriodInfo == eLivePeriodInfo.OverTime || period == 2 || this.LivePeriodInfo == eLivePeriodInfo.Paused) && bd.BetTag == "SCR1FTR" + "_L")
                {
                    return true;
                }

                if ((period == 1 || period == BetDomainMapItem.PART_UNKNOWN) && bd.BetTag == BetDomainTypeLn.BET_TAG_WINHFT + "_L")
                {
                    return true;
                }
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS && IsLiveBet)
            {
                return BetDomainTypeLn.TENNIS_SET_WINNERS.Contains(bd.BetDomainType.Tag);
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY && bd.BetTag.Contains("WINP") && bd.BetTag.Contains("_L"))
            {
                return true;
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
            {
                return BetDomainTypeLn.BASKETBALL_DRAW_BET_TAGS.Contains(bd.BetTag);
            }
            //if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL && bd.BetDomainView.IsEnabled && BetDomainTypeLn.VOLLEYALL_TOTAL_BET_TAGS.Contains(bd.BetTag.Value))
            //{
            //    return true;
            //}

            return false;
        }

        /*
        public BetDomainVw BottomUnderOverBetdomain
        {
            get 
            {
                long SportId = this.SportView.LineObject.SvrGroupId;
                int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

                SyncList<BetDomainLn> lBetDomains = this.LineObject.BetDomains.Clone();

                if (SportId == 1) //soccer
                {
                    if (period == 2)
                        return BetDomainLn.EmptyBetDomain.BetDomainView;

                    foreach (BetDomainLn bd in lBetDomains)
                    {
                        if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                        {
                            continue;
                        }

                        if (bd.BetTag.Value == "TTLHFT")
                            return bd.BetDomainView;
                    }

                }
                else if (SportId == 2) //basketball
                {
                    foreach (BetDomainLn bd in lBetDomains)
                    {
                        if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                        {
                            continue;
                        }

                        if (period == 1 && bd.BetTag.Value == "TTLQ1")
                            return bd.BetDomainView;
                        if (period == 2 && bd.BetTag.Value == "TTLQ2")
                            return bd.BetDomainView;
                        if (period == 3 && bd.BetTag.Value == "TTLQ3")
                            return bd.BetDomainView;
                        if (period == 4 && bd.BetTag.Value == "TTLQ4")
                            return bd.BetDomainView;
                    }
                }
                else if (SportId == 4) //ice hockey
                {
                    //foreach (BetDomainLn bd in lBetDomains)
                    //{
                    //    if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                    //    {
                    //        continue;
                    //    }

                    //    if (bd.BetTag.Value == "SCR1OT")
                    //        return bd.BetDomainView;
                    //}
                }

                return BetDomainLn.EmptyBetDomain.BetDomainView;
            }
        }
        */

        public IBetDomainVw BottomUnderOverBetdomain { get { return m_bottomUnderOverBetdomainView; } }

        public bool IsBottomUnderOverBetdomainView(BetDomainLn bd)
        {
            if (this.LineObject.SourceType == eServerSourceType.BtrVfl || this.LineObject.SourceType == eServerSourceType.BtrVhc)
                return false;

            int period = LiveMatchInfoLn.LivePeriodInfoToTimeType(this.LivePeriodInfo);

            if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER) //soccer
            {
                if (period == 2 || this.LivePreviousPeriodInfo == eLivePeriodInfo.Soccer_1st_Period)
                {
                    return false;
                }
                if (bd.BetTag == "TTLHFT")
                {
                    return true;
                }
                if (IsLiveBet && bd.BetTag == "TTLHFT" + "_L")
                {
                    return true;
                }
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL) //basketball
            {
                if (BetDomainTypeLn.BASKETBALL_TOTAL_BET_TAGS.Contains(bd.BetTag) && bd.BetDomainView.IsEnabled)
                {
                    return true;
                }
            }
            else if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY) //ice hockey
            {
                //foreach (BetDomainLn bd in lBetDomains)
                //{
                //    if (bd.Status.Value == eBetDomainStatus.Hidden || bd.Status.Value == eBetDomainStatus.Forbidden || (int)bd.Status.Value == 8)
                //    {
                //        continue;
                //    }

                //    if (bd.BetTag.Value == "SCR1OT")
                //        return bd.BetDomainView;
                //}
                return BetDomainTypeLn.BASKETBALL_TOTAL_BET_TAGS.Contains(bd.BetTag);
            }

            return false;
        }

        private bool _homeGoal = false;
        private bool _awayGoal = false;

        public bool HomeTeamGoal
        {
            get
            {
                return _homeGoal;
            }

            set
            {
                _homeGoal = value;
                DoPropertyChanged("HomeTeamGoal");
            }
        }

        public bool AwayTeamGoal
        {
            get
            {
                return _awayGoal;
            }

            set
            {
                _awayGoal = value;
                DoPropertyChanged("AwayTeamGoal");
            }
        }

        public string GoalsTextTest
        {
            get
            {
                CalculateGoals();
                return "";
            }
        }

        public void CalculateGoals()
        {
            if (this.SportDescriptor != SportSr.SPORT_DESCRIPTOR_SOCCER || !this.IsLiveBet)
                return;

            if (IsStartUp)
            {
                IsStartUp = false;
                AwayTeamGoal = false;
                HomeTeamGoal = false;
                return;
            }

            int posOld = this.LineObject.LiveMatchInfo.Score.PreviousValue != null ? this.LineObject.LiveMatchInfo.Score.PreviousValue.IndexOf("(") : 0;
            int posNew = this.LineObject.LiveMatchInfo.Score.Value != null ? this.LineObject.LiveMatchInfo.Score.Value.IndexOf("(") : 0;

            string oldScore = "";
            string newScore = "";

            if (string.IsNullOrEmpty(this.LineObject.LiveMatchInfo.Score.PreviousValue))
                oldScore = "0:0";
            else
                oldScore = posOld > 0 ? this.LineObject.LiveMatchInfo.Score.PreviousValue.Substring(0, posOld) : this.LineObject.LiveMatchInfo.Score.PreviousValue;

            if (string.IsNullOrEmpty(this.LineObject.LiveMatchInfo.Score.Value))
                newScore = "0:0";
            else
                newScore = posNew > 0 ? this.LineObject.LiveMatchInfo.Score.Value.Substring(0, posNew) : this.LineObject.LiveMatchInfo.Score.Value;

            string[] oldvalues = oldScore.Split(':');
            string[] newValues = newScore.Split(':');

            bool homeGoal = Convert.ToInt32(oldvalues[0]) < Convert.ToInt32(newValues[0]);
            bool awayGoal = Convert.ToInt32(oldvalues[1]) < Convert.ToInt32(newValues[1]);

            if (homeGoal)
            {
                HomeTeamGoal = true;

                GoalsTimer = new Timer();
                GoalsTimer.Interval = 10000;
                GoalsTimer.Elapsed += ClearGoals;
                GoalsTimer.Start();
                return;
            }

            if (awayGoal)
            {
                AwayTeamGoal = true;

                GoalsTimer = new Timer();
                GoalsTimer.Interval = 10000;
                GoalsTimer.Elapsed += ClearGoals;
                GoalsTimer.Start();

            }
        }

        public System.Timers.Timer GoalsTimer { get; set; }

        private void ClearGoals(object sender, ElapsedEventArgs e)
        {
            GoalsTimer.Stop();
            HomeTeamGoal = false;
            AwayTeamGoal = false;
        }

        public bool IsStartUp
        {
            get { return _isStartUp; }
            set { _isStartUp = value; }
        }


        public SyncList<IBetDomainVw> VisibleBetDomains { get { return m_lVisibleBetDomains; } }


        public SyncObservableCollection<IBetDomainVw> VisibleBetDomainViews
        {
            get
            {
                m_socVisibleBetDomainViews = new SyncObservableCollection<IBetDomainVw>();
                BindingOperations.EnableCollectionSynchronization(m_socVisibleBetDomainViews, m_socVisibleBetDomainViews.Locker);
                m_lVisibleBetDomains.Sort(delegate(IBetDomainVw bdmn1, IBetDomainVw bdmn2)
                {

                    if (bdmn1.Sort == bdmn2.Sort)
                    {
                        if (bdmn1.BetTypeName == bdmn2.BetTypeName)
                        {
                            return bdmn2.BetDomainId.CompareTo(bdmn1.BetDomainId);
                        }
                        return bdmn1.BetTypeName.CompareTo(bdmn2.BetTypeName);
                    }

                    return bdmn1.Sort.CompareTo(bdmn2.Sort);
                });
                m_socVisibleBetDomainViews.ApplyChanges(m_lVisibleBetDomains);

                return m_socVisibleBetDomainViews;
            }
        }

        public long StreamID { get; set; }

        public bool HaveStream
        {
            get { return _haveStream; }
            set
            {
                if (_haveStream == value)
                    return;
                _haveStream = value;
                AddToChangedPropNames("HaveStream");
                DoPropertyChanged("HaveStream");
            }
        }

        public bool StreamStarted
        {
            get { return _streamStarted; }
            set
            {
                if (_streamStarted == value)
                    return;
                _streamStarted = value;
                AddToChangedPropNames("StreamStarted");
                DoPropertyChanged("StreamStarted");

            }
        }

        public Visibility ShowHeaderDetails
        {
            get
            {
                if (this.LineObject.SourceType == eServerSourceType.BtrVfl)
                    return System.Windows.Visibility.Collapsed;
                else
                    return System.Windows.Visibility.Visible;
            }
        }

        public void RefreshProps()
        {
            try
            {
                if (m_iAllBetDomainCount != m_objLine.BetDomains.Count)
                {
                    m_iAllBetDomainCount = m_objLine.BetDomains.Count;
                    AddToChangedPropNames("AllBetDomainCount");
                    DoPropertyChanged("AllBetDomainCount");
                }
                allBetDomainViews.Clear();
                int iAllVisibleOddCount = 0;
                int iAllEnabledOddCount = 0;
                int iVisibleOddCount = 0;
                var hsVisibleDomains = new SyncHashSet<IBetDomainVw>();
                var lVisibleBetDomains = m_lVisibleBetDomains.Clone();
                var lBetDomains = m_objLine.BetDomains.Clone();
                int iBasketOddCount = 0;

                lBetDomains.Sort(delegate(BetDomainLn bdmn1, BetDomainLn bdmn2)
                {
                    if (bdmn1.Sort.Value == bdmn2.Sort.Value)
                    {
                        if (bdmn1.BetDomainType.Part == bdmn2.BetDomainType.Part)
                        {
                            return bdmn2.Id.CompareTo(bdmn1.Id);
                        }
                        return bdmn1.BetDomainType.Part.CompareTo(bdmn2.BetDomainType.Part);
                    }

                    return bdmn1.Sort.Value.CompareTo(bdmn2.Sort.Value);
                });

                IBetDomainVw baseBetDomainView = BetDomainLn.EmptyBetDomain.BetDomainView;
                IBetDomainVw underOverBetDomainView = BetDomainLn.EmptyBetDomain.BetDomainView;
                IBetDomainVw bottomUnderOverBetdomainView = BetDomainLn.EmptyBetDomain.BetDomainView;
                IBetDomainVw bottomSpecialBetDomainView = BetDomainLn.EmptyBetDomain.BetDomainView;

                if (m_objLine.LiveMatchInfo != null)
                {
                    m_iLiveMatchMinuteEx = Math.Max(m_iLiveMatchMinuteEx, m_objLine.LiveMatchInfo.MatchMinute.Value);
                }

                string tournamtntId = this.TournamentView.LineObject.SvrGroupId.ToString();
                ActiveTournamentLn at = null;
                foreach (var bdmn in lBetDomains)
                {
                    allBetDomainViews.Add(bdmn.BetDomainView);
                    if (this.LineObject.SourceType == eServerSourceType.BtrPre)
                    {
                        at = LineSr.Instance.AllObjects.ActiveTournaments.SafelyGetValue(tournamtntId);
                        if (at == null || !at.Markets.Split(',').Contains(bdmn.BetDomainNumber.Value.ToString()))
                            continue;
                    }

                    if (bdmn.Status.Value == eBetDomainStatus.Visible || bdmn.Status.Value == eBetDomainStatus.Inactive)
                    {
                        var lOdds = bdmn.Odds.Clone();

                        int iBetDomainOddsCount = 0;
                        int BetDomainEnbaledOddsCount = 0;

                        foreach (var odd in lOdds)
                        {
                            if (odd.Active.Value && odd.Value.Value >= OddLn.VALID_ODD_VALUE && !odd.IsManuallyDisabled.Value && !bdmn.IsManuallyDisabled.Value)
                            {
                                iBetDomainOddsCount++;
                            }
                            if (odd.Active.Value && odd.Value.Value >= OddLn.VALID_ODD_VALUE && odd.IsEnabled && !odd.IsManuallyDisabled.Value && !bdmn.IsManuallyDisabled.Value)
                            {
                                BetDomainEnbaledOddsCount++;
                            }
                        }

                        if (iBetDomainOddsCount == 0)
                        {
                            continue;
                        }

                        if (!bdmn.IsManuallyDisabled.Value)
                        {
                            iAllVisibleOddCount += iBetDomainOddsCount;
                            iAllEnabledOddCount += BetDomainEnbaledOddsCount;
                            hsVisibleDomains.Add(bdmn.BetDomainView);
                        }



                        if (!lVisibleBetDomains.Contains(bdmn.BetDomainView) && !bdmn.IsManuallyDisabled.Value)
                        {
                            lVisibleBetDomains.Add(bdmn.BetDomainView);
                        }

                        iBasketOddCount += iBetDomainOddsCount;

                        // Base BetDomain
                        if (IsBaseBetDomain(bdmn))
                        {
                            baseBetDomainView = bdmn.BetDomainView;
                            continue;
                        }

                        // Under/Over BetDomain
                        if (IsUnderOverBetDomain(bdmn))
                        {
                            underOverBetDomainView = bdmn.BetDomainView;
                            continue;
                        }

                        // Bottom Under/Over BetDomain
                        if (IsBottomUnderOverBetdomainView(bdmn))
                        {
                            bottomUnderOverBetdomainView = bdmn.BetDomainView;
                            DoPropertyChanged("SecondMatchButtonRowText");
                            continue;
                        }

                        // Bottom Special BetDomain
                        if (IsBottomSpecialBetDomainView(bdmn))
                        {
                            bottomSpecialBetDomainView = bdmn.BetDomainView;
                            DoPropertyChanged("SecondMatchButtonRowText");
                            continue;
                        }

                        iVisibleOddCount += iBetDomainOddsCount;
                    }
                }

                for (int i = 0; i < lVisibleBetDomains.Count; )
                {
                    IBetDomainVw betDomainView = lVisibleBetDomains[i];

                    if (!hsVisibleDomains.Contains(betDomainView))
                    {
                        lVisibleBetDomains.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                lVisibleBetDomains.Sort(delegate(IBetDomainVw bdmn1, IBetDomainVw bdmn2)
                {
                    int iSort1 = bdmn1.Sort;
                    int iSort2 = bdmn2.Sort;


                    if (iSort1 == iSort2)
                    {
                        return bdmn1.BetDomainId.CompareTo(bdmn2.BetDomainId);
                    }

                    return iSort1.CompareTo(iSort2);
                });

                m_lVisibleBetDomains.SafelySynchronize(lVisibleBetDomains);
                if (m_socVisibleBetDomainViews != null)
                {
                    m_socVisibleBetDomainViews.ApplyChanges(lVisibleBetDomains);

                    //SafelyApplyChanges(lVisibleBetDomains, m_socVisibleBetDomainViews);
                }

                if (m_iVisibleBetDomainCount != hsVisibleDomains.Count)
                {
                    m_iVisibleBetDomainCount = hsVisibleDomains.Count;
                    AddToChangedPropNames("VisibleBetDomainCount");
                    DoPropertyChanged("VisibleBetDomainCount");
                }

                if (m_iAllVisibleOddCount != iAllVisibleOddCount)
                {
                    m_iAllVisibleOddCount = iAllVisibleOddCount;
                    AddToChangedPropNames("AllVisibleOddCount");
                    DoPropertyChanged("AllVisibleOddCount");
                }

                if (m_iVisibleOddCount != iVisibleOddCount)
                {
                    m_iVisibleOddCount = iVisibleOddCount;
                    AddToChangedPropNames("VisibleOddCount");
                    DoPropertyChanged("VisibleOddCount");
                }

                if (m_iAllEnabledOddCount != iAllEnabledOddCount)
                {
                    m_iAllEnabledOddCount = iAllEnabledOddCount;
                    AddToChangedPropNames("AllEnabledOddCount");
                    DoPropertyChanged("AllEnabledOddCount");
                }

                // Base BetDomain
                if (m_baseBetDomainView != baseBetDomainView)
                {
                    m_baseBetDomainView = baseBetDomainView;

                    AddToChangedPropNames("BaseBetDomainView");
                    DoPropertyChanged("BaseBetDomainView");
                }

                // Under/Over BetDomain
                if (m_underOverBetDomainView != underOverBetDomainView)
                {
                    m_underOverBetDomainView = underOverBetDomainView;

                    AddToChangedPropNames("UnderOverBetDomain");
                    DoPropertyChanged("UnderOverBetDomain");
                }

                // Bottom Under/Over BetDomain
                if (m_bottomUnderOverBetdomainView != bottomUnderOverBetdomainView)
                {
                    m_bottomUnderOverBetdomainView = bottomUnderOverBetdomainView;

                    AddToChangedPropNames("BottomUnderOverBetdomain");
                    DoPropertyChanged("BottomUnderOverBetdomain");
                    DoPropertyChanged("LiveSecondRowVisibility");
                }

                // Bottom Special BetDomain
                if (m_bottomSpecialBetDomainView != bottomSpecialBetDomainView)
                {
                    m_bottomSpecialBetDomainView = bottomSpecialBetDomainView;

                    AddToChangedPropNames("BottomSpecialBetDomain");
                    DoPropertyChanged("BottomSpecialBetDomain");
                    DoPropertyChanged("LiveSecondRowVisibility");
                }

                if (m_iBasketOddCount != iBasketOddCount)
                {
                    m_iBasketOddCount = iBasketOddCount;
                    DoPropertyChanged("BasketOddCount");
                }
                var newMoreBetdomain = VisibleBetDomainCount;
                if (BaseBetDomainView.Visibility == Visibility.Visible)
                    newMoreBetdomain -= 1;
                if (BottomSpecialBetDomain.Visibility == Visibility.Visible)
                    newMoreBetdomain -= 1;
                if (UnderOverBetDomain.Visibility == Visibility.Visible)
                    newMoreBetdomain -= 1;
                if (BottomUnderOverBetdomain.Visibility == Visibility.Visible)
                    newMoreBetdomain -= 1;
                if (moreBetdomainCount != newMoreBetdomain)
                {
                    moreBetdomainCount = newMoreBetdomain;
                    DoPropertyChanged("MoreBetdomainCount");

                }
            }
            catch (Exception excp)
            {
                m_logger.Error(excp.Message, excp);
                m_logger.Excp(excp, "RefreshProps() ERROR for {0}", m_objLine);

                StackTrace st = new StackTrace();

                m_logger.WarnFormat("RefreshProps() Stack:\r\n{0}", st.ToString());
            }
        }

        public int DefaultSorting
        {
            get
            {
                int sort = 9999;

                string descriptor = this.SportDescriptor;
                if (descriptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    sort = 1;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    sort = 2;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    sort = 3;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    sort = 4;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    sort = 5;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    sort = 6;

                return sort;
            }
        }

        public int ChnagedCount { get { return m_objLine.ChangedCount.Value; } }



        protected override void OnRaisePropertiesChanged<T>(T objLine)
        {
            base.OnRaisePropertiesChanged<T>(objLine);
            RefreshProps();

            var mtch = objLine as MatchLn;

            if (mtch != null && mtch.ChangedProps.Contains(mtch.ChangedCount))
            {
                mtch.NotifyBetDomainsEnabledChanged();
            }
        }

        public string LiveScore
        {
            get { return m_objLine.LiveMatchInfo == null ? string.Empty : m_objLine.LiveMatchInfo.Score.Value; }
        }

        public System.Windows.Visibility LiveSecondRowVisibility
        {
            get
            {
                if (!this.IsLiveBet || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_RUGBY || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    return System.Windows.Visibility.Collapsed;

                if (this.LineObject.SourceType == eServerSourceType.BtrVfl)
                    return System.Windows.Visibility.Collapsed;

                if (this.BottomSpecialBetDomain != BetDomainLn.EmptyBetDomain.BetDomainView || this.BottomUnderOverBetdomain != this.BottomSpecialBetDomain)
                    return System.Windows.Visibility.Visible;

                return System.Windows.Visibility.Collapsed;
            }
        }

        public delegate bool DelegateFilterBetDomains(BetDomainLn bdl);

        public void SearchBetDomains(SortableObservableCollection<IBetDomainVw> ocBetDomains, DelegateFilterBetDomains dfbd)
        {
            ocBetDomains.ApplyChanges(m_lVisibleBetDomains.Clone());
        }

        public override System.Windows.Visibility Visibility
        {
            get
            {
                if (m_objLine.LiveMatchInfo != null)
                {
                    return
                        m_objLine.LiveMatchInfo.Status.Value == eMatchStatus.NotStarted ||
                        m_objLine.LiveMatchInfo.Status.Value == eMatchStatus.Started ||
                        m_objLine.LiveMatchInfo.Status.Value == eMatchStatus.Stopped
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                }
                if (!IsLiveBet)
                {
                    return System.Windows.Visibility.Visible;
                }

                return System.Windows.Visibility.Collapsed;
            }
        }

        public int? MinTournamentCombination
        {
            get
            {
                return TournamentView.LineObject.GroupTournament.MinCombination;
            }
        }


        public string SportIcon
        {
            get
            {

                string BetcenterImageRelativePath = ConfigurationManager.AppSettings["betcenter_images_relative_path"];
                string WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

                if (string.IsNullOrEmpty(BetcenterImageRelativePath) || string.IsNullOrEmpty(WorkingDirectory))
                    return "";

                var m_sportGroup = this.SportView.LineObject;
                Debug.Assert(m_sportGroup != null && m_sportGroup.GroupSport != null &&
                             !string.IsNullOrEmpty(m_sportGroup.GroupSport.SportDescriptor));

                string sSportDescroptor = m_sportGroup.GroupSport.SportDescriptor;

                string path = WorkingDirectory + BetcenterImageRelativePath;
                if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    path += "Icon_Icehockey.png";
                else if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    path += "Icon_basketball.png";
                else if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    path += "Icon_handball.png";
                else if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    path += "Icon_footbal.png";
                else if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    path += "Icon_soccer.png";
                else if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    path += "Icon_Tennis.png";

                return path;
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return m_objLine.IsEnabled;
            }
        }

        public override int GetHashCode()
        {
            return m_objLine.MatchId.GetHashCode();
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
        private bool _isHeaderForLiveMonitor;
        private bool _isStartUp = true;

        public bool IsHeaderForLiveMonitor
        {
            get
            {
                return _isHeaderForLiveMonitor;
            }
            set
            {
                if (_isHeaderForLiveMonitor != value)
                {
                    _isHeaderForLiveMonitor = value;
                    AddToChangedPropNames("IsHeaderForLiveMonitor");
                    DoPropertyChanged("IsHeaderForLiveMonitor");
                }
            }
        }

        private bool _isHeaderForPreMatch;
        private bool _haveStream;
        private bool _streamStarted;

        public bool IsHeaderForPreMatch
        {
            get
            {
                return _isHeaderForPreMatch;
            }
            set
            {
                if (_isHeaderForPreMatch != value)
                {
                    _isHeaderForPreMatch = value;
                    AddToChangedPropNames("IsHeaderForPreMatch");
                    DoPropertyChanged("IsHeaderForPreMatch");
                }
            }
        }

        private bool _isHeaderForRotation;
        public bool IsHeaderForRotation
        {
            get
            {
                return _isHeaderForRotation;
            }
            set
            {
                if (_isHeaderForRotation != value)
                {
                    _isHeaderForRotation = value;
                    AddToChangedPropNames("IsHeaderForRotation");
                    DoPropertyChanged("IsHeaderForRotation");
                }
            }
        }

        public Visibility ShowXTip
        {
            get
            {
                if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL || this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility ShowUOBetDomain
        {
            get
            {
                if (this.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public override bool Equals(object obj)
        {
            var mv = obj as MatchVw;

            return mv != null ? m_objLine.MatchId.Equals(mv.m_objLine.MatchId) : base.Equals(obj);
        }

        public bool ShowSoccerLiveBetDomainHeader
        {
            get
            {
                if (Sport.GroupView.DisplayName.Equals("Soccer") && BaseBetDomainView.BetTag.Contains("WINFTR") && IsHeader)
                {
                    return true;
                }
                return false;
            }
        }

        public bool ShowIceHockeyLiveBetDomainHeader
        {
            get
            {
                if (Sport.GroupView.DisplayName.Equals("Ice Hockey") && BaseBetDomainView.BetTag.Contains("WINFTR") && IsHeader)
                {
                    return true;
                }
                return false;
            }
        }

        private DateTime _lastPlayedStreamAt = DateTime.Now;
        public DateTime LastPlayedStreamAt { get { return _lastPlayedStreamAt; } set { _lastPlayedStreamAt = value; } }

    }

    public class OutrightCompetitorVw
    {
        private CompetitorToOutrightLn m_cto = null;
        private CompetitorLn m_competitor = null;

        public OutrightCompetitorVw(CompetitorToOutrightLn cto)
        {
            m_cto = cto;
        }

        public CompetitorLn Competitor
        {
            get
            {
                if (m_competitor == null)
                {
                    m_competitor = LineSr.Instance.AllObjects.Competitors.GetObject(m_cto.CompetitorId);
                    ExcpHelper.ThrowIf(m_competitor == null, "Cannot get competitor for {0}", m_cto);
                }

                return m_competitor;
            }
        }

        public long Position { get { return m_cto.hometeam; } }
        public string Name { get { return this.Competitor.GetDisplayName(DalStationSettings.Instance.Language); } }

        public IOddVw WinOdd
        {
            get
            {
                IOddVw oddToReturn = OddLn.EmptyOdd.OddView;

                MatchLn match = LineSr.Instance.AllObjects.Matches.SafelyGetValue(m_cto.MatchId);
                BetDomainLn bd = null;
                OddLn od = null;
                if (match != null)
                    bd = match.BetDomains.Where(x => x.BetTag == "VHCWNR").FirstOrDefault();
                if (bd != null)
                    od = bd.Odds.Where(x => x.OddTag.Value == this.Position.ToString()).FirstOrDefault();
                if (od != null)
                    oddToReturn = od.OddView;
                return oddToReturn;
            }
        }

        public IOddVw LayWinOdd
        {
            get
            {
                IOddVw oddToReturn = OddLn.EmptyOdd.OddView;

                MatchLn match = LineSr.Instance.AllObjects.Matches.SafelyGetValue(m_cto.MatchId);
                BetDomainLn bd = null;
                OddLn od = null;
                if (match != null)
                    bd = match.BetDomains.Where(x => x.BetTag == "VHCWNRL").FirstOrDefault();
                if (bd != null)
                    od = bd.Odds.Where(x => x.OddTag.Value == this.Position.ToString()).FirstOrDefault();
                if (od != null)
                    oddToReturn = od.OddView;
                return oddToReturn;
            }
        }

        public IOddVw PlaceOdd
        {
            get
            {
                IOddVw oddToReturn = OddLn.EmptyOdd.OddView;

                MatchLn match = LineSr.Instance.AllObjects.Matches.SafelyGetValue(m_cto.MatchId);
                BetDomainLn bd = null;
                OddLn od = null;
                if (match != null)
                    bd = match.BetDomains.Where(x => x.BetTag == "VHCPL").FirstOrDefault();
                if (bd != null)
                    od = bd.Odds.Where(x => x.OddTag.Value == this.Position.ToString()).FirstOrDefault();
                if (od != null)
                    oddToReturn = od.OddView;
                return oddToReturn;
            }
        }

        public IOddVw LayPlaceOdd
        {
            get
            {
                IOddVw oddToReturn = OddLn.EmptyOdd.OddView;

                MatchLn match = LineSr.Instance.AllObjects.Matches.SafelyGetValue(m_cto.MatchId);
                BetDomainLn bd = null;
                OddLn od = null;
                if (match != null)
                    bd = match.BetDomains.Where(x => x.BetTag == "VHCLPL").FirstOrDefault();
                if (bd != null)
                    od = bd.Odds.Where(x => x.OddTag.Value == this.Position.ToString()).FirstOrDefault();
                if (od != null)
                    oddToReturn = od.OddView;
                return oddToReturn;
            }
        }

        private ImageSource _shirtImage = null;
        public ImageSource ShirtImage
        {
            get
            {
                //string baseData = Competitor.Base64Image.Value;
                //if(String.IsNullOrEmpty(baseData))
                //    return new BitmapImage();

                //byte[] data = Convert.FromBase64String(baseData);
                //var source = new BitmapImage();
                //source.BeginInit();
                //source.StreamSource = new MemoryStream(data);
                //source.EndInit();
                //source.Freeze();
                SetShirt();

                if (_shirtImage == null)
                    return new BitmapImage();

                return _shirtImage;
            }
        }

        private void SetShirt()
        {
            if (_shirtImage != null)
                return;

            string baseData = Competitor.Base64Image;
            if (String.IsNullOrEmpty(baseData))
                return;

            byte[] data = Convert.FromBase64String(baseData);
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = new MemoryStream(data);
            source.EndInit();
            source.Freeze();

            _shirtImage = source;
        }
    }
}
