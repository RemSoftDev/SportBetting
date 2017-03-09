using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Shared;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public interface ITicketHandler
    {
        Tuple<MultistringTag, string[], bool> OnChangeStake(string stake, Ticket ticket, decimal cashpool);
        void UpdateTicket();
        TicketStates TicketState { get; set; }
        decimal CurrentTicketPossibleWin { get; }
        decimal TotalOddDisplay { get; }
        int Count { get; }
        string BonusValueRounded { get; }
        decimal BonusValue { get; }
        decimal ManipulationFeeValue { get; }
        decimal Stake { get; }
        decimal MaxWin { get; }
        decimal MaxBet { get; }
        decimal MinBet { get; }
        decimal BonusPercentage { get; }
        decimal ManipulationFeePercentage { get; }
        SyncObservableCollection<Ticket> TicketsInBasket { get; }
        bool IsVisibleBank { get; }
        string ManipulationFeeRounded { get; }
        event PropertyChangedEventHandler PropertyChanged;
        void CopyValues();
    }
}