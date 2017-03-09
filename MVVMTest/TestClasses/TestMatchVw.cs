using System;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestMatchVw : IMatchVw
    {
        public IGroupVw SportView { get; set; }
        public eMatchStatus LiveBetStatus { get; set; }
        public eLivePeriodInfo LivePeriodInfo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int LiveMatchMinute { get; set; }
        public int LiveMatchMinuteEx { get; set; }
        public int AllBetDomainCount { get; set; }
        public int VisibleBetDomainCount { get; set; }
        public int VisibleOddCount { get; set; }
        public IBetDomainVw BaseBetDomainView { get; set; }
        public IBetDomainVw UnderOverBetDomain { get; set; }
        public IBetDomainVw BottomSpecialBetDomain { get; set; }
        public string LiveScore { get; set; }
        public bool IsEnabled { get; set; }
        public string SportDescriptor { get; set; }
        public int AllVisibleOddCount { get; set; }
        public int MinCombination { get; set; }
        public MatchLn LineObject { get; set; }
        public bool IsStartUp { get; set; }
        public Timer GoalsTimer { get; set; }
        public Visibility Visibility { get; set; }
        public int? MinTournamentCombination { get; set; }
        public long StreamID { get; set; }
        public DateTime LastPlayedStreamAt { get; set; }
        public Visibility ShowHeaderDetails { get; set; }
        public Visibility ShowXTip { get; set; }
        public Visibility ShowUOBetDomain { get; set; }

        public void DoPropertyChanged(string name)
        {

        }

        public bool ShowGreenBackgroungForOddCount { get; set; }
        public DateTime StartDate { get; set; }
        public string Name { get; set; }
        public bool IsHeader { get; set; }
        public bool IsHeaderForLiveMonitor { get; set; }
        public bool IsHeaderForPreMatch { get; set; }
        public bool IsHeaderForRotation { get; set; }
        public bool IsOutright { get; set; }
        public SyncObservableCollection<IBetDomainVw> VisibleBetDomainViews { get; set; }
        public bool HaveStream { get; set; }
        public bool StreamStarted { get; set; }
        public Visibility LiveMInuteVisibility { get; set; }
        public Visibility InversedLiveMInuteVisibility { get; set; }
        public IGroupVw CategoryView { get; set; }
        public IGroupVw TournamentView { get; set; }
        public IGroupVw CountryView { get; set; }
        public int Code { get; set; }
        public string HomeCompetitorName { get; set; }
        public string AwayCompetitorName { get; set; }
        public bool Active { get; set; }
        public bool IsLiveBet { get; set; }
        public string TournamentNameToShow { get; set; }
        public int DefaultSorting { get; set; }
        public int VirtualSeason { get; private set; }
        public int VirtualDay { get; private set; }
        public SyncObservableCollection<OutrightCompetitorVw> OutrightCompetitorsVHC { get; private set; }

        public SyncObservableCollection<int> AwayTeamRedCards { get; set; }

        public SyncObservableCollection<int> HomeTeamRedCards { get; set; }

        public Visibility ShomMinutes { get; set; }

        public string LivePeriodInfoString { get; set; }

        public int AllEnabledOddCount { get; set; }
        public eServerSourceType? MatchSource { get; private set; }

        public int BasketOddCount { get; private set; }

        public string LiveColor { get; set; }
        public LinearGradientBrush LiveGradientColor { get; set; }

        public void RefreshProps()
        {
            throw new NotImplementedException();
        }

        public static IMatchVw CreateMatch(long id, bool isLiveBet = false, bool isOutright = false)
        {
            var match = new TestMatchVw()
                {
                    LineObject = new MatchLn() { BtrMatchId = id },
                    SportView = TestGroupVw.CreateGroup(id,true),
                    CategoryView = TestGroupVw.CreateGroup(id,false),
                    TournamentView = TestGroupVw.CreateGroup(id,false),
                    VisibleBetDomainCount = 10,
                    AllVisibleOddCount = 10,
                    IsOutright = isOutright,
                    IsLiveBet = isLiveBet,
                    IsEnabled = true,
                    Name = "Test",
                    StartDate = DateTime.Now,
                    Visibility = Visibility.Visible,
                    SportDescriptor = id.ToString(),
                };
            if (!isLiveBet)
            {
                match.StartDate = DateTime.Now.AddDays(1);
            }
            match.TournamentView = TestGroupVw.CreateGroup(id,false);
            return match;
        }
    }
}