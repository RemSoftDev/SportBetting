using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using IocContainer;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Annotations;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.oldcode;

namespace SportBetting.WPF.Prism.Shared.Services
{


    public class TicketHandler : ITicketHandler, INotifyPropertyChanged
    {

        public TicketHandler()
        {

        }

        public static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        private IDataBinding _dataBinding;
        public IDataBinding DataBinding
        {
            get
            {
                return _dataBinding ?? (_dataBinding = IoCContainer.Kernel.Get<IDataBinding>());
            }
        }

        private IChangeTracker _changeTracker;
        public IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }



        private SyncObservableCollection<Ticket> _ticketsInBasket = new SyncObservableCollection<Ticket>();

        public SyncObservableCollection<Ticket> TicketsInBasket
        {
            get
            {
                return _ticketsInBasket;
            }
        }

        public bool IsVisibleBank { get { return TicketsInBasket.ToSyncList().Select(x => x.IsVisibleBank).FirstOrDefault(); } }
        public void CopyValues()
        {
            if (!ChangeTracker.TicketBuyActive)
            {
                foreach (var ticket in TicketsInBasket.ToSyncList())
                {
                    foreach (var tipItemVw in ticket.TipItems.ToSyncList())
                    {
                        tipItemVw.Value = tipItemVw.OddView.Value;
                    }
                }
            }
        }

        private object _lockUpdate = new object();


        public decimal ManipulationFeePercentage
        {
            get
            {
                var ticketsInBasket = TicketsInBasket.ToSyncList();
                if (ticketsInBasket.Count == 0)
                    return 0;

                return StationRepository.GetManipulationFeePercentage(
                    TicketState == TicketStates.MultySingles
                    ? ticketsInBasket.OrderByDescending(x => x.BonusPercentage).First()
                    : ticketsInBasket[0]
                    );
            }
        }

        public decimal BonusPercentage
        {
            get
            {
                var ticketsInBasket = TicketsInBasket.ToSyncList();
                if (ticketsInBasket.Count == 0)
                    return 0;
                return StationRepository.GetBonusValueForBets(
                    TicketState == TicketStates.MultySingles
                    ? ticketsInBasket.OrderByDescending(x => x.BonusPercentage).First()
                    : ticketsInBasket[0]
                    );
            }
        }

