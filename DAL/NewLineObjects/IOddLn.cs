using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public interface IOddLn
    {
        long OutcomeId { get; set; }
        ObservableProperty<long> OddId { get; set; }
        IBetDomainLn BetDomain { get; }
        ObservableProperty<decimal> Value { get; set; }
        IOddVw OddView { get; }
        //bool IsEnabled { get; }
        ObservableProperty<bool> IsLiveBet { get; set; }
        ObservableProperty<string> OddTag { get; set; }
        ObservableProperty<int> Sort { get; set; }
        ObservableProperty<string> NameTag { get; set; }
        ObservableProperty<bool> Active { get; set; }
        ObservableProperty<string> ExtendedState { get; set; }
        ObservableProperty<bool> IsManuallyDisabled { get; set; }
        long BetDomainId { get; set; }
        bool IsEnabled { get;  }
        bool IsSelected { get;  }
        bool DoesViewObjectExist { get; }
        void SetSelected(bool bIsSelected);
        string GetDisplayName(string language);
    }
}
