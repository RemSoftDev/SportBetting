using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public interface IMatchVw
    {
        IGroupVw SportView { get; }
        eMatchStatus LiveBetStatus { get;  }
        eLivePeriodInfo LivePeriodInfo { get; }
        DateTime StartDate { get; }
        string Name { get; }
        bool IsHeader { get; set; }
        bool IsHeaderForLiveMonitor { get; set; }
        bool IsHeaderForRotation { get; set; }
        Visibility LiveMInuteVisibility { get; }
        Visibility InversedLiveMInuteVisibility { get; }
        IGroupVw CategoryView { get; }
        IGroupVw TournamentView { get; }
        IGroupVw CountryView { get; }
        int Code { get; }
        string HomeCompetitorName { get; }
        string AwayCompetitorName { get; }
        bool Active { get; }
        bool IsLiveBet { get; }
        DateTime ExpiryDate { get; }
        int LiveMatchMinute { get; }
        int LiveMatchMinuteEx { get; }
        int AllBetDomainCount { get; }
        int VisibleBetDomainCount { get; }
        int VisibleOddCount { get; }
        IBetDomainVw BaseBetDomainView { get; }
        IBetDomainVw UnderOverBetDomain { get; }
        IBetDomainVw BottomSpecialBetDomain { get; }
        string LiveScore { get; }
        bool IsEnabled { get; }
        string SportDescriptor { get; }
        int AllVisibleOddCount { get; }
        int MinCombination { get; }
        MatchLn LineObject { get; }
        bool IsStartUp { get; set; }
        Timer GoalsTimer { get; }
        string TournamentNameToShow { get; }
        Visibility Visibility { get; }
        int? MinTournamentCombination { get; }
        void DoPropertyChanged(string name);
        bool IsHeaderForPreMatch { get; set; }
        bool IsOutright { get; }
        SyncObservableCollection<IBetDomainVw> VisibleBetDomainViews { get; }
        bool HaveStream { get; set; }
        bool StreamStarted { get; set; }
        long StreamID { get; set; }
        DateTime LastPlayedStreamAt { get; set; }
        Visibility ShowHeaderDetails { get; }
        int DefaultSorting { get; }
        int VirtualSeason { get;  }
        int VirtualDay { get;  }
        void RefreshProps();
        Visibility ShowXTip { get; }
        Visibility ShowUOBetDomain { get; }
        SyncObservableCollection<OutrightCompetitorVw> OutrightCompetitorsVHC { get; }
        SyncObservableCollection<int> AwayTeamRedCards { get; }
        SyncObservableCollection<int> HomeTeamRedCards { get; }

        Visibility ShomMinutes { get; }
        string LivePeriodInfoString { get; }
        int AllEnabledOddCount { get; }
        eServerSourceType? MatchSource { get; }
        int BasketOddCount { get; }
    }
}
