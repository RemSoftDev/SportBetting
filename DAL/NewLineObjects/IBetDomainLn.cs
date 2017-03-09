using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public interface IBetDomainLn
    {
        OddList Odds { get; }
        IMatchLn Match { get;  }
        IBetDomainVw BetDomainView { get; }
        ObservableProperty<eBetDomainStatus> Status { get; set; }
        long BetDomainId { get; set; }
        string GetDisplayName(string language);
        ObservableProperty<int> BetDomainNumber { get; set; }
        string BetTag { get; set; }
        ObservableProperty<string> SpecialOddValue { get; set; }
        ObservableProperty<int> ChangedCount { get; set; }
        ObservableProperty<string> SpecialOddValueFull { get; set; }
        ObservableProperty<bool> IsLiveBet { get; set; }
        ObservableProperty<long> BtrLiveBetId { get; set; }
        ObservableProperty<string> Result { get; set; }
        string ExtendedState { get; set; }
        void SetSelected(IOddLn odd, bool bSelected);
        SyncList<IOddLn> GetSelectedOdds();
        string NameTag { get;  }
        ObservableProperty<int> Sort { get; }
        BetDomainTypeLn BetDomainType { get;  }
        long Id { get;  }
        ObservableProperty<bool> IsManuallyDisabled { get; set; }
        bool DoesViewObjectExist { get; }
        bool IsSelected { get; }
        BetDomainExternalState BetDomainExternalState { get; }
        long MatchId { get; set; }
        bool IsCashierEnabled { get; }
        void SetActiveChanged();
        void NotifyOddsEnabledChanged();
        SyncDictionary<int, OddLn> GetSortedOdds();
    }
}
