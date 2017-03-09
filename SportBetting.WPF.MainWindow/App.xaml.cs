using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BaseObjects;
using BaseObjects.ViewModels;
using DefaultViews.Views;
using IocContainer;
using MainWpfWindow.Properties;
using MainWpfWindow.ViewModels;
using MainWpfWindow.Views;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Database;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Converters;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportBetting.WPF.Prism.Views;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.SportRadarOldLineProvider;
using SportRadar.DAL.ViewObjects;
using TimeZoneTest;
using TranslationByMarkupExtension;
using ViewModels;
using ViewModels.ViewModels;
using WsdlRepository;
using WsdlRepository.oldcode;

namespace SportBetting.WPF.Prism
{
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using System.IO;
    using System.Collections;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ILog Log = LogFactory.CreateLog(typeof(App));

        private static readonly string WorkingDirectory =
                      System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(ImagePathConverter)).Location);
        private static readonly string ImageRelativePath = ConfigurationManager.AppSettings["images_relative_path"];
        private static string _sErrorFilePath = AppDomain.CurrentDomain.BaseDirectory + "_restartLog.txt";
        private static int _iErrorFileTimeoutSeconds = Settings.Default.RestartMaxTimeoutSec;  // used to check is errors occured recently or those are old and might be removed to let system start
        private static int _iErrorFileMaximumEntries = Settings.Default.RestartCount;    // keeps number of allowed restarts of application after crash.
        public static string _sExecutablePath;
        StartWindow _startWindow;
        private Mutex mutex;
        static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        public IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }

        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }

        public readonly uint DWM_EC_DISABLECOMPOSITION = 0;
        public readonly uint DWM_EC_ENABLECOMPOSITION = 1;
        [DllImport("dwmapi.dll", EntryPoint = "DwmEnableComposition")]
        protected extern static uint Win32DwmEnableComposition(uint uCompositionAction);
        public bool ControlAero(bool enable)
        {
            try
            {
                if (enable)
                    Win32DwmEnableComposition(DWM_EC_ENABLECOMPOSITION);
                if (!enable)
                    Win32DwmEnableComposition(DWM_EC_DISABLECOMPOSITION);

                return true;
            }
            catch { return false; }
        }

        //private static readonly ILog Log = LogManager.GetLogger(typeof(App));
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            int id = 0;
            Int32.TryParse(e.Args.FirstOrDefault(), out id);
            //var id = Convert.ToInt32(e.Args.FirstOrDefault());

            Log.Debug("start terminalwindow");

            // Check how many total processes have the same name as the current one
            if (!IsSingleInstance())
            {
                Environment.Exit(1);
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ControlAero(false);


            IoCContainer.Kernel = new StandardKernel();
            IoCContainer.Kernel.Bind<IRepository>().To<Repository>().InSingletonScope();
            IoCContainer.Kernel.Bind<IMessageStorage>().To<MessageStorage>().InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().To<OldLineProvider>().InSingletonScope();
            //IoCContainer.Kernel.Bind<IWsdlRepository>().To<WcfService.WsdlRepository>().InSingletonScope();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().To<LanguageRepository>().InSingletonScope();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Bind<IDatabaseManager>().To<DbManager>().InSingletonScope();
            IoCContainer.Kernel.Bind<IWaitOverlayProvider>().To<WaitOverlayProvider>().InSingletonScope();
            IoCContainer.Kernel.Bind<ISelectDate>().To<DateHelper>().InSingletonScope();
            IoCContainer.Kernel.Bind<IErrorWindowService>().To<ErrorWindowService>().InSingletonScope();
            IoCContainer.Kernel.Bind<IQuestionWindowService>().To<QuestionWindowService>().InSingletonScope();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            IoCContainer.Kernel.Bind<ITicketActions>().To<TicketActions>().InSingletonScope();
            IoCContainer.Kernel.Bind<IPrinterHandler>().To<PrinterHandler>().InSingletonScope();
            IoCContainer.Kernel.Bind<IMyRegionManager>().To<MyRegionManager>().InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().To<BusinessPropsHelper>().InSingletonScope();
            IoCContainer.Kernel.Bind<IDataBinding>().To<DataBinding>().InSingletonScope();
            IoCContainer.Kernel.Bind<IEnterPinWindowService>().To<EnterPinWindowService>().InSingletonScope();
            IoCContainer.Kernel.Bind<ILiveStreamService>().To<LiveStreamService>().InSingletonScope();
            IoCContainer.Kernel.Bind<ILineSr>().To<SharedLineSr>().InSingletonScope();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().To<ConfidenceFactor>().InSingletonScope();
            IoCContainer.Kernel.Bind<ITransactionQueueHelper>().To<TransactionQueueHelper>().InSingletonScope();
            IoCContainer.Kernel.Bind<ITicketSaveHandler>().To<TicketSaveHandler>().InSingletonScope();
            IoCContainer.Kernel.Bind<INDEServer>().ToConstant(new NDEServer("terminal")).InSingletonScope();
            IoCContainer.Kernel.Bind<IProcessExecutor>().To<ProcessExecutor>().InSingletonScope();
            IoCContainer.Kernel.Bind<IStationSettings>().To<StationSettings>().InSingletonScope();
            if (e.Args.Contains("testmode"))
            {
                StationRepository.IsTestMode = true;
                IoCContainer.Kernel.Bind<IWsdlRepository>().To<WcfService.TestWsdlRepository>().InSingletonScope();
                IoCContainer.Kernel.Bind<ITranslationProvider>().To<TestDBTranslationProvider>().InSingletonScope();
                DbManager.Instance.DropDatabase(true);
            }
            else
            {
                IoCContainer.Kernel.Bind<IWsdlRepository>().To<WcfService.WsdlRepository>().InSingletonScope();
                IoCContainer.Kernel.Bind<ITranslationProvider>().To<DBTranslationProvider>().InSingletonScope();
            }

            if (!Debugger.IsAttached)
            {
                Process proc2 = new Process(); // clear internet explorer cache
                proc2.StartInfo.FileName = "RunDll32.exe";
                proc2.StartInfo.Arguments = "InetCpl.cpl,ClearMyTracksByProcess 255";
                proc2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc2.Start();

            }
            //StyleHelper.CreateStyleForwardersForDefaultStyles();
            var mainScreen = Screen.AllScreens.First(s => s.Primary);
            if (mainScreen.WorkingArea.Width < 1920)
            {
                ChangeTracker.Screen2WindowScale = (double)mainScreen.WorkingArea.Width / 1920.0d / 1.08d;
            }

            _startWindow = new StartWindow();
            _startWindow.Topmost = !Debugger.IsAttached;
            _startWindow.Show();

            if (StationRepository.StationNumber != "0000" && StationRepository.LayoutName == null)
                StationRepository.Refresh();
            var LayoutName = MyRegionManager.DefaultLayout;
            if (!string.IsNullOrEmpty(StationRepository.LayoutName))
            {
                LayoutName = StationRepository.LayoutName;
            }

            // TODO: Using a custom IoC container like Unity? Register it here:
            // Catel.IoC.ServiceLocator.Default.RegisterExternalContainer(MyUnityContainer);
            var foo = new Uri("pack://application:,,,/" + LayoutName + ";component/Resources/CommonStyles.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo });
            var foo2 = new Uri("pack://application:,,,/" + LayoutName + ";component/Resources/DataTemplates.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo2 });
            var foo3 = new Uri("pack://application:,,,/" + LayoutName + ";component/Resources/LiveDataTemplates.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo3 });


            try
            {
                ResolveImagePath resolveImagePath = new ResolveImagePath();
                var images = Directory.GetFiles(WorkingDirectory + ImageRelativePath + LayoutName, "*.png");
                foreach (var image in images)
                {
                    resolveImagePath.Path = Path.GetFileName(image);
                    resolveImagePath.ProvideValue(null);
                }
            }
            catch (Exception ex)
            {

                Log.Error(ex.Message, ex);
            }



            if (id != 0)
            {
                Process[] processlist = Process.GetProcesses();
                foreach (Process theprocess in processlist)
                {
                    if (theprocess.Id == id)
                    {
                        theprocess.Kill();
                        break;
                    }
                }
            }
            base.OnStartup(e);

            _startWindow.SetMessage("Initializing...");
            Log.Debug("start window show");


            // AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

            try
            {
                Log.Debug("IOC kernel start");



                var dispatcher = new MyDispatcher(Dispatcher.CurrentDispatcher);

                IoCContainer.Kernel.Bind<IDispatcher>().ToConstant<IDispatcher>(dispatcher).InSingletonScope();

                if (GetRestartErrorsNumber() >= _iErrorFileMaximumEntries)
                {
                    Thread.Sleep(2000);
                    var f = new FatalCrashWindow();
                    f.ShowDialog();
                }

                //STARTING IN TEST MODE

                Log.Debug("IOC kernel end");

                //if (!System.Diagnostics.Debugger.IsAttached)
                //{
                //    SetTimeZone(0, false);
                //}
                //SetTimeZone(0, false);


                //_startWindow.SetMessage("Clearing Database...");
                Log.Debug("clear updater files");


                // check if we still have to finish Update
                if (File.Exists(Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater1.exe"))
                {
                    try
                    {
                        File.Delete(Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater.exe");
                        File.Move(Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater1.exe", Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater.exe");
                        File.Delete(Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater.pdb");
                        File.Move(Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater1.pdb", Directory.GetCurrentDirectory().ToString() + "\\SportBetting.Updater.pdb");
                    }
                    catch (Exception)
                    {
                        //
                    }
                }

                //if (Convert.ToBoolean(ConfigurationManager.AppSettings["DoAutoUpdate"]))
                //{
                string sVersion = string.Empty;
                Updater up = new Updater();
                up.IsError = false;
                Log.Debug("updater start");
                string sProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                string sAssemblyName = "SportBetting.WPF.Prism.exe";

                if (!Debugger.IsAttached && up.DoBinariesUpdate(out sVersion, ref _autoEvent, ref _startWindow) == 1)
                //if (up.DoBinariesUpdate(out sVersion, ref _autoEvent, ref _startWindow) == 1)
                {
                    _startWindow.SetMessage("Clearing Database...");
                    Log.Debug("clear db start");

                    Log.Debug("clear db end");

                    if (_autoEvent.WaitOne(420000)) //wait download complete for 60 seconds
                    {
                        if (!up.IsError)
                        {
                            // continue if there're no errors during directories copy and extraction zip file

                            // start updater
                            Process proc = new Process();
                            proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\SportBetting.Updater.exe";
                            proc.StartInfo.Arguments = " " + sProcessName + "|" + sAssemblyName + "|" + sVersion;
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.Start();

                            Environment.Exit(0);
                        }
                        else
                        {
                            // log error while copy / extract
                            _startWindow.SetMessage("Error while copy / extract files");
                        }
                    }
                    else
                    {
                        // log timeout issue
                        // update wasn't downloaded.
                        _startWindow.SetMessage("Update downloading timed out. Will try next time.");
                    }

                }
                //}

                _startWindow.SetMessage("Starting Station. Please wait...");
                Log.Debug("init db end");
                //DbManager.Instance.DeleteOldObjects();
                if (!InitDatabase())
                    return;
                Log.Debug("show wait window");

                IoCContainer.Kernel.Get<IStationRepository>().LastPrintedObjects = string.Empty;

                var myRegionManager = IoCContainer.Kernel.Get<IMyRegionManager>();

                var waitOverlay = new WaitOverlayProvider();
                waitOverlay.ShowWaitOverlay();

                //var mainScreen = Screen.AllScreens.First(s => s.Primary);

                Log.Debug("checking mode. Monitor width = " + mainScreen.WorkingArea.Width.ToString());

                if ((ConfigurationManager.AppSettings["Is34Mode"] ?? string.Empty).Trim().ToLowerInvariant() == "true")
                {
                    ChangeTracker.Is34Mode = true;
                }
                else if ((ConfigurationManager.AppSettings["Is34Mode"] ?? string.Empty).Trim().ToLowerInvariant() == "false")
                {
                    ChangeTracker.Is34Mode = false;
                }
                else
                {
                    Log.Debug("checking mode. Is34Mode not found in config");
                    if (mainScreen.WorkingArea.Width == 1280 && mainScreen.WorkingArea.Width > mainScreen.WorkingArea.Height)
                        ChangeTracker.Is34Mode = true;
                    else
                        ChangeTracker.Is34Mode = false;
                }

                if (!ChangeTracker.Is34Mode)
                {
                    if ((ConfigurationManager.AppSettings["landscape_mode"] ?? string.Empty).Trim().ToLowerInvariant() == "true")
                    {
                        ChangeTracker.IsLandscapeMode = true;
                    }
                    else if ((ConfigurationManager.AppSettings["landscape_mode"] ?? string.Empty).Trim().ToLowerInvariant() == "false")
                    {
                        ChangeTracker.IsLandscapeMode = false;
                    }
                    else
                    {
                        Log.Debug("checking mode. landscape_mode not found in config");

                        if (mainScreen.WorkingArea.Width > mainScreen.WorkingArea.Height)
                        {
                            ChangeTracker.IsLandscapeMode = true;
                        }
                    }
                }

                Log.Debug("init main window");

                var mainWindowView = myRegionManager.FindWindowByViewModel<MainViewModel>();
                ChangeTracker.MainWindow = mainWindowView;
                MaximizeWindow(ChangeTracker.MainWindow, mainScreen);

                //if (!bool.Parse(ConfigurationManager.AppSettings["fullscreen"]))
                SetWindowSettings(ChangeTracker.MainWindow);

                Current.MainWindow = ChangeTracker.MainWindow;
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                ViewObjectBase.Dispatcher = Current.MainWindow.Dispatcher;
                Log.Debug("close start window");
                _startWindow.Close();
                Current.MainWindow.Show();
                Log.Debug("show main window");

                // close starter window




            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
        }
        private void SetWindowSettings(Window window)
        {
            if ((ConfigurationManager.AppSettings["HIDE_MAIN_WINDOW_BORDER"] ?? string.Empty).Trim().ToLowerInvariant() == "true")
            {
                window.WindowStyle = WindowStyle.None;
            }
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
        private static readonly string _sVersionFile = AppDomain.CurrentDomain.BaseDirectory
                                                + System.Configuration.ConfigurationManager.AppSettings["VersionInfoFilename"];

        private static string GetBuildVersion()
        {


            if (File.Exists(_sVersionFile))
                using (var streamReader = new StreamReader(_sVersionFile))
                {
                    try
                    {
                        return streamReader.ReadLine();
                    }
                    catch (Exception)
                    {

                    }
                }
            return "";
        }

        private bool InitDatabase()
        {
            if (DbManager.Instance.Version != GetBuildVersion())
            {
                DbManager.Instance.DeleteOldObjects();
            }
            try
            {
                //serviceLocator.RegisterType<IDatabaseManager, DbManager>();
                DbManager.Instance.EnsureDatabase(StationRepository.IsTestMode);
                if (DbManager.Instance.Version != GetBuildVersion())
                {
                    DbManager.Instance.SetVesrion(GetBuildVersion());
                }
                return true;
            }
            catch (Exception ex)
            {
                _startWindow.SetMessage("No Database service found!");
                Log.Error("No Database service found!", ex);
                Thread.Sleep(1000);
                return false;

            }

        }

        private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.StackTrace, e.Exception.Message);
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void HandleException(Exception exc)
        {
            if (exc == null) return;
            //var ex = exc.GetBaseException();
            Log.Error(exc.Message, exc);

            // TODO wpf designed ErrorMessage
            //if (!ex.Message.Contains("No main window found"))
            //{
            //    // AK: ugly workaround. later might be rewrited
            //    MessageBox.Show("Unhandled Exception \r\n" + ex.Message + "\r\n" + ex.StackTrace);
            //}
            //bool disableRestart = false;
            //Boolean.TryParse(ConfigurationManager.AppSettings["disable_auto_restart"], out disableRestart);

            //if (!Debugger.IsAttached && !disableRestart)
            //{
            //    StartNewInstance();
            //}
            //Environment.Exit(1);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            bool disableRestart = false;
            Boolean.TryParse(ConfigurationManager.AppSettings["disable_auto_restart"], out disableRestart);

            if (!Debugger.IsAttached && !disableRestart)
            {
                StartNewInstance();
            }

            //dispose the disposables
            base.OnExit(e);
            Environment.Exit(1);
        }

        #region Restarting routines

        private void StartNewInstance()
        {
            bool boolX = false;
            bool trackerCheck = false;
            try
            {
                trackerCheck = IoCContainer.Kernel.Get<IChangeTracker>().VerificationRestart;
                boolX = true;
            }
            catch (Exception)
            {
                boolX = false;
            }

            if ((boolX && !trackerCheck) || (!boolX && !trackerCheck))
            {
                LogRestartErrorMessage("Got system shutdown. Restarting");
                Process proc = new Process();
                proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\restarter.exe";
                proc.StartInfo.Arguments = "\"" + Application.ResourceAssembly.Location + "\" " + Process.GetCurrentProcess().Id;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
            }
        }

        bool IsSingleInstance()
        {
            Mutex result;
            Mutex.TryOpenExisting("SportBetting", out result);
            if (result == null)
            {
                mutex = new Mutex(true, "SportBetting");
                return true;
            }
            return false;
        }

        private static int GetRestartErrorsNumber()
        {
            ArrayList arrRestartsLog = new ArrayList();
            if (File.Exists(_sErrorFilePath))
            {
                using (StreamReader r = new StreamReader(_sErrorFilePath))
                {
                    string sLine = "";
                    while ((sLine = r.ReadLine()) != null)
                    {
                        arrRestartsLog.Add(sLine);
                    }
                }
            }
            else
            {
                return 0;
            }

            // check last error date and time
            // if it exceeds definded timeout, then remove file content

            if (arrRestartsLog.Count > 0)
            {
                string sLastEntry = arrRestartsLog[arrRestartsLog.Count - 1].ToString();
                string[] arrParts = sLastEntry.Split('|');
                DateTime datX;
                DateTime.TryParse(arrParts[0].Trim(), new CultureInfo("et-EE"), DateTimeStyles.AssumeLocal, out datX);
                if (datX.Day == 1 && datX.Month == 1 && datX.Year == 1 &&
                    datX.Hour == 0 && datX.Minute == 0 && datX.Second == 0)
                {
                    return _iErrorFileMaximumEntries + 1;
                }
                DateTime dtArray = datX;
                DateTime dtNow = DateTime.Now;

                if (arrParts[1].Contains("[WebAdmin]") || arrParts[1].Contains("[Verification]"))
                {
                    // remove file since we are just doing restart after WebAdmin request
                    File.Delete(_sErrorFilePath);
                    return -1;
                }

                if ((dtNow - dtArray).TotalSeconds > _iErrorFileTimeoutSeconds)
                {
                    // remove file since errors are quite old
                    File.Delete(_sErrorFilePath);
                    return 0;
                }
                else
                {
                    // return amount of entries
                    return arrRestartsLog.Count;
                }
            }
            else
            {
                // file exists but it is empty
                return 0;
            }
        }

        public static void LogRestartErrorMessage(string sMessage)
        {
            using (StreamWriter w = new StreamWriter(_sErrorFilePath, true))
            {
                w.WriteLine(DateTime.Now.ToString(new CultureInfo("et-EE")) + " | " + sMessage);
            }
            Log.Error("* * * SYSTEM RESTART * * *", new Exception(sMessage));
        }
        #endregion

        public static void DeleteLogRestartErrorFile()
        {
            try
            {
                File.Delete(_sErrorFilePath);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);

            }
        }

        private void SetTimeZone(int timeZone, bool refreshLive)
        {
            TimeZoneControl.ChangeTimeZone(timeZone, refreshLive);
        }
    }
}