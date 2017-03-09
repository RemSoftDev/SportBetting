using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using System.Globalization;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.DAL;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using System.Runtime.InteropServices;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Footer view model.
    /// </summary>
    [ServiceAspect]
    public class FooterViewModel : BaseViewModel
    {
        public System.Timers.Timer StreamTimer { get; set; }

        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool DeleteUrlCacheEntry(string lpszUrlName);

        private static ILog Log = LogFactory.CreateLog(typeof(FooterViewModel));
        private readonly ScrollViewerModule _ScrollViewerModule;
        private static readonly string _sVersionFile = AppDomain.CurrentDomain.BaseDirectory
                                                        + System.Configuration.ConfigurationManager.AppSettings["VersionInfoFilename"];
        // string would be like "2.1_build_MasterBet_Terminal_83.13032 " the reg.exp. result "83.13032"
        private static readonly string _sVersionRegExpFilter = @"[^a-zA-Z_]*?$";
        private FrameworkPropertyMetadata FrameworkPropertyMetadata;

        private bool FirstStart = true;

        //TODO into separated part

        #region Constructors

        public FooterViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            HidePleaseWait = false;
            LanguageRepository.GetAllLanguages(Languages);
            CurrentLanguage = Languages.FirstOrDefault(x => x.ShortName == StationRepository.DefaultDisplayLanguage);

            ScrollDownStart = new Command(OnScrollDownStartExecute);
            ScrollDownStop = new Command(OnScrollDownStopExecute);
            ScrollUpStart = new Command(OnScrollUpStartExecute);
            ScrollUpStop = new Command(OnScrollUpStopExecute);
            HideBrowserCommand = new Command(HideBrowser);
            ShowTermsAndConditions = new Command(OnShowTermsAndConditionsExecute);
            ShowResponsibleGaming = new Command(onShowResponsibleGaming);
            ScrollToTopCommand = new Command(ScrollToTop);

            Mediator.Register<int>(this, AutoLogoutWaitWindow, MsgTag.AutoLogoutWaitWindow);
            Mediator.Register<string>(this, SelectLanguage, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<IMatchVw>(this, ShowStream, MsgTag.ShowStream);
            Mediator.Register<bool>(this, HideStream, MsgTag.HideStream);

            Mediator.Register<decimal>(this, OddPlaced, MsgTag.OddPlaced);

            DataCopy.UpdateProgressBarEvent += UpdateProgress;
            DataCopy.UpdateLanguagesEvent += DataCopy_UpdateLanguagesEvent;
            BuildVersion = GetBuildVersion();

        }


        #endregion

        #region Properties

        private DispatcherTimer _timer;
        private bool _showTransactionQueueCounter;
        private int _transactionQueueCounter;
        private SyncObservableCollection<Language> _languages = new SyncObservableCollection<Language>();



        public int TotalUpdates
        {
            get { return _totalUpdates; }
            set
            {
                if (_totalUpdates != value)
                {
                    _totalUpdates = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _webBrowserVisibility = Visibility.Collapsed;
        public Visibility WebBrowserVisibility
        {
            get
            {
                return _webBrowserVisibility;
            }
            set
            {
                if (_webBrowserVisibility == value)
                    return;

                _webBrowserVisibility = value;
                OnPropertyChanged();
            }
        }

        private IMatchVw _selectedMatch;
        public IMatchVw SelectedMatch
        {
            get
            {
                return _selectedMatch;
            }
            set
            {
                if (_selectedMatch == value)
                    return;

                _selectedMatch = value;
                OnPropertyChanged();
            }
        }

        public bool ShowProgresBar
        {
            get { return _showProgresBar; }
            set
            {
                if (_showProgresBar != value)
                {
                    _showProgresBar = value;
                    OnPropertyChanged();
                }
            }
        }





        public bool ShowTransactionQueueCounter
        {
            get { return _showTransactionQueueCounter; }
            set
            {
                if (_showTransactionQueueCounter != value)
                {
                    _showTransactionQueueCounter = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TransactionQueueCounter
        {
            get { return _transactionQueueCounter; }
            set
            {
                if (_transactionQueueCounter != value)
                {
                    _transactionQueueCounter = value;
                    ShowTransactionQueueCounter = value > 0;
                    OnPropertyChanged();
                }
            }
        }


        private static ILanguageRepository LanguageRepository
        {
            get { return IoCContainer.Kernel.Get<ILanguageRepository>(); }
        }
        private static ITransactionQueueHelper TransactionQueueHelper
        {
            get { return IoCContainer.Kernel.Get<ITransactionQueueHelper>(); }
        }
        private bool IsAnonymous
        {
            get { return ChangeTracker.CurrentUser is AnonymousUser; }
        }
        /// <summary>
        /// Gets or sets the languages.
        /// </summary>
        public SyncObservableCollection<Language> Languages
        {
            get { return _languages; }
            set
            {
                _languages = value;
                OnPropertyChanged();
            }
        }




        /// <summary>
        /// Gets or sets the selected language.
        /// </summary>
        private Language _currentLanguage;
        public Language CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {
                if (value == null)
                    return;

                if (_currentLanguage == null)
                {
                    _currentLanguage = value;
                    AsyncSelectLanguage(value.ShortName);
                    OnPropertyChanged();

                }
                else if (value.ShortName != CurrentLanguage.ShortName)
                {
                    _currentLanguage = value;
                    AsyncSelectLanguage(value.ShortName);
                    OnPropertyChanged();
                }
            }

        }

        public int ProcessedUpdates
        {
            get { return _processedUpdates; }
            set
            {
                if (_processedUpdates != value)
                {
                    _processedUpdates = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _streamWebAddress = "about:blank"; //"http://192.168.0.238/";// "https://lbc.betradar.com";
        public string StreamWebAddress
        {
            get
            {
                return _streamWebAddress;
            }
            set
            {
                _streamWebAddress = value;
                OnPropertyChanged("StreamWebAddress");
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the Show Terms And Conditions command.
        /// </summary>
        public Command ShowTermsAndConditions { get; private set; }

        public Command ShowResponsibleGaming { get; private set; }

        /// <summary>
        /// Gets the ScrollDownStart command.
        /// </summary>
        public Command ScrollDownStart { get; private set; }
        /// <summary>
        /// Gets the ScrollDownStop command.
        /// </summary>
        public Command ScrollDownStop { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStart command.
        /// </summary>
        public Command ScrollUpStart { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStop command.
        /// </summary>
        public Command ScrollUpStop { get; private set; }

        public Command HideBrowserCommand { get; private set; }
        public Command ScrollToTopCommand { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {


            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += _timer_Elapsed;
            _timer.Start();

            

            base.OnNavigationCompleted();
        }

       

        private void OddPlaced(decimal odd)
        {
            if (WebBrowserVisibility != Visibility.Visible)
                return;

            if (StreamTimer == null)
                return;

            Random random = new Random();
            int randomNumber = (random.Next(ChangeTracker.VideoTimePeriodMin, ChangeTracker.VideoTimePeriodMax) - ChangeTracker.VideoWarningBefore) * 1000 + (int)odd * 1000;
            StreamTimer.Elapsed -= NotifyUserOfStreamEnding;
            StreamTimer.Elapsed -= EndStream;

            StreamTimer = new System.Timers.Timer();
            StreamTimer.Interval = randomNumber;
            StreamTimer.Elapsed += NotifyUserOfStreamEnding;
            StreamTimer.Start();
        }

        private void HideBrowser()
        {
            StopTimer();
            StreamWebAddress = "about:blank";

            WebBrowserVisibility = Visibility.Collapsed;
        }

        private void NotifyUserOfStreamEnding(object sender, ElapsedEventArgs e)
        {
            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_VIDEO_TIMES_OUT, ChangeTracker.VideoWarningBefore), null, false, ChangeTracker.VideoWarningBefore);
            StreamTimer.Stop();
            StreamTimer.Interval = ChangeTracker.VideoWarningBefore * 1000;
            StreamTimer.Elapsed -= NotifyUserOfStreamEnding;
            StreamTimer.Elapsed += EndStream;
            StreamTimer.Start();
        }

        private void EndStream(object sender, ElapsedEventArgs e)
        {
            SelectedMatch.LastPlayedStreamAt = DateTime.Now;
            HideBrowser();
        }

        void DataCopy_UpdateLanguagesEvent(int? totalUpdates)
        {
            LanguageRepository.GetAllLanguages(Languages);
            if (CurrentLanguage == null)
            {
                CurrentLanguage = Languages.FirstOrDefault(x => x.ShortName == StationRepository.DefaultDisplayLanguage);
            }
            if (CurrentLanguage != null)
            {
                AsyncSelectLanguage(CurrentLanguage.ShortName);

            }

        }

        public void UpdateProgress(int? obj)
        {
            if (obj >= 100)
            {
                if (TotalUpdates == 0)
                    TotalUpdates = obj.Value;
            }
            if (TotalUpdates > 0)
                ProcessedUpdates = TotalUpdates - obj.Value;

            if (ProcessedUpdates == TotalUpdates)
            {
                ProcessedUpdates = 0;
                TotalUpdates = 0;
            }
            ShowProgresBar = TotalUpdates > 0;
        }

        private static string GetBuildVersion()
        {


            bool bShowBuildNumber = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowBuildNumber"]);
            if (bShowBuildNumber != null && bShowBuildNumber)
            {
                if (File.Exists(_sVersionFile))
                    using (StreamReader streamReader = new StreamReader(_sVersionFile))
                    {
                        try
                        {
                            return string.Format("build {0}",
                     System.Text.RegularExpressions.Regex.Match(streamReader.ReadLine(), _sVersionRegExpFilter).Value);

                        }
                        catch (Exception)
                        {

                        }
                    }
            }
            return string.Empty;
        }

        private DateTime lastRefresh = new DateTime();
        void _timer_Elapsed(object sender, EventArgs eventArgs)
        {
            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now - lastRefresh > new TimeSpan(23, 59, 0))
            {
                lastRefresh = DateTime.Now;
                Mediator.SendMessage(true, MsgTag.Refresh);
                Mediator.SendMessage(true, MsgTag.RefreshLiveMonitor);
            }
            try
            {
                LanguageRepository.GetAllLanguages(Languages);

                if (CurrentLanguage == null)
                {
                    CurrentLanguage = Languages.FirstOrDefault(x => x.ShortName == StationRepository.DefaultDisplayLanguage.ToUpperInvariant());
                    if (CurrentLanguage != null)
                        AsyncSelectLanguage(CurrentLanguage.ShortName);
                }

                if (!Languages.Contains(CurrentLanguage) && CurrentLanguage != Languages.FirstOrDefault())
                {
                    CurrentLanguage = Languages.FirstOrDefault();
                    if (CurrentLanguage != null)
                        AsyncSelectLanguage(CurrentLanguage.ShortName);
                }

                TransactionQueueCounter = TransactionQueueHelper.GetCountTransactionQueue();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);

            }



            Datetime = DateTime.Now;
        }

        private void onShowResponsibleGaming()
        {
            Mediator.SendMessage("RG", MsgTag.ShowTermsAndConditions);
        }

        private void OnShowTermsAndConditionsExecute()
        {
            Mediator.SendMessage("", MsgTag.ShowTermsAndConditions);
        }

        /// <summary>
        /// Method to invoke when the ScrollDownStart command is executed.
        /// </summary>
        private void OnScrollDownStartExecute()
        {
            Mediator.SendMessage("", MsgTag.LoadNextPage);
            Mediator.SendMessage("", "ScrollDown");
            this._ScrollViewerModule.OnScrollDownStartExecute(this.GetScrollviewer(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollDownStop command is executed.
        /// </summary>
        private void OnScrollDownStopExecute()
        {
            this._ScrollViewerModule.OnScrollDownStopExecute();
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        private void OnScrollUpStartExecute()
        {
            Mediator.SendMessage("", MsgTag.LoadPrevPage);
            this._ScrollViewerModule.OnScrollUpStartExecute(this.GetScrollviewer(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStop command is executed.
        /// </summary>
        private void OnScrollUpStopExecute()
        {
            this._ScrollViewerModule.OnScrollUpStopExecute();
        }

        private int _totalUpdates;
        private int _processedUpdates;
        private bool _showProgresBar;


        private void Refresh(bool state)
        {
            LanguageRepository.GetAllLanguages(Languages);
        }

        object _locker = new object();
        //[AsyncMethod]
        private void AsyncSelectLanguage(string language)
        {
            lock (_locker)
            {
                PleaseWaitSelectLanguage(language);
            }
        }

        public override void Close()
        {
            if (_timer != null)
                _timer.Stop();

            base.Close();
        }

        private void HideStream(bool res)
        {
            HideBrowser();
        }

        private void ShowStream(IMatchVw match)
        {
            if (FirstStart)
            {
                //https://lbc.betradar.com/screen/#/terminal/76/4900522/656
                StreamWebAddress = "https://lbc.betradar.com/";
                WebBrowserVisibility = Visibility.Visible;
                FirstStart = false;
                return;
            }

            //add timers logic
            SelectedMatch = match;

            if (DateTime.Now < SelectedMatch.LastPlayedStreamAt.AddSeconds(ChangeTracker.VideoWarningBefore))
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_STREAM_BLOCKED).ToString());
                return;
            }

            SelectedMatch.LastPlayedStreamAt = DateTime.Now;

            WebBrowserVisibility = Visibility.Visible;

            StopTimer();

            bool res = DeleteUrlCacheEntry("https://lbc.betradar.com/screen/jwplayer.flash.swf");
            StreamWebAddress = "https://lbc.betradar.com/screen/#/terminal/76/" + SelectedMatch.LineObject.BtrMatchId.ToString() + "/" + SelectedMatch.StreamID.ToString();

            Random random = new Random();
            int randomNumber = (random.Next(ChangeTracker.VideoTimePeriodMin, ChangeTracker.VideoTimePeriodMax) - ChangeTracker.VideoWarningBefore) * 1000;

            StreamTimer = new System.Timers.Timer();
            StreamTimer.Interval = randomNumber;
            StreamTimer.Elapsed += NotifyUserOfStreamEnding;
            StreamTimer.Start();
        }

        private void StopTimer()
        {
            if (StreamTimer != null)
            {
                StreamTimer.Stop();
                StreamTimer.Elapsed -= NotifyUserOfStreamEnding;
                StreamTimer.Elapsed -= EndStream;
                StreamTimer = null;
            }
        }

        [PleaseWaitAspect]
        private void PleaseWaitSelectLanguage(string language)
        {
            Dispatcher.Invoke(
               (Action)(() =>
               {
                   Thread.CurrentThread.CurrentCulture = new CultureInfo(StationRepository.CultureInfos[language.ToUpperInvariant()]);
                   Thread.CurrentThread.CurrentUICulture = new CultureInfo(StationRepository.CultureInfos[language.ToUpperInvariant()]);
               }
           ));

            TranslationProvider.CurrentLanguage = language;

            TranslationManager.Instance.SetLanguage(CurrentLanguage.ShortName.ToUpperInvariant(), new CultureInfo(StationRepository.CultureInfos[CurrentLanguage.ShortName.ToUpperInvariant()]));

            TranslationProvider.DefaultLanguage = StationRepository.DefaultDisplayLanguage;
            DalStationSettings.Instance.Language = language;

            if (FrameworkPropertyMetadata == null)
            {
                FrameworkPropertyMetadata = new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag));
                try
                {
                    FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), FrameworkPropertyMetadata);
                }
                catch (Exception ex)
                {
                    Log.Error("", ex);
                }
            }

            Dispatcher.Invoke((Action)(() =>
            {
                if (ChangeTracker.CurrentMatch != null)
                {
                    ChangeTracker.CurrentMatch.IsStartUp = true;
                }

                if (ChangeTracker.MainWindow != null)
                    ChangeTracker.MainWindow.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

                if (IoCContainer.Kernel.Get<IChangeTracker>().UserProfileWindow != null)
                    IoCContainer.Kernel.Get<IChangeTracker>().UserProfileWindow.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

                if (ChangeTracker.LiveMonitors != null)
                    foreach (var liveMonitor in ChangeTracker.LiveMonitors)
                    {
                        LiveWindowEntry monitor = liveMonitor;
                        Dispatcher.Invoke(() =>
                            {
                                monitor.Window.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                            });
                    }
            }));
            ChangeTracker.OnPropertyChanged("AdminTitle1");
            ChangeTracker.OnPropertyChanged("AdminTitle2");
            Mediator.SendMessage(language, MsgTag.LanguageChosenHeader);
            Mediator.SendMessage(true, MsgTag.Refresh);
            Mediator.SendMessage(true, MsgTag.BasketRebindWheel);
        }

        private void SelectLanguage(string language)
        {
            language = language.ToUpperInvariant();
            if (Languages != null)
                CurrentLanguage = Languages.FirstOrDefault(x => x.ShortName == language);
        }

        //TODO something smarter to place the below message handlers

        private void AutoLogoutWaitWindow(int counter)
        {
            StationRepository.DisableCashIn();

            Dispatcher.Invoke((Action)(
                () => Mediator.SendMessage(IsAnonymous
                                    ? TranslationProvider.Translate(MultistringTags.TERMINAL_AUTOMOVEMAINPAGE_WARNING).ToString()
                                    : TranslationProvider.Translate(MultistringTags.TERMINAL_AUTOLOGOUT_WARNING).ToString()
                 , MsgTag.ShowWarning)));
        }




        #endregion
    }
}