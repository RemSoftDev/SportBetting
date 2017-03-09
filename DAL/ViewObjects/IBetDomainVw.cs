using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public interface IBetDomainVw
    {
        long BetDomainId { get; }
        SortableObservableCollection<IOddVw> Odds { get; }
        string DisplayName { get; }
        int Sort { get; }
        bool IsToInverse { get; }
        bool IsEnabled { get; }
        string SpecialOddValue { get; }
        Visibility Visibility { get;  }
        string BetTypeName { get; }
        string BetTag { get;  }
        string ScoreTypeName { get;  }
        string TimeTypeName { get;  }
        IMatchVw MatchView { get;  }
        IBetDomainLn LineObject { get; }
        IOddVw Odd1 { get;  }
        IOddVw Odd2 { get;  }
        IOddVw Odd3 { get;  }
        SyncObservableCollection<IOddVw> OddViews { get; }
        bool IsSelected { get; }
        void DoPropertyChanged(string name);
        bool ContainsOdds();
    }
}
