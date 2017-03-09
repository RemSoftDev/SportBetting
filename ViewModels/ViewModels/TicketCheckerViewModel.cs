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
    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class TicketCheckerViewModel : BaseViewModel
    {

        #region Constructors
        private static ILog Log = LogFactory.CreateLog(typeof(TicketCheckerViewModel));

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketCheckerViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public TicketCheckerViewModel()
        {
            ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.TICKET;
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockTimeFilter);
            Mediator.SendMessage<string>("", MsgTag.ResetFilters);
            ButtonDetailsCommand = new Command(OpenTicketDetails);
            Mediator.Register<string>(this, OnClear, MsgTag.ClearTicketNumber);
            Mediator.Register<string>(this, OnBackSpace, MsgTag.PinBackspace);
            Mediator.Register<string>(this, OnPinButton, MsgTag.PinButton);
            Mediator.Register<long>(this, OnLoadTicket, MsgTag.LoadTicket);

            StationRepository.BarcodeScannerTempActive = true;
            Log.Debug(String.Format("{0}.{1}", "Enabling scanner", "TicketCheckerViewModel"));
            if (ChangeTracker.LoadedTicketcheckSum.Length == BarCodeConverter.TicketCodeLen &&
                ChangeTracker.LoadedTicket.Length == BarCodeConverter.TicketNumLen && ChangeTracker.RedirectToTicketDetails)
            {
                LoadTicket(true);
            }
        }

        #endregion

        #region Properties

        public bool IsFocusedNumber
        {
            get { return _isFocusedNumber; }
            set
            {
                _isFocusedNumber = value;
                OnPropertyChanged("IsFocusedNumber");
            }
        }

        public bool IsFocusedCode
        {
            get { return _isFocusedCode; }
            set
            {
                _isFocusedCode = value;
                OnPropertyChanged("IsFocusedCode");
            }
        }

        private bool _detailsEnabled;
        private bool _isFocusedNumber;
        private bool _isFocusedCode;
        private bool _isTicket = true;
        private bool _isCreditNote;

        public bool IsTicket
        {
            get { return _isTicket; }
            set
            {
                _isTicket = value;
                if (value)
                {
                    _isCreditNote = false;
                    ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.TICKET;
                }
                else
                {
                    _isCreditNote = true;
                    ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.CREDIT_NOTE;
                }
                OnPropertyChanged("IsTicket");
                OnPropertyChanged("IsCreditNote");
            }
        }

        public bool IsCreditNote
        {
            get { return _isCreditNote; }
            set
            {
                _isCreditNote = value;
                if (value)
                {
                    _isTicket = false;
                    ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.CREDIT_NOTE;
                }
                else
                {
                    _isTicket = true;
                    ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.TICKET;
                }
                OnPropertyChanged("IsCreditNote");
                OnPropertyChanged("IsTicket");
            }
        }
        public string TicketNumber
        {
            get
            {
                if (null == ChangeTracker.LoadedTicket)
                    ChangeTracker.LoadedTicket = "";
                return ChangeTracker.LoadedTicket;
            }
            set
            {
                if (value != ChangeTracker.LoadedTicket)
                {
                    ChangeTracker.LoadedTicket = value;
                    OnPropertyChanged("TicketNumber");
                }
            }
        }

        public bool DetailsEnabled
        {
            get { return _detailsEnabled; }
            set
            {
                _detailsEnabled = value;
                OnPropertyChanged("DetailsEnabled");
            }
        }

        public bool IsDetailsButtonhidden
        {
            get { return ChangeTracker.LoadedTicketType == BarCodeConverter.BarcodeType.STORED_TICKET || ChangeTracker.LoadedTicketType == BarCodeConverter.BarcodeType.CREDIT_NOTE; }
        }

        public String TicketCode
        {
            get
            {
                if (ChangeTracker.LoadedTicketcheckSum == null)
                    ChangeTracker.LoadedTicketcheckSum = "";
                return ChangeTracker.LoadedTicketcheckSum;
            }
            set
            {
                if (value != ChangeTracker.LoadedTicketcheckSum && value != null && value.Length <= StationRepository.CheckSumLength)
                {
                    ChangeTracker.LoadedTicketcheckSum = value;
                    OnPropertyChanged();
                }
                if (ChangeTracker.LoadedTicketcheckSum != null && ChangeTracker.LoadedTicketcheckSum.Length == StationRepository.CheckSumLength)
                {
                    LoadTicket(true);
                }
            }
        }

        protected TicketWS CurrentTicket
        {
            get { return ChangeTracker.CurrentTicket; }
            set { ChangeTracker.CurrentTicket = value; }
        }


        #endregion

        #region Commands

        public Command ButtonDetailsCommand { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            ChangeTracker.SelectedTicket = true;
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.PinKeyboardRegion);

            base.OnNavigationCompleted();
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);
        }

        public void OnPinButtonTest(string obj)
        {
            OnPinButton(obj);
        }

        private void OnPinButton(string obj)
        {
            if (obj == null)
            {
                Log.Debug(String.Format(" Error: TicketCheckerViewModel.OnPinButton, obj empty"));
                return;
            }

            TextBox textBox = null;
            IInputElement target = null;
            if (TicketNumber == null) TicketNumber = "";
            if (TicketNumber.Length < BarCodeConverter.TicketNumLen)
            {
                if (!IsFocusedNumber)
                {
                    IsFocusedNumber = true;
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
                if (textBox != null) TicketNumber = textBox.Text;
            }
            else
            {
                if (TicketCode == null) TicketCode = "";

                if (!IsFocusedCode)
                {
                    IsFocusedCode = true;
                    target = System.Windows.Input.Keyboard.FocusedElement;
                    textBox = target as TextBox;
                    if (textBox != null)
                        textBox.SelectionStart = textBox.Text.Length;
                }
                var routedEvent = TextCompositionManager.TextInputEvent;
                target = System.Windows.Input.Keyboard.FocusedElement;
                textBox = target as TextBox;

                if (textBox.Text.Length < StationRepository.CheckSumLength)
                {
                    target.RaiseEvent(
                        new TextCompositionEventArgs(
                            InputManager.Current.PrimaryKeyboardDevice,
                            new TextComposition(InputManager.Current, target, obj)) { RoutedEvent = routedEvent }
                        );
                    if (textBox != null) TicketCode = textBox.Text;

                }
                //if (TicketNumber.Length == BarCodeConverter.TicketNumLen &&
                //    TicketNumber.Length == BarCodeConverter.TicketCodeLen)
                //{
                //    if (_isTicket)
                //    {
                //        Mediator.SendMessage(new Tuple<string, string>(TicketNumber, TicketCode),
                //                             MsgTag.AddMoneyFromTicket);
                //    }
                //    else if (_isCreditNote)
                //    {

                //        Mediator.SendMessage<Tuple<string, string>>(
                //            new Tuple<string, string>(TicketNumber, TicketCode), MsgTag.AddMoneyFromCreditNote);
                //    }
                //}
            }
        }

        private void OnBackSpace(string obj)
        {
            if (TicketCode.Length == 0 && IsFocusedCode)
            {
                IsFocusedNumber = true;
            }
            if (TicketCode.Length > 0 && !IsFocusedCode && !IsFocusedNumber)
            {
                TicketCode = TicketCode.Remove(TicketCode.Length - 1);
            }
            else if (TicketNumber.Length > 0 && !IsFocusedCode && !IsFocusedNumber)
            {
                TicketNumber = TicketNumber.Remove(TicketNumber.Length - 1);
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

                if (IsFocusedNumber)
                {
                    if (textBox != null) TicketNumber = textBox.Text;
                }
                if (IsFocusedCode)
                {
                    if (textBox != null) TicketCode = textBox.Text;
                }
            }
        }

        private void OnOpenDetails(long obj)
        {
            OpenTicketDetails();
        }

        private void OnLoadTicket(long obj)
        {
            PleaseWaitLoadTicket(true);
        }


        private void OnClear(string obj)
        {
            TicketCode = "";
            TicketNumber = "";
        }

        private void OpenTicketDetails()
        {
            MyRegionManager.NavigateUsingViewModel<TicketDetailsViewModel>(RegionNames.ContentRegion);
        }

        [AsyncMethod]
        private void LoadTicket(bool redirect)
        {
            PleaseWaitLoadTicket(redirect);
        }


        [PleaseWaitAspect]
        private void PleaseWaitLoadTicket(bool redirect)
        {

            if (ChangeTracker.LoadedTicketcheckSum.Length < BarCodeConverter.TicketCodeLen || ChangeTracker.LoadedTicket.Length < BarCodeConverter.TicketNumLen) return;
            DetailsEnabled = false;
            try
            {
                if (ChangeTracker.LoadedTicketType == BarCodeConverter.BarcodeType.CREDIT_NOTE)
                {
                    Mediator.SendMessage(new Tuple<string, string>(TicketNumber, TicketCode), MsgTag.AddMoneyFromCreditNote);
                    return;
                }
                CurrentTicket = WsdlRepository.LoadTicket(ChangeTracker.LoadedTicket, ChangeTracker.LoadedTicketcheckSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);
                if (CurrentTicket != null)
                {
                    long id = 1;
                    string result = "1";
                    try
                    {
                        result = WsdlRepository.GetAccountByTicket(ChangeTracker.LoadedTicket);
                    }
                    catch (Exception)
                    {
                        result = "1";
                    }
                    long.TryParse(result, out id);
                    if ((ChangeTracker.CurrentUser != null && (id == ChangeTracker.CurrentUser.AccountId || id == 1)) || !StationRepository.AuthorizedTicketScanning)
                    {
                        ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.TICKET;
                    }
                    else
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.THIS_TICKET_DOES_NOT_BELONG_TO_YOU).ToString());
                        return;
                    }
                }



                if (ChangeTracker.LoadedTicketType == BarCodeConverter.BarcodeType.STORED_TICKET)
                {
                    Mediator.SendMessage("", MsgTag.OpenStoredTicket);
                    return;
                }

                if (CurrentTicket == null && ChangeTracker.LoadedTicketType == BarCodeConverter.BarcodeType.TICKET)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_TICKET_NOT_FOUND).ToString());
                }
                else
                {
                    DetailsEnabled = true;
                    if (redirect)
                        MyRegionManager.NavigateUsingViewModel<TicketDetailsViewModel>(RegionNames.ContentRegion);
                }
            }
            catch (FaultException<HubServiceException> ex)
            {
                if (ex.Detail.code == 220)
                    ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_TICKET_NOT_FOUND).ToString());
                else if (ex.Detail.code == 1791)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_INVALIDFRANCHISOR).ToString());
                }
                else if (ex.Detail.code == 1001)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.ANONYMOUS_BETTING_IS_DISABLED).ToString());
                }
                else
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_INVALIDLOCATION).ToString());
            }
            catch (Exception)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR).ToString());
            }
        }

        public override void Close()
        {
            StationRepository.BarcodeScannerTempActive = false;
            Log.Debug(String.Format("{0}.{1}", "Disabling scanner", "TicketCheckerViewModel"));
            MyRegionManager.CloseAllViewsInRegion(RegionNames.PinKeyboardRegion);


            base.Close();
        }

        #endregion
    }
}