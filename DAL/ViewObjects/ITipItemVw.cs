using System;
using System.Windows;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public interface ITipItemVw
    {
        bool IsChecked { get; set; }
        bool IsBankReadOnly { get; set; }
        IOddLn Odd { get; }
        bool IsBank { get; set; }
        IMatchLn Match { get; }
        bool IsLiveBet { get; }
        bool IsWay { get; set; }
        decimal Value { get; set; }
        bool IsSelected { get; set; }
        bool IsBankEnabled { get; set; }
        bool IsBankEditable { get; set; }
        IBetDomainLn BetDomain { get; }
        bool IsEnabled { get; }
        IOddVw OddView { get; }
        string BetDomainName { get; }
        int MatchCode { get; }
        string SportName { get; }
        DateTime StartDate { get; }
        string TournamentName { get; }
        string HomeCompetitor { get; }
        string AwayCompetitor { get; }
        Visibility AreAdditionalOddsNumberVisible { get; }
    }
}