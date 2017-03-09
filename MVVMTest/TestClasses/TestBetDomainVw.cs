using System;
using System.Data;
using System.Windows;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestBetDomainVw : IBetDomainVw
    {
        public long BetDomainId { get; set; }
        public SortableObservableCollection<IOddVw> Odds { get; set; }
        public string DisplayName { get; set; }
        public int Sort { get; set; }
        public bool IsToInverse { get; set; }
        public bool IsEnabled { get; set; }
        public string SpecialOddValue { get; set; }
        public Visibility Visibility { get; set; }
        public string BetTypeName { get; set; }
        public string BetTag { get; set; }
        public string ScoreTypeName { get; set; }
        public string TimeTypeName { get; set; }
        public IMatchVw MatchView { get; set; }
        public IBetDomainLn LineObject { get; private set; }
        public IOddVw Odd1 { get; private set; }
        public IOddVw Odd2 { get; private set; }
        public IOddVw Odd3 { get; private set; }
        public SyncObservableCollection<IOddVw> OddViews { get; private set; }

        public bool IsSelected { get; private set; }

        public void DoPropertyChanged(string name)
        {

        }

        public bool ContainsOdds()
        {
            throw new NotImplementedException();
        }

        public static IBetDomainVw CreateBetDomain()
        {
            var testBetdomain = new TestBetDomainVw();
            return testBetdomain;
        }
    }
}