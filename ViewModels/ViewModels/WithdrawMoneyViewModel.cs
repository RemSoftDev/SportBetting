using System.ServiceModel;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using System;
using System.Globalization;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class WithdrawMoneyViewModel : BaseViewModel
    {
        private const string AmountDescriptionFormatStringTemplate = "###,###,###,###,###,###,##0.00";

        /// <summary>
        /// Initializes a new instance of the <see cref="WithdrawMoneyViewModel"/> class.
        /// </summary>
        public WithdrawMoneyViewModel()
        {
            Mediator.Register<bool>(this, RefreshCashpool, MsgTag.RefreshCashpool);
            this.Amount = 0;
            this.NumberOfDecimals = 0;

            WithdrawCommand = new Command(OnWithdraw);
            WithdrawButCommand = new Command(OnWithdrawBut);
            WithdrawAllCommand = new Command(OnWithdrawAll);
            CashOutCommand = new Command(OnCashOut);
            Mediator.Register<string>(this, OnClearExecute, "WithdrawClearTicketNumber");
            Mediator.Register<string>(this, OnBackSpaceExecute, "WithdrawPinBackspace");
            Mediator.Register<string>(this, OnPinButtonExecute, "WithdrawPinButton");
            Mediator.Register<bool>(this, OnMoneyAdded, "UpdateBalance");
        }

        private void OnCashOut()
        {
            Amount = 0;
            WithdrawAll = false;
            WithdrawBut = false;
            OnPropertyChanged("WithdrawBut");
        }

        #region --- props ---

        public bool CashOutChecked
        {
            get { return _cashOutChecked; }
            set
            {
                _cashOutChecked = value;
                OnPropertyChanged("CashOutChecked");
            }
        }

        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set
            {
                _numberOfDecimals = value;
                OnPropertyChanged();
            }
        }

        protected FoundUser EditUserId
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }


        /// <summary>
        /// Gets or sets Amount (0 by default)
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged();
                AmountChanged();
                this.OnPropertyChanged("IsWithdrawEnabled");
            }
        }

        public bool WithdrawBut
        {
            get { return _withdrawBut; }
            set
            {
                _withdrawBut = value;
                AmountChanged();
                this.OnPropertyChanged("IsWithdrawEnabled");
            }
        }

        public bool IsWithdrawEnabled
        {
            get 
            {
                if ((CashOutChecked && Amount == 0) || (WithdrawAll && AmountDescription == "0.00") || (WithdrawBut && AmountDescription == "0.00") || (WithdrawAll && AmountDescription == "0,00") || (WithdrawBut && AmountDescription == "0,00"))
                   return false;

                return true;
            }
        }

        private bool _withdrawAll = false;
        private bool WithdrawAll 
        {
            get
            {
                return _withdrawAll;
            }
            set
            {
                _withdrawAll = value;
                this.OnPropertyChanged("IsWithdrawEnabled");
            }
        }

        public decimal NewAmount
        {
            get { return WithdrawBut ? Cashpool - (Cashpool - Amount) : Cashpool - Amount; }
        }
        public string AmountDescription
        {
            get
            {
                CultureInfo cultureInfo = null;

                if ((StationRepository != null) && (StationRepository.Culture != null))
                {
                    cultureInfo = StationRepository.Culture;
                }
                else
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }

                if (WithdrawBut)
                {
                    return (ChangeTracker.CurrentUser.Cashpool - Amount).ToString(AmountDescriptionFormatStringTemplate,
                                                                           cultureInfo);
                }

                return Amount.ToString(AmountDescriptionFormatStringTemplate, cultureInfo);
            }
        }

        public string CashpoolDescription
        {
            get
            {
                CultureInfo cultureInfo = null;

                if ((StationRepository != null) && (StationRepository.Culture != null))
                {
                    cultureInfo = StationRepository.Culture;
                }
                else
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }

                return Cashpool.ToString(AmountDescriptionFormatStringTemplate, cultureInfo);
            }
        }

        public string NewAmountDescription
        {
            get
            {
                CultureInfo cultureInfo = null;

                if ((StationRepository != null) && (StationRepository.Culture != null))
                {
                    cultureInfo = StationRepository.Culture;
                }
                else
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }

                return NewAmount.ToString(AmountDescriptionFormatStringTemplate, cultureInfo);
            }
        }


        public decimal Cashpool
        {
            get
            {
                if (ChangeTracker.CurrentUser != null)
                    return ChangeTracker.CurrentUser.Cashpool;
                return 0;
            }
        }

        #endregion


        public Command WithdrawCommand { get; set; }
        public Command WithdrawButCommand { get; set; }
        public Command WithdrawAllCommand { get; set; }
        public Command CashOutCommand { get; set; }

        #region --- methods ---

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.EnterAmountPayoutPinKeyboardRegion);

            base.OnNavigationCompleted();
        }
        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.EnterAmountPayoutPinKeyboardRegion);
            base.Close();
        }

        [AsyncMethod]
        private void OnWithdraw()
        {
            OnWithdrawPleaseWait();
        }

        private void OnWithdrawAll()
        {
            WithdrawBut = false;
            WithdrawAll = true;
            Amount = ChangeTracker.CurrentUser.Cashpool;
            OnPropertyChanged("WithdrawBut");
        }

        private void OnWithdrawBut()
        {
            Amount = 0;
            WithdrawAll = false;
            WithdrawBut = true;
            OnPropertyChanged("WithdrawBut");
        }

        private bool _withdrawBut;
        private bool _cashOutChecked = true;
        private int _numberOfDecimals;
        private decimal _amount;

        private void OnWithdrawPleaseWait()
        {
            var amount = WithdrawBut ? (Cashpool - Amount) : Amount;
            var text = TranslationProvider.Translate(MultistringTags.U_WANT_TO_WITHDRAW_X_MONEY, amount, Currency);

            QuestionWindowService.ShowMessage(text, null, null, messageWindow_YesClick, messageWindow_NoClick);
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_CACHE_NOTE).ToString() + ", ";

            switch (status)
            {
                case 0:
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                    return;
                case 4:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_PAPER).ToString();
                    break;
                case 6:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_TONER).ToString();
                    break;
                case 7:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OPEN).ToString();
                    break;
                case 8:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_JAMMED).ToString();
                    break;
                case 9:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OFFLINE).ToString();
                    break;
            }

            ShowError(errorMessage, null, true);
        }

        void messageWindow_NoClick(object sender, EventArgs e)
        {
        }
        [WsdlServiceSyncAspect]
        void messageWindow_YesClick(object sender, EventArgs e)
        {
            var amount = WithdrawBut ? (Cashpool - Amount) : Amount;

            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }

            bool Valid = true;
            string errormessage = "";
            string code = "";

            try
            {
                DateTime expiration = DateTime.Now;
                bool money_withdraw = false;
                var uid = StationRepository.GetUid(ChangeTracker.CurrentUser);
                if (!(ChangeTracker.CurrentUser is LoggedInUser))
                    uid = StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, "", 0, 0, 0, 0));
                string number;
                bool withdrawFrombalance;
                var amountRef = amount;
                code = WsdlRepository.RegisterPaymentNote(uid, ref amountRef, out expiration, out money_withdraw, out number, out withdrawFrombalance);
                if (string.IsNullOrEmpty(code))
                {
                    Valid = false;
                    errormessage = TranslationProvider.Translate(MultistringTags.TERMINAL_CONNECTION_LOST_LOGGEDIN).ToString();
                }
            }

            catch (FaultException<HubServiceException> ex)
            {
                Valid = false;
                errormessage = ex.Detail.message;
            }
            if (Valid)
            {
                PrinterHandler.PrintPaymentNote(amount, code, DateTime.Now);
            }
            else
            {
                ShowError(errormessage);
            }
            ChangeTracker.CurrentUser.Withdrawmoney(-amount);
            Amount = 0;
            OnPropertyChanged("CashpoolDescription");

            Dispatcher.Invoke((Action)(() =>
            {
                var window = (Window)GetActiveWindow();
                if (window != null)
                    window.Focus();
            }));

        }





        private void AmountChanged()
        {
            this.OnPropertyChanged("NewAmountDescription");
            this.OnPropertyChanged("CashpoolDescription");
            this.OnPropertyChanged("AmountDescription");
        }

        private void OnMoneyAdded(bool res)
        {
            this.OnPropertyChanged("NewAmountDescription");
            this.OnPropertyChanged("CashpoolDescription");
            this.OnPropertyChanged("AmountDescription");
        }

        private void OnPinButtonExecute(string obj)
        {
            if (WithdrawAll)
                return;
            int number;
            if (int.TryParse(obj, out number))
            {
                decimal power = Power(this.NumberOfDecimals);

                var amount = this.Amount;
                amount *= 10m; // move numbers forward
                amount += (number / power); // add number as last decimal

                if (amount > ChangeTracker.CurrentUser.Cashpool)
                    amount = ChangeTracker.CurrentUser.Cashpool;

                this.Amount = amount;                
            }
        }

        private void OnBackSpaceExecute(string obj)
        {
            if (WithdrawAll)
                return;

            decimal power = Power(this.NumberOfDecimals);

            var amount = this.Amount;
            if (amount != 0m) // if already 0 then do nothing
            {
                amount *= power; // get rid of decimals
                amount = Math.Floor(amount / 10); // remove last number
                amount = amount / power; // create decimals

                this.Amount = amount;
            }
        }

        private void OnClearExecute(string obj)
        {
            if (WithdrawAll)
                return;

            this.Amount = 0m;
        }

        private static decimal Power(int numOfDecimals)
        {
            decimal result = 1m;

            if (numOfDecimals > 0)
            {
                for (int nI = 0; nI < numOfDecimals; nI++)
                {
                    result *= 10m;
                }
            }
            else if (numOfDecimals < 0)
            {
                for (int nI = numOfDecimals; nI < 0; nI++)
                {
                    result /= 10m;
                }
            }

            return result;
        }



        private void RefreshCashpool(bool state)
        {
            OnPropertyChanged("Cashpool");
        }
        #endregion
    }
}
