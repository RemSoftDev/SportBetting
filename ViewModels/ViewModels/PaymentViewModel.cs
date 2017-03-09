using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class PaymentViewModel : BaseViewModel
    {
        private string _paymentNoteNumber = "";
        private bool _isFocusedPaymentNote;
        private bool _paymentNoteChecked = true;
        private bool _creditNoteChecked;
        private DateTime? _expiryDate;
        private static ILog Log = LogFactory.CreateLog(typeof(PaymentViewModel));

        #region Constructors

        public PaymentViewModel()
        {
            OpenShiftReport = new Command(OnOpenShiftReport);
            ShowHideShift = new Command(OnShowHideShift);

            Mediator.Register<string>(this, OnClear, MsgTag.ClearTicketNumber);
            Mediator.Register<string>(this, OnBackSpace, MsgTag.PinBackspace);
            Mediator.Register<string>(this, OnPinButton, MsgTag.PinButton);
            Mediator.Register<string>(this, LoadPaymentNote, MsgTag.LoadPaymentNote);
            Mediator.Register<bool>(this, SetCreditNoteButton, MsgTag.SetCreditNoteButton);

            IsFocusedPaymentNote = true;
            StationRepository.BarcodeScannerTempActive = true;
            ChangeTracker.OperatorPaymentViewOpen = true;
            Log.Debug(String.Format("{0}.{1}", "Enabling scanner", "PaymentViewModel"));
        }



        #endregion

        #region Properties

        private OperatorShiftData osd;
        public OperatorShiftData OSD
        {
            get { return osd; }

        }

        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        public bool ShowShift
        {
            get { return _showShift; }
            set
            {
                _showShift = value;
                OnPropertyChanged();
            }
        }

        public bool IsFocusedPaymentNote
        {
            get { return _isFocusedPaymentNote; }
            set
            {
                _isFocusedPaymentNote = value;
                OnPropertyChanged();
            }
        }

        public string PaymentNoteNumber
        {
            get { return _paymentNoteNumber; }
            set
            {
                _paymentNoteNumber = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region Commands

        public Command OpenShiftReport { get; set; }
        public Command ShowHideShift { get; set; }
        #endregion

        #region Methods

        [AsyncMethod]
        private void LoadOperatorShifts()
        {
            onLoadData();
        }


        private void OnShowHideShift()
        {
            ShowShift = !ShowShift;
        }


        private void OnOpenShiftReport()
        {
            MyRegionManager.NavigateUsingViewModel<OperatorShiftReportViewModel>(RegionNames.UsermanagementContentRegion);
        }

        [WsdlServiceSyncAspect]
        private void onLoadData()
        {
            try
            {
                decimal tempBalance;
                WsdlRepository.GetOperatorShiftCheckpoints(StationRepository.LocationID, Int16.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).account_id), out tempBalance, false);
                //Balance = tempBalance;
                osd = WsdlRepository.GetOperatorShiftReport(StationRepository.LocationID, Int16.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).account_id));
                Balance = osd.balance;
                OnPropertyChanged("osd");

            }
            catch (FaultException<HubServiceException> e)
            {
                
                Log.Error(e.Message,e);
            }

        }


        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.PaymentNotePinKeyboardRegion);
            ChangeTracker.AdminTitle2 = null;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_TICKET;
            ChangeTracker.TicketChecked = true;
            LoadOperatorShifts();
            base.OnNavigationCompleted();
        }

        private void SetCreditNoteButton(bool obj)
        {
            PaymentNoteChecked = !obj;
            CreditNoteChecked = obj;
        }

        public bool PaymentNoteChecked
        {
            get { return _paymentNoteChecked; }
            set
            {
                _paymentNoteChecked = value;
                OnPropertyChanged();
            }
        }
        public bool CreditNoteChecked
        {
            get { return _creditNoteChecked; }
            set
            {
                _creditNoteChecked = value;
                OnPropertyChanged();
            }
        }

        private void OnPinButton(string obj)
        {
            TextBox textBox = null;
            IInputElement target = null;
            if (PaymentNoteNumber == null) PaymentNoteNumber = "";
            if (PaymentNoteNumber.Length < StationRepository.PaymentNoteLength)
            {

                if (!IsFocusedPaymentNote)
                {
                    IsFocusedPaymentNote = true;
                    target = System.Windows.Input.Keyboard.FocusedElement;
                    textBox = target as TextBox;
                    if (textBox != null)
                        textBox.SelectionStart = textBox.Text.Length;
                }
                var routedEvent = TextCompositionManager.TextInputEvent;
                target = System.Windows.Input.Keyboard.FocusedElement;
                textBox = target as TextBox;
                target.RaiseEvent(
                  new TextCompositionEventArgs(
                    InputManager.Current.PrimaryKeyboardDevice,
                    new TextComposition(InputManager.Current, target, obj)) { RoutedEvent = routedEvent }
                );
                if (textBox != null) PaymentNoteNumber = textBox.Text;
            }

            if (PaymentNoteNumber.Length == StationRepository.PaymentNoteLength)
                LoadPaymentNote(PaymentNoteNumber);
            if (!PaymentNoteChecked && BarCodeConverter.TicketNumLen + BarCodeConverter.TicketCodeLen == PaymentNoteNumber.Length)
                LoadPaymentNote(PaymentNoteNumber);


        }


        private void LoadPaymentNote(string paymentNoteNumber)
        {
            Log.Debug("BARCODE: LoadPaymentNote(1)");
            OnLoadPaymentNote(paymentNoteNumber);
        }



        private decimal amount = 0;
        private bool _showShift = true;
        private decimal _balance;

        [WsdlServiceSyncAspectSilent]
        private void OnLoadPaymentNote(string paymentNoteNumber)
        {
            Log.Debug("BARCODE: LoadPaymentNote(2)");
            Blur();
            PaymentNoteNumber = paymentNoteNumber;
            if (_paymentNoteChecked) Log.Debug("Paymentnote number:" + PaymentNoteNumber);
            else
                Log.Debug("Credit number:" + PaymentNoteNumber);
            try
            {
                if (_paymentNoteChecked)
                {
                    amount = 0;

                    if (StationRepository.IsImmediateWithdrawEnabled)
                    {
                        OnRegisterPaymentNote();
                    }
                    else
                    {
                        var userData = WsdlRepository.LoadPaymentNote(PaymentNoteNumber, StationRepository.GetUid(ChangeTracker.CurrentUser), out amount);
                        //var username = userData.fields.Where(x => x.name == "username").Select(x => x.value).FirstOrDefault();
                        var firstname = "";
                        var lastname = "";
                        var documentType = "";
                        var documentNumber = "";


                        foreach (var formField in userData.fields)
                        {
                            if (formField.name == "firstname")
                                firstname = formField.value ?? "";
                            if (formField.name == "lastname")
                                lastname = formField.value ?? "";
                            if (formField.name == "document_type")
                                documentType = formField.value ?? "";
                            if (formField.name == "document_number")
                                documentNumber = formField.value ?? "";
                        }
                        var amountEuros = (int)amount;
                        int amountCents = (int)((amount - amountEuros) * 100);

                        var text = TranslationProvider.Translate(MultistringTags.PAY_PAYMENTNOTE, amount, StationRepository.Currency, firstname, lastname) + "\r\n";
                        text += TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DOCUMENT_TYPE, TranslationProvider.Translate(MultistringTag.Assign(documentType, documentType)).ToString()) + "\r\n";
                        text += TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DOCUMENT_NUMBER, documentNumber) + "\r\n";

                        var yesButtonText = TranslationProvider.Translate(MultistringTags.PAYMENT_DONE).ToString();
                        var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL).ToString();

                        QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, model_YesClick, model_NoClick);
                    }

                }
                else
                {
                    if (!StationRepository.AllowAnonymousBetting)
                    {
                        UnBlur();
                        ShowError(TranslationProvider.Translate(MultistringTags.ANONYMOUS_BETTING_IS_DISABLED_CREDITNOTE) as string);
                        return;
                    }
                    if (StationRepository.IsCreditNoteImmediatelyPaid)
                    {
                        OnRegisterCreditNote();
                    }
                    else
                    {
                        var creditNote =
                            WsdlRepository.LoadCreditNote(PaymentNoteNumber.Substring(0, PaymentNoteNumber.Length - 4),
                                                          PaymentNoteNumber.Substring(PaymentNoteNumber.Length - 4),
                                                          StationRepository.StationNumber);

                        amount = creditNote.amount;
                        _expiryDate = creditNote.expireDate;
                        var amountEuros = (int)creditNote.amount;
                        int amountCents = (int)((creditNote.amount - amountEuros) * 100);
                        string text = null;

                        var yesButtonText = TranslationProvider.Translate(MultistringTags.PAYMENT_DONE).ToString();
                        var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL).ToString();

                        if (_expiryDate < DateTime.Now && StationRepository.PayoutExpiredPaymentCreditNotes)
                        {
                            text =
                                string.Format(
                                    TranslationProvider.Translate(MultistringTags.PAY_EXPIRED_CREDITNOTE).ToString(),
                                    creditNote.amount, StationRepository.Currency);

                            QuestionWindowService.ShowMessage(text, null, null, modelCreditNote_YesClick, null,
                                                              warning: true);
                        }
                        else if (_expiryDate < DateTime.Now && !StationRepository.PayoutExpiredPaymentCreditNotes)
                        {
                            ErrorWindowService.ShowError(
                                TranslationProvider.Translate(MultistringTags.EXPIRED_CREDITNOTE).ToString());
                        }
                        else
                        {
                            text =
                                TranslationProvider.Translate(MultistringTags.PAY_CREDITNOTE,
                                              creditNote.amount, StationRepository.Currency) + "\r\n";
                            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText,
                                                              modelCreditNote_YesClick, model_NoClick);
                        }
                    }

                }
            }
            catch (FaultException<HubServiceException> error)
            {

                switch (error.Detail.code)
                {
                    case 170:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENTNOTE_NOTFOUND).ToString());
                        break;
                    case 171:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_EXPIRED).ToString());
                        break;
                    case 174:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_PAID).ToString());
                        break;
                    case 179:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDLOCATION).ToString());
                        break;
                    case 1791:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDFRANCHISOR).ToString());
                        break;
                    case 401:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_ALREADY_EXISTS).ToString());
                        break;
                    case 402:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_NOT_ACTIVE).ToString());
                        break;
                    case 403:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_INVALID_AMOUNT).ToString());
                        break;
                    case 404:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_PAID_CREDITNOTE).ToString());
                        break;
                    case 405:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_EXPIRED).ToString());
                        break;
                    default:
                        ErrorWindowService.ShowError(error.Detail.message);

                        break;
                }

            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            UnBlur();
        }

        private void modelCreditNote_YesClick(object sender, EventArgs e)
        {
            UnBlur();
            modelCreditNote_YesClickPleaseWait(sender, e);
        }

        private void OnRegisterCreditNote()
        {
            modelCreditNote_YesClickPleaseWait(null, null, true);
        }

        [WsdlServiceSyncAspect]
        private void modelCreditNote_YesClickPleaseWait(object sender, EventArgs e, bool immediatelyPaid = false)
        {

            try
            {
                bool result = WsdlRepository.PayCreditNote(StationRepository.GetUid(ChangeTracker.CurrentUser), PaymentNoteNumber.Substring(0, PaymentNoteNumber.Length - 4), PaymentNoteNumber.Substring(PaymentNoteNumber.Length - 4), ChangeTracker.CurrentUser.AccountId.ToString(), StationRepository.StationNumber);
                if (!result)
                {
                    ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CANT_WITHDRAW_MONEY_FROM_PAID_CREDIT_NOTE) as string);
                }
                else
                {
                    if (immediatelyPaid)
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.REGISTER_PAYMENT_DONE, PaymentNoteNumber), null, false, 3);
                    else
                    {
                        PrinterHandler.PrintPaymentRecept(PaymentNoteNumber.Substring(0, PaymentNoteNumber.Length - 4),
                                                          PaymentNoteNumber.Substring(PaymentNoteNumber.Length - 4),
                                                          amount, true);
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_DONE) as string);
                    }
                }
                PaymentNoteNumber = "";

                LoadOperatorShifts();

            }
            catch (FaultException<HubServiceException> error)
            {
                switch (error.Detail.code)
                {
                    case 150:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.NOT_ENOUGHT_MONEY_TO_WITHDRAW).ToString());
                        break;

                    case 179:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDLOCATION).ToString());
                        break;
                    case 1791:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDFRANCHISOR).ToString());
                        break;
                    case 404:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_PAID_CREDITNOTE).ToString());
                        break;
                    case 170:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENTNOTE_NOTFOUND).ToString());
                        break;
                    case 171:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_EXPIRED).ToString());
                        break;
                    case 174:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_PAID).ToString());
                        break;
                    case 401:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_ALREADY_EXISTS).ToString());
                        break;
                    case 402:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_NOT_ACTIVE).ToString());
                        break;
                    case 403:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_INVALID_AMOUNT).ToString());
                        break;
                    case 405:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_EXPIRED).ToString());
                        break;
                    default:
                        ErrorWindowService.ShowError(error.Detail.message);

                        break;

                }
            }
        }


        void model_NoClick(object sender, EventArgs e)
        {
            UnBlur();
            PaymentNoteNumber = "";
        }

        [AsyncMethod]
        void model_YesClick(object sender, EventArgs e)
        {
            PleaseWaitmodel_YesClick(sender, e);
        }

        [WsdlServiceSyncAspect]
        private void OnRegisterPaymentNote()
        {
            try
            {
                decimal amount = 0;
                bool withFrombalance;
                WsdlRepository.WithdrawByPaymentNote(PaymentNoteNumber, StationRepository.GetUid(ChangeTracker.CurrentUser), out amount, out withFrombalance);
                ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.REGISTER_PAYMENT_DONE, PaymentNoteNumber), null, false, 3);
                PaymentNoteNumber = "";

            }
            catch (FaultException<HubServiceException> error)
            {
                switch (error.Detail.code)
                {
                    case 150:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.NOT_ENOUGHT_MONEY_TO_WITHDRAW).ToString());
                        break;

                    case 179:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_INVALIDLOCATION).ToString());
                        break;

                    case 1791:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDFRANCHISOR).ToString());
                        break;
                    case 170:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENTNOTE_NOTFOUND).ToString());
                        break;
                    case 171:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_EXPIRED).ToString());
                        break;
                    case 174:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_PAID).ToString());
                        break;
                    case 401:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_ALREADY_EXISTS).ToString());
                        break;
                    case 402:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_NOT_ACTIVE).ToString());
                        break;
                    case 403:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_INVALID_AMOUNT).ToString());
                        break;
                    case 404:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_PAID_CREDITNOTE).ToString());
                        break;
                    case 405:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_EXPIRED).ToString());
                        break;
                    default:
                        ErrorWindowService.ShowError(error.Detail.message);

                        break;

                }
            }
        }

        [WsdlServiceSyncAspect]
        void PleaseWaitmodel_YesClick(object sender, EventArgs e)
        {
            try
            {
                decimal amount = 0;
                bool withFrombalance;
                WsdlRepository.WithdrawByPaymentNote(PaymentNoteNumber, StationRepository.GetUid(ChangeTracker.CurrentUser), out amount, out withFrombalance);
                PrinterHandler.PrintPaymentRecept(PaymentNoteNumber, "", amount, false);
                ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_DONE) as string);
                PaymentNoteNumber = "";

                LoadOperatorShifts();

            }
            catch (FaultException<HubServiceException> error)
            {
                switch (error.Detail.code)
                {
                    case 150:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.NOT_ENOUGHT_MONEY_TO_WITHDRAW).ToString());
                        break;

                    case 179:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_INVALIDLOCATION).ToString());
                        break;

                    case 1791:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDFRANCHISOR).ToString());
                        break;
                    case 170:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENTNOTE_NOTFOUND).ToString());
                        break;
                    case 171:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_EXPIRED).ToString());
                        break;
                    case 174:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_PAID).ToString());
                        break;
                    case 401:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_ALREADY_EXISTS).ToString());
                        break;
                    case 402:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_NOT_ACTIVE).ToString());
                        break;
                    case 403:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_INVALID_AMOUNT).ToString());
                        break;
                    case 404:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_PAID_CREDITNOTE).ToString());
                        break;
                    case 405:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_EXPIRED).ToString());
                        break;
                    default:
                        ErrorWindowService.ShowError(error.Detail.message);

                        break;

                }
            }
        }

        private void OnBackSpace(string obj)
        {
            if (PaymentNoteNumber.Length > 0 && !IsFocusedPaymentNote)
            {
                PaymentNoteNumber = PaymentNoteNumber.Remove(PaymentNoteNumber.Length - 1);
            }
            else
            {
                var key = Key.Back;
                var target = System.Windows.Input.Keyboard.FocusedElement;
                var routedEvent = System.Windows.Input.Keyboard.KeyDownEvent;

                target.RaiseEvent(
                  new KeyEventArgs(
                    System.Windows.Input.Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(target as Visual),
                    0,
                    key) { RoutedEvent = routedEvent }
                );
                var textBox = target as TextBox;

                if (textBox != null) PaymentNoteNumber = textBox.Text;
            }

        }

        private void OnClear(string obj)
        {
            PaymentNoteNumber = "";
        }

        public override void Close()
        {
            ChangeTracker.OperatorPaymentViewOpen = false;
            StationRepository.BarcodeScannerTempActive = false;
            Log.Debug(String.Format("{0}.{1}", "Disabling scanner", "PaymentViewModel"));
            MyRegionManager.CloseAllViewsInRegion(RegionNames.PaymentNotePinKeyboardRegion);

            base.Close();
        }

        #endregion
    }
}