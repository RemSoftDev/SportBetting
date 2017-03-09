using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportRadar.Common.Collections;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.oldcode;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest
{
    [TestClass]
    public class BaseTestClass
    {

        public static IKernel Kernel
        {
            get { return IoCContainer.Kernel; }
        }
        //public static ICategoryRepository CategoryRepository;
        //public ILockOfferRepository LockOfferRepository;
        //public IMatchListRepository MatchListRepository;
        public Mock<IStationRepository> StationRepository;
        public Mock<ILineSr> MyLineSr;
        public Mock<ITranslationProvider> TranslationProvider;
        public Mock<IChangeTracker> ChangeTracker;
        public Mock<ITicketHandler> TicketHandler;
        public Mock<ITicketActions> TicketActions;
        public Mock<IPrinterHandler> PrinterHandler;
        public Mock<IWsdlRepository> WsdlRepository;
        public Mock<IRepository> Repository;
        //public ITournamentRepository TournamentRepository;
        public Mock<ILanguageRepository> LanguageRepository;
        //public ISportRepository SportRepository;
        public Mock<IWaitOverlayProvider> WaitOverlayProvider;
        //public IEntityHelper EntityHelper;
        public Mock<IMediator> MessageMediator;
        public Mock<IErrorWindowService> ErrorWindowService;
        public Mock<ISelectDate> SelectDate;
        public Mock<IMyRegionManager> MyRegionManager;
        public Mock<IBusinessPropsHelper> BusinessPropsHelper;
        public Mock<IDataBinding> DataBinding;
        public Mock<IQuestionWindowService> QuestionWindowService;
        public Mock<ILiveStreamService> LiveStreamService;
        public Mock<ILineProvider> LineProvider;
        public Mock<IConfidenceFactor> ConfidenceFactor;
        public Mock<ITransactionQueueHelper> TransactionQueueHelper;
        public Mock<ITicketSaveHandler> TicketSaveHandler;
        public Mock<INDEServer> NDEServer;
        public Mock<IStationSettings> StationSettings;
        public Dispatcher dispatcher = null;


        [TestInitialize]
        public void StartUp()
        {

            IoCContainer.Kernel = new StandardKernel();
            MyLineSr = new Mock<ILineSr>();
            StationSettings = new Mock<IStationSettings>();
            NDEServer = new Mock<INDEServer>();
            TicketSaveHandler = new Mock<ITicketSaveHandler>();
            TransactionQueueHelper = new Mock<ITransactionQueueHelper>();
            LineProvider = new Mock<ILineProvider>();
            LiveStreamService = new Mock<ILiveStreamService>();
            DataBinding = new Mock<IDataBinding>();
            ErrorWindowService = new Mock<IErrorWindowService>();
            BusinessPropsHelper = new Mock<IBusinessPropsHelper>();
            MyRegionManager = new Mock<IMyRegionManager>();
            TicketActions = new Mock<ITicketActions>();
            PrinterHandler = new Mock<IPrinterHandler>();
            TicketHandler = new Mock<ITicketHandler>();
            MessageMediator = new Mock<IMediator>();
            Repository = new Mock<IRepository>();
            StationRepository = new Mock<IStationRepository>();
            TranslationProvider = new Mock<ITranslationProvider>();
            ChangeTracker = new Mock<IChangeTracker>();
            WsdlRepository = new Mock<IWsdlRepository>();
            LanguageRepository = new Mock<ILanguageRepository>();
            WaitOverlayProvider = new Mock<IWaitOverlayProvider>();
            SelectDate = new Mock<ISelectDate>();
            QuestionWindowService = new Mock<IQuestionWindowService>();



            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>());
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            ChangeTracker.Setup(x => x.SelectedTimeFilter).Returns(new ComboBoxItem("0", 0));
            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("0", "0"));
            TranslationProvider.Setup(x => x.CurrentLanguage).Returns("EN");
            DataBinding.Setup(x => x.TipListInfo).Returns(new TipListInfo());

            ConfidenceFactor = new Mock<IConfidenceFactor>();

            Kernel.Bind<IMessageStorage>().To<MessageStorage>().InSingletonScope();
            Kernel.Bind<INDEServer>().ToConstant<INDEServer>(NDEServer.Object).InSingletonScope();
            Kernel.Bind<IStationSettings>().ToConstant<IStationSettings>(StationSettings.Object).InSingletonScope();
            Kernel.Bind<ITicketSaveHandler>().ToConstant<ITicketSaveHandler>(TicketSaveHandler.Object).InSingletonScope();
            Kernel.Bind<ITransactionQueueHelper>().ToConstant<ITransactionQueueHelper>(TransactionQueueHelper.Object).InSingletonScope();
            Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider.Object).InSingletonScope();
            Kernel.Bind<ILineSr>().ToConstant<ILineSr>(MyLineSr.Object).InSingletonScope();
            Kernel.Bind<ILiveStreamService>().ToConstant<ILiveStreamService>(LiveStreamService.Object).InSingletonScope();
            Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding.Object).InSingletonScope();
            Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper.Object).InSingletonScope();
            Kernel.Bind<IMyRegionManager>().ToConstant<IMyRegionManager>(MyRegionManager.Object).InSingletonScope();
            Kernel.Bind<ITicketActions>().ToConstant<ITicketActions>(TicketActions.Object).InSingletonScope();
            Kernel.Bind<IPrinterHandler>().ToConstant<IPrinterHandler>(PrinterHandler.Object).InSingletonScope();
            Kernel.Bind<ISelectDate>().ToConstant<ISelectDate>(SelectDate.Object).InSingletonScope();
            Kernel.Bind<IErrorWindowService>().ToConstant<IErrorWindowService>(ErrorWindowService.Object).InSingletonScope();
            Kernel.Bind<IRepository>().ToConstant<IRepository>(Repository.Object).InSingletonScope();
            Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator.Object).InSingletonScope();
            Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(ChangeTracker.Object).InSingletonScope();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository.Object).InSingletonScope();
            Kernel.Bind<IWsdlRepository>().ToConstant<IWsdlRepository>(WsdlRepository.Object).InSingletonScope();
            Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository.Object).InSingletonScope();
            Kernel.Bind<IWaitOverlayProvider>().ToConstant<IWaitOverlayProvider>(WaitOverlayProvider.Object).InSingletonScope();
            Kernel.Bind<ITranslationProvider>().ToConstant<ITranslationProvider>(TranslationProvider.Object).InSingletonScope();
            Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(TicketHandler.Object).InSingletonScope();
            Kernel.Bind<IQuestionWindowService>().ToConstant<IQuestionWindowService>(QuestionWindowService.Object).InSingletonScope();
            Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();

            StationRepository.Setup(x => x.HubSettings).Returns(new Dictionary<string, string>());
            StationRepository.Setup(x => x.StationNumber).Returns("123");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationSettings.Setup(x => x.IsCashinOk).Returns(true);
            WsdlRepository.Setup(x => x.SecureKey).Returns("123");
            WsdlRepository.Setup(x => x.OpenSession(It.IsAny<string>(), true, string.Empty, string.Empty, false))
                .Returns(new SessionWS(){session_id = "123"});
            TranslationProvider.Setup(x => x.DefaultLanguage).Returns("EN");
            var newWindowThread = new Thread(() =>
            {
                Dispatcher.Run();
            });
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
            while (dispatcher == null)
            {
                Thread.Sleep(10);
                dispatcher = Dispatcher.FromThread(newWindowThread);

            }

            var dispatchermy = new MyDispatcher(dispatcher);

            IoCContainer.Kernel.Unbind<IDispatcher>();
            IoCContainer.Kernel.Bind<IDispatcher>().ToConstant<IDispatcher>(dispatchermy).InSingletonScope();


            

        }

        [TestCleanup]
        public void TearDown()
        {
            ThreadHelper.StopAll();
            dispatcher.InvokeShutdown();
            while (!dispatcher.HasShutdownFinished)
            {
                Thread.Sleep(1);
            }
        }
    }
}
