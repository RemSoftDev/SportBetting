using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using IocContainer;
using Ninject;
using Shared;
using Shared.Annotations;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using System.Linq;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;
using System.Timers;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Timer = System.Timers.Timer;
using UserControl = System.Windows.Controls.UserControl;
using SportRadar.DAL.OldLineObjects;

namespace BaseObjects.ViewModels
{
    [ServiceAspect]
    public class BaseViewModel : IBaseViewModel, INotifyPropertyChanged
    {
        public static ILog Log = LogFactory.CreateLog(typeof(BaseViewModel));
        static DateTime _datetime = DateTime.Now;

        public BaseViewModel()
        {
            SBTimer.Interval = 1000;
            SBTimer.Elapsed += SBTimerElapsed;
            MouseDownCommand = new Command<MouseButtonEventArgs>(OnPreviewMouseDown);
            MouseMoveCommand = new Command<TouchEventArgs>(OnPreviewMouseMove);
            KeydownCommand = new Command<KeyEventArgs>(OnKeyDown);
        }

        public DateTime Datetime
        {
            get
            {
                return _datetime;
            }
            set
            {
                _datetime = value;
                OnPropertyChanged();
            }
        }

        public string BuildVersion { get; set; }



        public string SelectedLanguage
        {
            get { return TranslationProvider.CurrentLanguage; }
            set { TranslationProvider.CurrentLanguage = value; }
        }

        public IRepository _repository;
        public IRepository Repository
        {
            get
            {
                return _repository ?? (_repository = IoCContainer.Kernel.Get<IRepository>());
            }
        }

        private ITranslationProvider _translationProvider;
        public ITranslationProvider TranslationProvider
        {
            get
            {
                return _translationProvider ?? (_translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>());
            }
        }

        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }
        private IStationSettings _stationSettings;
        public IStationSettings StationSettings
        {
            get
            {
                return _stationSettings ?? (_stationSettings = IoCContainer.Kernel.Get<IStationSettings>());
            }
        }
        private ITransactionQueueHelper _transactionQueueHelper;
        public ITransactionQueueHelper TransactionQueueHelper
        {
            get
            {
                return _transactionQueueHelper ?? (_transactionQueueHelper = IoCContainer.Kernel.Get<ITransactionQueueHelper>());
            }
        }
        public INDEServer NDEServer { get { return IoCContainer.Kernel.Get<INDEServer>(); } }
        public IProcessExecutor ProcessExecutor { get { return IoCContainer.Kernel.Get<IProcessExecutor>(); } }

        private ILineSr _LineSr;
        public ILineSr LineSr
        {
            get
            {
                return _LineSr ?? (_LineSr = IoCContainer.Kernel.Get<ILineSr>());
            }
        }
        public ILineProvider LineProvider
        {
            get { return IoCContainer.Kernel.Get<ILineProvider>(); }
        }
        public IErrorWindowService ErrorWindowService
        {
            get { return IoCContainer.Kernel.Get<IErrorWindowService>(); }
        }
        public IQuestionWindowService QuestionWindowService
        {
            get { return IoCContainer.Kernel.Get<IQuestionWindowService>(); }
        }



        public IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        public string Currency
        {
            get { return StationRepository.Currency; }
            set { StationRepository.Currency = value; }
        }


        protected bool IsBindingCard
        {
            get { return ChangeTracker.IsBindingCard; }
            set { ChangeTracker.IsBindingCard = value; }
        }


