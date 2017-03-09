using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using BaseObjects;
using BaseObjects.ViewModels;
using DefaultViews.Views;
using IocContainer;
using MainWpfWindow.ViewModels;
using MainWpfWindow.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nbt.Services.Scf.CashIn;
using Nbt.Services.Scf.CashIn.Validator;
using Nbt.Services.Spf.Printer;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using SportBetting.WPF.Prism.OldCode;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class HeaderViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void LogoutLoggedInUser()
        {
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1);
            var tickethandler = IoCContainer.Kernel.Get<ITicketHandler>();
            HeaderViewModel authorizationService = new HeaderViewModel();
            authorizationService.PlaceBetMethod(TestTipItem.CreateTipItem());
            authorizationService.PlaceBetMethod(TestTipItem.CreateTipItem());
            authorizationService.PlaceBetMethod(TestTipItem.CreateTipItem());

            WsdlRepository.Setup(x => x.OpenSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new SessionWS { session_id = "123" });

            Assert.AreEqual(1, tickethandler.Count);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "1", 0, 0, 0, 0);
            authorizationService.ClearAndOpenAnonymousSession();
            Assert.AreEqual(0, tickethandler.Count);





        }

        [TestMethod]
        public void ExceedDailyLimit()
        {
            var stationSettings = new TestStationSettings();

            var stationrepository = new StationRepository();
            stationrepository.IsReady = true;
            stationrepository.PrinterStatus = 1;
            stationrepository.CashAcceptorAlwayActive = true;
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(stationSettings).InSingletonScope();
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant(stationrepository).InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new LoggedInUser(1, "1", 0, 15, 15, 15);
            Exception error = null;
            TransactionQueueHelper.Setup(
                x =>
                x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), 5, true, ref error,
                                       CashAcceptorType.BillValidator)).Returns(new accountBalance() {amount = 5});
            HeaderViewModel headerViewModel = new HeaderViewModel();
            stationrepository.PrinterStatus = 1;
            stationSettings.RaiseAddMoney(5, false);

            TranslationProvider.Setup(x => x.Translate(MultistringTags.DAILYLIMIT, It.IsAny<decimal>(), It.IsAny<string>())).Returns("daily 10 EUR");
            ErrorWindowService.Setup(x => x.ShowError("daily 10 EUR", null, false, 5, ErrorLevel.Normal));
            stationSettings.RaiseLimitExceeded();
            Assert.AreEqual(5, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(10, stationSettings.Limit);
            ErrorWindowService.Verify(x => x.ShowError("daily 10 EUR", null, false, 5, It.IsAny<ErrorLevel>()));






        }

        [TestMethod]
        public void LogoutOperatorUser()
        {
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1);
            var tickethandler = IoCContainer.Kernel.Get<ITicketHandler>();
            HeaderViewModel authorizationService = new HeaderViewModel();
            authorizationService.PlaceBetMethod(TestTipItem.CreateTipItem());
            authorizationService.PlaceBetMethod(TestTipItem.CreateTipItem());
            authorizationService.PlaceBetMethod(TestTipItem.CreateTipItem());

            WsdlRepository.Setup(x => x.OpenSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new SessionWS { session_id = "123" });

            Assert.AreEqual(1, tickethandler.Count);
            ChangeTracker.CurrentUser = new OperatorUser("1");
            authorizationService.ClearAndOpenAnonymousSession();
            Assert.AreEqual(1, tickethandler.Count);


        }
        [TestMethod]
        public void LoginLogoutOperatorUserwhileAnonymous()
        {


            MyRegionManager myRegionManager = new MyRegionManager();
            IoCContainer.Kernel.Rebind<IDispatcher>().ToConstant<MyDispatcher>(new MyDispatcher(dispatcher)).InSingletonScope();
            IoCContainer.Kernel.Rebind<IQuestionWindowService>().To<QuestionYesMock>().InSingletonScope();
            IoCContainer.Kernel.Rebind<IMyRegionManager>().ToConstant<MyRegionManager>(myRegionManager).InSingletonScope();

            dispatcher.Invoke(() =>
            {
                var window = new MainWindow();
                new EmptyView();

                window.Show();
            });

            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1);
            var tickethandler = IoCContainer.Kernel.Get<ITicketHandler>();
            HeaderViewModel headerViewModel = new HeaderViewModel();
            headerViewModel.PlaceBetMethod(TestTipItem.CreateTipItem());
            headerViewModel.PlaceBetMethod(TestTipItem.CreateTipItem());
            headerViewModel.PlaceBetMethod(TestTipItem.CreateTipItem());
            headerViewModel.ShowResultsViewCommand.Execute("");
            Assert.AreEqual(1, tickethandler.Count);

            Assert.AreEqual((typeof(MatchResultsViewModel)),
                myRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion).GetType());

            headerViewModel.OpenAuthorizationCommand.Execute("");
            var loginviewModel =
                myRegionManager.CurrentViewModelInRegion(RegionNames.AuthContentRegion) as LoginViewModel;
            loginviewModel.UserName.Value = "123";
            loginviewModel.Password.Value = "123";
            WsdlRepository.Setup(x => x.OpenSession(It.IsAny<string>(), false, "123", "123", It.IsAny<bool>())).Returns(new SessionWS { session_id = "123", permissions = new string[] { "empty" } });

            loginviewModel.DoLoginCommand.Execute("");
            Assert.IsTrue(ChangeTracker.CurrentUser is OperatorUser);
            Assert.AreNotEqual((typeof(MatchResultsViewModel)),
                myRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion).GetType());

            headerViewModel.LogoutCommand.Execute("");

            Assert.AreEqual(1, tickethandler.Count);
            Assert.AreEqual((typeof(MatchResultsViewModel)),
    myRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion).GetType());



        }

        [TestMethod]
        public void NavigateVflLive()
        {
            MyRegionManager myRegionManager = new MyRegionManager();
            IoCContainer.Kernel.Rebind<IDispatcher>().ToConstant<MyDispatcher>(new MyDispatcher(dispatcher)).InSingletonScope();
            IoCContainer.Kernel.Rebind<IMyRegionManager>().ToConstant<MyRegionManager>(myRegionManager).InSingletonScope();

            dispatcher.Invoke(() =>
            {
                var window = new MainWindow();
                new EmptyView();

                window.Show();
            });
            ChangeTracker.Setup(x => x.SourceType).Returns(eServerSourceType.BtrVfl);
            StationRepository.Setup(x => x.AllowVfl).Returns(true);
            var firstVfl = myRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
            var firstLive = myRegionManager.NavigateUsingViewModel<LiveViewModel>(RegionNames.ContentRegion);
            var secondVfl = myRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
            var secondLive = myRegionManager.NavigateUsingViewModel<LiveViewModel>(RegionNames.ContentRegion);
            Assert.IsNotNull(firstLive);
            Assert.IsNotNull(firstVfl);
            Assert.AreEqual(firstLive, secondLive);
            Assert.AreEqual(firstVfl, secondVfl);

            var VflviewModel = myRegionManager.CurrentViewModelInRegion(RegionNames.VirtualContentRegion);
            Console.WriteLine(VflviewModel);
            Assert.IsTrue(VflviewModel is VFLViewModel);
            Assert.IsTrue(VflviewModel.IsClosed);





        }


        [TestMethod]
        public void AnonymousEnabledCashpoolBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(10m);
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 20, reserved = 0 });
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.LocationID).Returns(1);
            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            //StationRepository.Setup(x => x.SaveStationAppConfig(null)).IgnoreArguments();
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 20 });

            ChangeTracker.IsBasketOpen = true;
            var model = new HeaderViewModel();
            ChangeTracker.CurrentUser = new AnonymousUser("0", 0);
            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            Assert.AreEqual(0, TicketHandler.TicketsInBasket[0].Stake);
            model.AddMoney(null, new CashInEventArgs(10, false));


            Assert.AreEqual(20, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(10, TicketHandler.TicketsInBasket[0].Stake);


        }

        [TestMethod]
        public void NegativeCashpoolAnonymousBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);

            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 20, reserved = 0 });


            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(20m);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.LocationID).Returns(1);
            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            //StationRepository.Setup(x => x.SaveStationAppConfig(null));
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL)).Returns("money_to_cashpool");
            MessageMediator.Setup(x => x.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_CASHPOOL, null), MsgTag.ShowNotificationBar)).Returns(true);
            ChangeTracker.IsBasketOpen = true;
            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);


            var model = new HeaderViewModel();
            ChangeTracker.CurrentUser = new AnonymousUser("0", 0);
            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            TicketHandler.TicketsInBasket[0].Stake = 410;
            Assert.AreEqual(410, TicketHandler.TicketsInBasket[0].Stake);
            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(20, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(410, TicketHandler.TicketsInBasket[0].Stake);
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>())); ;
            TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL), Times.Never); ;




        }


        [TestMethod]
        public void NegativeToPositiveCashpoolAnonymousBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();




            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 40, reserved = 0 });

            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(20m);

            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(40m);

            //StationRepository.Setup(x => x.StationAppConfig).Returns(dict);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.LocationID).Returns(1);
            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            //StationRepository.Setup(x => x.SaveStationAppConfig(null));
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL)).Returns("money_to_cashpool");
            //TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("money_to_cashpool").Repeat.Once();
            MessageMediator.Setup(x => x.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_CASHPOOL, null), MsgTag.ShowNotificationBar)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN, MsgTag.HideNotificationBar)).Returns(true);
            ChangeTracker.IsBasketOpen = true;
            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);
            Exception error = null;

            var model = new HeaderViewModel();
            ChangeTracker.CurrentUser = new AnonymousUser("0", 0);
            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            TicketHandler.TicketsInBasket[0].Stake = 40;
            Assert.AreEqual(40, TicketHandler.TicketsInBasket[0].Stake);
            model.AddMoney(null, new CashInEventArgs(30, false));
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 80 });

            Assert.AreEqual(40, TicketHandler.TicketsInBasket[0].Stake);
            model.AddMoney(null, new CashInEventArgs(40, false));

            //Assert.AreEqual(90, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(80, TicketHandler.TicketsInBasket[0].Stake);

            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS));
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL));

            MessageMediator.Verify(x => x.SendMessage(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar));
            MessageMediator.Verify(x => x.SendMessage(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN, MsgTag.HideNotificationBar));



        }

        [TestMethod]
        public void NegativeToPositive5EurCashpoolAnonymousBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 145, reserved = 0 });


            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(135m);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.LocationID).Returns(1);
            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            //StationRepository.Setup(x => x.SaveStationAppConfig(null));
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL)).Returns("money_to_cashpool");
            //MessageMediator.Setup(x => x.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_CASHPOOL, null), MsgTag.ShowNotificationBar)).Returns(true).Repeat.Once();
            MessageMediator.Setup(x => x.SendMessage(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN, MsgTag.HideNotificationBar)).Returns(true);
            ChangeTracker.IsBasketOpen = true;
            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);
            var model = new HeaderViewModel();
            ChangeTracker.CurrentUser = new AnonymousUser("0", 0);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            TicketHandler.TicketsInBasket[0].Stake = 140;
            Assert.AreEqual(140, TicketHandler.TicketsInBasket[0].Stake);
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 145 });
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(145m);
            model.AddMoney(null, new CashInEventArgs(10, false));


            Assert.AreEqual(145, TicketHandler.TicketsInBasket[0].Stake);

            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS), Times.Exactly(5)); ;
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL), Times.Once); ;


            MessageMediator.Verify(x => x.SendMessage(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar), Times.Once);
            MessageMediator.Verify(x => x.SendMessage(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN, MsgTag.HideNotificationBar), Times.Once);




        }


        [TestMethod]
        public void NegativeToPositive5EurCashpoolLoggedinBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            //WsdlRepository.Setup(x => x.Deposit("1", new uid(), 10, true, CashAcceptorType.BillValidator)).Returns(new accountBalance() { amount = 135, reserved = 0 }).IgnoreArguments().Repeat.Once();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();



            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.LocationID).Returns(1);
            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_AVAILABLECASH)).Returns("money_to_cashpool");
            //MessageMediator.Setup(x => x.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_CASHPOOL, null), MsgTag.ShowNotificationBar)).Returns(true).Repeat.Once();
            MessageMediator.Setup(x => x.SendMessage(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage<long>(0, MsgTag.UpdateHistory)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage(true, MsgTag.UpdateBalance)).Returns(true);
            ChangeTracker.IsBasketOpen = true;

            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);

            var model = new HeaderViewModel();

            ChangeTracker.CurrentUser = new LoggedInUser(1, "0", 136, 0, 0, 0);
            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            TicketHandler.TicketsInBasket[0].Stake = 140;
            Assert.AreEqual(140, TicketHandler.TicketsInBasket[0].Stake);
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 141 });

            model.AddMoney(null, new CashInEventArgs(5, false));

            Assert.AreEqual(141, TicketHandler.TicketsInBasket[0].Stake);

            MessageMediator.Verify(x => x.SendMessage(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, MsgTag.HideNotificationBar)); ;
            MessageMediator.Verify(x => x.SendMessage<long>(0, MsgTag.UpdateHistory), Times.Once);
            MessageMediator.Verify(x => x.SendMessage(true, MsgTag.UpdateBalance), Times.Once);
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS), Times.Exactly(5)); ;
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_AVAILABLECASH), Times.Once); ;
            //WsdlRepository.Verify(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>()));




        }

        [TestMethod]
        public void NegativeCashpoolLogedinBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 20, reserved = 0 });
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));

            //StationRepository.Setup(x => x.StationAppConfig).Returns(dict);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.LocationID).Returns(1);
            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            //StationRepository.Setup(x => x.SaveStationAppConfig(null));
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            //TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_AVAILABLECASH)).Returns("money_to_cashpool");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>())).Returns("{0}");
            MessageMediator.Setup(x => x.SendMessage(It.IsAny<object>(), MsgTag.ShowNotificationBar)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage<long>(0, MsgTag.UpdateHistory)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage(true, MsgTag.UpdateBalance)).Returns(true);
            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 20 });
            ChangeTracker.IsBasketOpen = true;
            var model = new HeaderViewModel();
            ChangeTracker.CurrentUser = new LoggedInUser(1, "0", 10, 0, 0, 0);
            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            TicketHandler.TicketsInBasket[0].Stake = 410;
            Assert.AreEqual(410, TicketHandler.TicketsInBasket[0].Stake);
            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(20, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(410, TicketHandler.TicketsInBasket[0].Stake);

            MessageMediator.Verify(x => x.SendMessage<long>(0, MsgTag.UpdateHistory), Times.Once);
            MessageMediator.Verify(x => x.SendMessage(true, MsgTag.UpdateBalance), Times.Once);


            MessageMediator.Verify(x => x.Register(It.IsAny<IActionDetails>()), Times.Exactly(8));
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_AVAILABLECASH), Times.Once);
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>()), Times.Exactly(5));

            TransactionQueueHelper.Verify(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>()));



        }

        [TestMethod]
        public void LogoutOperatorWithSelectedBets()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            WsdlRepository.Setup(x => x.OpenSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new SessionWS() { session_id = "1234" });

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());

            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);

            var model = new HeaderViewModel();
            var mainModel = new MainViewModel();
            ChangeTracker.CurrentUser = new OperatorUser("1");
            model.LogoutCommand.Execute(null);
            model.PleaseWaitLogout();

            Assert.AreEqual(2, TicketHandler.TicketsInBasket.Count);
            WsdlRepository.Verify(x => x.OpenSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));


        }
        [TestMethod]
        public void LogoutUserWithSelectedBets()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            IoCContainer.Kernel.Rebind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Rebind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            WsdlRepository.Setup(x => x.OpenSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new SessionWS() { session_id = "333" });

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);

            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);

            HeaderViewModel model = new HeaderViewModel();
            model.OnNavigationCompleted();
            var mainModel = new MainViewModel();
            ChangeTracker.CurrentUser = new LoggedInUser(1, "1", 3m, 0, 0, 0);
            model.LogoutCommand.Execute(null);
            model.PleaseWaitLogout();
            Assert.AreEqual(0, TicketHandler.TicketsInBasket.Count);
            WsdlRepository.Verify(x => x.OpenSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));

        }

        [TestMethod]
        public void MultyAnonymousEnabledCashpoolBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 500, reserved = 0 });

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>())).Returns("{0}");
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL)).Returns("cashpool");
            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);
            MessageMediator.Setup(x => x.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_CASHPOOL, null), MsgTag.ShowNotificationBar)).Returns(true);
            var model = new HeaderViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1) { Cashpool = 500 };

            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            ChangeTracker.IsBasketOpen = true;

            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(500, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(0, TicketHandler.TicketsInBasket[0].Stake);
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>()));
            TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL), Times.Never);


        }
        [TestMethod]
        public void SingleTicketAnonymousEnabledCashpoolBasket()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            StationRepository.Setup(x => x.IsReady).Returns(true);
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 500, reserved = 0 });

            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            var model = new HeaderViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1) { Cashpool = 500 };

            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            ChangeTracker.IsBasketOpen = true;

            TicketHandler.TicketsInBasket[0].MaxBet = 510;
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 500 });


            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(500, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(10, TicketHandler.TicketsInBasket[0].Stake);


        }

        [TestMethod]
        public void AnonymousDisabledCashpool()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            StationRepository.Setup(x => x.StationNumber).Returns("1111");
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.LocationID).Returns(1);

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetBonusValueForBets(It.IsAny<Ticket>())).Returns(0);
            StationRepository.Setup(x => x.GetMinStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());

            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 10, reserved = 0 });

            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 10 });


            var model = new HeaderViewModel();

            ChangeTracker.CurrentUser = new AnonymousUser("0", 0);
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.Object.TicketsInBasket[0].Stake = 5;
            model.AddMoney(null, new CashInEventArgs(10, false));
            Assert.AreEqual(10, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(5, TicketHandler.Object.TicketsInBasket[0].Stake);

        }

        [TestMethod]
        public void LoggedIn()
        {
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 20, reserved = 0 });
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 20 });
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(1, "1", 10, 0, 0, 0));

            var model = new HeaderViewModel();
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(20, ChangeTracker.Object.CurrentUser.Cashpool);
            Assert.AreEqual(0, TicketHandler.Object.TicketsInBasket[0].Stake);


        }
        [TestMethod]
        public void MultiLoggedInShownBasket()
        {
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            //TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            WsdlRepository.Setup(x => x.Deposit(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<CashAcceptorType>(), It.IsAny<bool>())).Returns(new accountBalance() { amount = 20, reserved = 0 });
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>())).Returns("{0}");

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            ChangeTracker.CurrentUser = new LoggedInUser(1, "1", 10, 0, 0, 0);
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            ChangeTracker.IsBasketOpen = true;

            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);



            TranslationProvider.Setup(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL)).Returns("cashpool");
            MessageMediator.Setup(x => x.SendMessage(new Tuple<MultistringTag, string[]>(MultistringTags.MONEY_ADDED_TO_AVAILABLECASH, new[] { "10" }), It.IsAny<string>())).Returns(true);
            MessageMediator.Setup(x => x.SendMessage<long>(0, MsgTag.UpdateHistory)).Returns(true);
            MessageMediator.Setup(x => x.SendMessage<bool>(true, MsgTag.UpdateBalance)).Returns(true);
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 20 });

            var model = new HeaderViewModel();
            model.OnNavigationCompleted();
            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(20, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(0, TicketHandler.TicketsInBasket[0].Stake);

            //TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS, It.IsAny<object[]>()), Times.Exactly(10));

            TranslationProvider.Verify(x => x.Translate(MultistringTags.MONEY_ADDED_TO_CASHPOOL), Times.Never);
            MessageMediator.Verify(x => x.SendMessage(It.IsAny<object>(), MsgTag.ShowNotificationBar), Times.Once);
            MessageMediator.Verify(x => x.SendMessage<long>(It.IsAny<long>(), MsgTag.UpdateHistory), Times.Once);
            MessageMediator.Verify(x => x.SendMessage<bool>(true, MsgTag.UpdateBalance), Times.Once);
            MessageMediator.Verify(x => x.Register(It.IsAny<IActionDetails>()), Times.Exactly(8));


        }
        [TestMethod]
        public void SingleLoggedInShownBasket()
        {

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);

            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStake(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            var model = new HeaderViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            ChangeTracker.CurrentUser = new LoggedInUser(1, "1", 10, 0, 0, 0);
            ChangeTracker.IsBasketOpen = true;
            Exception error = null;
            TransactionQueueHelper.Setup(x => x.TryDepositMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<decimal>(), It.IsAny<bool>(), ref error, It.IsAny<CashAcceptorType?>())).Returns(new accountBalance() { amount = 20 });

            model.AddMoney(null, new CashInEventArgs(10, false));

            Assert.AreEqual(20, ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(10, TicketHandler.TicketsInBasket[0].Stake);

        }


        [TestMethod]
        public void InsertCreditNoteMethodTest()
        {
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("1", 1));

            var model = new HeaderViewModel();
            model.InsertCreditNote();

            Assert.AreEqual(BarCodeConverter.BarcodeType.TICKET, model.ChangeTracker.LoadedTicketType);
        }
        [Ignore]
        public void FillLiveSportsTest()
        {
            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            ChangeTracker.Setup(x => x.SelectedLive).Returns(true);
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>());
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.AllowFutureMatches).Returns(true);
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            var model = new HeaderViewModel();

            Repository.Setup(x => x.GetByBtrMatchId(1, true)).Returns(TestMatchLn.CreateMatch(1, true));

            //var returnValue = model.HeaderMatchFilter(TestMatchLn.CreateMatch(1, true));

            //Assert.IsTrue(returnValue);

            var matchLiveNotActive = TestMatchLn.CreateMatch(1, true);
            matchLiveNotActive.Active.Value = false;
            Repository.Setup(x => x.GetByBtrMatchId(1, true)).Returns(matchLiveNotActive);

            StationRepository.Setup(x => x.AllowFutureMatches).Returns(false);

            //returnValue = model.HeaderMatchFilter(TestMatchLn.CreateMatch(1, false));

            //Assert.IsFalse(returnValue);


        }

        [Ignore]
        public void FillPrematchSportsTest()
        {

            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            ChangeTracker.Setup(x => x.SelectedLive).Returns(false);
            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>());
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.AllowFutureMatches).Returns(false);
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            var model = new HeaderViewModel();

            Repository.Setup(x => x.GetByBtrMatchId(1, true)).Returns(TestMatchLn.CreateMatch(1, true));

            //var returnValue = model.HeaderMatchFilter(TestMatchLn.CreateMatch(1, true));

            //Assert.IsFalse(returnValue);

            var matchLiveNotActive = TestMatchLn.CreateMatch(1, true);
            matchLiveNotActive.Active.Value = false;
            Repository.Setup(x => x.GetByBtrMatchId(1, true)).Returns(matchLiveNotActive);

            StationRepository.Setup(x => x.AllowFutureMatches).Returns(false);

            //returnValue = model.HeaderMatchFilter(TestMatchLn.CreateMatch(1, false));

            //Assert.IsTrue(returnValue);


        }

        [Ignore]
        public void PreLiveMatchesOnlyFor24Hours()
        {
            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            ChangeTracker.Setup(x => x.SelectedLive).Returns(true);
            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>() { new ComboBoxItemStringId("1", "1"), new ComboBoxItemStringId("2", "2") });
            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "1"));
            StationRepository.Setup(x => x.AllowFutureMatches).Returns(true);
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            var matchLiveNotActive = TestMatchLn.CreateMatch(1, true);
            matchLiveNotActive.Active.Value = false;
            ((TestMatchVw)matchLiveNotActive.MatchView).LiveBetStatus = eMatchStatus.NotStarted;

            Repository.Setup(x => x.GetByBtrMatchId(1, true)).Returns(matchLiveNotActive);


            var model = new HeaderViewModel();

            var match = TestMatchLn.CreateMatch(1, false);
            match.StartDate.Value = new DateTimeSr(DateTime.Now.AddHours(1));
            //var result = model.HeaderMatchFilter(match);

            //Assert.IsTrue(result);



            match = TestMatchLn.CreateMatch(1, false);
            match.StartDate.Value = new DateTimeSr(DateTime.Now.AddHours(24).AddMinutes(1));
            match.ExpiryDate.Value = new DateTimeSr(DateTime.Now.AddHours(24).AddMinutes(1));
            ((TestMatchVw)match.MatchView).LiveBetStatus = eMatchStatus.NotStarted;
            //result = model.HeaderMatchFilter(match);

            //Assert.IsFalse(result);



        }



        [Ignore]
        public void FillSportsTest()
        {
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Returns("{0}");
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());

            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);
            ChangeTracker.Setup(x => x.SelectedLive).Returns(true);
            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>());
            StationRepository.Setup(x => x.TurnOffCashInInit).Returns(true);
            StationRepository.Setup(x => x.AllowFutureMatches).Returns(true);
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });


            var model = new HeaderViewModel();
            Assert.AreEqual(2, ChangeTracker.Object.SportFilters.Count);


            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1), TestMatchVw.CreateMatch(2), TestMatchVw.CreateMatch(3), TestMatchVw.CreateMatch(4) });


            LineSr.RaiseEvent();

            Assert.AreEqual(5, ChangeTracker.Object.SportFilters.Count);




        }

    }

    public class TestStationSettings : IStationSettings
    {

        public bool IsCashinOk { get { return true; } }
        public int Active { get; set; }
        public string PrefFileName { get; set; }
        public bool TurnOffCashInInit { get; set; }
        public bool UsePrinter { get; set; }
        public IPrinter Printer { get; set; }
        public int PrinterStatus { get; set; }
        public CultureInfo Culture { get; set; }
        public int SyncInterval { get; set; }
        public bool IsCashInEnabled { get; set; }
        private EventHandler<CashInEventArgs> asyncAddMoney;
        private EventHandler<ValidatorEventArgs<string>> limitExceeded;

        public void RaiseLimitExceeded()
        {
            this.limitExceeded(null, null);
        }
        public void RaiseAddMoney(decimal value, bool iscoin)
        {
            this.asyncAddMoney(null, new CashInEventArgs(value, iscoin));
        }

        public void SubscribeCashin(EventHandler<CashInEventArgs> asyncAddMoney)
        {
            this.asyncAddMoney = asyncAddMoney;
        }

        public void SubscribeLimitExceeded(EventHandler<ValidatorEventArgs<string>> limitExceeded)
        {
            this.limitExceeded = limitExceeded;
        }

        public void Init()
        {

        }

        public void ReadPrefFileData()
        {
        }

        public void AddTestMoNeyFromKeyboard(decimal money)
        {
        }
        public decimal Limit { get; set; }
        public void EnableCashIn(decimal stake, decimal limit)
        {
            Limit = limit;
        }

        public void CashInDisable()
        {
        }

        public bool IsCashDatasetValid()
        {
            return true;
        }

        public void UnSubscribeCashin(EventHandler<CashInEventArgs> depositCashInCashIn)
        {

        }

        public bool CheckBillValidator()
        {
            return true;
        }

        public bool CheckCoinAcceptor()
        {
            return true;
        }

        public List<DeviceInfo> GetDeviceInventoryList()
        {
            return new List<DeviceInfo>();
        }

        public void InitializeCashIn()
        {

        }
    }

    public class TestLineSr : ILineSr
    {
        public event DelegateDataSqlUpdateSucceeded events;
        public void SubsribeToEvent(DelegateDataSqlUpdateSucceeded dataSqlUpdateSucceeded)
        {
            events += dataSqlUpdateSucceeded;
        }

        public void UnsubscribeFromEnent(DelegateDataSqlUpdateSucceeded dataSqlUpdateSucceeded)
        {
            events -= dataSqlUpdateSucceeded;
        }

        public void VerifySelectedOdds(SortableObservableCollection<ITipItemVw> sortableObservableCollection, SyncHashSet<ITipItemVw> shsToRemove = null)
        {
            return;
        }

        public void RaiseEvent()
        {
            events(eUpdateType.LiveBet, "test");
        }

        public TournamentMatchLocksDictionary TournamentMatchLocks()
        {
            return null;
        }

        public SyncList<GroupLn> GetAllGroups()
        {
            throw new NotImplementedException();
        }

        public LiabilityLn GetAllLiabilities(string key)
        {
            throw new NotImplementedException();
        }

        public bool IsTournamentVisible(string svrId)
        {
            return true;
        }

    }
}