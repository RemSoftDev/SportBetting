using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.Win32;
using Nbt.Station.Design;
using Newtonsoft.Json;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;
using SportBetting.WPF.Prism.Modules.Services.Implementation;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using ViewModels;
using ViewModels.Helpers;
using ViewModels.ViewModels;
using WebLiveMonitorWindowApp.ViewModels;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;
using Application = System.Windows.Application;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MainWpfWindow.ViewModels
{
    /// <summary>
    /// MainWindow view model.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {

        private static ILog Log = LogFactory.CreateLog(typeof(MainViewModel));

        #region Variables

        private decimal cashpool = 0;


        ActiveMQClient stationPropertyService = new ActiveMQClient();

        #endregion

        #region Constructor & destructor
        public MainViewModel()
        {

            NDEServer.Register();

            Mediator.Register<Tuple<string, string, bool, int>>(this, ShowError, MsgTag.Error);
            Mediator.Register<long>(this, OnBindUserCard, MsgTag.BindUserCard);
            Mediator.Register<long>(this, OnBindOperatorCard, MsgTag.BindOperatorCard);
            Mediator.Register<Tuple<string, string>>(this, LoadCreditNote, MsgTag.AddMoneyFromCreditNote);
            Mediator.Register<Tuple<string, string>>(this, LoadTicket, MsgTag.AddMoneyFromTicket);
            Mediator.Register<string>(this, OnOpenStoredTicket, MsgTag.OpenStoredTicket);
            Mediator.Register<decimal>(this, AskAboutCashPool, MsgTag.AskAboutCashPool);
            Mediator.Register<string>(this, HideOtherWindows, MsgTag.HideOtherWindows);
            Mediator.Register<string>(this, OnShowTermsAndConditionsExecute, MsgTag.ShowTermsAndConditions);
            Mediator.Register<string>(this, OnShowCalibrationExecute, MsgTag.ShowCalibration);
            Mediator.Register<long>(this, HideLogineWindow, MsgTag.HideLogin);
            Mediator.Register<long>(this, HideUserProfileWindow, MsgTag.HideUserProfile);
            Mediator.Register<long>(this, ShowUserManagementWindow, MsgTag.ShowUserManagement);
            Mediator.Register<string>(this, CardInserted, MsgTag.CardInserted);
            Mediator.Register<long>(this, CardRemoved, MsgTag.CardRemoved);
            Mediator.Register<long>(this, IdCardError, MsgTag.IdCardError);
            Mediator.Register<string>(this, StartedCardReading, MsgTag.StartedCardReading);
            Mediator.Register<Tuple<string, string, BarCodeConverter.BarcodeType>>(this, LoadTicket, MsgTag.LoadTicket);
            Mediator.Register<string[]>(this, ShowMsg, MsgTag.ZippedLogsUploaded);
            Mediator.Register<decimal>(this, PrinterNotReady, MsgTag.PrinterNotReady);
            Mediator.Register<long>(this, RestartApplication, MsgTag.RestartApplication);
            Mediator.Register<string>(this, RestartInTestMode, MsgTag.RestartInTestMode);
            Mediator.Register<long>(this, RestartStation, MsgTag.RestartStation);
            Mediator.Register<long>(this, SessionClosed, MsgTag.SessionClosed);
            Mediator.Register<long>(this, UserBlocked, MsgTag.UserBlocked);
            Mediator.Register<bool>(this, PrinterErrorValue, MsgTag.PrinterErrorValue);
            Mediator.Register<int>(this, CheckRestart, MsgTag.CheckRestart);
            Mediator.Register<string>(this, ShowWarning, MsgTag.ShowWarning);
            Mediator.Register<char>(this, EmulateBarcode, MsgTag.EmulateBarcode);
            Mediator.Register<string>(this, UnBlur, MsgTag.Unblur);
            Mediator.Register<long>(this, ShowSystemMessage, MsgTag.ShowSystemMessage);
            Mediator.Register<string>(this, GetFocus, MsgTag.GetFocus);
            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);
            Mediator.Register<string>(this, SetUpBrowser, MsgTag.SetUpBrowser);
            Mediator.Register<string>(this, ChangeVisibility, MsgTag.ChangeVisibility);
            Mediator.Register<bool>(this, AcceptNewTermsVersion, MsgTag.AcceptNewTermsVersion);
            Mediator.Register<string>(this, ShowNotification, MsgTag.ShowNotification);
            Mediator.Register<string>(this, HideNotification, MsgTag.HideNotification);
            if (!StationRepository.IsIdCardEnabled)
            {
                Mediator.Register<string>(this, WriteCardNumber, MsgTag.WriteNumber);

            }
        }

        private void AcceptNewTermsVersion(bool isFinal)
        {
            if (StationRepository.Active == 0)
                return;

            //ShowError popup
            if (!ChangeTracker.NewTermsAccepted)
            {
                if (isFinal)
                {
                    QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMS_NEW_VERSION_CONFIRM).ToString(),
                        TranslationProvider.Translate(MultistringTags.TERMINAL_PRINT_YES),
                        TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_LOGOUT),
                        AcceptNewTerms, RejectNewTerms);
                }
                else
                    QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMS_NEW_VERSION).ToString(),
                        TranslationProvider.Translate(MultistringTags.TERMS_NEW_VERSION_AGREE),
                        TranslationProvider.Translate(MultistringTags.TERMS_NEW_VERSION_VIEW),
                        AcceptNewTerms, NavigateToTermsAndConditions);
            }
        }

        private void RejectNewTerms(object sender, EventArgs e)
        {
            ClearAndOpenAnonymousSession();
        }

        [WsdlServiceSyncAspect]
        private void AcceptNewTerms(object sender, EventArgs e)
        {
            valueForm valueForm = new valueForm();
            var fields = new List<valueField>();
            valueField field = new valueField();
            field.name = "terms_and_cond_version";
            field.value = ChangeTracker.TermsAndConditionsVersion;
            fields.Add(field);
            valueForm.fields = fields.ToArray();

            WsdlRepository.UpdateProfile(null, StationRepository.GetUid(ChangeTracker.CurrentUser), valueForm);

            ChangeTracker.NewTermsAccepted = true;
        }

        private void NavigateToTermsAndConditions(object sender, EventArgs e)
        {
            OnShowTermsAndConditionsExecute("");
        }

        private void WriteCardNumber(string obj)
        {

            PrinterHandler.WriteBarcodeCardNumber(obj);
            ChangeTracker.CardNumber = obj;
            ChangeTracker.CurrentUser.HasActiveCard = true;
            ChangeTracker.CurrentUser.CardNumber = obj;

        }

        System.Timers.Timer monitorsTimer;
        private List<string> changeVisMessage = new List<string>();
        private List<string> setUpBrowserMEssage = new List<string>();
        private void ChangeVisibility(string obj)
        {
            if (NDEServer.NumberOfClients() != Screen.AllScreens.Where(s => !s.Primary).Count())
            {
                if (!changeVisMessage.Contains(obj))
                    changeVisMessage.Add(obj);

                if (monitorsTimer == null)
                {
                    monitorsTimer = new System.Timers.Timer(1000);
                    monitorsTimer.Elapsed += MonitorsTimerElapsed;
                    monitorsTimer.Start();
                }

            }
            else
                NDEServer.AdviseALL(obj + "|" + MsgTag.ChangeVisibility);
        }

        private void SetUpBrowser(string obj)
        {
            if (NDEServer.NumberOfClients() != Screen.AllScreens.Where(s => !s.Primary).Count())
            {
                if (!setUpBrowserMEssage.Contains(obj))
                    setUpBrowserMEssage.Add(obj);

                if (monitorsTimer == null)
                {
                    monitorsTimer = new System.Timers.Timer(1000);
                    monitorsTimer.Elapsed += MonitorsTimerElapsed;
                    monitorsTimer.Start();
                }
            }
            else
                NDEServer.AdviseALL(obj + "|" + MsgTag.SetUpBrowser);
        }

        private void MonitorsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (NDEServer.NumberOfClients() == Screen.AllScreens.Where(s => !s.Primary).Count())
            {
                monitorsTimer.Stop();
                monitorsTimer = null;

                if (changeVisMessage.Count > 0)
                    foreach (string s in changeVisMessage)
                        NDEServer.AdviseALL(s + "|" + MsgTag.ChangeVisibility);

                if (setUpBrowserMEssage.Count > 0)
                    foreach (string s in setUpBrowserMEssage)
                        NDEServer.AdviseALL(setUpBrowserMEssage + "|" + MsgTag.SetUpBrowser);
            }
        }

        private void LanguageChosen(string lang)
        {
            OnPropertyChanged("ErrorMessageText");
        }
        public override void Close()
        {
            NDEServer.Unregister();
            base.Close();
        }


        private void EmulateBarcode(char obj)
        {
            Dispatcher.Invoke(() =>
                {
                    if ((StationRepository.BarcodeScannerAlwaysActive) || StationRepository.BarcodeScannerTempActive)
                    {
                        BarCodeConverter.ProcessBarcode(obj);
                        HandleBarcode();
                    }

                });
        }

        #endregion

        #region Properties




        public bool recievedBP = false;

        protected TicketWS CurrentTicket
        {
            get { return ChangeTracker.CurrentTicket; }
            set { ChangeTracker.CurrentTicket = value; }
        }

        protected FoundUser EditUser
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }



        #endregion


        #region Methods



        public void ShowSystemMessage(long status)
        {
            if (ChangeTracker.IsBasketOpen && StationRepository.Active == 4 && !ChangeTracker.IsLandscapeMode)
                Mediator.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.TERMINAL_STATION_CASH_LOCKED, new string[] { "" }), MsgTag.ShowNotificationBar);
            else if (ChangeTracker.IsBasketOpen && !ChangeTracker.IsLandscapeMode)
                Mediator.SendMessage<MultistringTag>(MultistringTags.TERMINAL_STATION_CASH_LOCKED, MsgTag.HideNotificationBar);

            if (!ChangeTracker.IsUserManagementWindowVisible && !ChangeTracker.IsBasketOpen && !ChangeTracker.IsUserProfile && (StationRepository.Active == 4 || StationRepository.Active == 0))
                LockedMessageVisibility = Visibility.Visible;
            else if ((StationRepository.Active == 4 || StationRepository.Active == 0) && !ChangeTracker.IsUserManagementWindowVisible && !ChangeTracker.IsUserProfile && ChangeTracker.IsLandscapeMode)
            {
                LockedMessageVisibility = Visibility.Visible;
            }
            else
                LockedMessageVisibility = Visibility.Collapsed;


            if (LockedMessageVisibility == Visibility.Visible)
            {
                if (StationRepository.Active == 4)
                    ErrorMessageText = TranslationProvider.Translate(MultistringTags.TERMINAL_STATION_CASH_LOCKED).ToString();
                else if (StationRepository.Active == 0)
                    ErrorMessageText = TranslationProvider.Translate(MultistringTags.TERMINAL_STATION_TERMINAL_LOCKED).ToString();
                else
                    ErrorMessageText = "";
            }
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            _mainWindowActive = false;
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            _mainWindowActive = true;
        }

        private bool _mainWindowActive;


        public bool IsFirstViewOrDefaultActive
        {
            get
            {
                var currentModelType = MyRegionManager.CurrentViewModelType(RegionNames.ContentRegion);

                if (ChangeTracker.AutoLogoutActive)
                    return true;
                return _mainWindowActive && (currentModelType.ToString().Contains("TopTournamentsViewModel") || (currentModelType.ToString().Contains("LiveViewModel") && ChangeTracker.LiveSelectedMode == 0 && ChangeTracker.LiveSelectedAllSports));
            }
        }

        public override void OnNavigationCompleted()
        {
            ViewWindow.Activated += MainWindow_Activated;
            ViewWindow.Deactivated += MainWindow_Deactivated;


            Log.Debug("mainviewmodel OnNavigationCompleted start");

            var header = MyRegionManager.NavigateUsingViewModel<HeaderViewModel>(RegionNames.HeaderRegion);
            var footer = MyRegionManager.NavigateUsingViewModel<FooterViewModel>(RegionNames.FooterRegion);

            try
            {
                MyRegionManager.NavigateUsingViewModel<BasketViewModel>(RegionNames.BasketContentRegion);
            }
            catch (Exception ex)
            {

            }

            new Thread(() => { GetBussinesProps(); }).Start();
            ThreadHelper.RunThread("TransactionQueueHelperServise", SaveTransactionQueueItems);


            try
            {
                SetBrowserFeatureControl();

            }
            catch (Exception e)
            {

                Log.Error(e.Message, e);
            }
            stationPropertyService.MessageReceived += stationPropertyService_MessageReceived;

            new Thread(() =>
            {
                bool sucess = false;
                while (!recievedBP)
                {
                    Thread.Sleep(100);
                }
                while (!sucess)
                {
                    try
                    {
                        StationRepository.Init();
                        while (!StationRepository.IsReady)
                        {
                            StationRepository.Refresh();
                            Thread.Sleep(3000);
                        }
                        sucess = true;
                        Log.Debug("Init stationrepository done");

                        stationPropertyService.StartService();
                        LineProvider.Initialize(StationRepository.StationNumber);
                        LineProvider.Run(eLineType.All);


                    }
                    catch (CashinException)
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CASHIN_ERROR_BODY).ToString());
                        sucess = true;
                    }
                    catch (CashinDatasetException)
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CASHIN_DATASET_INVALID).ToString());
                        sucess = true;
                    }
                    catch (FileNotFoundException)
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.PREFFILE_READING_ERROR).ToString());
                    }
                }
            }).Start();

            if ((string.IsNullOrEmpty(StationRepository.StationNumber) || StationRepository.StationNumber == "0000") && !StationRepository.IsTestMode)
            {
                new IdCardService(true);

                try
                {
                    StationRepository.Init();
                    StationSettings.InitializeCashIn();
                }
                catch (CashinException)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CASHIN_ERROR_BODY).ToString());
                }
                catch (CashinDatasetException)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CASHIN_DATASET_INVALID).ToString());
                }
                catch (FileNotFoundException)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.PREFFILE_READING_ERROR).ToString());
                }
                this.ViewWindow.Hide();
                var window = MyRegionManager.FindWindowByViewModel<StationVerificationViewModel>();
                MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.VerificationKeyboardRegion);
                window.ShowDialog();
            }


            if (Boolean.Parse(ConfigurationManager.AppSettings["show_live_monitor"]))
            {
                Log.Debug("show live monitors");

                ChangeTracker.LiveMonitors = new SyncList<LiveWindowEntry>();
                var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();
                for (int i = 0; i < screens.Count; i++)
                {
                    var screen = screens[i];
                    var entry = new LiveWindowEntry();

                    try
                    {
                        Log.DebugFormat("webliveMonitor on {0}", i);
                        ProcessExecutor.Run(Directory.GetCurrentDirectory() + "\\" + typeof(WebLiveMonitorViewModel).Assembly.GetName().Name + ".exe", " " + i + " " + ConfigurationManager.AppSettings["HIDE_MAIN_WINDOW_BORDER"]);

                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message, e);
                    }

                    var liveMonitor = MyRegionManager.FindWindowByViewModel<LiveMonitorViewModel>();
                    MaximizeWindow(liveMonitor, screen);
                    entry.Window = liveMonitor;
                    entry.DataContext = liveMonitor.DataContext;
                    liveMonitor.Show();
                    Log.DebugFormat("liveMonitor on {0}", i);


                    ChangeTracker.LiveMonitors.Add(entry);

                    Thread.Sleep(1000);
                }
            }


            DriversMonitor DrvMonitor = DriversMonitor.Instance;


            string sLocalVersionFile = Environment.CurrentDirectory + "/version.txt";
            if (File.Exists(sLocalVersionFile))
            {
                string[] sVersionInfoContent = File.ReadAllLines(sLocalVersionFile);
                if (sVersionInfoContent.Length > 0 && sVersionInfoContent[0] != null)
                {
                    StationRepository.ServerVersion = sVersionInfoContent[0].Trim();
                }
            }
            Log.Debug("getting TeamViewerID");

            // getting TeamViewerID
            string sTeamViewerID = "";
            try
            {

                StationRepository.TeamViewerID = GetTeamviewerID();
                if (StationRepository.TeamViewerID == null)
                {
                    StationRepository.TeamViewerID = GetTeamViewer10Id();
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error readingTeamViewerID", ex);
            }


            new Thread(() =>
            {
                while (!StationRepository.IsReady || !recievedBP)
                {
                    Log.Error("Cannot get station properties or business props", new Exception());
                    Thread.Sleep(3000);
                }
                if (StationRepository.IsIdCardEnabled)
                    new IdCardService(false);

                StartAutologoutService();

            }).Start();

            if (ChangeTracker.LiveMonitors != null && ChangeTracker.LiveMonitors.Count > 0)
            {
                //foreach (var liveMonitor in ChangeTracker.LiveMonitors)
                //{
                //    liveMonitor.Show();
                //    liveMonitor.WindowState = WindowState.Normal;
                //    liveMonitor.WindowState = WindowState.Maximized;
                //}

                LiveMonitorsService LMService = new LiveMonitorsService();
            }

            if ((ConfigurationManager.AppSettings["showTestInput"] != null) && (ConfigurationManager.AppSettings["showTestInput"] != "0"))
            {

                Thread newWindowThread = new Thread((input) =>
                {
                    var window = MyRegionManager.FindWindowByViewModel<TestInputViewModel>();
                    window.ShowDialog();
                });

                newWindowThread.SetApartmentState(ApartmentState.STA);
                newWindowThread.IsBackground = true;

                newWindowThread.Start();
                var dispatcher = System.Windows.Threading.Dispatcher.FromThread(newWindowThread);

                do
                {
                    Thread.Sleep(10);
                    dispatcher = System.Windows.Threading.Dispatcher.FromThread(newWindowThread);

                }
                while (dispatcher == null);


            }


            bool disableFullscreen = false;
            bool.TryParse(ConfigurationManager.AppSettings["disable_fullscreen"], out disableFullscreen);

            if (!Debugger.IsAttached && !disableFullscreen)
            {
                if (ChangeTracker.MainWindow != null)
                {
                    ChangeTracker.MainWindow.WindowState = WindowState.Normal;
                    ChangeTracker.MainWindow.WindowState = WindowState.Maximized;
                }
            }
            else
            {
                if (ChangeTracker.MainWindow != null)
                {
                    ChangeTracker.MainWindow.WindowState = WindowState.Normal;
                    ChangeTracker.MainWindow.WindowState = WindowState.Maximized;
                    ChangeTracker.MainWindow.WindowState = WindowState.Normal;
                }
            }
            if (header != null)
                HidePleaseWait = header.IsReady && footer.IsReady;
            base.OnNavigationCompleted();




        }
        public static string GetTeamviewerID()
        {
            var versions = new[] { "4", "5", "5.1", "6", "7", "8", "9", "10" }.Reverse().ToList(); //Reverse to get ClientID of newer version if possible

            foreach (var path in new[] { "SOFTWARE\\TeamViewer", "SOFTWARE\\Wow6432Node\\TeamViewer" })
            {
                if (Registry.LocalMachine.OpenSubKey(path) != null)
                {
                    foreach (var version in versions)
                    {
                        var subKey = string.Format("{0}\\Version{1}", path, version);
                        if (Registry.LocalMachine.OpenSubKey(subKey) != null)
                        {
                            var clientID = Registry.LocalMachine.OpenSubKey(subKey).GetValue("ClientID");
                            if (clientID != null) //found it?
                            {
                                return Convert.ToInt32(clientID).ToString();
                            }
                        }
                    }
                }
            }
            //Not found, return an empty string
            return null;
        }
        string GetTeamViewer10Id()
        {
            try
            {
                string regPath = Environment.Is64BitOperatingSystem ? @"SOFTWARE\Wow6432Node\TeamViewer" : @"SOFTWARE\TeamViewer";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath);
                if (key == null)
                    return null;
                object clientId = key.GetValue("ClientID");
                if (clientId != null) //ver. 10
                    return clientId.ToString();
                foreach (string subKeyName in key.GetSubKeyNames().Reverse()) //older versions
                {
                    clientId = key.OpenSubKey(subKeyName).GetValue("ClientID");
                    if (clientId != null)
                        return clientId.ToString();
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void SaveTransactionQueueItems(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                TryToSaveTransactionObjectsOnHub();
                if (ChangeTracker.CurrentUser != null)
                    ChangeTracker.CurrentUser.Refresh();
                Thread.Sleep(StationRepository.SyncInterval * 1000);
            }
        }
        public void TryToSaveTransactionObjectsOnHub()
        {
            TransactionQueueHelper.TryToSaveTransactionObjectsOnHub();
        }

        public void GetFocus(string str)
        {
            Dispatcher.BeginInvoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType().ToString().Contains("Auth"))
                    {
                        window.Activate();
                        window.Focus();
                        break;
                    }
                    if (window.GetType().ToString().Contains("TermsAndConditions"))
                    {
                        window.Activate();
                        window.Focus();
                        break;
                    }
                    if (window.GetType().ToString().Contains("Main"))
                    {
                        window.Activate();
                        window.Focus();
                    }
                }
            });
        }

        public void stationPropertyService_MessageReceived(string message)
        {
            var deserializedFeed = JsonConvert.DeserializeObject<ActiveMqMessage>(message);
            bool doRefresh = false;
            if (deserializedFeed == null)
                return;
            if (deserializedFeed.station_number != null)
                foreach (var number in deserializedFeed.station_number)
                {
                    if (number == StationRepository.StationNumber)
                    {
                        doRefresh = true;
                        break;
                    }
                }
            if (!doRefresh)
            {
                if (deserializedFeed.location_id != null)
                    foreach (var number in deserializedFeed.location_id)
                    {
                        if (number == StationRepository.LocationID.ToString())
                        {
                            doRefresh = true;
                            break;
                        }
                    }
            }
            if (!doRefresh)
            {
                if (deserializedFeed.franchisor_id != null)
                    foreach (var number in deserializedFeed.franchisor_id)
                    {
                        if (number == StationRepository.FranchisorID.ToString())
                        {
                            doRefresh = true;
                            break;
                        }
                    }
            }

            if (doRefresh && deserializedFeed.event_name == "station_properties")
                StationRepository.Refresh();
        }

        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }

        private void SetBrowserFeatureControl()
        {
            Log.Debug("SetBrowserFeatureControl");

            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode());
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_NINPUT_LEGACYMODE", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 10001; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode. Default value for Internet Explorer 10.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;
                default:
                    // use IE10 mode by default
                    break;
            }

            return mode;
        }

        [WsdlServiceSyncAspectSilent]
        private void GetBussinesProps()
        {
            BusinessProps bp = null;
            while (true)
            {
                while (StationRepository.StationNumber == "0000")
                    Thread.Sleep(10);
                try
                {
                    long ticketnumber, creditnotenumber, transactionid;

                    creditnotenumber = WsdlRepository.GetBusinessProps(StationRepository.StationNumber, out ticketnumber, out transactionid);

                    if (ticketnumber == 0 && creditnotenumber == 0)
                    {
                        Log.Error("Cannot get business properties", new Exception());
                    }
                    else
                    {
                        bp = new BusinessProps(ticketnumber, creditnotenumber, transactionid);
                        BusinessPropsHelper.Initialize(StationRepository.StationNumber, bp);
                        recievedBP = true;
                    }
                }
                catch (System.ServiceModel.FaultException<HubServiceException> exception)
                {
                    switch (exception.Detail.code)
                    {
                        case 181:
                            ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_NO_STATION_DB).ToString());
                            break;
                        default:
                            ShowError(exception.Detail.message);
                            break;

                    }
                }

                catch (Exception excp)
                {
                    if (excp is System.ServiceModel.Security.SecurityNegotiationException)
                    {
                        Mediator.SendMessage(new Tuple<string, string, bool, int>("SertificateError", "", false, 0), "Error");

                    }
                    else
                    {
                        var lostConnection = new Tuple<string, string>("LostInternetConnection", "");
                        Mediator.SendMessage(lostConnection, "Error");
                        Log.Error(ExcpHelper.FormatException(excp, "Initialize(sStationNumber = '{0}', {1}) ERROR", StationRepository.StationNumber, ""), new Exception());
                        //ShowStopNotification(TranslationProvider.Translate(MultistringTags.TERMINAL_INTERNET_ERROR_BODY) + "\n" + TranslationProvider.Translate(MultistringTags.TERMINAL_RESTART_AFTER_INTERNET_LOST));
                    }
                }
                Thread.Sleep(6000);
                if (bp != null)
                    return;
            }
        }

        private void ShowMsg(string[] fileName)
        {
            if (fileName[1] != "0")
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_SUCCESS_SENDLOGS).ToString() + " (" + fileName[0] + ")");
            else ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_FAIL_SENDLOGS).ToString());

        }



        public void HideOtherWindows(string obj)
        {
            QuestionWindowService.Close();
            ErrorWindowService.Close();
            EnterPinWindowService.Close();
            Dispatcher.BeginInvoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.ToString().Contains("MainWindow") || window.ToString().Contains("LiveMonitorWindow"))
                    {

                    }
                    else
                    {
                        window.Close();
                    }
                }
            });

        }

        [WsdlServiceSyncAspect]
        private void askCashpool_NoClick(object sender, EventArgs e)
        {
            var amount = cashpool;
            SaveCreditNote(amount);
        }


        private void OnShowTermsAndConditionsExecute(string obj)
        {
            if (String.IsNullOrEmpty(obj))
            {
                var window = MyRegionManager.FindWindowByViewModel<TermsAndConditionsViewModel>();
                window.ShowDialog();
            }
            else if (obj == "VHC") //open on vhc part
            {
                var window = MyRegionManager.FindWindowByViewModel<TermsAndConditionsViewModel>();
                Mediator.SendMessage<bool>(true, MsgTag.OpenVHCtAC);
                window.ShowDialog();
            }
            else if (obj == "RG")
            {
                var window = MyRegionManager.FindWindowByViewModel<TermsAndConditionsViewModel>();
                Mediator.SendMessage<bool>(true, MsgTag.ShowResponibleGamng);
                window.ShowDialog();
            }
        }

        private void OnShowCalibrationExecute(string obj)
        {
            var window = MyRegionManager.FindWindowByViewModel<CalibrationViewModel>();
            window.ShowDialog();
        }

        [WsdlServiceSyncAspect]
        private void askCashpool_YesClick(object sender, EventArgs e)
        {
            Exception error = null;
            TransactionQueueHelper.TryDepositMoneyOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(ChangeTracker.CurrentUser), cashpool, false, ref error, null);
            if (error != null)
                Log.Error("", error);

            ChangeTracker.CurrentUser.Addmoney(cashpool);
            foreach (var ticket1 in TicketHandler.TicketsInBasket.ToSyncList())
            {
                ticket1.Stake = 0;
            }
            TicketHandler.UpdateTicket();
            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;

        }

        private void AskAboutCashPool(decimal cashDecimal)
        {
            if (!StationRepository.AllowAnonymousBetting)
                return;
            cashpool = cashDecimal;
            //ChangeTracker.CurrentUser.CashToTransfer = cashpool;
            //uncomment when we have credit note
            var text = TranslationProvider.Translate(MultistringTags.TERMINAL_CREDITNOTE_MOVECREDITTOBALANCE_LOGIN, cashpool);
            QuestionWindowService.ShowMessage(text, null, null, askCashpool_YesClick, askCashpool_NoClick, clearCashToTransfer: true);
        }


        private void OnBindUserCard(long obj)
        {
            idCardUserId = obj;
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }
            if (StationRepository.IsIdCardEnabled)
            {
                Dispatcher.Invoke((Action)(() =>
                    {
                        var window = MyRegionManager.FindWindowByViewModel<BindCardViewModel>();
                        var viewModel = (BindCardViewModel)window.DataContext;
                        viewModel.AcceptClick += new EventHandler(viewModelBindUser_AcceptClick);
                        window.ShowDialog();
                    }));
            }
            else
            {
                viewModelBindUser_AcceptClick(null, null);
            }
        }

        [WsdlServiceSyncAspectSilent]
        void viewModelBindUser_AcceptClick(object sender, EventArgs e)
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }

            //if (!String.IsNullOrEmpty(CardNumber))
            //{
            //    try
            //    {
            //        WsdlRepository.UpdateIdCard(CardNumber, "1", false, null);
            //    }
            //    catch (FaultException<HubServiceException>)
            //    {
            //    }
            //}
            string pin = "";

            string number = null;
            Random random = new Random();
            if (!StationRepository.IsIdCardEnabled)
            {
                number += random.Next(1, 9);

                for (int i = 0; i < 14; i++)
                {
                    number += random.Next(10);
                }
            }

            cardNumber = WsdlRepository.RegisterIdCard(number, true, StationRepository.FranchisorID.ToString(), out pin);

            bool isBoundToUser = Boolean.Parse(WsdlRepository.UpdateIdCard(cardNumber, "1", true, (int)idCardUserId));


            if (!string.IsNullOrEmpty(cardNumber) && isBoundToUser)
            {
                if (ChangeTracker.TestInputActive && StationRepository.IsIdCardEnabled)
                {
                    Mediator.SendMessage(cardNumber, MsgTag.TestWriteNumber);
                }
                else
                {
                    Mediator.SendMessage(cardNumber, MsgTag.WriteNumber);
                }
                if (cardNumber.Equals(ChangeTracker.CardNumber))
                {
                    PrinterHandler.PrintPinNote(pin);
                    if (EditUser != null)
                    {
                        var cards = WsdlRepository.GetIdCardInfo(EditUser.AccountId, Role.User);
                        if (cards != null && cards.Any(card => card.active == "1"))
                        {
                            EditUser.HasCard = 1;
                        }
                        else EditUser.HasCard = 0;
                    }

                    ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                }
                else
                {
                    ShowMessage(TranslationProvider.Translate(MultistringTags.ERROR_WHILE_SAVING_TRY_AGAIN).ToString());
                }
            }
        }

        private void OnBindOperatorCard(long obj)
        {
            idCardUserId = obj;
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }

            if (StationRepository.IsIdCardEnabled)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    var window = MyRegionManager.FindWindowByViewModel<BindCardViewModel>();
                    var viewModel = (BindCardViewModel)window.DataContext;
                    viewModel.CurrentCardNumber = ChangeTracker.CurrentUser.CardNumber;
                    viewModel.AcceptClick += new EventHandler(OperatorBindCard_AcceptClick);
                    window.ShowDialog();
                }));
            }
            else
            {
                OperatorBindCard_AcceptClick(null, null);
            }

        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_PINCODE).ToString() + ", ";

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

        private long idCardUserId;

        private string cardNumber;

        [WsdlServiceSyncAspectSilent]
        void OperatorBindCard_AcceptClick(object sender, EventArgs e)
        {

            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }

            //if (!String.IsNullOrEmpty(CardNumber))
            //{
            //    try
            //    {
            //        WsdlRepository.UpdateIdCard(CardNumber, "1", false, null);
            //    }
            //    catch (FaultException<HubServiceException>)
            //    {
            //    }
            //}
            string pin = "";
            string number = "";
            var random = new Random();
            if (!StationRepository.IsIdCardEnabled)
            {
                number += random.Next(1, 9);

                for (int i = 0; i < 14; i++)
                {
                    number += random.Next(10);
                }
            }

            cardNumber = WsdlRepository.BindOperatorIdCard((int)idCardUserId, number, out pin);


            if (!string.IsNullOrEmpty(cardNumber))
            {
                if (ChangeTracker.TestInputActive && StationRepository.IsIdCardEnabled)
                {
                    Mediator.SendMessage(cardNumber, MsgTag.TestWriteNumber);
                }
                else
                {
                    Mediator.SendMessage(cardNumber, MsgTag.WriteNumber);
                }
                if (cardNumber.Equals(ChangeTracker.CardNumber))
                {
                    ChangeTracker.CardNumber = cardNumber;
                    ChangeTracker.CurrentUser.HasActiveCard = true;
                    ChangeTracker.CurrentUser.CardNumber = cardNumber;


                    ChangeTracker.CurrentUser.HasActiveCard = true;
                    PrinterHandler.PrintPinNote(pin);
                    ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                }
                else
                {
                    ChangeTracker.CurrentUser.HasActiveCard = false;
                    ShowMessage(TranslationProvider.Translate(MultistringTags.ERROR_WHILE_SAVING_TRY_AGAIN).ToString());
                }
            }
        }



        private static object syncIsertRemove = new Object();

        private void CardInserted(string number)
        {
            ChangeTracker.MouseClickLastTime = DateTime.Now;

            lock (syncIsertRemove)
            {
                if (StationRepository.HubSettings.ContainsKey("id_card_enabled") && StationRepository.HubSettings["id_card_enabled"].Equals("false"))
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_LOGIN_WITH_IDCARD_FORBIDDEN).ToString(), null, false, 5);
                    UnBlur();
                    return;
                }

                if (ChangeTracker.CurrentUser is LoggedInUser)
                {
                    try
                    {
                        Blur();
                        Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                        Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);
                        Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);
                        ClearAndOpenAnonymousSession();
                    }
                    catch (Exception e)
                    {
                        Log.Error("", e);
                    }
                }
                UnBlur();
                ChangeTracker.CardNumber = number;

                if (ChangeTracker.CurrentUser is AnonymousUser || (ChangeTracker.CurrentUser is OperatorUser && !ChangeTracker.IsUserManagementWindowVisible))
                    CardInserted();
            }
        }

        public bool CheckIDCard(string cardNumber)
        {
            SessionWS sessionId;

            decimal reserved = 0;
            decimal factor;
            decimal cashpool = 0;
            if (ChangeTracker.CurrentUser is AnonymousUser)
            {
                cashpool = WsdlRepository.GetBalance(StationRepository.GetUid(ChangeTracker.CurrentUser), out reserved, out factor) - reserved;
            }

            bool result = WsdlRepository.ValidateIdCard(cardNumber, StationRepository.StationNumber, false, out sessionId);

            // result == true, sessionId null -> PIN needed
            // result == true, sessionId exists -> PIN not needed
            // result == false -> some other error

            if (result && sessionId != null && sessionId.permissions != null)
            {
                var user = new OperatorUser(sessionId.session_id) { Username = sessionId.username };
                user.Username = user.Username.Trim(new Char[] { ' ', '@', '.' });
                user.AccountId = sessionId.account_id;
                user.Role = sessionId.roleName;
                user.RoleColor = sessionId.highlight_color;
                user.Permissions = sessionId.permissions;
                user.RoleID = GetRoleId(sessionId.role_id);
                user.HasActiveCard = true;
                user.IsLoggedInWithIDCard = true;
                user.CardNumber = cardNumber;
                ChangeTracker.CurrentUser = user;
            }
            else if (result && sessionId != null)
            {
                foreach (var ticket in TicketHandler.TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)))
                {
                    TicketHandler.OnChangeStake("clear", ticket, ChangeTracker.CurrentUser.Cashpool);
                }
                var user = new LoggedInUser(sessionId.account_id, sessionId.session_id, sessionId.balance.amount - sessionId.balance.reserved, sessionId.accountSystemSettings.user_deposit_limit_daily, sessionId.accountSystemSettings.user_deposit_limit_weekly, sessionId.accountSystemSettings.user_deposit_limit_monthly) { Username = sessionId.username };
                user.Permissions = sessionId.permissions;
                ChangeTracker.CurrentUser = user;
                user.CardNumber = cardNumber;
                user.IsLoggedInWithIDCard = true;
                ChangeTracker.CurrentUser = user;
                ChangeTracker.CurrentUser.Currency = StationRepository.Currency;
                //user.RoleID = sessionId.role_id ;
                //user.Role = sessionId.role_idSpecified;
                user.RoleColor = sessionId.highlight_color;
                if (cashpool > 0)
                {
                    Mediator.SendMessage<decimal>(cashpool, MsgTag.AskAboutCashPool);
                }
                GetUserPinSettingFromProfile();
            }

            return result;
        }


        private void CardInserted()
        {
            Blur();
            bool checkedId = false;
            try
            {
                checkedId = CheckIDCard(ChangeTracker.CardNumber);
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                if (exception.Detail.code == 169)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.CARD_NOT_FOUND).ToString());
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.BARCODECARD_NOT_FOUND).ToString());

                }
                else if (exception.Detail.code == 164 || exception.Detail.code == 167)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.CARD_BLOCKED).ToString());
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.BARCODECARD_BLOCKED).ToString());

                }
                else if (exception.Detail.code == 172)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.USERNAME_BLOCKED).ToString());
                }
                else if (exception.Detail.code == 1620)
                {
                    // if card is OK, location requires PIN, but user has PIN disabled, 
                    // then don't show error, only lead to PIN asking window
                    checkedId = true;
                }
                else if (exception.Detail.code == 163)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.CARD_NOT_FOUND).ToString());
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.BARCODECARD_NOT_FOUND).ToString());
                }
                else
                    ShowError(exception.Message);
            }

            if (checkedId)
            {
                // if we still have AnonymousUser, then we have to ask PIN for card
                // if PIN disabled, then user will be created in AuthorizationService.CheckIDCard()
                if (ChangeTracker.CurrentUser is AnonymousUser)
                {
                    if (string.IsNullOrEmpty(ChangeTracker.CardNumber))
                    {
                        UnBlur();
                        return;
                    }
                    EnterPinWindowService.AskPin(enterpinviewModel_OkClick1, enterCardpinviewModel_CloseClick);
                }
                if (ChangeTracker.CurrentUser is OperatorUser)
                {
                    Mediator.SendMessage<long>(2, MsgTag.ShowUserManagement);
                }
            }
            UnBlur();
        }

        void enterCardpinviewModel_CloseClick(object sender, EventArgs<string> e)
        {
            ClearAndOpenAnonymousSession();
        }

        private void StartedCardReading(string number)
        {
            IoCContainer.Kernel.Get<IChangeTracker>().MouseClickLastTime = DateTime.Now;
            Blur();
        }

        [AsyncMethod]
        private void CardRemoved(long obj)
        {
            IoCContainer.Kernel.Get<IChangeTracker>().MouseClickLastTime = DateTime.Now;
            OnCardRemoved(false);
        }

        [AsyncMethod]
        private void IdCardError(long obj)
        {
            OnCardRemoved(true);
        }



        [WsdlServiceSyncAspectSilent]
        private void OnCardRemoved(bool is_error)
        {
            bool show_error_msg_and_exit = false;

            UnBlur();

            lock (syncIsertRemove)
            {
                Mediator.SendMessage(true, MsgTag.CloseEnterPinWindow);

                if (!IsBindingCard)
                    Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);

                Mediator.SendMessage<bool>(true, MsgTag.CloseRegistration);
                Mediator.SendMessage<bool>(true, MsgTag.CloseLogin);
                if (ChangeTracker.CurrentUser is AnonymousUser)
                {
                    show_error_msg_and_exit = true;
                    ChangeTracker.CardNumber = "";
                }


                if (ChangeTracker.CurrentUser is OperatorUser
                    && (String.IsNullOrEmpty(ChangeTracker.CurrentUser.CardNumber) || !ChangeTracker.CurrentUser.IsLoggedInWithIDCard))
                    show_error_msg_and_exit = true;

                if (!show_error_msg_and_exit)
                {
                    StationRepository.DisableCashIn();
                    ChangeTracker.CardNumber = "";
                    CardRemovedMessage(is_error);
                }
                else if (is_error)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.CARD_READER_ERROR).ToString());
                }
            }
        }

        private void CardRemovedMessage(bool is_error)
        {
            if (!IsBindingCard)
            {
                Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);
                Mediator.SendMessage<bool>(true, MsgTag.CloseWarningWindow);
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);

                ClearAndOpenAnonymousSession();
                if (!is_error)
                {
                    ErrorWindowService.Close();
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.USER_LOGGED_OUT_CARD_REMOVED).ToString(),
                                  null, false, 3);
                    else
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.USER_LOGGED_OUT_BARCODECARD_REMOVED).ToString(),
                                  null, false, 3);
                    }
                }
                else
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.CARD_READER_ERROR).ToString());
                }

            }
            else if (!ChangeTracker.CurrentUser.IsLoggedInWithIDCard)
            {
                Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);
            }
        }

        private decimal _dPrinterErrorDeposit = 0;
        private object _locker = new object();
        public const int RESTART_WIN_NOW = 1;
        public const int RESTART_WIN_SOON = 2;
        public const int RESTART_APP_NOW = 3;
        public const int RESTART_APP_SOON = 4;
        private void CheckRestart(int iRestart)
        {
            IMediator mediator;
            mediator = IoCContainer.Kernel.Get<IMediator>();
            switch (iRestart)
            {
                case RESTART_APP_NOW:
                    mediator.SendMessage<long>(0, MsgTag.RestartApplication);
                    break;
                case RESTART_APP_SOON:
                    if (!IsFirstViewOrDefaultActive
                        || ChangeTracker.CurrentUser is LoggedInUser
                        || ChangeTracker.CurrentUser is OperatorUser)
                    {
                        // wait for logout service
                        ChangeTracker.RestartPending = RESTART_APP_SOON;
                    }
                    else
                    {
                        // restart now
                        mediator.SendMessage<long>(0, MsgTag.RestartApplication);
                    }
                    break;
                case RESTART_WIN_NOW:
                    mediator.SendMessage<long>(0, MsgTag.RestartStation);
                    break;
                case RESTART_WIN_SOON:
                    if (!IsFirstViewOrDefaultActive
                        || ChangeTracker.CurrentUser is LoggedInUser
                        || ChangeTracker.CurrentUser is OperatorUser)
                    {
                        // wait for logout service
                        ChangeTracker.RestartPending = RESTART_WIN_SOON;
                    }
                    else
                    {
                        // restart now
                        mediator.SendMessage<long>(0, MsgTag.RestartStation);
                    }
                    break;
            }
        }

        public bool KeepRunning
        {
            get { return StationRepository.IsAutoLogoutEnabled; }
        }
        private bool IsAnonymous
        {
            get { return ChangeTracker.CurrentUser == null || ChangeTracker.CurrentUser is AnonymousUser; }
        }
        private void Run(ThreadContext tc)
        {
            int _delay = 0;
            int.TryParse(ConfigurationManager.AppSettings["AUTOLOGOUT_DELAY"], out _delay);
            while (true)
            {
                if (!tc.IsToStop && KeepRunning)
                {
                    try
                    {
                        if (StationRepository.HubSettings.ContainsKey("station_autologout_timeout"))
                        {
                            int.TryParse(StationRepository.HubSettings["station_autologout_timeout"], out _delay);
                        }
                        _delay = (_delay < StationRepository.AutoLogoutWindowLiveTimeInSec) ? StationRepository.AutoLogoutWindowLiveTimeInSec + 1 : _delay;


                        //TODO
                        var delayAfterLastMouseClick = DateTime.Now - ChangeTracker.MouseClickLastTime;

                        var isFirstView = IsFirstViewOrDefaultActive;
                        if (delayAfterLastMouseClick.TotalSeconds > _delay && (!isFirstView || !IsAnonymous))
                        {
                            ChangeTracker.DoLogout = true;
                            Mediator.SendMessage<int>(StationRepository.AutoLogoutWindowLiveTimeInSec, MsgTag.AutoLogoutWaitWindow);
                            if (ChangeTracker.DoLogout)
                            {
                                AutoLogout();
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        Log.Error("", e);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void AutoLogout()
        {
            try
            {
                if (ChangeTracker.RestartPending == RESTART_APP_SOON)
                {
                    Mediator.SendMessage<long>(0, MsgTag.RestartApplication);

                    return;
                }
                if (ChangeTracker.RestartPending == RESTART_WIN_SOON)
                {
                    Mediator.SendMessage<long>(0, MsgTag.RestartStation);

                    return;
                }
                Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);
                ErrorWindowService.Close();
                QuestionWindowService.Close();
                EnterPinWindowService.Close();
                Mediator.SendMessage<long>(0, MsgTag.HideVerification);
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                Mediator.SendMessage<long>(0, MsgTag.HideLogin);
                Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);
                ClearAndOpenAnonymousSession();

                var minLimit = ChangeTracker.CurrentUser.DailyLimit;
                if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                    minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
                if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                    minLimit = ChangeTracker.CurrentUser.MonthlyLimit;


                StationRepository.SetCashInDefaultState(minLimit);
            }
            catch (Exception ex)
            {
                Log.Error("", ex);
            }
            WaitOverlayProvider.DisposeAll();
            UnBlur();


        }

        public void StartAutologoutService()
        {
            ThreadHelper.RunThread("AutologoutPropertyService", Run);

        }



        [AsyncMethod]
        private void PrinterNotReady(decimal obj)
        {
            _dPrinterErrorDeposit = obj;
            Mediator.SendMessage<bool>(true, "PrinterErrorValue");
            PrinterErrorMessage();
        }

        [PleaseWaitAspect]
        private void PrinterErrorMessage()
        {
            if (_dPrinterErrorDeposit > 0)
            {
                string sTmp = TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString() + "\n";
                sTmp += TranslationProvider.Translate(MultistringTags.TERMINAL_CONNECTION_LOST_PRINTER_ERROR).ToString() + "\n";
                sTmp += TranslationProvider.Translate(MultistringTags.DEPOSIT) + " " + _dPrinterErrorDeposit.ToString() + " ";
                sTmp += StationRepository.Currency;

                ShowError(sTmp, null, true);
            }
            else
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
            }
        }

        private void enterpinviewModel_OkClick1(object sender, EventArgs<string> e)
        {
            OnPinNumberEntered(e.Value);
        }

        public bool LoginWithIdCard(string cardNumber, string pin)
        {
            decimal reserved = 0;
            decimal factor;
            decimal cashpool = 0;
            if (ChangeTracker.CurrentUser is AnonymousUser)
            {
                cashpool = WsdlRepository.GetBalance(StationRepository.GetUid(ChangeTracker.CurrentUser), out reserved, out factor) - reserved;
            }

            ClearEverythingAfterUser();
            int accountId = 0;
            SessionWS sessionId = WsdlRepository.LoginWithIdCard(StationRepository.StationNumber, cardNumber, pin);

            string username = sessionId.username;

            var lang = sessionId.default_language;
            string[] permissions = sessionId.permissions;
            string role = sessionId.roleName, roleColor = sessionId.highlight_color;
            accountId = sessionId.account_id;

            if (TranslationProvider.CurrentLanguage != lang && lang != null)
            {

                TranslationProvider.CurrentLanguage = lang;
                Mediator.SendMessage(lang, MsgTag.LanguageChosenHeader);
                Mediator.SendMessage(lang, MsgTag.LanguageChosen);
            }

            if (sessionId.session_id == InvalidSessionID || sessionId == null)
            {
                ClearAndOpenAnonymousSession();
                return false;
            }
            else if (permissions != null)
            {
                var user = new OperatorUser(sessionId.session_id) { Username = username };
                user.Username = user.Username.Trim(new Char[] { ' ', '@', '.' });
                user.Role = role;
                user.RoleColor = roleColor;
                user.AccountId = accountId;
                user.Permissions = permissions;
                user.CardNumber = cardNumber;
                user.HasActiveCard = true;
                user.IsLoggedInWithIDCard = true;
                user.PinEnabled = sessionId.card_pin_enabledSpecified;
                ChangeTracker.CurrentUser = user;
                user.RoleID = GetRoleId(sessionId.role_id);
                Mediator.SendMessage<long>(2, MsgTag.ShowUserManagement);
                return true;
            }
            else
            {

                if (StationRepository.Active == 0)
                {
                    Mediator.SendMessage(new Tuple<string, string, bool, int>(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString(), "", false, 3), MsgTag.Error);
                    ClearAndOpenAnonymousSession();
                    return true;
                }
                foreach (var ticket in TicketHandler.TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)))
                {
                    TicketHandler.OnChangeStake("clear", ticket, ChangeTracker.CurrentUser.Cashpool);
                }

                var user = new LoggedInUser(accountId, sessionId.session_id, sessionId.balance.amount - sessionId.balance.reserved, sessionId.accountSystemSettings.user_deposit_limit_daily, sessionId.accountSystemSettings.user_deposit_limit_weekly, sessionId.accountSystemSettings.user_deposit_limit_monthly) { Username = username };
                //user.Refresh();
                user.CardNumber = cardNumber;
                user.Role = role;
                user.RoleColor = roleColor;
                user.IsLoggedInWithIDCard = true;
                ChangeTracker.CurrentUser = user;
                ChangeTracker.CurrentUser.Currency = StationRepository.Currency;
                if (cashpool > 0)
                {
                    Mediator.SendMessage<decimal>(cashpool, MsgTag.AskAboutCashPool);

                }
                GetUserPinSettingFromProfile();
                Mediator.SendMessage(true, MsgTag.RefreshTicketDetails);
                return true;
            }

        }


        [WsdlServiceSyncAspect]
        private void OnPinNumberEntered(string pin)
        {
            var sucess = false;
            try
            {
                sucess = LoginWithIdCard(ChangeTracker.CardNumber, pin);
                if (!sucess)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR).ToString());
                }
                else
                {
                    //send message to close login window if was open
                    Mediator.SendMessage<bool>(true, MsgTag.CloseLogin);
                }
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {

                if (exception.Detail.code == 163)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.INVALID_CARD_NUMBER));
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.INVALID_BARCODECARD_NUMBER));

                }
                else if (exception.Detail.code == 168)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.INCORRECT_PIN).ToString(), ErrorOkClick);
                }
                else if (exception.Detail.code == 115)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.CANNOT_LOGIN_TO_THIS_STATION).ToString(), ErrorOkClick);
                }
                else if (exception.Detail.code == 116)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.LOGIN_RESTRICTED).ToString(), ErrorOkClick);
                }
                else if (exception.Detail.code == 117)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.USER_INACTIVE).ToString(), ErrorOkClick);
                }
                else if (exception.Detail.code == 118)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.INVALIDLOCATION).ToString(), ErrorOkClick);
                }
                else if (exception.Detail.code == 119)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.USER_BLOCKED).ToString(), ErrorOkClick);
                }
                else if (exception.Detail.code == 162)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.CARD_NOT_REGISTERED), ErrorOkClick);
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.BARCODECARD_NOT_REGISTERED), ErrorOkClick);

                }
                else if (exception.Detail.code == 113)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.LOGIN_FRANCHISOR_INCORRECT).ToString(), ErrorOkClick);
                }
                else
                {

                    string sTime = "";
                    string sUsername = "";
                    if (exception.Detail.parameters != null)
                    {
                        for (int i = 0; i < exception.Detail.parameters.Length; i++)
                        {
                            if (exception.Detail.parameters[i].name == "blockedUntil")
                            {
                                sTime = exception.Detail.parameters[i].value;
                                long milliSeconds = Int64.Parse(sTime);
                                DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                DateTime dt = UTCBaseTime.Add(new TimeSpan(milliSeconds * TimeSpan.TicksPerMillisecond)).ToLocalTime();
                                sTime = dt.ToString("dd.MM.yyyy HH:mm");
                            }

                            if (exception.Detail.parameters[i].name == "username")
                                sUsername = exception.Detail.parameters[i].value;
                        }
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_USERNAME_BLOCKED, sUsername, sTime), ErrorOkClick);
                    }
                    else ShowError(exception.Detail.message);
                }
            }

        }

        void ErrorOkClick(object sender, EventArgs e)
        {
            WaitOverlayProvider.DisposeAll();
            RetryEnterPinDialog();
        }


        private void RetryEnterPinDialog()
        {
            Mediator.SendMessage(true, MsgTag.CloseEnterPinWindow);

            if (!String.IsNullOrEmpty(ChangeTracker.CardNumber))
                EnterPinWindowService.AskPin(enterpinviewModel_OkClick1, pinViewModel_CloseClick);
        }



        void pinViewModel_CloseClick(object sender, EventArgs<string> e)
        {
            ClearAndOpenAnonymousSession();
        }






        private void OnOpenStoredTicket(string obj)
        {

            var iOddsCount = TicketHandler.TicketsInBasket[0].TipItems.Count;
            if (iOddsCount > 0)
            {
                var text = TranslationProvider.Translate(MultistringTags.WANT_TO_RESTORE).ToString();
                QuestionWindowService.ShowMessage(text, null, null, viewModel_YesClick, viewModel_NoClick);
            }
            else
            {
                viewModel_YesClick(null, null);
            }
        }

        private void viewModel_NoClick(object sender, System.EventArgs e)
        {
            ChangeTracker.LoadedTicket = "";
            ChangeTracker.LoadedTicketcheckSum = "";
        }

        private void viewModel_YesClick(object sender, System.EventArgs e)
        {
            Mediator.SendMessage(true, MsgTag.CloseEnterPinWindow);


            EnterPinWindowService.AskPin(enterpinviewModel_OkClick, enterpinviewModel_CloseClick);

        }

        private void enterpinviewModel_CloseClick(object sender, EventArgs<string> e)
        {
            ChangeTracker.LoadedTicket = "";
            ChangeTracker.LoadedTicketcheckSum = "";
        }

        [WsdlServiceSyncAspect]
        private void enterpinviewModel_OkClick(object sender, EventArgs<string> e)
        {
            try
            {
                var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();

                while (TicketsInBasket[0].TipItems.Count > 0)
                {
                    var odd = TicketsInBasket[0].TipItems[0];
                    TicketsInBasket[0].TipItems.Remove(odd);
                }
                // TODO dummy String.Empty
                var ticket = WsdlRepository.LoadStoredTickets(StationRepository.GetUid(ChangeTracker.CurrentUser), StationRepository.StationNumber, ChangeTracker.LoadedTicket, e.Value, ChangeTracker.LoadedTicketcheckSum);
                ChangeTracker.LoadedTicket = "";
                ChangeTracker.LoadedTicketcheckSum = "";
                if (ticket != null && ticket.Length > 0)
                {
                    foreach (var ticketWs in ticket)
                    {
                        // check if ticket belongs to user
                        // anonymous ticket (id=1)  might be resored by everyone
                        if ((ticketWs.userId != 1) && (ticketWs.userId != ChangeTracker.CurrentUser.AccountId))
                        {
                            ShowError(TranslationProvider.Translate(MultistringTags.THIS_TICKET_DOES_NOT_BELONG_TO_YOU).ToString());
                            return;
                        }

                        foreach (var betws in ticketWs.bets)
                        {
                            var bankOdds = new List<ITipItemVw>();
                            foreach (var tipWs in betws.bankTips)
                            {
                                var odd = ConvertOddFromTipWs(tipWs);
                                if (odd != null && odd.IsBank) bankOdds.Add(odd);
                                if (odd != null) TicketsInBasket[0].TipItems.Add(odd);
                            }
                            foreach (var tipWs in betws.tips2BetMulti)
                            {
                                var odd = ConvertOddFromTipWs(tipWs);
                                if (odd != null && odd.IsBank) bankOdds.Add(odd);
                                if (odd != null) TicketsInBasket[0].TipItems.Add(odd);
                            }
                            switch (betws.betType)
                            {
                                case Bet.BET_TYPE_COMBI:
                                case Bet.BET_TYPE_COMBIPATH:
                                    TicketHandler.TicketState = TicketStates.Multy;
                                    break;
                                case Bet.BET_TYPE_SYSTEM:
                                case Bet.BET_TYPE_SYSTEMPATH:
                                    TicketHandler.TicketState = TicketStates.System;
                                    break;
                                case Bet.BET_TYPE_SINGLE:
                                    TicketHandler.TicketState = TicketStates.Single;
                                    break;
                            }
                            foreach (var odd in bankOdds)
                            {
                                odd.IsBank = true;
                                CheckSystemOdds(false);
                            }
                        }
                    }
                    //OpenPlaceBetWindow();
                }
                else
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_TICKET_NOT_FOUND).ToString());
                }
            }
            catch (Exception)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_TICKET_NOT_FOUND).ToString());
            }

        }

        private bool CheckSystemOdds(bool isShowMessage)
        {
            var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
            if (!LimitHandling.SystemBetYAllowed(TicketsInBasket[0].TipItems.Count, TicketsInBasket[0]) && TicketHandler.TicketState == TicketStates.System)
            {
                TicketsInBasket[0].TotalOddDisplay = 0;
                if (isShowMessage)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_A_PERMBET_MAY_HAVE_A_MAXIMUM_OF_ITEMS, StationRepository.GetMaxSystemBet(TicketsInBasket[0])));
                }
                return false;
            }
            else
            {
                return true;
            }
        }


        private ITipItemVw ConvertOddFromTipWs(TipWS tipWs)
        {
            var tipItemVw = Repository.GetOddBySvrId(tipWs.svrOddID);
            var tip = PlaceBetMethod(new TipItemVw(tipItemVw.LineObject));
            if (tip != null)
            {
                tip.IsBank = tipWs.bank;
                tip.IsSelected = true;
                return tip;
            }
            return null;
        }

        private void ShowUserManagementWindow(long obj)
        {
            WaitOverlayProvider.ShowWaitOverlay();

            ChangeTracker.SelectedLive = false;
            ChangeTracker.SelectedResults = false;
            ChangeTracker.SelectedTicket = false;
            ChangeTracker.SelectedSports = false;
            ChangeTracker.SelectedVirtualSports = false;
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            MyRegionManager.NavigateUsingViewModel<UserManagementViewModel>(RegionNames.ContentRegion);
        }


        [AsyncMethod]
        public void LoadTicket(Tuple<string, string, BarCodeConverter.BarcodeType> values)
        {
            ChangeTracker.RedirectToTicketDetails = true;
            LoadTicketPleaseWait(values);

            //    Mediator.SendMessage<long>(0, MsgTag.LoadTicket);

        }


        [PleaseWaitAspect]
        private bool LoadTicketPleaseWait(Tuple<string, string, BarCodeConverter.BarcodeType> values)
        {

            var number = values.Item1;
            var code = values.Item2;
            var type = values.Item3;
            long id = 1;
            string result = "1";
            try
            {
                result = WsdlRepository.GetAccountByTicket(number);
            }
            catch (Exception)
            {
                result = "1";
            }
            long.TryParse(result, out id);
            if ((ChangeTracker.CurrentUser != null && (id == ChangeTracker.CurrentUser.AccountId || id == 1)) || !StationRepository.AuthorizedTicketScanning)
            {
                ChangeTracker.LoadedTicket = number;
                ChangeTracker.LoadedTicketType = type;
                ChangeTracker.LoadedTicketcheckSum = code;

                try
                {
                    CurrentTicket = WsdlRepository.LoadTicket(ChangeTracker.LoadedTicket, ChangeTracker.LoadedTicketcheckSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);

                    if (MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) is TicketDetailsViewModel)
                    {
                        Mediator.SendMessage(true, MsgTag.Refresh);
                    }
                    else
                    {
                        MyRegionManager.NavigateUsingViewModel<TicketDetailsViewModel>(RegionNames.ContentRegion);
                        ChangeTracker.SelectedTicket = true;
                    }

                    return true;
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

                return false;
            }
            else
            {
                ShowError(TranslationProvider.Translate(MultistringTags.THIS_TICKET_DOES_NOT_BELONG_TO_YOU).ToString());
                return false;
            }
        }






        private void HideUserProfileWindow(long obj)
        {
            ChangeTracker.IsUserProfile = false;
            Dispatcher.Invoke((Action)(() =>
                {
                    if (ChangeTracker.UserProfileWindow == null || !ChangeTracker.UserProfileWindow.IsVisible)
                        return;
                    ChangeTracker.UserProfileWindow.Close();
                    ChangeTracker.HeaderVisible = true;
                    ChangeTracker.FooterVisible = true;
                }));

        }


        private void HideLogineWindow(long obj)
        {

            Dispatcher.Invoke((Action)(() =>
                {
                    if (ChangeTracker.LoginWindow == null || !ChangeTracker.LoginWindow.IsVisible)
                        return;
                    ChangeTracker.LoginWindow.Close();
                }));

        }

        private bool networkLostStarted = false;
        private bool NetworkError = false;
        private void ShowError(Tuple<string, string, bool, int> ssErrorTuple)
        {
            string sErrorMsg = ssErrorTuple.Item1;

            if (ChangeTracker.PrinterErrorChecked)
            {
                string sTmp = TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER) + "\n";
                sTmp += TranslationProvider.Translate(MultistringTags.TERMINAL_CONNECTION_LOST_PRINTER_ERROR) + "\n";
                sTmp += TranslationProvider.Translate(MultistringTags.DEPOSIT) + " " + _dPrinterErrorDeposit.ToString() + " ";
                sTmp += StationRepository.Currency;
                sErrorMsg = sTmp;
            }
            else
            {
                switch (sErrorMsg)
                {
                    case "LostInternetConnection":
                        if (!NetworkError)
                        {
                            sErrorMsg = TranslationProvider.Translate(MultistringTags.SHOP_FORM_NO_CONNECTION_TO_SERVER);
                            ErrorWindowService.ShowError(sErrorMsg, null, false, 0, ErrorLevel.ModalWindow);
                        }
                        NetworkError = true;
                        //ErrorWindowService.Close();
                        StationRepository.IsReady = false;
                        if (!networkLostStarted)
                            new Thread(() =>
                                {

                                    networkLostStarted = true;
                                    while (!StationRepository.IsReady)
                                    {
                                        StationRepository.Refresh();
                                        Thread.Sleep(3000);
                                    }
                                    networkLostStarted = false;
                                }).Start();
                        return;
                    case "GotInternetConnection":
                        if (NetworkError)
                        {
                            NetworkError = false;
                            ErrorWindowService.Close();
                        }
                        return;
                    case "NoService":
                        sErrorMsg = TranslationProvider.Translate(MultistringTags.TERMINAL_NO_SERVICE_AVAILABLE, ssErrorTuple.Item2);
                        break;
                    case "IdCardError":
                        sErrorMsg = TranslationProvider.Translate(MultistringTags.ERROR_WHILE_READING_CARD);
                        break;
                    case "IdCardReaderError":
                        sErrorMsg = TranslationProvider.Translate(MultistringTags.CARD_READER_ERROR);
                        break;
                    case "SertificateError":
                        lock (_locker)
                        {
                            ThreadHelper.StopAll();
                            ShowError(TranslationProvider.Translate(MultistringTags.INVALID_CERTIFICATE_VERIFY_STATION).ToString());
                            Dispatcher.Invoke((Action)(() =>
                                {
                                    this.ViewWindow.Hide();
                                    var window = MyRegionManager.FindWindowByViewModel<StationVerificationViewModel>();
                                    MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.VerificationKeyboardRegion);
                                    window.ShowDialog();


                                    Log.Debug("Verification window closed");
                                    if (!System.Diagnostics.Debugger.IsAttached)
                                    {
                                        Log.Debug("Verification: Starting new terminal instance");
                                        Process proc = new Process();
                                        proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\restarter.exe";
                                        proc.StartInfo.Arguments = "\"" + Application.ResourceAssembly.Location + "\" " + Process.GetCurrentProcess().Id;
                                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                        proc.Start();
                                        Log.Debug("Verification: New terminal instance started");
                                    }
                                    Application.Current.Shutdown();
                                    Environment.Exit(0);
                                }));

                        }
                        break;
                    case "StationVerificationFail":
                        sErrorMsg =
                            "Station verification fail. Please check verification number"
                            ;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(sErrorMsg))
                ShowError(sErrorMsg);


            HideUserProfileWindow(0);

            switch (ssErrorTuple.Item1)
            {
                case "LostInternetConnection":
                    Blur();
                    Mediator.SendMessage<long>(0, MsgTag.HideVerification);
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                    Mediator.SendMessage<long>(0, MsgTag.HideLogin);
                    Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);
                    Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);
                    ChangeTracker.CurrentUser = new EmptyUser();
                    AsyncLogout();
                    UnBlur();
                    break;
            }
        }

        private bool logoutActive = false;

        private Visibility _lockedMessageVisibility = Visibility.Collapsed;
        public Visibility LockedMessageVisibility
        {
            get
            {
                return _lockedMessageVisibility;
            }
            set
            {
                _lockedMessageVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessageText;

        Timer timer = new Timer();
        public void ShowNotification(string text)
        {
            LockedMessageVisibility = Visibility.Visible;
            ErrorMessageText = text;
            timer.Elapsed += CloseNotification;
            timer.Interval = 15000;
            timer.AutoReset = false;
            timer.Start();
        }
        public void HideNotification(string text)
        {
            LockedMessageVisibility = Visibility.Collapsed;

            timer.Stop();
        }

        private void CloseNotification(object sender, ElapsedEventArgs e)
        {
            LockedMessageVisibility = Visibility.Collapsed;
        }


        public string ErrorMessageText
        {
            get { return _errorMessageText; }

            set
            {
                _errorMessageText = value;
                OnPropertyChanged();
            }
        }

        [AsyncMethod]
        private void AsyncLogout()
        {
            if (!logoutActive)
            {
                logoutActive = true;
                var previousUser = ChangeTracker.CurrentUser;
                ClearEverythingAfterUser();
                OpenAnonymousSession(true, previousUser);
                logoutActive = false;
            }

        }


        private void RestartApplication(long obj)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                App.DeleteLogRestartErrorFile();
                if (obj != 123)
                {
                    ShowWarning(TranslationProvider.Translate(MultistringTags.RESTART_APPLICATION_NOW).ToString());
                }
                if (!Debugger.IsAttached)
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\restarter.exe";
                    proc.StartInfo.Arguments = "\"" + Application.ResourceAssembly.Location + "\" " + Process.GetCurrentProcess().Id;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                }
                Environment.Exit(0);
            }));
        }

        private void RestartInTestMode(string value)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                App.DeleteLogRestartErrorFile();
                if (value == "testmode")
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\restarter.exe";
                    proc.StartInfo.Arguments = "\"" + Application.ResourceAssembly.Location + "\" " + Process.GetCurrentProcess().Id + " " + value;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                }
                else if (!Debugger.IsAttached)
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\restarter.exe";
                    proc.StartInfo.Arguments = "\"" + Application.ResourceAssembly.Location + "\" " + Process.GetCurrentProcess().Id;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                }
                Environment.Exit(0);
            }));
        }

        private void RestartStation(long obj)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ShowWarning(TranslationProvider.Translate(MultistringTags.RESTART_STATION_NOW).ToString());
                WindowsController.ExitWindows(RestartOptions.Reboot, true);
            }));
        }
        protected void ShowWarning(string obj)
        {
            WaitOverlayProvider.DisposeAll();

            ChangeTracker.AutoLogoutActive = true;
            var window = MyRegionManager.FindWindowByViewModel<WarningViewModel>();
            var model = (WarningViewModel)window.DataContext;
            model.Text = obj;
            MaximizeWindow(window);
            window.ShowDialog();
            ChangeTracker.AutoLogoutActive = false;

        }
        private void SessionClosed(long obj)
        {
            ShowError(TranslationProvider.Translate(MultistringTags.SESSION_CLOSED) as string);
            Logout();
        }

        private void UserBlocked(long obj)
        {
            ShowError(TranslationProvider.Translate(MultistringTags.USER_BLOCKED) as string);
            Logout();
        }

        private void Logout()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);
            Mediator.SendMessage<bool>(true, MsgTag.CloseCurrentWindow);

            Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
            ClearAndOpenAnonymousSession();
            WaitOverlayProvider.DisposeAll();
        }


        private void PrinterErrorValue(bool bValue)
        {
            if (bValue)
                ChangeTracker.PrinterErrorChecked = true;
            else
            {
                _dPrinterErrorDeposit = 0;
                ChangeTracker.PrinterErrorChecked = false;
            }
        }
        #endregion
    }

    public class ActiveMqMessage
    {
        public string[] station_number;
        public string[] location_id;
        public string[] franchisor_id;
        public string event_name;
    }
}