        public decimal MinBet
        {
            get
            {
                return TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)).Sum(x => x.MinBet);
            }
        }

        public decimal MaxBet
        {
            get
            {
                return TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)).Sum(x => x.MaxBet);
            }
        }

        public decimal MaxWin
        {
            get
            {
                return TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)).Sum(x => x.MaxWin);
            }
        }

        private decimal stake = 0m;
        public decimal Stake
        {
            get
            {
                try
                {
                    return stake;

                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public decimal ManipulationFeeValue
        {
            get
            {
                return TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)).Sum(x => x.ManipulationFeeValue);
            }
        }
        public decimal BonusValue
        {
            get
            {
                return TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)).Sum(x => x.BonusValue);
            }
        }
        public string BonusValueRounded
        {
            get
            {
                //decimal multiplied = BonusValue * 100;
                //decimal tempBonus = decimal.Truncate(multiplied);
                //tempBonus = tempBonus / 100;

                decimal tempBonus = ChangeTracker.TruncateDecimal(BonusValue);
                return string.Format(CultureInfo.InvariantCulture, "{0:N2}", tempBonus);

                //return tempBonus.ToString(CultureInfo.InvariantCulture);

                //string a = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                //var _bonusValueRounded = BonusValue.ToString(CultureInfo.InvariantCulture);
                //if (_bonusValueRounded.IndexOf(a) != -1)
                //{
                //    _bonusValueRounded = _bonusValueRounded.Substring(0, _bonusValueRounded.IndexOf(a) + 3);
                //}
                //return _bonusValueRounded;
            }
        }
        public string ManipulationFeeRounded
        {
            get
            {
                decimal manipulationFee = ChangeTracker.TruncateDecimal(ManipulationFeeValue);
                return manipulationFee.ToString(CultureInfo.InvariantCulture);
            }
        }



        public int Count
        {
            get
            {
                var value = TicketsInBasket.ToSyncList().Sum(x => x.TipItems.Count);
                return value;
            }
        }
        public decimal TotalOddDisplay
        {
            get
            {
                return TicketsInBasket.ToSyncList().Sum(x => x.TotalOddDisplay);
            }
        }
        public decimal CurrentTicketPossibleWin
        {
            get
            {
                return TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)).Sum(x => x.CurrentTicketPossibleWin);
            }
        }

        public TicketStates TicketState
        {
            get
            {
                lock (_lockUpdate)
                {
                    var state = TicketsInBasket.ToSyncList().Select(x => x.TicketState).FirstOrDefault();
                    return state == TicketStates.Empty ? TicketStates.Single : state;
                }
            }
            set
            {
                var ticketsInBasket = TicketsInBasket.ToSyncList();
                if (ticketsInBasket.Count == 0) return;
                lock (_lockUpdate)
                {
                    var prevValue = ticketsInBasket[0].TicketState;

                    if (prevValue == value)
                        return;
                    var oldStake = ticketsInBasket.Max(x => x.Stake);

                    if (value == TicketStates.Single && prevValue != TicketStates.Single)
                    {
                        var tipItems = ticketsInBasket.SelectMany(x => x.TipItems.ToSyncList()).ToList();

                        foreach (var ticket in ticketsInBasket)
                        {
                            TicketsInBasket.Remove(ticket);

                        }
                        foreach (var tipItemVw in tipItems)
                        {
                            var ticket = new Ticket();
                            TicketsInBasket.Add(ticket);
                            ticket.TipItems.Add(tipItemVw);
                        }
                    }
                    else if (prevValue == TicketStates.Single)
                    {
                        if (ticketsInBasket.Count > 1)
                        {
                            var tipItems = ticketsInBasket.SelectMany(x => x.TipItems.ToSyncList()).ToList();

                            foreach (var ticket in ticketsInBasket)
                            {
                                TicketsInBasket.Remove(ticket);

                            }
                            TicketsInBasket.Add(new Ticket());
                            ticketsInBasket = TicketsInBasket.ToSyncList();
                            foreach (var tipItemVw in tipItems)
                            {
                                ticketsInBasket[0].TipItems.Add(tipItemVw);
                            }
                        }
                    }
                    ticketsInBasket = TicketsInBasket.ToSyncList();

                    ticketsInBasket[0].TicketState = value;
                    UpdateTicket();

                    if (oldStake > 0 && prevValue != value)
                    {
                        foreach (var ticket in ticketsInBasket)
                        {
                            ticket.Stake = oldStake < ticket.MaxBet ? oldStake : ticket.MaxBet;

                        }
                    }
                }
                UpdateStake();
                if (ChangeTracker.CurrentUser != null)
                    ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - Stake;

                OnPropertyChanged();
            }
        }

        //private decimal CalculateConfidenceFactor(Ticket ticket)
        //{
        //    string localDelimeter = "*";
        //    string serverDelimeter = "|";

        //    decimal matchConfidenceFactor = 0;
        //    decimal maxBetLiability = 0;
        //    decimal marketConfidenceFactor = 0;

        //    for (int i = 0; i < ticket.TipItems.Count; i++)
        //    {
        //        string matchId = ticket.TipItems.ElementAt(i).Match.MatchId.ToString();
        //        string oddTag = ticket.TipItems.ElementAt(i).Odd.OddTag.Value;
        //        string tournamentId = ticket.TipItems.ElementAt(i).Match.MatchView.TournamentView.LineObject.SvrGroupId.ToString();
        //        string sportId = ticket.TipItems.ElementAt(i).Match.MatchView.SportView.LineObject.SvrGroupId.ToString();
        //        LiabilityLn liab = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue(matchId + localDelimeter + LineSr.MATCH_FACTOR);
        //        if (liab != null && (liab.factor.Value < matchConfidenceFactor || matchConfidenceFactor == 0))
        //        {
        //            matchConfidenceFactor = liab.factor.Value;
        //        }

        //        liab = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue(tournamentId + localDelimeter + LineSr.TOURN_CONF_RATING);
        //        if (liab != null)
        //        {
        //            LiabilityLn franchisorRating = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue(liab.factor.Value.ToString() + localDelimeter + LineSr.CONF_RATING_VALUES);
        //            if (franchisorRating != null && (franchisorRating.factor.Value < maxBetLiability || maxBetLiability == 0))
        //                maxBetLiability = franchisorRating.factor.Value;
        //        }
        //        else
        //        {
        //            LiabilityLn franchisorRating = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue("3" + localDelimeter + LineSr.CONF_RATING_VALUES);
        //            if (franchisorRating != null && (franchisorRating.factor.Value < maxBetLiability || maxBetLiability == 0))
        //                maxBetLiability = franchisorRating.factor.Value;
        //        }

        //        //marketConfidenceFactor "MATCH|180201|BD_TAG_0_1*LIMIT_FACTORS"
        //        liab = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue("MATCH" + serverDelimeter + matchId + serverDelimeter + oddTag + localDelimeter + LineSr.LIMIT_FACTORS);
        //        if (liab == null)
        //        {
        //            liab = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue("TOURNAMENT" + serverDelimeter + tournamentId + serverDelimeter + oddTag + localDelimeter + LineSr.LIMIT_FACTORS);
        //            if (liab == null)
        //            {
        //                liab = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue("SPORT" + serverDelimeter + sportId + serverDelimeter + oddTag + localDelimeter + LineSr.LIMIT_FACTORS);
        //                if (liab == null)
        //                {
        //                    liab = LineSr.Instance.AllObjects.Liabilities.SafelyGetValue("SPORT" + serverDelimeter + sportId + serverDelimeter + "DEFAULT" + localDelimeter + LineSr.LIMIT_FACTORS);
        //                    if (liab != null && (liab.factor.Value < marketConfidenceFactor || marketConfidenceFactor == 0))
        //                    {
        //                        marketConfidenceFactor = liab.factor.Value;
        //                    }
        //                }
        //                else if (liab.factor.Value < marketConfidenceFactor || marketConfidenceFactor == 0)
        //                {
        //                    marketConfidenceFactor = liab.factor.Value;
        //                }
        //            }
        //            else if (liab.factor.Value < marketConfidenceFactor || marketConfidenceFactor == 0)
        //            {
        //                marketConfidenceFactor = liab.factor.Value;
        //            }
        //        }
        //        else if(liab.factor.Value < marketConfidenceFactor || marketConfidenceFactor == 0)
        //        {
        //            marketConfidenceFactor = liab.factor.Value;
        //        }
        //    }

        //    if (matchConfidenceFactor == 0)
        //        matchConfidenceFactor = 1;

        //    if (marketConfidenceFactor == 0)
        //        marketConfidenceFactor = 1;

        //    return matchConfidenceFactor * maxBetLiability * marketConfidenceFactor;
        //}

        public void UpdateTicket()
        {
            lock (_lockUpdate)
            {
                var ticketsInBasket = TicketsInBasket.ToSyncList();
                foreach (var ticket in ticketsInBasket)
                {
                    ticket.User = ChangeTracker.CurrentUser;
                    if (TicketState == TicketStates.Single && ticket.TipItems.ToSyncList().Count(x => x.IsChecked) >= 2)
                    {
                        TicketState = TicketStates.Multy;
                    }
                    if (TicketState != TicketStates.Single && ticket.TipItems.ToSyncList().Count(x => x.IsChecked) < 2)
                    {
                        if (ticketsInBasket.Count == 1)
                        {
                            ticketsInBasket.First().TicketState = TicketStates.Single;
                        }
                        TicketState = TicketStates.Single;
                    }

                    DataBinding.UpdateSystemOrCombiticket(ticket);
                    if (ticket.Stake > ticket.MaxBet && ChangeTracker.CurrentUser.Cashpool > 0)
                        OnChangeStake("max", ticket, ChangeTracker.CurrentUser.Cashpool);
                }
                UpdateStake();
            }



            OnPropertyChanged("MaxBet");
            OnPropertyChanged("MaxWin");
            OnPropertyChanged("MinBet");
            OnPropertyChanged("CurrentTicketPossibleWin");

            OnPropertyChanged("Count");
            OnPropertyChanged("TotalOddDisplay");
            OnPropertyChanged("ManipulationFeeValue");
            OnPropertyChanged("ManipulationFeePercentage");
            OnPropertyChanged("BonusValue");
            OnPropertyChanged("BonusValueRounded");
            OnPropertyChanged("IsVisibleBank");
            OnPropertyChanged("BonusPercentage");

        }

        public void UpdateStake()
        {
            var newStake = TicketsInBasket.ToSyncList().Sum(x => x.Stake);
            if (newStake != stake)
            {
                stake = newStake;
                OnPropertyChanged("Stake");
            }
        }


        public Tuple<MultistringTag, string[], bool> OnChangeStake(string stake, Ticket ticket, decimal cashpool)
        {

            Tuple<MultistringTag, string[], bool> returnString = null;



            var tempStake = 0m;


            if (stake == "max")
            {
                if (cashpool > 0)
                    ticket.Stake = ticket.MaxBet > cashpool ? cashpool : ticket.MaxBet;

            }
            else if (stake == "clear")
            {
                ticket.Stake = 0;
                returnString = new Tuple<MultistringTag, string[], bool>(MultistringTags.NOT_ENOUGHT_MONEY, null, false);

            }
            else if (stake == "back")
            {
                if (ticket.Stake % 1 > 0)
                {
                    ticket.Stake = (int)ticket.Stake;
                }
                else
                {
                    ticket.Stake = ((int)ticket.Stake * 10) / 100;
                }
                returnString = new Tuple<MultistringTag, string[], bool>(MultistringTags.NOT_ENOUGHT_MONEY, null, false);

            }
            else
            {
                decimal dNewStake = 0;
                if (stake.Substring(0, 1) == "*")
                {
                    tempStake = ticket.Stake;

                    var cents = tempStake % 1;
                    dNewStake = (((int)tempStake) * 10 + Convert.ToDecimal(stake.Substring(1, stake.Length - 1), new CultureInfo("en-US")));
                    dNewStake += cents;
                    dNewStake -= ticket.Stake;
                }
                else if (stake.Substring(0, 1) == "+")
                {
                    dNewStake = decimal.Parse(stake.Substring(1, stake.Length - 1), new CultureInfo("en-US"));
                }
                else
                {
                    dNewStake = decimal.Parse(stake, new CultureInfo("en-US"));
                }

                if (ticket.Stake + dNewStake > ticket.MaxBet)
                {
                    if (ticket.TipItems.ToSyncList().All(x => x.IsChecked == false))
                    {
                        returnString = new Tuple<MultistringTag, string[], bool>(MultistringTags.TERMINAL_SELECT_BET, null, true);
                    }
                    else
                    {
                        returnString = new Tuple<MultistringTag, string[], bool>(MultistringTags.TERMINAL_STAKE_EXCEEDED_MAXBET, null, true);
                    }


                    //if ((ticket.Stake + dNewStake > Stake + cashpool) && ticket.TicketState != TicketStates.MultySingles)
                    //{
                    //    ticket.Stake += (Stake + cashpool)/3;
                    //}
                    //else
                    //{
                    //    ticket.Stake = ticket.MaxBet;
                    //}

                    //if ((ticket.Stake + dNewStake > Stake + cashpool) && cashpool > 0)
                    //{
                    //    ticket.Stake += cashpool; // ticket.MaxBet;
                    //    UpdateTicket();
                    //    ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - Stake;

                    //}

                    if ((ticket.Stake + dNewStake > Stake + cashpool) && (ticket.Stake + cashpool < ticket.MaxBet) && cashpool >= 0)
                        ticket.Stake += cashpool; // ticket.MaxBet;
                    else
                        ticket.Stake = ticket.MaxBet;
                    UpdateTicket();
                    ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - Stake;

                    return returnString;
                }

                tempStake = dNewStake;
            }
            if (tempStake < 0)
            {
                tempStake = 0;
            }

            if (ticket.Stake + tempStake > cashpool && !StationRepository.IsCashier)
            {
                if (cashpool > ticket.Stake)
                    ticket.Stake = cashpool;
                returnString = new Tuple<MultistringTag, string[], bool>(MultistringTags.NOT_ENOUGHT_MONEY, null, true);

            }
            else
            {
                ticket.Stake += tempStake;
            }


            UpdateTicket();
            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - Stake;

            return returnString;
        }



    }
}