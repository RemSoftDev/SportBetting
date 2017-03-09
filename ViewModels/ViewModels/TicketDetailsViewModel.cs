using System;
using System.ServiceModel;
using BaseObjects;
using BaseObjects.ViewModels;
using System.Collections.ObjectModel;
using Nbt.Station.Design;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Windows;

namespace ViewModels.ViewModels
{


    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class TicketDetailsViewModel : BaseViewModel
    {
        private static ILog Log = LogFactory.CreateLog(typeof(TicketDetailsViewModel));

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TicketCheckerViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public TicketDetailsViewModel()
        {
            ChangeTracker.RedirectToTicketDetails = false;
            Mediator.Register<TicketWS>(this, ReloadTicket, MsgTag.ReloadTicket);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            CloseCommand = new Command(CloseView);
            PrintCreditNoteCommand = new Command(PrintCreditNoteFromTicket);
            TransferMoneyCommand = new Command(TransferMoney);
        }

        [PleaseWaitAspect]
        private void Refresh(bool obj)
        {
            try
            {
                var ticket = WsdlRepository.LoadTicket(CurrentTicket.ticketNbr, CurrentTicket.checkSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);

                ReloadTicket(ticket);
            }
            catch (FaultException<HubServiceException> ex)
            {
                if (ex.Detail.code == 220)
                    ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_TICKET_NOT_FOUND));
                else if (ex.Detail.code == 1791)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_INVALIDFRANCHISOR));
                }
                else
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_INVALIDLOCATION));
            }
            catch (Exception)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR));
            }

        }

        public override void OnNavigationCompleted()
        {
            Mediator.Register<bool>(this, RefreshButtons, MsgTag.RefreshTicketDetails);
            if (CurrentTicket != null && CurrentTicket.wonExpired)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_EXPIRED_POPUP));
            }
            ReloadTicket(null);

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            base.Close();
        }

        private void RefreshButtons(bool res)
        {
            OnPropertyChanged("ShowTranferToAccountButton");
            OnPropertyChanged("ShowCreditNoteButton");
        }

        private void CloseView()
        {
            ChangeTracker.LoadedTicket = "";
            ChangeTracker.LoadedTicketcheckSum = "";

            if (ChangeTracker.IsUserProfile)
            {
                MyRegionManager.NavigatBack(RegionNames.UserProfileContentRegion);
            }
            else
            {
                Mediator.SendMessage(true, MsgTag.NavigateBack);

            }

            ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.TICKET;
        }

        public decimal ManipulationFee
        {
            get { return _manipulationFee; }
            set
            {
                _manipulationFee = value;
                OnPropertyChanged();
            }
        }

        private void ReloadTicket(TicketWS obj)
        {
            if (obj != null)
                CurrentTicket = obj;


            if (CurrentTicket != null)
            {
                OnPropertyChanged("PendingApprovalVisibility");
                    ShowSuperBonus = CurrentTicket.superBonus > 1;
                SuperBonusValue = CurrentTicket.superBonusValue;
                    ShowManipulationFee = CurrentTicket.manipulationFee > 0;
                ManipulationFee = CurrentTicket.manipulationFee;
                ManipulationFeeValue = CurrentTicket.manipulationFeeValue;
                Stake = CurrentTicket != null ? CurrentTicket.stake : 0;

                Station = CurrentTicket.ticketNbr.Substring(0, 4);
                TicketNumber = CurrentTicket.ticketNbr;
                Date = CurrentTicket.acceptedTime;
                ShowStakePerRow = "";
                SuperBonusPercent = (CurrentTicket.superBonus - 1) * 100;
                if (CurrentTicket.bets != null)
                {
                    long prevBankGroupID = -1;
                    int bankCount = 0;
                    int multiWayCount = 0;
                    foreach (TipWS tip in CurrentTicket.bets[0].tips2BetMulti)
                    {
                        if (tip.bankGroupID != prevBankGroupID && !tip.bank)
                        {
                            multiWayCount++;
                            prevBankGroupID = tip.bankGroupID;
                        }
                        if (tip.bank)
                        {
                            bankCount++;
                        }
                    }
                    StakePerRow = CurrentTicket.stake / CurrentTicket.bets[0].rows;
                    switch (CurrentTicket.bets[0].betType)
                    {
                        case Bet.BET_TYPE_COMBI:
                            BetType = TranslationProvider.Translate(MultistringTags.Multiple);
                            break;
                        case Bet.BET_TYPE_COMBIPATH:
                            BetType = TranslationProvider.Translate(MultistringTags.Multiple_and_Ways);
                            ShowStakePerRow = "1";
                            break;
                        case Bet.BET_TYPE_SINGLE:
                            BetType = TranslationProvider.Translate(MultistringTags.SINGLES);
                            break;
                        case Bet.BET_TYPE_SYSTEM:
                            BetType = TranslationProvider.Translate(MultistringTags.Full_Cover) + " " + CurrentTicket.bets[0].systemX + "/" + CurrentTicket.bets[0].systemY;
                            if (bankCount > 0)
                            {
                                BetType += "+" + bankCount + "B";
                            }
                            ShowStakePerRow = "1";
                            break;
                        case Bet.BET_TYPE_SYSTEMPATH:
                            string sTmp =
                                TranslationProvider.Translate(MultistringTags.TERMINAL_PRINT_SYSTEM) + " " +
                                CurrentTicket.bets[0].systemX + "/" + CurrentTicket.bets[0].systemY;
                            if (bankCount > 0)
                            {
                                sTmp += "+" + bankCount + "B";
                            }
                            if (multiWayCount > 0)
                            {
                                sTmp += "+" + multiWayCount + "W";
                            }
                            BetType = sTmp;
                            ShowStakePerRow = "1";
                            break;
                    }
                    PossibleWinning = CurrentTicket.bets[0].maxWin;
                }
                Paid = CurrentTicket.paid
                           ? TranslationProvider.Translate(MultistringTags.terminal_yes)
                           : TranslationProvider.Translate(MultistringTags.terminal_no);
                //PaidAt = CurrentTicket.Paid ? CurrentTicket.PaidTime.ToShortDateString() : "-";
                PaidAt = CurrentTicket.paid ? String.Format("{0:dd.MM.yyyy HH:mm}", CurrentTicket.paidTime) : "-";
                PaymentAmount = CurrentTicket.wonAmount;

                if (CurrentTicket.bets != null)
                    BalanceOperations = LoadOdds(new ObservableCollection<BetWS>(CurrentTicket.bets));
                string[] tabColor = GetTabColor();
                TabForecolor = tabColor[0];
                TabBgcolor = tabColor[1];
                OnPropertyChanged("ShowTranferToAccountButton");
                OnPropertyChanged("ShowCreditNoteButton");
                OnPropertyChanged("TransferMoneyString");

            }

        }

        public decimal ManipulationFeeValue
        {
            get { return _manipulationFeeValue; }
            set
            {
                _manipulationFeeValue = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Properties
        private bool UseCashpool
        {
            get
            {
                return true;
            }
        }

        public bool ShowTranferToAccountButton
        {
            get
            {
                return !CurrentTicket.wonExpired && !StationRepository.DisableTransferToTerminal && CurrentTicket != null
                       && CurrentTicket.isAnonymous && !CurrentTicket.paid && CurrentTicket.wonAmount > 0
                       && CurrentTicket.wonExpireTime > DateTime.Now && UseCashpool
                       && StationRepository.FranchisorID == CurrentTicket.franchisorId
                       && StationRepository.LocationID == CurrentTicket.locationId;
            }
        }

        public bool ShowCreditNoteButton
        {
            get
            {
                return !CurrentTicket.wonExpired && CurrentTicket != null && StationRepository.AllowAnonymousBetting
                    && CurrentTicket.isAnonymous && !CurrentTicket.paid
                    && CurrentTicket.wonAmount > 0 && CurrentTicket.wonExpireTime > DateTime.Now
                    && ChangeTracker.CurrentUser is AnonymousUser
                    && StationRepository.FranchisorID == CurrentTicket.franchisorId
                    && StationRepository.LocationID == CurrentTicket.locationId;
            }
        }

        public string TransferMoneyString
        {
            get
            {
                if (ChangeTracker.CurrentUser is LoggedInUser)
                    return TranslationProvider.Translate(MultistringTags.TRANSFER_TO_ACCOUNT).ToString();

                return TranslationProvider.Translate(MultistringTags.TRANSFER_TO_CASHPOOL).ToString();
            }
        }

        public TicketWS CurrentTicket
        {
            get { return ChangeTracker.CurrentTicket; }
            set { ChangeTracker.CurrentTicket = value; }
        }

        private ObservableCollection<Tip> bets;


        public Visibility PendingApprovalVisibility 
        { 
            get 
            {
                if (CurrentTicket == null)
                    return Visibility.Collapsed;

                return CurrentTicket.isPendingApproval ? Visibility.Visible : Visibility.Collapsed; 
            } 
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public ObservableCollection<Tip> BalanceOperations
        {
            get { return bets; }
            set
            {
                bets = value;
                OnPropertyChanged();
            }
        }

        public string Station
        {
            get { return _station; }
            set
            {
                _station = value;
                OnPropertyChanged("Station");
            }
        }

        public string TicketNumber
        {
            get { return _ticketNumber; }
            set
            {
                _ticketNumber = value;
                OnPropertyChanged("TicketNumber");
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged("Date");
            }
        }

        public string BetType
        {
            get { return _betType; }
            set
            {
                _betType = value;
                OnPropertyChanged("BetType");
            }
        }

        public string ShowStakePerRow
        {
            get { return _showStakePerRow; }
            set
            {
                _showStakePerRow = value;
                OnPropertyChanged("ShowStakePerRow");
            }
        }

        public decimal PossibleWinning
        {
            get { return _possibleWinning; }
            set
            {
                _possibleWinning = value;
                OnPropertyChanged("PossibleWinning");
            }
        }

        public string Paid
        {
            get { return _paid; }
            set
            {
                _paid = value;
                OnPropertyChanged("Paid");
            }
        }

        public string PaidAt
        {
            get { return _paidAt; }
            set
            {
                _paidAt = value;
                OnPropertyChanged("PaidAt");
            }
        }

        public decimal PaymentAmount
        {
            get { return _paymentAmount; }
            set
            {
                _paymentAmount = value;
                OnPropertyChanged("PaymentAmount");
            }
        }

        public string TabForecolor
        {
            get { return _tabForecolor; }
            set
            {
                _tabForecolor = value;
                OnPropertyChanged("TabForecolor");
            }
        }

        public string TabBgcolor
        {
            get { return _tabBgcolor; }
            set
            {
                _tabBgcolor = value;
                OnPropertyChanged("TabBgcolor");
            }
        }

        public decimal Stake
        {
            get { return _stake; }
            set
            {
                _stake = value;
                OnPropertyChanged();
            }
        }

        public decimal StakePerRow
        {
            get
            {
                return _stakePerRow; // bet.stake / bet.rows
            }
            set
            {
                _stakePerRow = value;
                OnPropertyChanged("StakePerRow");
            }
        }

        private string _station;
        private string _ticketNumber;
        private DateTime _date;
        private string _betType;
        private string _showStakePerRow;
        private decimal _possibleWinning;
        private string _paid;
        private string _paidAt;
        private decimal _paymentAmount;
        private string _tabForecolor;
        private string _tabBgcolor;
        private decimal _stakePerRow;
        private string _tabText1;

        public string TabText
        {
            get { return _tabText1; }
            set
            {
                if (value == _tabText1)
                    return;
                _tabText1 = value;
                OnPropertyChanged("TabText");
            }
        }

        private string _tabText2;

        public string TabTextApproval
        {
            get { return _tabText2; }
            set
            {
                if (value == _tabText2)
                    return;
                _tabText2 = value;
                OnPropertyChanged("TabTextApproval");
            }
        }

        private bool _showSuperBonus = false;
        public bool ShowSuperBonus
        {
            get
            {
                return _showSuperBonus;
            }
            set
            {
                if (_showSuperBonus == value)
                    return;

                _showSuperBonus = value;
                OnPropertyChanged("ShowSuperBonus");
                OnPropertyChanged("SuperBonusPercent");
            }
        }

        private bool _showManipulationFee = false;
        private decimal _superBonusPercent;
        private decimal _stake;
        private decimal _manipulationFee;
        private decimal _manipulationFeeValue;
        private decimal _superBonusValue;

        public bool ShowManipulationFee
        {
            get
            {
                return _showManipulationFee;
            }
            set
            {
                _showManipulationFee = value;
                OnPropertyChanged("ShowManipulationFee");
            }
        }

        public decimal SuperBonusPercent
        {
            get { return _superBonusPercent; }
            set
            {
                _superBonusPercent = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public Command PrintCreditNoteCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command TransferMoneyCommand { get; private set; }

        public decimal SuperBonusValue
        {
            get { return _superBonusValue; }
            set
            {
                _superBonusValue = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        [AsyncMethod]
        private void PrintCreditNoteFromTicket()
        {
            PrintCreditNoteFromTicketPleaseWait();
        }
        [WsdlServiceAsyncAspect]
        private void PrintCreditNoteFromTicketPleaseWait()
        {
            string error = "";
            var number = BusinessPropsHelper.GenerateNextCreditNoteNumber();
            var checkSum = new PasswordGenerator().Generate(4, 4, true);


            var result = TransactionQueueHelper.TryDepositByTicketMoneyOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(ChangeTracker.CurrentUser), CurrentTicket.ticketNbr, CurrentTicket.checkSum, number, checkSum, ref error);
            if (!result)
            {
                ShowError(error);
            }
            else
            {
                var sucess = PrinterHandler.PrintCreditNote(CurrentTicket.wonAmount, number, checkSum, false, DateTime.MinValue, DateTime.MinValue);

                if (!sucess)
                {
                    GetMoneyFromCreditNote(new CreditNoteWS() { amount = CurrentTicket.wonAmount, number = number, code = checkSum });
                    ShowError(TranslationProvider.Translate(MultistringTags.UNABLE_TO_PRINT_CREDITNOTE) + "\r\n" + TranslationProvider.Translate(MultistringTags.SHOP_FORM_CREDITNOTE) + ": " + number + " " + checkSum);
                }
            }
            Log.Error(error,new Exception(error));

            CurrentTicket = WsdlRepository.LoadTicket(CurrentTicket.ticketNbr, CurrentTicket.checkSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);
            ReloadTicket(CurrentTicket);
        }


        private void TransferMoney()
        {
            Mediator.SendMessage(new Tuple<string, string>(CurrentTicket.ticketNbr, CurrentTicket.checkSum), MsgTag.AddMoneyFromTicket);
        }

        private ObservableCollection<Tip> LoadOdds(ObservableCollection<BetWS> bets)
        {
            var result = new ObservableCollection<Tip>();
            foreach (var betWs in bets)
            {
                if (betWs.tips2BetMulti != null)
                    foreach (var tipWs in betWs.tips2BetMulti)
                    {

                        var odd = FindOddFromdetails(tipWs.svrOddID);
                        odd.IsBanker = tipWs.bank;
                        if (odd != null)
                        {
                            //odd.Stake = betWs.stake;
                            odd.State = GetOddStatus(odd);
                            //odd.StateString = GetStatusString(odd.State);
                            result.Add(odd);

                        }
                    }
                if (betWs.bankTips != null)
                    foreach (var tipWs in betWs.bankTips)
                    {
                        var odd = FindOddFromdetails(tipWs.svrOddID);
                        if (odd != null)
                        {
                            //odd.Stake = betWs.stake;
                            odd.State = GetOddStatus(odd);
                            //odd.StateString = GetStatusString(odd.State);
                            result.Add(odd);

                        }
                    }
            }
            return result;
        }

        public Tip FindOddFromdetails(long oddId)
        {
            if (CurrentTicket != null)
            {
                for (int i = 0; i < CurrentTicket.bets.Length; i++)
                {
                    for (int y = 0; y < CurrentTicket.bets[i].bankTips.Length; y++)
                    {
                        if (CurrentTicket.bets[i].bankTips[y].svrOddID != oddId) continue;
                        Tip result = new Tip();
                        result.Canceled = CurrentTicket.cancelled;
                        result.PendingApproval = CurrentTicket.isPendingApproval;
                        result.Id = oddId;
                        result.ResultName = CurrentTicket.bets[i].bankTips[y].tipDetailsWS.tip;
                        result.Text = CurrentTicket.bets[i].bankTips[y].odd.ToString();
                        result.ExpiryDate = CurrentTicket.bets[i].bankTips[y].tipDetailsWS.startdate;

                        var odd = Repository.GetOddBySvrId(CurrentTicket.bets[i].bankTips[y].svrOddID);

                        if (odd != null && odd.BetDomainView != null && odd.BetDomainView.IsToInverse)
                        {
                            result.BetDomainNameFromTicket = string.Format(CurrentTicket.bets[i].bankTips[y].tipDetailsWS.betDomainName, CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialOddValue == "-1" ? CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialLiveOddValueFull : odd.SpecialBetdomainValue, CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialLiveOddValueFull);
                        }
                        else
                        {
                            result.BetDomainNameFromTicket = string.Format(CurrentTicket.bets[i].bankTips[y].tipDetailsWS.betDomainName, CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialOddValue == "-1" ? CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialLiveOddValueFull : CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialOddValue, CurrentTicket.bets[i].bankTips[y].tipDetailsWS.specialLiveOddValueFull);
                        }

                        result.HomeTeam = CurrentTicket.bets[i].bankTips[y].tipDetailsWS.team1;
                        result.AwayTeam = CurrentTicket.bets[i].bankTips[y].tipDetailsWS.team2;

                        result.Score = string.IsNullOrEmpty(CurrentTicket.bets[i].bankTips[y].tipDetailsWS.result) ?
                            CurrentTicket.ticketTyp != 4 ? "--" : CurrentTicket.bets[i].bankTips[y].tipDetailsWS.winnerTip :
                            CurrentTicket.bets[i].bankTips[y].tipDetailsWS.result;

                        result.Won = CurrentTicket.bets[i].bankTips[y].won;
                        result.Stake = CurrentTicket.bets[i].bankTips[y].stake;
                        result.Competitors = string.Format("{0} {1} {2}", result.HomeTeam, TranslationProvider.Translate(MultistringTags.VERSUS), result.AwayTeam);
                        result.EventName = CurrentTicket.bets[i].bankTips[y].tipDetailsWS.event_name;
                        if (CurrentTicket.bets[i].bankTips[y].calculated)
                        {
                            if (CurrentTicket.bets[i].bankTips[y].won)
                            {
                                result.StateString = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETWON).ToUpperInvariant();
                                if (CurrentTicket.isPendingApproval)
                                    result.StateString += " " + TranslationProvider.Translate(MultistringTags.TERMINAL_PENDING_APPROVAL).ToString();
                            }
                            else
                                result.StateString = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_LOST).ToUpperInvariant();
                        }
                        else
                            result.StateString = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETOPEN).ToUpperInvariant();

                        result.Calculated = CurrentTicket.bets[i].bankTips[y].calculated;



                        result.CurrentCurrency = ChangeTracker.CurrentUser.GetType() != typeof(AnonymousUser)
                                                     ? ChangeTracker.CurrentUser.Currency
                                                     : "";
                        result.CorrectTip = string.IsNullOrEmpty(CurrentTicket.bets[i].bankTips[y].tipDetailsWS.winnerTip) ? "--" : CurrentTicket.bets[i].bankTips[y].tipDetailsWS.winnerTip;

                        return result;
                    }

                    //check also 2multitips
                    for (int z = 0; z < CurrentTicket.bets[i].tips2BetMulti.Length; z++)
                    {
                        if (CurrentTicket.bets[i].tips2BetMulti[z].svrOddID != oddId) continue;
                        Tip result = new Tip();
                        result.Canceled = CurrentTicket.cancelled;
                        result.PendingApproval = CurrentTicket.isPendingApproval;
                        result.Id = oddId;
                        result.ResultName = CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.tip;
                        result.Text = CurrentTicket.bets[i].tips2BetMulti[z].odd.ToString();

                        result.ExpiryDate = CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.startdate;

                        var odd = Repository.GetOddBySvrId(CurrentTicket.bets[i].tips2BetMulti[z].svrOddID);

                        if (odd != null && odd.BetDomainView != null && odd.BetDomainView.IsToInverse)
                        {
                            result.BetDomainNameFromTicket = string.Format(CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.betDomainName, CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialOddValue == "-1" ? CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialLiveOddValueFull : odd.SpecialBetdomainValue, CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialLiveOddValueFull);

                        }
                        else
                        {
                            result.BetDomainNameFromTicket = string.Format(CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.betDomainName, CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialOddValue == "-1" ? CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialLiveOddValueFull : CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialOddValue, CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.specialLiveOddValueFull);
                        }

                        result.HomeTeam = CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.team1;
                        result.AwayTeam = CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.team2;

                        result.Score = string.IsNullOrEmpty(CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.result) ?
                            CurrentTicket.ticketTyp != 4 ? "--" : CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.winnerTip :
                            CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.result;

                        result.Calculated = CurrentTicket.bets[i].tips2BetMulti[z].calculated;
                        result.Won = CurrentTicket.bets[i].tips2BetMulti[z].won;
                        result.Stake = CurrentTicket.bets[i].tips2BetMulti[z].stake;
                        result.Competitors = string.Format("{0} {1} {2}", result.HomeTeam, TranslationProvider.Translate(MultistringTags.VERSUS).ToString(), result.AwayTeam);
                        result.EventName = CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.event_name;
                        if (CurrentTicket.bets[i].tips2BetMulti[z].calculated)
                        {
                            if (CurrentTicket.bets[i].tips2BetMulti[z].won)
                            {
                                result.StateString = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETWON).ToUpperInvariant();
                                if (CurrentTicket.isPendingApproval)
                                    result.StateString += " " + TranslationProvider.Translate(MultistringTags.TERMINAL_PENDING_APPROVAL).ToString();
                            }
                            else
                                result.StateString = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_LOST).ToUpperInvariant();
                        }
                        else
                            result.StateString = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETOPEN).ToUpperInvariant();


                        result.Calculated = CurrentTicket.bets[i].tips2BetMulti[z].calculated;

                        result.CurrentCurrency = ChangeTracker.CurrentUser.GetType() != typeof(AnonymousUser)
                             ? ChangeTracker.CurrentUser.Currency
                             : "";
                        result.CorrectTip = string.IsNullOrEmpty(CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.winnerTip) ? "--" : CurrentTicket.bets[i].tips2BetMulti[z].tipDetailsWS.winnerTip;

                        return result;
                    }

                    //                    foreach (var currentTicketDetail in CurrentTicketDetails)
                    //{
                    //    if (currentTicketDetail.OddORMID != oddId.ToString()) continue;
                    //    Tip result = new Tip(currentTicketDetail.Tip, currentTicketDetail.Tip, oddId, new Match(0, new MatchSr()), new OddSr(new BetDomainSr(new MatchSr())));
                    //    result.OddSr.BetDomain.Match.ExpiryDate = currentTicketDetail.Startdate;
                    //    result.BetDomainNameFromTicket = currentTicketDetail.BetDomainName;
                    //    result.Match.HomeTeam = currentTicketDetail.Team1;
                    //    result.Match.AwayTeam = currentTicketDetail.Team2;
                    //    result.Match.Score = currentTicketDetail.Result;
                    //    result.OddSr.Won = currentTicketDetail.Won;
                    //    result.StateString = currentTicketDetail.State;
                    //    result.CorrectTip = currentTicketDetail.WinnerTip;

                    //    return result;
                    //}
                }
            }
            return null;
        }




        private int GetOddStatus(Tip tipX)
        {
            TicketWS t = CurrentTicket;
            if (t == null) return TICKET_WONSTATUS_INVALID;
            if (t.cancelled)
                return TICKET_WONSTATUS_CANCELED;
            if (t.calculated)
            {
                if (t.won && t.enablePay && t.wonExpireTime > DateTime.Now)
                {
                    if (t.paid)
                        return TICKET_WONSTATUS_PAID;
                    else
                        return TICKET_WONSTATUS_WON;
                }
                else if (t.won && !t.enablePay)
                    return TICKET_WONSTATUS_OPEN;
                else
                    return TICKET_WONSTATUS_LOST;
            }

            if (tipX.Won)
            {
                if (tipX.ExpiryDate < DateTime.Now)
                {
                    return TICKET_WONSTATUS_LOST;
                }
                return TICKET_WONSTATUS_WON;
            }
            return TICKET_WONSTATUS_OPEN;

        }

        private string[] GetTabColor()
        {
            String forecolor = "#000000";
            String bgcolor = "#FFEFEFEF";
            TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETOPEN);

            TicketWS t = CurrentTicket;

            if (t.cancelled)
            {
                // CANCELLED
                forecolor = "#FFFFFF";
                bgcolor = "#61217C";
                TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CANCELLED);
            }

            if (t.calculated)
            {
                if (t.won)
                {
                    if (t.isPendingApproval)
                    {
                        // WON
                        forecolor = "#FFFFFF";
                        bgcolor = "#FF9933";
                        TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETWON);
                        TabTextApproval = TranslationProvider.Translate(MultistringTags.TERMINAL_PENDING_APPROVAL);
                        if (CurrentTicket.wonExpired)
                        {
                            forecolor = "#FFEFEFEF";
                            bgcolor = "#FFFF1313";

                            TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETEXPIRED).ToUpperInvariant();

                        }
                    }
                    else
                    {
                        // WON
                        forecolor = "#FFEFEFEF";
                        bgcolor = "#FF22B613";
                        TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETWON);
                        if (CurrentTicket.wonExpired)
                        {
                            forecolor = "#FFEFEFEF";
                            bgcolor = "#FFFF1313";

                            TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETEXPIRED).ToUpperInvariant();

                        }
                    }
                }
                else
                {
                    // LOST
                    forecolor = "#FFEFEFEF";
                    bgcolor = "#FFFF1313";
                    TabText = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETLOST);
                }
            }

            return new string[2] { forecolor, bgcolor };

        }

        #endregion
        public const int TICKET_WONSTATUS_INVALID = -1;
        public const int TICKET_WONSTATUS_OPEN = 0;
        public const int TICKET_WONSTATUS_WON = 1;
        public const int TICKET_WONSTATUS_LOST = 2;
        public const int TICKET_WONSTATUS_PAID = 3;
        public const int TICKET_WONSTATUS_CANCELED = 4;
    }
}