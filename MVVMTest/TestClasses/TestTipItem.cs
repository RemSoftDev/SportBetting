using System;
using System.Data;
using System.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestTipItem : ITipItemVw
    {
        public TestTipItem(IOddLn odd)
        {
            this.Odd = odd;
            this.OddView = odd.OddView;
            this.BetDomain = odd.BetDomain;
            this.Match = this.BetDomain.Match;

            this.StartValue = this.Odd.Value.Value;
        }

        public decimal StartValue { get; set; }

        public IMatchLn Match { get; set; }
        public bool IsLiveBet { get; set; }
        public bool IsWay { get; set; }
        public decimal Value { get; set; }
        public bool IsSelected { get; set; }
        public bool IsBankEnabled { get; set; }
        public bool IsBankEditable { get; set; }

        public IBetDomainLn BetDomain { get; set; }
        public bool IsEnabled { get; set; }
        public IOddVw OddView { get; set; }
        public string BetDomainName { get; set; }
        public int MatchCode { get; private set; }
        public string SportName { get; private set; }
        public DateTime StartDate { get; private set; }
        public string TournamentName { get; private set; }
        public string HomeCompetitor { get; private set; }
        public string AwayCompetitor { get; private set; }

        public Visibility AreAdditionalOddsNumberVisible { get; private set; }

        public bool IsChecked { get; set; }
        public bool IsBankReadOnly { get; set; }
        public bool IsBank { get; set; }
        public IOddLn Odd { get; set; }

        public static ITipItemVw CreateTipItem()
        {
            var tipItem = new TestTipItem(TestOdd.CreateOdd(1, 2, true));

            return tipItem;

        }
    }
}