using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Database;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.SportRadarOldLineProvider;
using WebLiveMonitorWindowApp.ViewModels;
using WebLiveMonitorWindowApp.Views;
using WsdlRepository;
using WsdlRepository.oldcode;
using Application = System.Windows.Application;

namespace WebLiveMonitorWindowApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ILog Log = LogFactory.CreateLog(typeof(App));
        public IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IoCContainer.Kernel = new StandardKernel();
            IoCContainer.Kernel.Bind<IRepository>().To<Repository>().InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();
            IoCContainer.Kernel.Bind<IMessageStorage>().To<MessageStorage>().InSingletonScope();
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

            var dispatcher = new MyDispatcher(Dispatcher);

            IoCContainer.Kernel.Bind<IDispatcher>().ToConstant<IDispatcher>(dispatcher).InSingletonScope();


            var mainScreen = Screen.AllScreens.First(s => s.Primary);

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

            int arg0 = 0;
            int.TryParse(e.Args[0], out arg0);

            bool arg1 = true;
            bool.TryParse(e.Args[1], out arg1);

            var webLive = new WebLiveMonitorWindow();
            var viewModel = new WebLiveMonitorViewModel(arg0);
            webLive.DataContext = viewModel;
            viewModel.ViewWindow = webLive;
            webLive.Show();


            MaximizeWindow(webLive, arg0, arg1);
            webLive.Show();
            webLive.WindowState = WindowState.Normal;
            webLive.WindowState = WindowState.Maximized;


        }

        public static void MaximizeWindow(Window window, int screenNumber, bool hideMainWindow)
        {
            var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();
            if (!window.IsLoaded)
                window.WindowStartupLocation = WindowStartupLocation.Manual;

            var workingArea = screens[screenNumber].WorkingArea;
            window.Left = workingArea.Left;
            window.Top = workingArea.Top;
            if (hideMainWindow)
            {
                window.WindowStyle = WindowStyle.None;
            }


        }

    }
}
