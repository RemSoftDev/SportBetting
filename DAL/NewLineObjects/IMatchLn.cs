using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public interface IMatchLn
    {
        ObservableProperty<bool> IsLiveBet { get; set; }
        ObservableProperty<int> Code { get; set; }
        ObservableProperty<bool> Active { get; set; }
        ObservableProperty<DateTimeSr> ExpiryDate { get; set; }
        ObservableProperty<string> NameTag { get; set; }
        BetDomainList BetDomains { get; }
        IMatchVw MatchView { get; }
        SyncList<GroupLn> ParentGroups { get; }
        CompetitorLn HomeCompetitor { get; }
        CompetitorLn AwayCompetitor { get; }
        ObservableProperty<DateTimeSr> StartDate { get; set; }
        long MatchId { get; set; }
        SyncList<BetDomainLn> GetSortedBetDomains();
        IBetDomainLn SelectedBetDomain { get; }
        void SetSelected(IOddLn oddToDo, bool bIsSelected);
        LiveMatchInfoLn LiveMatchInfo { get; }
        eServerSourceType SourceType { get; }
        eOutrightType outright_type { get; }
        long VhcChannelId { get; }
        PositionToOutrightDictionary OutrightCompetitors { get; }
        long BtrMatchId { get; }
        long Sort { get; set; }
        MatchExternalState MatchExternalState { get; }
        bool IsSelected { get; }
        bool IsEnabled { get; }
        ObservableProperty<int> ChangedCount { get; set; }
        string BtrPreLiveKeyName { get; }
        ObservableProperty<DateTimeSr> EndDate { get; set; }
        ObservableProperty<long> HomeCompetitorId { get; set; }
        void SetActiveChanged();
        string GetOutrightDisplayName(string language);
        IBetDomainLn GetBaseBetDomain();
    }
}
