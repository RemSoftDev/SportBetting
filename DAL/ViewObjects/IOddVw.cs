using System.Windows;
using SportRadar.DAL.NewLineObjects;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.ViewObjects
{
    public interface IOddVw
    {
        long OddId { get;  }
        OddLn LineObject { get; }
        bool IsEnabled { get; }
        string DisplayName { get;  }
        string DisplayNameForPrinting(string language);
        IBetDomainVw BetDomainView { get; }
        bool IsSelected { get;  }
        int Sort { get; }
        bool IsVisible { get; }
        decimal Value { get; }
        string DisplayValue { get; }
        Visibility Visibility { get; }
        void DoPropertyChanged(string name);
        string SpecialBetdomainValue { get; }
        bool ChangedUp { get; }
        bool ChangedDown { get; }
        SyncList<CompetitorLn> OutrightsCompetitors { get; }
    }
}