        private IChangeTracker _changeTracker;
        public IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }
        private ILiveStreamService _liveStreamService;
        public ILiveStreamService LiveStreamService
        {
            get
            {
                return _liveStreamService ?? (_liveStreamService = IoCContainer.Kernel.Get<ILiveStreamService>());
            }
        }
        private IDispatcher _dispatcher;
        public virtual IDispatcher Dispatcher
        {
            get
            {
                return _dispatcher ?? (_dispatcher = IoCContainer.Kernel.Get<IDispatcher>());
            }
            set { throw new NotImplementedException(); }
        }
        private IBusinessPropsHelper _businessPropsHelper;
        public IBusinessPropsHelper BusinessPropsHelper
        {
            get
            {
                return _businessPropsHelper ?? (_businessPropsHelper = IoCContainer.Kernel.Get<IBusinessPropsHelper>());
            }
        }

        private IMyRegionManager _myRegionManager;
        public IMyRegionManager MyRegionManager
        {
            get
            {
                return _myRegionManager ?? (_myRegionManager = IoCContainer.Kernel.Get<IMyRegionManager>());
            }
        }

        private IWaitOverlayProvider _waitOverlayProvider;
        public IWaitOverlayProvider WaitOverlayProvider
        {
            get
            {
                return _waitOverlayProvider ?? (_waitOverlayProvider = IoCContainer.Kernel.Get<IWaitOverlayProvider>());
            }
        }

        private ITicketHandler _ticketHandler;
        public ITicketHandler TicketHandler
        {
            get
            {
                return _ticketHandler ?? (_ticketHandler = IoCContainer.Kernel.Get<ITicketHandler>());
            }
        }
        private IDataBinding _dataBinding;
        public IDataBinding DataBinding
        {
            get
            {
                return _dataBinding ?? (_dataBinding = IoCContainer.Kernel.Get<IDataBinding>());
            }
        }

        private ITicketActions _ticketActions;
        public ITicketActions TicketActions
        {
            get
            {
                return _ticketActions ?? (_ticketActions = IoCContainer.Kernel.Get<ITicketActions>());
            }
        }
        private IEnterPinWindowService _enterPinWindowService;
        public IEnterPinWindowService EnterPinWindowService
        {
            get
            {
                return _enterPinWindowService ?? (_enterPinWindowService = IoCContainer.Kernel.Get<IEnterPinWindowService>());
            }
        }
        private IPrinterHandler _printerHandler;
        public IPrinterHandler PrinterHandler
        {
            get
            {
                return _printerHandler ?? (_printerHandler = IoCContainer.Kernel.Get<IPrinterHandler>());
            }
        }


        private IMessageStorage _mediator;
        public IMessageStorage Mediator
        {
            get
            {
                return _mediator ?? (_mediator = IoCContainer.Kernel.Get<IMessageStorage>());
            }
        }



        public Command<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            ChangeTracker.MouseClickLastTime = DateTime.Now;
            //            CurrentViewName = e.Source.GetType().FullName;
            var command = SportBetting.WPF.Prism.WpfHelper.ButtonBehaviour.GetPreviewMouseDownCommand(e.Source as DependencyObject);
            if (command != null) command.Execute(null);
        }

        public Command<TouchEventArgs> MouseMoveCommand { get; set; }
        public void OnPreviewMouseMove(TouchEventArgs e)
        {
            ChangeTracker.MouseClickLastTime = DateTime.Now;
        }



        public Command<KeyEventArgs> KeydownCommand { get; set; }
        public void OnKeyDown(KeyEventArgs e)
        {
            var charinput = KeyInterop.VirtualKeyFromKey(e.Key);
            Log.Debug(((char)charinput).ToString());

            switch (e.Key)
            {
                case System.Windows.Input.Key.NumPad0:
                    StationRepository.AddTestMoNeyFromKeyboard(10);
                    break;
                case System.Windows.Input.Key.Enter:
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.NumPad2:
                    StationRepository.AddTestMoNeyFromKeyboard(0.10M);
                    break;
                case System.Windows.Input.Key.NumPad3:
                    StationRepository.AddTestMoNeyFromKeyboard(0.20M);
                    break;
                case System.Windows.Input.Key.NumPad4:
                    StationRepository.AddTestMoNeyFromKeyboard(0.50M);
                    break;
                case System.Windows.Input.Key.NumPad1:
                case System.Windows.Input.Key.Up:
                    StationRepository.AddTestMoNeyFromKeyboard(1);
                    break;
                case System.Windows.Input.Key.NumPad5:
                    StationRepository.AddTestMoNeyFromKeyboard(5);
                    break;

            }


            //if ((StationRepository.BarcodeScannerAlwaysActive && !System.Diagnostics.Debugger.IsAttached) || StationRepository.BarcodeScannerTempActive)

            if ((StationRepository.BarcodeScannerAlwaysActive) || StationRepository.BarcodeScannerTempActive || StationRepository.BarcodeScannerTestMode)
            {
                BarCodeConverter.ProcessBarcode((char)charinput);
                if (!Debugger.IsAttached)
                    e.Handled = true;
                HandleBarcode();
            }

        }



        public void HandleBarcode()
        {
            if (BarCodeConverter.IsComplete())
            {
                ErrorWindowService.Close();
                Log.Debug("BARCODE: got barcode " + BarCodeConverter.TicketNumber + BarCodeConverter.CheckSum);
                if (StationRepository.BarcodeScannerTestMode)
                {
                    Mediator.SendMessage(true, MsgTag.BarcodeScannerTest);
                    return;
                }
                switch (BarCodeConverter.Type)
                {

                    case BarCodeConverter.BarcodeType.REGISTRATION_NOTE:
                        {
                            if (!(StationRepository.BarcodeScannerAlwaysActive || ChangeTracker.OperatorSearchUserViewOpen))
                            {
                                break;
                            }
                            string registration_note = BarCodeConverter.TicketNumber + BarCodeConverter.CheckSum;
                            bool is_account_verified = false;
                            valueField[] acc = null;

                            try
                            {
                                acc = WsdlRepository.GetAccountByRegistrationNote(registration_note, StationRepository.FranchisorID);
                                if (acc != null)
                                {
                                    foreach (var registrationField in acc)
                                    {
                                        if (registrationField.name == "verified")
                                        {
                                            if (registrationField.value == "1")
                                            {
                                                is_account_verified = true;
                                            }
                                            break;
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_ACCOUNT_NOT_FOUND).ToString(), null, false, 5);
                                break;
                            }
                            if (is_account_verified)
                            {
                                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_ACCOUNT_VERIFIED).ToString(), null, false, 5);
                                break;
                            }
                            if (ChangeTracker.CurrentUser is AnonymousUser)
                            {
                                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_ACCOUNT_UNVERIFIED).ToString(), null, false, 5);
                            }
                            else if (ChangeTracker.CurrentUser is OperatorUser)
                            {
                                if (!ChangeTracker.CurrentUser.AuthenticateUser)
                                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_OPERATOR_NO_RIGHT_TO_SEARCH).ToString(), null, false, 5);
                                else if (acc != null)
                                {
                                    ChangeTracker.UserChecked = true;
                                    ChangeTracker.SearchUsersChecked = true;
                                    Mediator.SendMessage(acc, MsgTag.OpenSearchUserView);
                                }
                            }
                            else
                            {
                                //ShowError ???
                            }
                            break;
                        }
                    case BarCodeConverter.BarcodeType.TICKET:
                    case BarCodeConverter.BarcodeType.STORED_TICKET:
                        if (BarCodeConverter.TicketNumber == "0000000000000")
                        {
                            DisplayHelper.RotateScreen(true);
                            break;
                        }
                        if (ChangeTracker.CurrentUser is OperatorUser)
                        {
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CANT_SCAN_TICKET_UNDER_OPERATOR_ACCOUNT).ToString(), null, false, 10);
                            break;
                        }

                        Mediator.SendMessage("", MsgTag.HideOtherWindows);


                        Mediator.SendMessage(new Tuple<string, string, BarCodeConverter.BarcodeType>(BarCodeConverter.TicketNumber, BarCodeConverter.CheckSum, (BarCodeConverter.BarcodeType)BarCodeConverter.Type), MsgTag.LoadTicket);
                        break;
                    case BarCodeConverter.BarcodeType.CREDIT_NOTE:
                        if (!(ChangeTracker.CurrentUser is OperatorUser))
                        {
                            Mediator.SendMessage(new Tuple<string, string>(null, null), MsgTag.AddMoneyFromCreditNote);
                        }
                        else
                        {
                            if (ChangeTracker.CurrentUser.PayoutPaymentNote)
                            {
                                if (!ChangeTracker.OperatorPaymentViewOpen)
                                {
                                    Log.Debug("BARCODE: send message OpenOperatorPaymentView for CREDIT_NOTE. " + BarCodeConverter.TicketNumber + BarCodeConverter.CheckSum);
                                    Mediator.SendMessage(new Tuple<BarCodeConverter.BarcodeType, string>(BarCodeConverter.BarcodeType.CREDIT_NOTE, BarCodeConverter.TicketNumber + BarCodeConverter.CheckSum), MsgTag.OpenOperatorPaymentView);
                                }
                                else
                                {
                                    Mediator.SendMessage(true, MsgTag.SetCreditNoteButton);
                                    Mediator.SendMessage(BarCodeConverter.TicketNumber + BarCodeConverter.CheckSum, MsgTag.LoadPaymentNote);

                                }
                            }
                            else
                            {
                                ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_SHOULD_BE_CHECKED_IN_SHOP).ToString());
                            }
                        }
                        break;
                    case BarCodeConverter.BarcodeType.PAYMENT_NOTE:
                        if (!(ChangeTracker.CurrentUser is OperatorUser))
                        {
                            ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_SHOULD_BE_CHECKED_IN_SHOP).ToString());
                            break;
                        }
                        if (!ChangeTracker.CurrentUser.PayoutPaymentNote)
                        {
                            ShowError(TranslationProvider.Translate(MultistringTags.PAYMENT_NOTE_SHOULD_BE_CHECKED_IN_SHOP).ToString());
                            break;
                        }

                        if (!ChangeTracker.OperatorPaymentViewOpen)
                        {
                            Log.Debug("BARCODE: send message OpenOperatorPaymentView for PAYMENT_NOTE. " + BarCodeConverter.TicketNumber + BarCodeConverter.CheckSum);
                            Mediator.SendMessage(new Tuple<BarCodeConverter.BarcodeType, string>(BarCodeConverter.BarcodeType.PAYMENT_NOTE, BarCodeConverter.PaymentNoteNumber), MsgTag.OpenOperatorPaymentView);
                        }
                        else
                        {
                            Mediator.SendMessage(false, MsgTag.SetCreditNoteButton);
                            Mediator.SendMessage(BarCodeConverter.PaymentNoteNumber, MsgTag.LoadPaymentNote);
                        }
                        break;
                    case BarCodeConverter.BarcodeType.TAXNUMBER:
                        string number = BarCodeConverter.TaxNumber;
                        Mediator.SendMessage(number, MsgTag.FillTaxNumber);
                        break;
                    case BarCodeConverter.BarcodeType.CARDBARCODE:
                        if (StationRepository.IsIdCardEnabled)
                            break;
                        Mediator.SendMessage(true, MsgTag.ClosePinWindow);
                        string barcodenumber = BarCodeConverter.CardBarcode;
                        Mediator.SendMessage(barcodenumber, MsgTag.CardInserted);
                        break;
                }
                BarCodeConverter.Clear();
            }
        }

        public virtual ScrollViewer GetScrollviewer()
        {

            ScrollViewer scrollViewer = null;
            if (Application.Current != null)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    if (ChangeTracker.IsForecastOpen)
                        scrollViewer = AppVisualTree.FindChild<ScrollViewer>(mainWindow, "ScrollViewerVirtual");
                    else if (ChangeTracker.IsBetdomainViewOpen)
                        scrollViewer = AppVisualTree.FindChild<ScrollViewer>(mainWindow, "ScrollViewerBetDomainView");
                    else
                        scrollViewer = AppVisualTree.FindChild<ScrollViewer>(mainWindow, "ScrollViewer");
                }
            }
            return scrollViewer;
        }

        public virtual ScrollViewer GetScrollviewerForActiveWindow()
        {

            ScrollViewer scrollViewer = null;
            if (Application.Current != null)
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() =>
                        {
                            var activeWindow = GetActiveWindow();
                            if (activeWindow != null)
                            {
                                scrollViewer =
                                    AppVisualTree.FindChild<ScrollViewer>(activeWindow, "ScrollViewer");
                            }

                        });
            }
            return scrollViewer;
        }

        public virtual ScrollViewer GetScrollviewerForActiveWindowByName(string xname)
        {

            ScrollViewer scrollViewer = null;
            if (Application.Current != null)
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() =>
                    {
                        var activeWindow = GetActiveWindow();
                        if (activeWindow != null)
                        {
                            scrollViewer =
                                AppVisualTree.FindChild<ScrollViewer>(activeWindow, xname);
                        }

                    });
            }
            return scrollViewer;
        }

        public DependencyObject GetActiveWindow()
        {
            if (this.ViewWindow != null)
                return this.ViewWindow;
            if (View != null)
                return Window.GetWindow(View);
            return null;
        }

        public void ScrollToTop()
        {
            ScrollViewer scrollViewer = GetScrollviewer();

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(0);
            }
        }

        public void ScrollToVertivalOffset(int offset)
        {
            ScrollViewer scrollViewer = GetScrollviewerForActiveWindow();

            if (scrollViewer != null)
            {
                Dispatcher.Invoke(() =>
                    {
                        scrollViewer.ScrollToVerticalOffset(offset);
                    });
            }
        }

        public void ScrollToVertivalOffsetByName(int offset, string xname)
        {
            ScrollViewer scrollViewer = GetScrollviewerForActiveWindowByName(xname);

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(offset);
            }
        }

        public void ShowMessage(string obj)
        {
            QuestionWindowService.ShowMessage(obj);
        }


        public static void MaximizeWindow(Window window, Screen screen)
        {
            if (!window.IsLoaded)
                window.WindowStartupLocation = WindowStartupLocation.Manual;

            var workingArea = screen.WorkingArea;
            window.Left = workingArea.Left;
            window.Top = workingArea.Top;
            if ((ConfigurationManager.AppSettings["HIDE_MAIN_WINDOW_BORDER"] ?? String.Empty).Trim().ToLowerInvariant() == "true")
            {
                window.WindowStyle = WindowStyle.None;
            }


        }

        public static void MaximizeWindow(Window window)
        {
            window.Left = 0;
            window.Top = 0;
            var screens = Screen.AllScreens.Where(s => s.Primary).FirstOrDefault();
            window.Width = screens.WorkingArea.Right;
            window.Height = screens.WorkingArea.Bottom;
        }

        public void ShowError(string obj, EventHandler okClick = null, bool bCreateButtonEvent = false, int iAddCounterSeconds = 0)
        {
            ErrorWindowService.ShowError(obj, okClick, bCreateButtonEvent, iAddCounterSeconds);
        }


        public void Blur()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (System.Windows.Application.Current != null)
                {
                    foreach (var window in System.Windows.Application.Current.Windows)
                    {
                        if (window.GetType().ToString().Contains("Live") || window.GetType().ToString().Contains("BasketWindow"))
                            continue;
                        EffectsHelper.Blur(((Window)window));
                    }
                }
            }));

        }
        public void UnBlur(string str)
        {
            UnBlur();
        }

        [WsdlServiceSyncAspectSilent]
        public void GetUserPinSettingFromProfile()
        {
            profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(ChangeTracker.CurrentUser));

            string sPinSeting = "1"; // set PIN ENABLED as default

            if (form != null)
            {
                foreach (var formField in form.fields)
                {
                    if (formField.name == "card_pin_enabled")
                    {
                        ChangeTracker.UserPinSetting = int.Parse(formField.value == "" ? sPinSeting : formField.value);
                        break;
                    }
                }
            }
        }


        public void OpenAnonymousSession(bool showNoConnection, User previousUser)
        {
            if (showNoConnection)
            {
                ChangeTracker.CurrentUser = new EmptyUser();
            }
            else
            {
                decimal reserved = 0;
                decimal factor;

                ChangeTracker.CurrentUser = new AnonymousUser("", 1);
                ChangeTracker.CurrentUser.Currency = StationRepository.Currency;
                ChangeTracker.NewTermsAccepted = true;
            }
            ChangeTracker.CardNumber = "";
            if (previousUser is LoggedInUser)
            {
                var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
                foreach (var ticket1 in TicketsInBasket)
                {
                    Ticket ticket2 = ticket1;
                    Dispatcher.Invoke(() => { TicketHandler.TicketsInBasket.Remove(ticket2); });
                }

                LineSr.VerifySelectedOdds(new SortableObservableCollection<ITipItemVw>());

                TicketHandler.UpdateTicket();
                ChangeTracker.BetSelected = false;
            }


            if (TranslationProvider.CurrentLanguage != TranslationProvider.DefaultLanguage)
            {
                TranslationProvider.CurrentLanguage = TranslationProvider.DefaultLanguage;

                TranslationProvider.CurrentLanguage = TranslationProvider.DefaultLanguage;
                Mediator.SendMessage(TranslationProvider.DefaultLanguage, MsgTag.LanguageChosenHeader);
                Mediator.SendMessage(TranslationProvider.DefaultLanguage, MsgTag.LanguageChosen);
            }

            ChangeTracker.LockTournamentAgainstAll = false;
            if (previousUser is OperatorUser)
            {
                var viewModel = MyRegionManager.NavigatBack(RegionNames.ContentRegion);
                if (viewModel == null)
                {
                    Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
                }
            }
            else
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);

            //SimpleWorkflow.ClearHistory();
            //SimpleWorkflow.ActivateOnlyFirstView();


            while (ChangeTracker.CurrentUser == null || string.IsNullOrEmpty(ChangeTracker.CurrentUser.SessionId))
            {
                try
                {
                    SessionWS sessid = WsdlRepository.OpenSession(StationRepository.StationNumber, true, string.Empty,
                        string.Empty, false);
                    ChangeTracker.CurrentUser = new AnonymousUser(sessid.session_id, sessid.account_id);
                    ChangeTracker.CurrentUser.Currency = StationRepository.Currency;
                    ChangeTracker.CurrentUser.RoleID = GetRoleId(sessid.role_id);
                    ChangeTracker.NewTermsAccepted = true;
                }
                catch (Exception ex)
                {
                    ChangeTracker.CurrentUser = new EmptyUser();
                    Log.Error("Error while trying to open anonymous session:" + ex.Message, ex);
                    Thread.Sleep(1000);
                }
            }
        }
        public string InvalidSessionID
        {
            get { return "000"; }
        }

        public void ClearAndOpenAnonymousSession()
        {
            Mediator.SendMessage<string>("", MsgTag.HideNotification);

            var previousUser = ChangeTracker.CurrentUser;
            ClearEverythingAfterUser();
            OpenAnonymousSession(false, previousUser);
        }

        public void ClearEverythingAfterUser(bool showNoConnection = false)
        {
            if (ChangeTracker.RestartPending == 2)
            {
                Mediator.SendMessage<long>(0, MsgTag.RestartStation);
                return;
            }
            if (ChangeTracker.RestartPending == 4)
            {
                Mediator.SendMessage<long>(0, MsgTag.RestartApplication);
                return;
            }

            ChangeTracker.CalendarStartDateAccounting = ChangeTracker.CalendarEndDateAccounting;

            ChangeTracker.EditableUser = null;
            ChangeTracker.CalendarStartDateAccounting = DateTime.MinValue;
            ChangeTracker.CalendarEndDateAccounting = DateTime.MinValue;
            ChangeTracker.StartDateAccounting = DateTime.MinValue;
            ChangeTracker.EndDateAccounting = DateTime.MinValue;
            ChangeTracker.FromCheckPointsAccounting = false;
            ChangeTracker.CashInAccounting = true;
            ChangeTracker.CashOutAccounting = true;

        }

        public void UnBlur()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (Application.Current != null)
                {
                    foreach (var window in Application.Current.Windows)
                    {
                        EffectsHelper.Unblur(((Window)window));
                    }
                }
            }));
        }

        public User.roleId GetRoleId(int n)
        {
            switch (n)
            {
                case 1:
                    return User.roleId.Administrator;
                case 2:
                    return User.roleId.Franchisorowner;
                case 3:
                    return User.roleId.Locationowner;
                case 4:
                    return User.roleId.Operator;
                case 5:
                    return User.roleId.Upfranchisor;
            }
            return User.roleId.Administrator;
        }

        public void ChangeSystem(int obj)
        {
            var ticketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
            if (ticketsInBasket.Count == 1)
            {
                DataBinding.ChangeSystemX(obj, TicketHandler.TicketState, ticketsInBasket[0]);
            }
        }

        public void DeleteTipItem(ITipItemVw tip)
        {
            var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
            ChangeSystem(-1);
            if (ChangeTracker.BasketWheelPosition > 2)
                ChangeTracker.BasketWheelPosition--;
            foreach (var ticket1 in TicketsInBasket)
            {
                if (ticket1.TipItems.Contains(tip))
                {
                    Ticket ticket2 = ticket1;
                    Dispatcher.Invoke(() =>
                        { ticket2.TipItems.Remove(tip); });
                    break;
                }
            }
            foreach (var ticket1 in TicketsInBasket)
            {
                if (ticket1.TipItems.Count == 0)
                {
                    Ticket ticket2 = ticket1;
                    Dispatcher.Invoke(() =>
                        {
                            TicketHandler.TicketsInBasket.Remove(ticket2);
                        });
                }
            }

            if (TicketHandler.TicketState == TicketStates.System)
            {
                var checkedways = TicketsInBasket[0].TipItems.ToSyncList().Where(x => x.Match.MatchId == tip.Match.MatchId).Where(x => x.IsChecked).ToList();
                if (checkedways.Count == 1)
                {
                    foreach (var checkedway in checkedways)
                    {
                        checkedway.IsBank = false;
                    }
                }
            }


            TicketHandler.UpdateTicket();

            LineSr.VerifySelectedOdds(new SortableObservableCollection<ITipItemVw>(TicketsInBasket.SelectMany(x => x.TipItems.ToSyncList()).ToList()));



        }

        public void ScrollChanged(double contentVerticalOffset)
        {
            if (ScrollerPreviousPosition == contentVerticalOffset)
            {
                return;
            }

            SBVisibility = Visibility.Visible;
            ScrollerPreviousPosition = contentVerticalOffset;
            SBTimer.Stop();
            SBTimer.Start();

        }

        private Visibility _sbVisibility = Visibility.Collapsed;
        public Visibility SBVisibility
        {
            get { return _sbVisibility; }
            set
            {
                _sbVisibility = value;
                OnPropertyChanged("SBVisibility");
            }
        }

        private double ScrollerPreviousPosition = 0.0;

        public System.Timers.Timer SBTimer = new Timer();

        private void SBTimerElapsed(object sender, ElapsedEventArgs e)
        {
            SBTimer.Stop();
            SBVisibility = Visibility.Collapsed;
        }

        public void OnBet(IOddVw chosenOdd)
        {
            if (ChangeTracker.CurrentUser == null)
            {
                chosenOdd.DoPropertyChanged("IsSelected");
                return;
            }
            if (chosenOdd == null)
                return;
            TipItemVw tiv = new TipItemVw(chosenOdd.LineObject);

            PlaceBetMethod(tiv);
        }
        ITipItemVw savedOdd;
        public ITipItemVw PlaceBetMethod(ITipItemVw chosenOdd, bool isDeletingDisabledOdd = false)
        {
            if (StationRepository.Active == 0 && chosenOdd.Odd != null && chosenOdd.Odd.OddView != null && !chosenOdd.Odd.OddView.IsSelected && !isDeletingDisabledOdd)
            {
                Mediator.SendMessage(new Tuple<string, string, bool, int>(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN), "", false, 3), MsgTag.Error);
                chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                return chosenOdd;
            }

            var ticketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();

            if (ticketsInBasket.Count > 0)
            {
                //check match descriptor - we use it for blocking motorsport and wintersports
                string descriptor = "";
                if (chosenOdd.Odd.OddView != null && chosenOdd.Odd.OddView.BetDomainView != null && chosenOdd.Odd.OddView.BetDomainView.MatchView != null)
                    descriptor = chosenOdd.Odd.OddView.BetDomainView.MatchView.SportDescriptor;

                //block related competitors
                if (chosenOdd.Odd.OddView != null && !chosenOdd.Odd.OddView.IsSelected && (chosenOdd.Odd.BetDomain.Match.outright_type == eOutrightType.Outright || descriptor == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS || descriptor == SportSr.SPORT_DESCRIPTOR_MOTOSPORT))
                {
                    List<long> oddIds = new List<long>();
                    foreach (Ticket tic in ticketsInBasket)
                    {
                        foreach (ITipItemVw tip in tic.TipItems)
                        {
                            if (tip.Odd.BetDomain.Match.MatchId == chosenOdd.Odd.BetDomain.Match.MatchId)
                                continue;

                            if (tip.Odd.BetDomain.Match.outright_type == eOutrightType.Outright)
                            {
                                if (tip.Odd.OddView.OutrightsCompetitors != null)
                                    foreach (CompetitorLn competitor in tip.Odd.OddView.OutrightsCompetitors)
                                        oddIds.Add(competitor.CompetitorId);
                            }
                            else
                            {
                                oddIds.Add(tip.Odd.BetDomain.Match.HomeCompetitor.CompetitorId);
                                oddIds.Add(tip.Odd.BetDomain.Match.AwayCompetitor.CompetitorId);
                            }
                        }
                    }

                    bool foundSameCompetitor = false;
                    if (chosenOdd.Odd.BetDomain.Match.outright_type == eOutrightType.Outright)
                    {
                        if (chosenOdd.Odd.OddView.OutrightsCompetitors != null)
                            foreach (CompetitorLn competitor in chosenOdd.Odd.OddView.OutrightsCompetitors)
                            {
                                if (oddIds.Contains(competitor.CompetitorId))
                                    foundSameCompetitor = true;
                            }
                    }
                    else
                    {
                        if (oddIds.Contains(chosenOdd.Odd.BetDomain.Match.HomeCompetitor.CompetitorId) || oddIds.Contains(chosenOdd.Odd.BetDomain.Match.AwayCompetitor.CompetitorId))
                            foundSameCompetitor = true;
                    }

                    if (foundSameCompetitor)
                    {
                        chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_RELATED_BLOCKED), null, false, 5);
                        return chosenOdd;
                    }
                }

                if (chosenOdd.Odd.OddView != null && ticketsInBasket[0].TipItems[0].Match != null)
                {
                    SportRadar.DAL.OldLineObjects.eServerSourceType oddType =
                        chosenOdd.Odd.OddView.BetDomainView.MatchView.LineObject.SourceType;
                    SportRadar.DAL.OldLineObjects.eServerSourceType existingOddType =
                        ticketsInBasket[0].TipItems[0].Match.MatchView.LineObject.SourceType;

                    if ((existingOddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl || existingOddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc) &&
                        existingOddType != oddType &&
                        (oddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl || oddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc))
                    {
                        chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                        string text = TranslationProvider.Translate(MultistringTags.TERMINAL_MIXED_STAKES_FORBIDDEN) + " " +
                            TranslationProvider.Translate(MultistringTags.TERMINAL_REPLACE_VIRTUAL_BETS);

                        QuestionWindowService.ShowMessage(text, null, null, ReplaceVirtualBets, null);

                        //clear all odds
                        savedOdd = chosenOdd;
                        return chosenOdd;
                    }

                    if (oddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl ||
                        existingOddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                    {
                        if (oddType != existingOddType)
                        {
                            chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_MIXED_STAKES_FORBIDDEN), null, false, 5);
                            return chosenOdd;
                        }
                    }

                    if (oddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc || existingOddType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                    {
                        if (oddType != existingOddType)
                        {
                            chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_MIXED_STAKES_FORBIDDEN), null, false, 5);
                            return chosenOdd;
                        }
                    }
                }
            }

            if (ticketsInBasket.Count >= 0 && !StationRepository.AllowMixedStakes && StationRepository.IsPrematchEnabled && !chosenOdd.IsSelected && TicketHandler.Count > 0)
            {

                if (chosenOdd.Odd.OddView.BetDomainView.MatchView.IsLiveBet != ticketsInBasket.Select(x => x.TipItems.ToSyncList().Select(c => c.Odd.BetDomain.Match.IsLiveBet.Value).FirstOrDefault()).FirstOrDefault())
                {
                    chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_MIXED_STAKES_FORBIDDEN), null, false, 5);
                    return chosenOdd;
                }
            }

            if (!CheckTicketForExceptions(chosenOdd) && !ticketsInBasket.Any(x => x.TipItems.Contains(chosenOdd)))
            {
                chosenOdd.Odd.OddView.DoPropertyChanged("IsSelected");
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TOO_MANY_ODDS), null, false, 5);
                return chosenOdd;
            }


            var foundTipItem = ticketsInBasket.SelectMany(x => x.TipItems.ToSyncList()).FirstOrDefault(c => c.Odd.OutcomeId == chosenOdd.Odd.OutcomeId);
            if (foundTipItem != null)
            {
                ChangeTracker.BetSelected = false;
                DeleteTipItem(foundTipItem);

                ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
            }
            else
            {
                ChangeTracker.BetSelected = false;
                ChangeTracker.BetSelected = true;
                if (TicketHandler.TicketState != TicketStates.Single || ticketsInBasket.Count == 1)
                {
                    ticketsInBasket[0].TipItems.Add(chosenOdd);
                    //TicketHandler.TicketState = TicketStates.Multy;
                }
                else
                {
                    var newTicket = new Ticket();
                    newTicket.TipItems.Add(chosenOdd);
                    TicketHandler.TicketsInBasket.Add(newTicket);


                }
                //chosenOdd.IsBank = AddBank(chosenOdd.Odd);
                chosenOdd.IsBankReadOnly = chosenOdd.IsBank;


                TicketHandler.UpdateTicket();

                LineSr.VerifySelectedOdds(new SortableObservableCollection<ITipItemVw>(TicketHandler.TicketsInBasket.ToSyncList().SelectMany(x => x.TipItems.ToSyncList()).ToList()));
            }
            if (ChangeTracker.CurrentUser is AnonymousUser || ChangeTracker.CurrentUser is LoggedInUser)
                IoCContainer.Kernel.Get<IMediator>().SendMessage(true, MsgTag.BasketRebindWheel);

            return chosenOdd;
        }

        public bool AddBank(IOddLn curOdd)
        {
            try
            {
                int count = 0;
                var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();

                foreach (var curItem in TicketsInBasket[0].TipItems.ToSyncList().Where(x => x.IsChecked))
                {
                    if (curItem.Odd.BetDomain.BetDomainId == curOdd.BetDomain.BetDomainId)
                    {
                        count++;
                        curItem.IsBank = true;
                        curItem.IsBankReadOnly = true;
                    }
                }
                return count > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        public UserControl View
        {
            get { return _view; }
            set
            {
                _view = value;
                View.Loaded += View_Loaded;
            }
        }
        public bool HidePleaseWait
        {
            get { return _hidePleaseWait; }
            set { _hidePleaseWait = value; }
        }
        public bool IsReady { get; set; }





        public void View_Loaded(object sender, RoutedEventArgs e)
        {
            OnNavigationCompleted();
        }
        public virtual void OnNavigationCompleted()
        {
            ScrollToVertivalOffset(0);
            IsClosed = false;
            if (HidePleaseWait)
            {
                WaitOverlayProvider.DisposeAll();
                Log.DebugFormat("hide Please wait:{0}", this.ToString());

            }
            Log.DebugFormat("navigated:{0}", this.ToString());

            //Mediator.ApplyRegistration();
            IsReady = true;
            if (View != null)
                View.Loaded -= View_Loaded;
            if (ViewWindow != null)
            {
                _viewWindow.Loaded -= View_Loaded;

            }
        }




        public Window ViewWindow
        {
            get { return _viewWindow; }
            set
            {
                _viewWindow = value;
                _viewWindow.Loaded += View_Loaded;
                _viewWindow.Closed += _viewWindow_Closed;
            }
        }

        void _viewWindow_Closed(object sender, EventArgs e)
        {
            if (ViewWindow != null)
            {
                _viewWindow.Closed -= _viewWindow_Closed;
            }
            Close();
        }


        public virtual void Close()
        {
            Mediator.UnregisterRecipientAndIgnoreTags(this);
            IsClosed = true;
            if (Dispatcher != null)
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (ViewWindow != null && ViewWindow.DataContext != null)
                    {
                        ViewWindow.DataContext = null;
                        ViewWindow.Close();
                    }
                }));

        }



        [WsdlServiceSyncAspect]
        public void LoadTicket(Tuple<string, string> number)
        {
            string ticketNumber = number.Item1 ?? BarCodeConverter.TicketNumber;
            string ticketCheckSum = number.Item2 ?? BarCodeConverter.CheckSum;
            BarCodeConverter.Clear();

            long ticketUserId;
            string result = "1";
            try
            {
                result = WsdlRepository.GetAccountByTicket(ticketNumber);
            }
            catch (Exception)
            {
            }
            long.TryParse(result, out ticketUserId);
            if (ticketUserId == ChangeTracker.CurrentUser.AccountId || ticketUserId == 1)
            {
                try
                {
                    var ticketresult = WsdlRepository.LoadTicket(ticketNumber, ticketCheckSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, false);
                    if (ticketresult.paid)
                        ShowError(TranslationProvider.Translate(MultistringTags.CANT_DEPOSIT_MONEY_FROM_PAID_TICKET).ToString());
                    else if (!ticketresult.won)
                        ShowError(TranslationProvider.Translate(MultistringTags.CANT_DEPOSIT_MONEY_FROM_LOST_TICKET).ToString());
                    else if (ticketresult.wonExpireTime < DateTime.Now && ticketresult.wonExpireTime > DataCopy.MIN_ALLOWED_DATE)
                        ShowError(TranslationProvider.Translate(MultistringTags.EXPIRED_TICKET).ToString());
                    else
                        UseTicket(ticketresult);

                }
                catch (FaultException<HubServiceException> exception)
                {
                    switch (exception.Detail.code)
                    {
                        case 220:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_TICKET_NOT_FOUND).ToString());
                            break;
                        case 1791:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_INVALIDFRANCHISOR).ToString());
                            break;
                        default:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_INVALIDLOCATION).ToString());
                            break;

                    }
                }

            }
            else
            {
                ShowError(TranslationProvider.Translate(MultistringTags.THIS_TICKET_DOES_NOT_BELONG_TO_YOU).ToString());
            }
        }
        private TicketWS ticket;

        public void UseTicket(TicketWS obj)
        {

            ticket = obj;
            var text = TranslationProvider.Translate(MultistringTags.TICKET_HAVE) + ": " + ticket.wonAmount + " " + Currency + "\r\n";
            if (ChangeTracker.CurrentUser is AnonymousUser)
            {
                text += TranslationProvider.Translate(MultistringTags.TICKET_TO_CASHPOOL) as string;

            }
            else
            {
                text += TranslationProvider.Translate(MultistringTags.TICKET_TO_ACCOUNT) as string;
            }

            QuestionWindowService.ShowMessage(text, null, null, ticketquestionViewModel_YesClick, null);


        }

        private void ReplaceVirtualBets(object sender, EventArgs e)
        {
            var ticketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
            foreach (var ticket1 in ticketsInBasket)
            {
                Ticket ticket2 = ticket1;
                Dispatcher.Invoke(() =>
                    { TicketHandler.TicketsInBasket.Remove(ticket2); });
            }

            if (savedOdd != null)
                PlaceBetMethod(savedOdd);

        }

        [WsdlServiceSyncAspect]
        private void ticketquestionViewModel_YesClick(object sender, EventArgs e)
        {

            if ((ChangeTracker.CurrentUser is AnonymousUser && StationRepository.AllowAnonymousBetting) || ChangeTracker.CurrentUser is LoggedInUser)
            {
                string error = "";
                var result = TransactionQueueHelper.TryDepositByTicketMoneyOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(ChangeTracker.CurrentUser), ticket.ticketNbr, ticket.checkSum, null, null, ref error);
                if (result)
                    ChangeTracker.CurrentUser.Addmoney(ticket.wonAmount);
                else
                {
                    ShowError(error);
                }
                Log.Error(error, new Exception(error));

            }
            else
            {
                ShowError(TranslationProvider.Translate(MultistringTags.ANONYMOUS_USER_CANT_GET_MONEY_FROM_TICKET).ToString());
            }
            var reloadedTicket = WsdlRepository.LoadTicket(ticket.ticketNbr, ticket.checkSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);
            Mediator.SendMessage<TicketWS>(reloadedTicket, MsgTag.ReloadTicket);
        }


        [PleaseWaitAspect]
        public void LoadCreditNote(Tuple<string, string> obj)
        {
            if (!StationRepository.AllowAnonymousBetting)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.ANONYMOUS_BETTING_IS_DISABLED) as string);
                return;
            }
            CreditNoteWS result = null;
            try
            {
                var number = BarCodeConverter.TicketNumber;
                var checksum = BarCodeConverter.CheckSum;
                if (obj.Item1 != null)
                {
                    number = obj.Item1;
                    checksum = obj.Item2;
                }
                result = WsdlRepository.LoadCreditNote(number, checksum, StationRepository.StationNumber);
            }
            catch (FaultException<HubServiceException> ex)
            {
                switch (ex.Detail.code)
                {
                    case 401:
                        ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_ALREADY_EXISTS).ToString());
                        break;
                    case 402:
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_NOT_ACTIVE).ToString());
                        break;
                    case 403:
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CREDIT_NOTE_INVALID_AMOUNT).ToString());
                        break;
                    case 404:
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_PAID_CREDITNOTE).ToString());
                        break;
                    case 405:
                        ShowError(TranslationProvider.Translate(MultistringTags.CREDIT_NOTE_EXPIRED).ToString());
                        break;
                    case 179:
                        ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDLOCATION).ToString());
                        break;
                    case 1791:
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NOTE_INVALIDFRANCHISOR).ToString());
                        break;
                    default:
                        ShowError(ex.Detail.message);

                        break;
                }
                return;
            }
            catch (Exception eexc)
            {
                Log.Error("", eexc);
            }
            if (result == null)
                ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_CREDIT_NOTE_NOT_FOUND).ToString());
            else if (result.status != 1)
                ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_PAID_CREDITNOTE).ToString());
            else if (result.expireDate < DateTime.Now && result.expireDate > DataCopy.MIN_ALLOWED_DATE)// && !StationRepository.PayoutExpiredPaymentCreditNotes)
                ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.EXPIRED_CREDITNOTE).ToString());
            else
            {
                UseCreditNote(result);
            }
        }
        private CreditNoteWS creditnote;

        private UserControl _view;
        public Window _viewWindow;
        private bool _hidePleaseWait = true;


        public void UseCreditNote(CreditNoteWS creditnotews)
        {
            if (StationRepository.Active == 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString());
                return;
            }

            creditnote = creditnotews;
            var text = TranslationProvider.Translate(MultistringTags.CREDITNOTE_HAVE, creditnote.amount, Currency);
            var yesButtonText = TranslationProvider.Translate(MultistringTags.TRANSFER_TO_TERMINAL) as string;
            var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, questionViewModel_YesClick, null);

        }

        [WsdlServiceSyncAspect]
        private void questionViewModel_YesClick(object sender, System.EventArgs e)
        {
            if (ChangeTracker.CurrentUser is AnonymousUser && StationRepository.AllowAnonymousBetting)
            {
                if (GetMoneyFromCreditNote(creditnote))
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE) as string);

            }
            else if (ChangeTracker.CurrentUser is LoggedInUser)
            {
                if (GetMoneyFromCreditNote(creditnote))
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE) as string);
            }
            else
            {
                ShowError(TranslationProvider.Translate(MultistringTags.ANONYMOUS_USER_CANT_USE_CREDIT_NOTE).ToString());
            }
        }

        public bool GetMoneyFromCreditNote(CreditNoteWS creditnote)
        {
            string error = "";
            var result = TransactionQueueHelper.TryDepositByCreditNoteMoneyOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(ChangeTracker.CurrentUser), creditnote.number, creditnote.code, ref error);
            if (result)
            {
                ChangeTracker.CurrentUser.Refresh();
                decimal reserved = 0;
                decimal factor;
                var balance = WsdlRepository.GetBalance(StationRepository.GetUid(ChangeTracker.CurrentUser), out reserved, out factor);// -reserved;
                AddMoneyToTerminal(creditnote.amount, error, new accountBalance { amount = balance, reserved = reserved });
                return true;
            }
            ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR));
            Log.Error(error, new Exception(error));
            return false;
        }


        public void SaveCreditNote(decimal amount)
        {
            var number = BusinessPropsHelper.GenerateNextCreditNoteNumber();
            var checkSum = new PasswordGenerator().Generate(4, 4, true);
            try
            {
                PayoutType payouttype;
                var result = WsdlRepository.SaveCreditNote(StationRepository.GetUid(ChangeTracker.CurrentUser), number, checkSum, amount, StationRepository.StationNumber, out payouttype);
                if (result)
                {

                    var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();

                    if (TicketsInBasket.Count != 0)
                    {
                        TicketsInBasket[0].Stake = 0;
                        TicketHandler.UpdateTicket();
                    }
                    ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;

                    var sucess = PrinterHandler.PrintCreditNote(amount, number, checkSum, false, DateTime.MinValue, DateTime.MinValue);
                    ChangeTracker.CurrentUser.Refresh();
                    if (!sucess)
                    {
                        GetMoneyFromCreditNote(new CreditNoteWS() { amount = amount, number = number, code = checkSum });
                        ShowError(TranslationProvider.Translate(MultistringTags.UNABLE_TO_PRINT_CREDITNOTE) + "\r\n" + TranslationProvider.Translate(MultistringTags.SHOP_FORM_CREDITNOTE) + ": " + number + " " + checkSum);
                    }
                }
            }
            catch (FaultException<HubServiceException> ex)
            {
                if (ex.Detail.code == 151)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.ANONYMOUS_USER_CANT_GET_MONEY_FROM_TICKET) as string);
                }
                else if (ex.Detail.code == 181)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_NO_STATION_DB));
                }
                else
                {
                    ShowError(ex.Detail.message);
                }
            }
        }

        public bool CheckTicketForExceptions(ITipItemVw sentOdd = null)
        {
            Ticket ticketToCalculate = TicketHandler.TicketsInBasket.ToSyncList().FirstOrDefault();
            if (ticketToCalculate == null)
                return true;
            var localTipItems = new List<ITipItemVw>();
            localTipItems = ticketToCalculate.TipItems.ToSyncList().Where(x => x.IsChecked).ToList();

            if (sentOdd != null)
                localTipItems.Add(sentOdd);

            decimal oddVal = 1;
            decimal multiWayOddVal = 1;

            Dictionary<long, List<ITipItemVw>> tipItemDict = new Dictionary<long, List<ITipItemVw>>();
            foreach (var t in localTipItems)
            {
                long iSvrMatchId = t.Match.MatchId;

                if (tipItemDict.ContainsKey(iSvrMatchId))
                {
                    tipItemDict[iSvrMatchId].Add(t);
                }
                else
                {
                    var list = new List<ITipItemVw>();
                    list.Add(t);
                    tipItemDict.Add(iSvrMatchId, list);
                }
            }

            double temp = 0;

            foreach (var list in tipItemDict.Values)
            {
                if (list.Count >= 1)
                {
                    decimal maxOdd = 1;
                    foreach (var tip in list)
                    {
                        if (maxOdd < tip.Value)
                        {
                            maxOdd = tip.Value;
                        }
                    }
                    var t = list[0];
                    if (t.IsBank || list.Count > 1)
                    {
                        temp = (double)multiWayOddVal * (double)maxOdd;
                        if (temp > (double)Decimal.MaxValue)
                            return false;

                        multiWayOddVal *= maxOdd;
                    }
                    else
                    {
                        temp = (double)oddVal * (double)maxOdd;
                        if (temp > (double)Decimal.MaxValue)
                            return false;

                        oddVal *= maxOdd;
                    }
                }
            }

            temp = (double)oddVal * (double)multiWayOddVal;
            if (temp > int.MaxValue)
                return false;

            if (temp * (double)multiWayOddVal > (double)Decimal.MaxValue)
                return false;

            return true;
        }

        public bool IsClosed { get; set; }

        protected void AddMoneyToTerminal(decimal moneyIn, String error, accountBalance balance)
        {
            var prevCashpoolValue = ChangeTracker.CurrentUser.Cashpool;
            ChangeTracker.MouseClickLastTime = DateTime.Now;
            StationRepository.DisableCashIn();


            if (!string.IsNullOrEmpty(error))
                Log.Error(error, new Exception(error));

            var anonymousUser = ChangeTracker.CurrentUser as AnonymousUser;
            var loggedinUser = ChangeTracker.CurrentUser as LoggedInUser;
            var amount = moneyIn;
            var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();

            if (anonymousUser != null && balance != null)
            {
                anonymousUser.Cashpool = balance.amount - balance.reserved;
                anonymousUser.AvailableCash = anonymousUser.Cashpool - TicketHandler.Stake;
                if (prevCashpoolValue < TicketHandler.Stake)
                {

                    amount = anonymousUser.Cashpool - TicketHandler.Stake;
                }
                if (TicketsInBasket.Count > 0 && ChangeTracker.IsBasketOpen && TicketsInBasket[0].MaxBet != 0 && TicketsInBasket.Count < 2 && amount > 0)
                {
                    TicketHandler.OnChangeStake(amount.ToString(), TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
                    if (TicketHandler.Stake >= TicketHandler.MinBet)
                    {
                        Mediator.SendMessage<MultistringTag>(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar);
                        Mediator.SendMessage<MultistringTag>(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN, MsgTag.HideNotificationBar);
                    }
                }
                else
                {
                    Mediator.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_CASHPOOL, new string[] { moneyIn.ToString() }), MsgTag.ShowNotificationBar);
                }

            }
            else if (loggedinUser != null)
            {
                if (!string.IsNullOrEmpty(error) || balance == null)
                {
                    PrinterHandler.InitPrinter(true);
                    if (StationRepository.PrinterStatus == 0)
                    {
                        Mediator.SendMessage<decimal>(moneyIn, MsgTag.PrinterNotReady);
                        return;
                    }
                    PrinterHandler.PrintDepositLostMessage(moneyIn, loggedinUser.Username);

                    return;
                }
                loggedinUser.Cashpool = balance.amount - balance.reserved;
                loggedinUser.AvailableCash = loggedinUser.Cashpool - TicketHandler.Stake;

                if (prevCashpoolValue < TicketHandler.Stake)
                {
                    amount = loggedinUser.Cashpool - TicketHandler.Stake;
                }
                if (TicketsInBasket.Count > 0 && ChangeTracker.IsBasketOpen && TicketsInBasket[0].MaxBet != 0 && TicketsInBasket.Count < 2 && amount > 0)
                {
                    TicketHandler.OnChangeStake(amount.ToString(), TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
                    if (TicketHandler.Stake >= TicketHandler.MinBet)
                    {
                        Mediator.SendMessage<MultistringTag>(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar);
                    }
                }
                else
                {
                    Mediator.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_AVAILABLECASH, new string[] { moneyIn.ToString() }), MsgTag.ShowNotificationBar);
                }

                Mediator.SendMessage<long>(0, MsgTag.UpdateHistory);
                Mediator.SendMessage<bool>(true, MsgTag.UpdateBalance);
            }

            var minLimit = ChangeTracker.CurrentUser.DailyLimit;
            if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
            if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.MonthlyLimit;

            StationRepository.SetCashInDefaultState(minLimit);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
