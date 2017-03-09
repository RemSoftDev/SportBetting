using System;
using System.Data;
using System.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.Common.Collections;

namespace MVVMTest
{
    public class TestOddVw : IOddVw
    {
        public long OddId { get; set; }
        public OddLn LineObject { get; set; }
        public bool IsEnabled { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameForPrinting(string language)
        {
            throw new NotImplementedException();
        }

        public IBetDomainVw BetDomainView { get; set; }
        public bool IsSelected { get; set; }
        public int Sort { get; set; }
        public bool IsVisible { get; set; }
        public decimal Value { get; set; }
        public Visibility Visibility { get; set; }
        public string DisplayValue { get; set; }
        
        public void DoPropertyChanged(string name)
        {
            
        }

        public string SpecialBetdomainValue { get; set; }

        public bool ChangedUp { get; set; }

        public bool ChangedDown { get; set; }

        public SyncList<CompetitorLn> OutrightsCompetitors { get { return new SyncList<CompetitorLn>(); } }
    }
}