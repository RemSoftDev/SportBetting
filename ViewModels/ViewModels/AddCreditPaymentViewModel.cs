using System;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class AddCreditPaymentViewModel : BaseViewModel
    {
        #region Constructors

        public AddCreditPaymentViewModel()
        {
            string type = ChangeTracker.PaymentFlowOperationType;
            if (type == "credit")
            {
                Text = TranslationProvider.Translate(MultistringTags.TERMINAL_ADD_CREDIT) as string;
                HeaderText = TranslationProvider.Translate(MultistringTags.TERMINAL_PLEASE_ADD_AMOUNT,  (TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT) as string));
            }
            else
            {
                Text = TranslationProvider.Translate(MultistringTags.TERMINAL_ADD_PAYMENT) as string;
                HeaderText = TranslationProvider.Translate(MultistringTags.TERMINAL_PLEASE_ADD_AMOUNT, (TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_PAYMENT) as string));
            }

            YesCommand = new Command(YesOnClick);
            CloseCommand = new Command(OnClose);
            NoButtonText = TranslationProvider.Translate(MultistringTags.TERMINAL_CANCEL) as string;
            YesButtonText = TranslationProvider.Translate(MultistringTags.TERMINAL_ADMIN_MENU_OK) as string;
        }

        #endregion

        #region Properties

        private string _text;
        private string _yesButtonText;
        private string _noButtonText;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
            }
        }

        private string _headerText;
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
            }
        }

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
            }
        }

        private bool _isFocusedNumber;
        public bool IsFocusedNumber
        {
            get { return _isFocusedNumber; }
            set
            {
                _isFocusedNumber = value;

                if (_isFocusedNumber)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }

                OnPropertyChanged("IsFocusedNumber");
            }
        }

        private bool _isFocusedComment;
        public bool IsFocusedComment
        {
            get { return _isFocusedComment; }
            set
            {
                _isFocusedComment = value;

                if (_isFocusedComment)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }

                OnPropertyChanged("IsFocusedComment");
            }
        }

        public string Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                ValidateFields();
                OnPropertyChanged();
            }
        }


        public string YesButtonText
        {
            get { return _yesButtonText; }
            set
            {
                _yesButtonText = value;
                OnPropertyChanged("YesButtonText");
            }
        }

        public string NoButtonText
        {
            get { return _noButtonText; }
            set
            {
                _noButtonText = value;
                OnPropertyChanged("NoButtonText");
            }
        }

        private decimal _cashPosition;
        public decimal CashPosition
        {
            get { return _cashPosition; }
            set
            {
                _cashPosition = value;
                OnPropertyChanged("CashPosition");
            }
        }

        private decimal _paymentBalance;
        public decimal PaymentBalance
        {
            get { return _paymentBalance; }
            set
            {
                _paymentBalance = value;
                OnPropertyChanged("PaymentBalance");
            }
        }

        private decimal _cashBalance;
        public decimal CashBalance
        {
            get { return _cashBalance; }
            set
            {
                _cashBalance = value;
                OnPropertyChanged("CashBalance");
            }
        }

        private decimal _saldo;
        public decimal Saldo
        {
            get { return _saldo; }
            set
            {
                _saldo = value;
                OnPropertyChanged("Saldo");
            }
        }

        private string AmountErrorMessage = "";
        private string _amount;

        //private static IWsdlRepository WsdlRepository
        //{
        //    get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        //}

        #endregion

        #region Commands

        public Command YesCommand { get; set; }
        public Command CloseCommand { get; set; }

        #endregion

        #region methods

        [AsyncMethod]
        private void YesOnClick()
        {
            YesOnClickCont();
        }

        [WsdlServiceSyncAspect]
        private void YesOnClickCont()
        {
            if (Amount == "" || Amount == null)
                return;

            decimal amount = 0;
            bool result = decimal.TryParse(Amount, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo, out amount);

            if (!result)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NONDECIMAL_VALUE_ERROR_BODY).ToString(), null, false, 5);
                return;
            }

            string type = ChangeTracker.PaymentFlowOperationType;
            PaymentFlowData request = new PaymentFlowData();

            if (type == "credit")
                request.type = "CREDIT";
            else
                request.type = "PAYMENT";

            //decimal tempAmount = 0;
            //Decimal.TryParse(Amount, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out tempAmount);

            request.amount = amount;
            request.comment = Comment;
            request.stationNumber = StationRepository.StationNumber;

            if (ChangeTracker.CurrentUser is OperatorUser)
                request.operatorName = ((OperatorUser)ChangeTracker.CurrentUser).Username;
            else
                request.operatorName = "";

            try
            {
                WsdlRepository.AddPaymentFlow(request);

                bool isPrinted = PrinterHandler.PrintShopPaymentReciept(ChangeTracker.PaymentFlowOperationType, amount, ChangeTracker.CurrentUser.AccountId, ChangeTracker.CurrentUser.Username);
                if (!isPrinted)
                    ShowPrinterErrorMessage();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        public void  OnClose()
        {
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        public override void Close()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            IsFocusedComment = false;
            IsFocusedNumber = false;
            base.Close();
        }

        protected void ValidateFields()
        {
            // TODO refactoring
            if (string.IsNullOrEmpty(Amount))
                return;

            decimal temp = 0;
            bool result = decimal.TryParse(Amount, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo, out temp);

            if (!result)
            {
                AmountErrorMessage = "Not decimal";
            }
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = "";

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

        [WsdlServiceSyncAspect]
        private void OnLoadData()
        {
            decimal paymentBalance = 0;
            decimal locationCashPosition = 0;
            decimal totalLocationBalance = 0;
            long itemsTotal = 0;
            PaymentFlowData[] list;

            //download data from hub
            Saldo = WsdlRepository.GetStationPaymentFlowData(StationRepository.StationNumber, 1, 1, out paymentBalance, out locationCashPosition, out totalLocationBalance, out list, out itemsTotal);

            CashPosition = locationCashPosition;
            CashBalance = totalLocationBalance;
            PaymentBalance = paymentBalance;
        }

        public override void OnNavigationCompleted()
        {
            OnLoadData();
            base.OnNavigationCompleted();
        }

        #endregion
    }
}
