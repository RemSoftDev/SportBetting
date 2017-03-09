using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class PrintTicketViewModel : BaseViewModel
    {
        private static readonly ILog Log = LogFactory.CreateLog(typeof(PrintTicketViewModel));

        #region Constructors

        public PrintTicketViewModel()
        {
            PrintTickets = new Command(OnPrintTickets);
            Print1Tickets = new Command(OnPrint1Tickets);
            Print5Tickets = new Command(OnPrint5Tickets);
            Print10Tickets = new Command(OnPrint10Tickets);
            Print20Tickets = new Command(OnPrint20Tickets);

            Mediator.Register<string>(this, OnClear, MsgTag.ClearTicketNumber);
            Mediator.Register<string>(this, OnBackSpace, MsgTag.PinBackspace);
            Mediator.Register<string>(this, OnPinButton, MsgTag.PinButton);

            Log.Debug(String.Format("{0}.{1}", "Enabling scanner", "PaymentViewModel"));
        }


        #endregion

        #region Properties

        private bool _isFocusedPaymentNote = true;
        private string _amountNumber;


        public bool IsFocusedPaymentNote
        {
            get { return _isFocusedPaymentNote; }
            set
            {
                _isFocusedPaymentNote = value;
                OnPropertyChanged();
            }
        }


        public string AmountNumber
        {
            get { return _amountNumber; }
            set
            {
                _amountNumber = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public Command PrintTickets { get; set; }
        public Command Print1Tickets { get; set; }
        public Command Print5Tickets { get; set; }
        public Command Print10Tickets { get; set; }
        public Command Print20Tickets { get; set; }

        #endregion

        #region Methods

        private void OnPrint1Tickets()
        {
            AmountNumber = "1";
            OnPrintTickets();
        }
        private void OnPrint5Tickets()
        {
            AmountNumber = "5";
            OnPrintTickets();
        }
        private void OnPrint10Tickets()
        {
            AmountNumber = "10";
            OnPrintTickets();
        }
        private void OnPrint20Tickets()
        {
            AmountNumber = "20";
            OnPrintTickets();
        }

        private void OnPrintTickets()
        {
            WaitOverlayProvider.ShowWaitOverlay(true);

            int amount = 0;

            int.TryParse(AmountNumber, out amount);

            if (amount > PrinterHandler.PrintedTicketsCount || amount > 20)
            {
                int maxAmount = PrinterHandler.PrintedTicketsCount > 20 ? 20 : PrinterHandler.PrintedTicketsCount;
                ShowError(TranslationProvider.Translate(MultistringTags.DUBLICATES_AMOUNT, maxAmount));
                return;
            }
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }
            if (!PrinterHandler.PrintLastTickets(amount))
                Log.Error("printing ticket duplicate failed",new Exception());

            WaitOverlayProvider.DisposeAll();
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;
            string errorMessage = "";

            errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_TICKET) + ", ";

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


        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.PrintTicketPinKeyboardRegion);
            ChangeTracker.AdminTitle2 = null;
            ChangeTracker.AdminTitle1 = MultistringTags.PRINT_TICKETS;
            ChangeTracker.PrintTicketChecked = true;
            base.OnNavigationCompleted();
        }


        private void OnPinButton(string obj)
        {
            TextBox textBox = null;
            IInputElement target = null;
            if (AmountNumber == null)
                AmountNumber = "";
            {
                if (!IsFocusedPaymentNote)
                {
                    IsFocusedPaymentNote = true;
                    target = System.Windows.Input.Keyboard.FocusedElement;
                    textBox = target as TextBox;
                    if (textBox != null)
                        textBox.SelectionStart = textBox.Text.Length;
                }
                RoutedEvent routedEvent = TextCompositionManager.TextInputEvent;
                target = System.Windows.Input.Keyboard.FocusedElement;
                textBox = target as TextBox;
                target.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice, new TextComposition(InputManager.Current, target, obj)) { RoutedEvent = routedEvent });
                if (textBox != null)
                    AmountNumber = textBox.Text;
            }
        }

        private void OnBackSpace(string obj)
        {
            if (AmountNumber.Length > 0)
            {
                AmountNumber = AmountNumber.Remove(AmountNumber.Length - 1);
            }
            else
            {
                var key = Key.Back;
                IInputElement target = System.Windows.Input.Keyboard.FocusedElement;
                RoutedEvent routedEvent = System.Windows.Input.Keyboard.KeyDownEvent;

                target.RaiseEvent(new KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, PresentationSource.FromVisual(target as Visual), 0, key) { RoutedEvent = routedEvent });
                var textBox = target as TextBox;

                if (textBox != null)
                    AmountNumber = textBox.Text;
            }
        }

        private void OnClear(string obj)
        {
            AmountNumber = "";
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