using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Windows.Threading;
using BaseObjects;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using System.Collections.ObjectModel;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;
using WsdlRepository;
using WsdlRepository.oldcode;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest
{
    [TestClass]
    public class BasketViewModelTest : BaseTestClass
    {
        [Ignore]
        [TestMethod]
        public void SpinWheelToSwitchToSingleWhenTwoOddsSelctedTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");



            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = true };

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv2);
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv2);
            ChangeTracker.Object.CurrentUser = new LoggedInUser(0, "", 10000,10,10,10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.Object.TicketsInBasket.Clear();
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv1);

            var model = new BasketViewModel();
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine3.TicketState, TicketStates.Multy);

            model.OnSpinWheel("1"); // spin down

            Assert.AreEqual(model.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Multy);


        }
        [TestMethod]
        public void ShowNotificationTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = true };

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv2);
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(0, "", 10000, 10, 10, 10));
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.Object.TicketsInBasket.Clear();
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(tiv1);

            var model = new BasketViewModel();
            model.ShowNotificationBar(MultistringTags.SERVER_ERROR);



        }

        [TestMethod]
        public void SpinWheelLinesTestWithSingleOdd()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");


            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = false };
            tiv1.IsChecked = true;
            tiv2.IsChecked = false;

            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(0, "", 10000, 10, 10, 10));
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv2);

            BasketViewModel model = new BasketViewModel();
            model.RebindWheel(true);

            Assert.AreEqual(TicketStates.Empty, model.WheelLine1.TicketState);
            Assert.AreEqual(TicketStates.Single, model.WheelLine2.TicketState);
            Assert.AreEqual(TicketStates.Empty, model.WheelLine3.TicketState);
        }

        [TestMethod]
        public void UncheckToSingleTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);



            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = true };
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket.Clear();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv2);
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Multy;

            var model = new BasketViewModel();

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(2, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(TicketStates.Multy, TicketHandler.TicketState);
            tiv1.IsChecked = false;
            model.CheckBetCommand.Execute(tiv1);

            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);





        }
        [TestMethod]
        public void UncheckToZeroTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            StationRepository.Setup(x => x.IsReady).Returns(true);



            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = false };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = false };
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket.Clear();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv2);
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Multy;
            TicketHandler.UpdateTicket();
            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(2, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);


            Assert.AreEqual(TicketStates.Empty, model.WheelLine1.TicketState);
            Assert.AreEqual(TicketStates.Empty, model.WheelLine2.TicketState);
            Assert.AreEqual(TicketStates.Empty, model.WheelLine3.TicketState);




        }

        [TestMethod]
        public void ChangeMultyToSngleStateBackToMultyTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            IoCContainer.Kernel.Unbind<IMediator>();
            Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator.Object).InSingletonScope();
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(50000);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);
            StationRepository.Setup(x => x.Currency).Returns("EUR");
            TicketHandler.TicketsInBasket.Add(new Ticket());

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket[0].Stake = 60;
            var basket = new BasketViewModel();
            basket.OnNavigationCompleted();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            Assert.AreEqual(basket.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(basket.WheelLine3.TicketState, (TicketStates)0);

            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);

            basket.ClickWheelButton.Execute("1");
            //Assert.AreEqual("{0}", basket.NotificationText);
            TranslationProvider.Verify(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>()));

            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[1].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[2].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[3].Stake);
            Assert.AreEqual(4, TicketHandler.Count);

            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine3.TicketState, TicketStates.Multy);

            basket.ClickWheelButton.Execute("3");

            Assert.AreEqual(basket.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(basket.WheelLine3.TicketState, (TicketStates)0);

            Assert.AreEqual(TicketStates.Multy, TicketHandler.TicketState);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(4, TicketHandler.Count);



        }


        [TestMethod]
        public void MultyToSngleEnoughtCashTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            IoCContainer.Kernel.Unbind<IMediator>();

            Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator.Object).InSingletonScope();
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd) { IsChecked = false });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 1000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket[0].Stake = 60;
            var basket = new BasketViewModel();
            basket.OnNavigationCompleted();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            Assert.AreEqual(basket.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(basket.WheelLine3.TicketState, (TicketStates)0);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);
            MessageMediator.Setup(x => x.SendMessage(It.IsAny<string>(), MsgTag.ShowNotificationBar)).Returns(true);

            basket.ClickWheelButton.Execute("1");
            MessageMediator.Verify(x => x.SendMessage(It.IsAny<string>(), MsgTag.ShowNotificationBar), Times.Never);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            Assert.AreEqual(0m, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[1].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[2].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[3].Stake);
            Assert.AreEqual(4, TicketHandler.Count);

            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine3.TicketState, TicketStates.Multy);



        }

        [TestMethod]
        public void SingleUncheckTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            IoCContainer.Kernel.Unbind<IMediator>();

            Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator.Object).InSingletonScope();
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd) { IsChecked = false });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 60, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket[0].Stake = 60;
            TicketHandler.UpdateTicket();
            var basket = new BasketViewModel();
            basket.OnNavigationCompleted();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            Assert.AreEqual(basket.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(basket.WheelLine3.TicketState, (TicketStates)0);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);
            MessageMediator.Setup(x => x.SendMessage(It.IsAny<string>(), MsgTag.ShowNotificationBar)).Returns(true);

            basket.ClickWheelButton.Execute("1");
            MessageMediator.Verify(x => x.SendMessage(It.IsAny<string>(), MsgTag.ShowNotificationBar), Times.Never);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            Assert.AreEqual(0m, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[1].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[2].Stake);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[3].Stake);
            Assert.AreEqual(180m, TicketHandler.Stake);
            Assert.AreEqual(4, TicketHandler.Count);

            Assert.IsTrue(!string.IsNullOrEmpty(basket.NotificationText));
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine3.TicketState, TicketStates.Multy);
            TicketHandler.TicketsInBasket[1].TipItems[0].IsChecked = false;
            basket.CheckBetCommand.Execute(TicketHandler.TicketsInBasket[1].TipItems[0]);
            TicketHandler.TicketsInBasket[2].TipItems[0].IsChecked = false;
            basket.CheckBetCommand.Execute(TicketHandler.TicketsInBasket[2].TipItems[0]);
            Console.WriteLine(basket.NotificationText);
            Assert.IsTrue(string.IsNullOrEmpty(basket.NotificationText));




        }

        [TestMethod]
        public void MultyExceedCashpool()
        {

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            IoCContainer.Kernel.Unbind<IMediator>();
            Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator.Object).InSingletonScope();
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);

            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(100);

            TicketHandler.TicketsInBasket.Add(new Ticket());
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd) { IsChecked = false });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket[0].Stake = 6;
            TicketHandler.UpdateTicket();
            var basket = new BasketViewModel();
            basket.OnNavigationCompleted();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(basket.WheelLine3.TicketState, (TicketStates)0);
            Assert.AreEqual(6m, TicketHandler.TicketsInBasket[0].Stake);
            MessageMediator.Setup(x => x.SendMessage(It.IsAny<string>(), MsgTag.ShowNotificationBar)).Returns(true);

            basket.ChangeStake.Execute("*6");
            MessageMediator.Verify(x => x.SendMessage(It.IsAny<string>(), MsgTag.ShowNotificationBar), Times.Never);
            Assert.AreEqual(10m, TicketHandler.Stake);




        }

        [TestMethod]
        public void ChangeSinglesTomultyTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(50);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1 } }));
            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 1000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket[0].Stake = 60;
            TicketHandler.TicketsInBasket[1].Stake = 60;
            TicketHandler.TicketsInBasket[2].Stake = 60;
            TicketHandler.TicketsInBasket[3].Stake = 60;
            var basket = new BasketViewModel();
            basket.OnNavigationCompleted();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            TicketHandler.TicketsInBasket[2].MaxBet = 500;
            TicketHandler.TicketsInBasket[3].MaxBet = 500;
            Assert.AreEqual(basket.WheelLine1.TicketState, (TicketStates)0);
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine3.TicketState, TicketStates.Multy);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);

            basket.ClickWheelButton.Execute("3");
            Assert.AreEqual(TicketStates.Multy, TicketHandler.TicketState);
            Assert.AreEqual(60m, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(4, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(60m, TicketHandler.Stake);
            Assert.AreEqual(4, TicketHandler.Count);
        }

        [TestMethod]
        public void MaxStakeMultyToSingleTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            IoCContainer.Kernel.Unbind<IMediator>();
            Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator.Object).InSingletonScope();
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.Currency).Returns("EUR");
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(50000);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);
            TicketHandler.TicketsInBasket.Add(new Ticket());

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            var basket = new BasketViewModel();
            basket.OnNavigationCompleted();

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }) { IsChecked = true });
            TicketHandler.TicketState = TicketStates.Multy;
            TicketHandler.TicketsInBasket[0].Stake = 5000;
            basket.DoRebindWheel(false);
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1 } }) { IsChecked = true });

            TicketHandler.UpdateTicket();

            TicketHandler.TicketsInBasket[0].MaxBet = 5000;
            Assert.AreEqual(basket.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(basket.WheelLine3.TicketState, (TicketStates)0);

            Assert.AreEqual(5000m, TicketHandler.TicketsInBasket[0].Stake);

            basket.ClickWheelButton.Execute("1");
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE, new[] { "1900", "EUR" }));
            Assert.AreEqual(500m, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(500m, TicketHandler.TicketsInBasket[1].Stake);
            Assert.AreEqual(500m, TicketHandler.TicketsInBasket[2].Stake);
            Assert.AreEqual(500m, TicketHandler.TicketsInBasket[3].Stake);
            Assert.AreEqual(4, TicketHandler.Count);

            Assert.AreEqual(basket.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(basket.WheelLine3.TicketState, TicketStates.Multy);

        }

        [TestMethod]
        public void RebindWheelSinglesTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            var changeTracker = new ChangeTracker();
            IoCContainer.Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(changeTracker).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();


            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var ticketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(ticketHandler).InSingletonScope();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(5000);
            ticketHandler.TicketsInBasket.Add(new Ticket());
            ticketHandler.TicketsInBasket.Add(new Ticket());
            ticketHandler.TicketsInBasket.Add(new Ticket());
            ticketHandler.TicketsInBasket.Add(new Ticket());


            ticketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            ticketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }));
            ticketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1 } }));
            ticketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1 } }));
            changeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            ticketHandler.TicketState = TicketStates.Single;
            ticketHandler.TicketsInBasket[0].MaxBet = 500;
            ticketHandler.TicketsInBasket[1].MaxBet = 500;
            ticketHandler.TicketsInBasket[2].MaxBet = 500;
            ticketHandler.TicketsInBasket[3].MaxBet = 500;

            var model = new BasketViewModel();
            model.OnNavigationCompleted();

            //model.RebindWheel(true);


            //Assert.AreEqual(model.WheelLine1.Text, "System 3/4");
            Assert.AreEqual(model.WheelLine1.TicketState, TicketStates.System);
            Assert.AreEqual(model.WheelLine1.SystemX, 3);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine3.TicketState, TicketStates.Multy);
        }
        [TestMethod]
        public void RebindWheel6SinglesMultyWayTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 12.3m } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.31m } }));
            TicketHandler.TicketsInBasket[4].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 10.9m } }));
            TicketHandler.TicketsInBasket[5].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.1m } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 300;
            TicketHandler.TicketsInBasket[2].MaxBet = 400;
            TicketHandler.TicketsInBasket[3].MaxBet = 200;
            TicketHandler.TicketsInBasket[4].MaxBet = 100;
            TicketHandler.TicketsInBasket[5].MaxBet = 50;

            model.RebindWheel(true);
            Assert.AreEqual(6, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);


            model.OnDeleteTipItem(new TipItemVw(testOdd));
            Assert.AreEqual(5, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);

            model.ChangeStake.Execute("max");

            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);
            Assert.AreEqual(1582.13m, TicketHandler.Stake);


            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine3.TicketState, TicketStates.Multy);
        }  
        
        [TestMethod]
        public void RebindWheel6SinglesMultyWayMultiMaxbetTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 12.3m } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.31m } }));
            TicketHandler.TicketsInBasket[4].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 10.9m } }));
            TicketHandler.TicketsInBasket[5].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.1m } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 300;
            TicketHandler.TicketsInBasket[2].MaxBet = 400;
            TicketHandler.TicketsInBasket[3].MaxBet = 200;
            TicketHandler.TicketsInBasket[4].MaxBet = 100;
            TicketHandler.TicketsInBasket[5].MaxBet = 50;

            model.RebindWheel(true);
            Assert.AreEqual(6, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);


            model.OnDeleteTipItem(new TipItemVw(testOdd));
            Assert.AreEqual(5, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);

            model.ChangeStake.Execute("max");
            Assert.AreEqual(100m, TicketHandler.Stake);
            model.ChangeStake.Execute("max");
            Assert.AreEqual(100m, TicketHandler.Stake);
            model.ChangeStake.Execute("max");

            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);
            Assert.AreEqual(100m, TicketHandler.Stake);


            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine3.TicketState, TicketStates.Multy);
        }


        [TestMethod]
        public void DeleteVFLOddTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            MyRegionManager.Setup(x => x.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion)).Returns(new EntertainmentViewModel());
            MyRegionManager.Setup(x => x.NavigateUsingViewModel<VFLViewModel>(RegionNames.ContentRegion));
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1,SourceType = eServerSourceType.BtrVfl} },
                Value = { Value = 1 }
            };

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);


            model.OnDeleteTipItem(new TipItemVw(testOdd));
            MyRegionManager.Verify(x => x.NavigateUsingViewModel<VFLViewModel>(RegionNames.VirtualContentRegion));

        }
        [TestMethod]
        public void DeleteVHCOddTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            MyRegionManager.Setup(x => x.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion)).Returns(new EntertainmentViewModel());
            MyRegionManager.Setup(x => x.NavigateUsingViewModel<VHCViewModel>(RegionNames.ContentRegion));
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1,SourceType = eServerSourceType.BtrVhc} },
                Value = { Value = 1 }
            };

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);


            model.OnDeleteTipItem(new TipItemVw(testOdd));
            MyRegionManager.Verify(x => x.NavigateUsingViewModel<VHCViewModel>(RegionNames.VirtualContentRegion));

        }

        
        [TestMethod]
        public void RebindWheel6SinglesMultyWayMultiMaxbetmoneyMorethanMaxbetTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 12.3m } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.31m } }));
            TicketHandler.TicketsInBasket[4].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 10.9m } }));
            TicketHandler.TicketsInBasket[5].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.1m } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 15000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(15000);

            var model = new BasketViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 300;
            TicketHandler.TicketsInBasket[2].MaxBet = 400;
            TicketHandler.TicketsInBasket[3].MaxBet = 200;
            TicketHandler.TicketsInBasket[4].MaxBet = 100;
            TicketHandler.TicketsInBasket[5].MaxBet = 50;

            model.RebindWheel(true);
            Assert.AreEqual(6, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);


            model.OnDeleteTipItem(new TipItemVw(testOdd));
            Assert.AreEqual(5, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);

            model.ChangeStake.Execute("max");
            Assert.AreEqual(1582.13m, TicketHandler.Stake);
            model.ChangeStake.Execute("max");
            Assert.AreEqual(1582.13m, TicketHandler.Stake);
            model.ChangeStake.Execute("max");
            Assert.AreEqual(null,model.NotificationText);
            Assert.AreEqual(null,ChangeTracker.LastNotificationTag);
            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);
            Assert.AreEqual(1582.13m, TicketHandler.Stake);


            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine3.TicketState, TicketStates.Multy);
        }
        
        [TestMethod]
        public void RebindWheel6SinglesMaxbetAfterStakeTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 12.3m } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.31m } }));
            TicketHandler.TicketsInBasket[4].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 10.9m } }));
            TicketHandler.TicketsInBasket[5].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.1m } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 300;
            TicketHandler.TicketsInBasket[2].MaxBet = 400;
            TicketHandler.TicketsInBasket[3].MaxBet = 200;
            TicketHandler.TicketsInBasket[4].MaxBet = 100;
            TicketHandler.TicketsInBasket[5].MaxBet = 50;

            model.RebindWheel(true);
            Assert.AreEqual(6, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);


            model.OnDeleteTipItem(new TipItemVw(testOdd));
            Assert.AreEqual(5, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);

            model.ChangeStake.Execute("*9");
            Assert.AreEqual(45m, TicketHandler.Stake);
            ChangeTracker.CurrentUser.Cashpool = 10m;
            model.ChangeStake.Execute("max");
            Assert.AreEqual(10m, TicketHandler.Stake);
            model.ChangeStake.Execute("max");
            Assert.AreEqual(10m, TicketHandler.Stake);
            model.ChangeStake.Execute("max");

            Assert.AreEqual(1582.13m, TicketHandler.MaxBet);
            Assert.AreEqual(10m, TicketHandler.Stake);


            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine3.TicketState, TicketStates.Multy);
        }

        [TestMethod]
        public void RebindWheelMultyWayTest()
        {

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetAllowMultiBet(It.IsAny<Ticket>())).Returns(true);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 }
            };
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(testOdd));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 2 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 3 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OddId = { Value = 4 }, BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1 } }));
            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            model.RebindWheel(true);
            Assert.AreEqual(model.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(model.WheelLine3.TicketState, (TicketStates)0);
        }

        [TestMethod]
        public void stake5and9ExceedCashpoolTest()
        {
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            var model = new BasketViewModel();
            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].IsEditingStake = true;
            model.ChangeStake.Execute("5");
            TicketHandler.TicketsInBasket[0].IsEditingStake = false;

            TicketHandler.TicketsInBasket[1].IsEditingStake = true;
            model.ChangeStake.Execute("9");
            TicketHandler.TicketsInBasket[1].IsEditingStake = false;

            Assert.AreEqual(MultistringTags.NOT_ENOUGHT_MONEY, ChangeTracker.LastNotificationTag);
            //Assert.AreEqual("4", model.NotificationText);
            Assert.AreEqual(TicketHandler.Stake, 10m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 5M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 5M);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            //Assert.AreEqual("4", model.NotificationText);
            Assert.AreEqual(ChangeTracker.CurrentUser.Cashpool, TicketHandler.Stake);


        }

        [TestMethod]
        public void stakeexceedCashpoolBasketOpenTest()
        {
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.Currency).Returns("EUR");
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 7M;
            TicketHandler.TicketsInBasket[1].Stake = 7M;
            TicketHandler.UpdateTicket();


            var model = new BasketViewModel();
            model.OnNavigationCompleted();


            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(TicketHandler.Stake, 14m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 7M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 7M);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            TranslationProvider.Verify(x => x.Translate(It.IsAny<MultistringTag>(), new[] { "4", "EUR" }), Times.AtLeast(2));


        }

        [TestMethod]
        public void stakeexceedCashpoolBasketOpenChangeSingleStakeTest()
        {





            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.Currency).Returns("EUR");
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 7M;
            TicketHandler.TicketsInBasket[1].Stake = 7M;
            TicketHandler.UpdateTicket();


            var model = new BasketViewModel();
            model.OnNavigationCompleted();

            model.ChangeStakeSingle.Execute(TicketHandler.TicketsInBasket[0]);

            model.ChangeStake.Execute("*5");
            Assert.AreEqual(MultistringTags.NOT_ENOUGHT_MONEY.Value, ChangeTracker.LastNotificationTag.Value);

            Thread.Sleep(31000);

            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE.Value, ChangeTracker.LastNotificationTag.Value);
            Assert.AreEqual(TicketHandler.Stake, 14m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 7M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 7M);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            TranslationProvider.Verify(x => x.Translate(It.IsAny<MultistringTag>(), new[] { "4", "EUR" }), Times.AtLeast(2));


        }

        [TestMethod]
        public void stakeexceedCashpoolBasketOpenChangeSingleStakeBackTest()
        {
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.Currency).Returns("EUR");
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 10M;
            TicketHandler.TicketsInBasket[1].Stake = 10M;


            var model = new BasketViewModel();
            model.OnNavigationCompleted();

            model.ChangeStakeSingle.Execute(TicketHandler.TicketsInBasket[0]);

            model.ChangeStake.Execute("back");

            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(TicketHandler.Stake, 11m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 10M);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            TranslationProvider.Verify(x => x.Translate(It.IsAny<MultistringTag>(), new[] { "1", "EUR" }), Times.Exactly(2));


        }


        [TestMethod]
        public void NotEnoughtMoneyAnonymousEnabledTest()
        {
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN)).Returns("login");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            //TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1234", 1234);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 0M;


            var model = new BasketViewModel();
            model.OnNavigationCompleted();


            model.ChangeStake.Execute("+10");

            Assert.AreEqual(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN, ChangeTracker.LastNotificationTag);
            Assert.AreEqual("login", model.NotificationText);
            Assert.AreEqual(0m, TicketHandler.Stake);
            Assert.AreEqual(0m, TicketHandler.TicketsInBasket[0].Stake);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(null, model.NotificationText);


        }
        [TestMethod]
        public void NotEnoughtMoneyCashpoolHaveSomeMoneyAnonymousEnabledTest()
        {
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN)).Returns("login");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.NOT_ENOUGHT_MONEY)).Returns("money");

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            //TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1234", 1234);
            ChangeTracker.CurrentUser.Cashpool = 1m;

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 1M;
            TicketHandler.UpdateTicket();


            var model = new BasketViewModel();
            model.OnNavigationCompleted();


            model.ChangeStake.Execute("+10");

            Assert.AreEqual(MultistringTags.NOT_ENOUGHT_MONEY, ChangeTracker.LastNotificationTag);
            Assert.AreEqual("money", model.NotificationText);
            Assert.AreEqual(1m, TicketHandler.Stake);
            Assert.AreEqual(1m, TicketHandler.TicketsInBasket[0].Stake);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(null, model.NotificationText);


        }

        [TestMethod]
        public void NoMoneyTestIsCashier()
        {
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN)).Returns("login");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.NOT_ENOUGHT_MONEY)).Returns("money");

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            //TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsCashier).Returns(true);
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1234", 1234);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 1M;
            TicketHandler.UpdateTicket();


            var model = new BasketViewModel();
            model.OnNavigationCompleted();


            model.ChangeStake.Execute("*2");
            model.ChangeStake.Execute("*3");

            Assert.AreEqual(null, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(-123m, ChangeTracker.CurrentUser.AvailableCash);
            Assert.AreEqual(null, model.NotificationText);
            Assert.AreEqual(123m, TicketHandler.Stake);
            Assert.AreEqual(123m, TicketHandler.TicketsInBasket[0].Stake);
            model.Close();


        }

        [TestMethod]
        public void NotEnoughtMoneyAnonymousDisabledTest()
        {
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN)).Returns("login");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            //TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(false);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1234", 1234);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 0M;


            var model = new BasketViewModel();
            model.OnNavigationCompleted();


            model.ChangeStake.Execute("+10");

            Assert.AreEqual(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN, ChangeTracker.LastNotificationTag);
            Assert.AreEqual("login", model.NotificationText);
            Assert.AreEqual(0m, TicketHandler.Stake);
            Assert.AreEqual(0m, TicketHandler.TicketsInBasket[0].Stake);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(null, model.NotificationText);


        }

        [TestMethod]
        public void stakeexceedCashpoolBasketOpenChangeSingleStakeMaxTest()
        {
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>(), It.IsAny<object[]>())).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.Currency).Returns("EUR");
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 10M;
            TicketHandler.TicketsInBasket[1].Stake = 10M;


            var model = new BasketViewModel();
            model.OnNavigationCompleted();

            model.ChangeStakeSingle.Execute(TicketHandler.TicketsInBasket[0]);

            model.ChangeStake.Execute("max");

            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(TicketHandler.Stake, 20m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 10M);
            model.Close();
            model = new BasketViewModel();
            model.OnNavigationCompleted();
            TranslationProvider.Verify(x => x.Translate(It.IsAny<MultistringTag>(), new[] { "10", "EUR" }), Times.AtLeast(2));


        }


        [TestMethod]
        public void DoNoChangeNotificationTest()
        {




            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE, It.IsAny<object[]>())).Returns("add money{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.ACCOUNT_BLOCKED)).Returns("blocked");
            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.Currency).Returns("EUR");

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);

            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 7M;
            TicketHandler.TicketsInBasket[1].Stake = 7M;
            TicketHandler.UpdateTicket();


            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(14, TicketHandler.Stake);

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            model.ChangeStake.Execute("14");

            Assert.AreEqual(14, TicketHandler.Stake);

            Assert.AreEqual(MultistringTags.NOT_ENOUGHT_MONEY.Value, ChangeTracker.LastNotificationTag.Value);
            //Assert.AreEqual("add money4", model.NotificationText);

            model.ShowNotificationBar(MultistringTags.ACCOUNT_BLOCKED);
            Assert.AreEqual(MultistringTags.ACCOUNT_BLOCKED, ChangeTracker.LastNotificationTag);
            Assert.AreEqual("blocked", model.NotificationText);
            Thread.Sleep(7000);

            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE.Value, ChangeTracker.LastNotificationTag.Value);
            //Assert.AreEqual("add money4", model.NotificationText);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE, new[] { "4", "EUR" }));



            model.ShowNotificationBar(MultistringTags.ADD_XX_TO_STAKE, new[] { "100", "EUR" });
            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE, new[] { "100", "EUR" }));
            //Assert.AreEqual("add money100", model.NotificationText);

            model.ChangeStake.Execute("+9");
            model.ChangeStake.Execute("+9");
            model.ChangeStake.Execute("+9");
            model.ChangeStake.Execute("+9");
            model.ChangeStake.Execute("+9");

            Assert.AreEqual(MultistringTags.NOT_ENOUGHT_MONEY.Value, ChangeTracker.LastNotificationTag.Value);
            //Assert.AreEqual("add money4", model.NotificationText);



            model.ChangeStake.Execute("+9");
            Thread.Sleep(32000);
            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE, new[] { "4", "EUR" }));
            //Assert.AreEqual("add money4", model.NotificationText);



        }


        [TestMethod]
        public void MaxSingleStakesTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TipItemVw tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[1].MaxBet = 50;
            TicketHandler.TicketsInBasket[1].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[2].MaxBet = 50;
            TicketHandler.TicketsInBasket[2].TipItems.Add(tiv1);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("{0}");
            var model = new BasketViewModel();
            Assert.AreEqual(null, ChangeTracker.LastNotificationTag);

            model.ChangeStake.Execute("max");
            TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE), Times.Never);

            Assert.AreEqual(null, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(TicketHandler.Stake, 9.99m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].MaxBet, 50m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 3.330m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].MaxBet, 50m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 3.33m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[2].MaxBet, 50m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[2].Stake, 3.33m);




        }


        [TestMethod]
        public void BuyTicketTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            TicketHandler.TicketsInBasket.Add(new Ticket());
            int i = 1;
            TicketWS newticket = new TicketWS();
            Ticket ticket = new Ticket();
            User user = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            TicketActions.Setup(x => x.CreateNewTicketWS(It.IsAny<Ticket>())).Returns(newticket);

            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketActions.Setup(x => x.SaveTicket(out i, It.IsAny<TicketWS>(), It.IsAny<User>())).Returns(newticket);
            TicketActions.Setup(x => x.PrintTicket(newticket, It.IsAny<bool>())).Returns(true);
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);



            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 2.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.3M } }));


            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 50;

            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            var model = new BasketViewModel();
            model.PleaseWaitSaveTicket();
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 0m);
            Assert.AreEqual(TicketHandler.Stake, 0m);
            Assert.AreEqual(TicketHandler.Count, 0m);
            TicketActions.Verify(x => x.PrintTicket(newticket, It.IsAny<bool>()), Times.Once); ;
            TicketActions.Verify(x => x.SaveTicket(out i, newticket,  It.IsAny<User>()), Times.Once());



        }

       


        [TestMethod]
        public void BuyTicketLockedMatchesTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            int i;
            TicketWS newticket = new TicketWS();
            Ticket ticket = new Ticket();
            User user = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketActions.Setup(x => x.CreateNewTicketWS(It.IsAny<Ticket>())).Returns(newticket);
            TicketActions.Setup(x => x.SaveTicket(out i, It.IsAny<TicketWS>(), It.IsAny<User>())).Throws(new System.ServiceModel.FaultException<HubServiceException>(new HubServiceException() { code = 1000 }));
            TicketActions.Setup(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>())).Returns(true);
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);



            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = TestMatchLn.CreateMatch(1, false) }, Value = { Value = 2.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = TestMatchLn.CreateMatch(2, false) }, Value = { Value = 2.3M } }));

            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            StationRepository.Setup(x => x.Active).Returns(1);
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 50;

            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            var model = new BasketViewModel();
            model.PlaceBetCommand.Execute(null);
            Thread.Sleep(10000);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 50m);
            Assert.AreEqual(TicketHandler.Count, 2);


            TicketActions.Verify(x => x.SaveTicket(out i, It.IsAny<TicketWS>(),  It.IsAny<User>()), Times.Once);
            TicketActions.Verify(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>()), Times.Never);



        }

        [TestMethod]
        public void BuyManyTicketsTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            var ticket = new Ticket();
            ticket.Stake = 1;
            ticket.MinBet = 1;
            ticket.MaxBet = 10;
            ticket.TipItems.Add(
                    new TipItemVw(new TestOdd()
                        {
                            OutcomeId = 1,
                            OddId =
                                {
                                    Value = 1
                                },
                            BetDomain = new TestBetDomain()
                            {
                                BetDomainId = 1,
                                Match = new TestMatchLn()
                                {
                                    MatchId = 1,
                                    MatchView = new TestMatchVw()
                                }
                            },
                            Value =
                            {
                                Value = 2.3M
                            }
                        }));
            TicketHandler.TicketsInBasket.Add(ticket);
            TicketHandler.TicketsInBasket.Add(ticket.Clone());
            TicketHandler.TicketsInBasket.Add(ticket.Clone());
            TicketHandler.TicketsInBasket.Add(ticket.Clone());
            TicketHandler.TicketsInBasket.Add(ticket.Clone());
            TicketHandler.TicketsInBasket.Add(ticket.Clone());
            TicketHandler.TicketsInBasket.Add(ticket.Clone());

            int i = 1;
            TicketWS newticket = new TicketWS();
            User user = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);


            TicketActions.Setup(x => x.CreateNewTicketWS(It.IsAny<Ticket>())).Returns(newticket);
            TicketActions.Setup(x => x.SaveTicket(out i, newticket, It.IsAny<User>())).Returns(newticket);
            TicketActions.Setup(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>())).Returns(true);
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);




            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            StationRepository.Setup(x => x.Active).Returns(1);

            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            var model = new BasketViewModel();
            Thread.Sleep(10000);
            model.PlaceBetCommand.Execute("");
            Thread.Sleep(10000);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 0);
            Assert.AreEqual(TicketHandler.Stake, 0m);
            Assert.AreEqual(TicketHandler.Count, 0);
            var timespan = DateTime.Now - ChangeTracker.MouseClickLastTime;
            Assert.IsTrue(timespan < new TimeSpan(0, 0, 10, 1));

            TicketActions.Verify(x => x.SaveTicket(out i, newticket,  It.IsAny<User>()), Times.Exactly(7));
            TicketActions.Verify(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>()), Times.Exactly(7));

        }


        [TestMethod]
        public void BuyUncheckedTicketTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var TicketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(TicketHandler).InSingletonScope();


            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            int i = 1;
            TicketWS newticket = new TicketWS();
            Ticket ticket = new Ticket();
            User user = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            TicketActions.Setup(x => x.CreateNewTicketWS(It.IsAny<Ticket>())).Returns(newticket);
            TicketActions.Setup(x => x.SaveTicket(out i, It.IsAny<TicketWS>(), It.IsAny<User>())).Returns(newticket);
            TicketActions.Setup(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>())).Returns(true);
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            var tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };
            var tiv2 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = false };
            var tiv3 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = false };
            var tiv4 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            StationRepository.Setup(x => x.PrinterStatus).Returns(1);

            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 5;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);

            TicketHandler.TicketsInBasket[1].MaxBet = 50;
            TicketHandler.TicketsInBasket[1].Stake = 5;
            TicketHandler.TicketsInBasket[1].TipItems.Add(tiv2);

            TicketHandler.TicketsInBasket[2].MaxBet = 50;
            TicketHandler.TicketsInBasket[2].Stake = 5;
            TicketHandler.TicketsInBasket[2].TipItems.Add(tiv3);

            TicketHandler.TicketsInBasket[3].MaxBet = 50;
            TicketHandler.TicketsInBasket[3].Stake = 5;
            TicketHandler.TicketsInBasket[3].TipItems.Add(tiv4);

            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            var model = new BasketViewModel();

            model.PleaseWaitSaveTicket();
            Thread.Sleep(10000);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 0m);
            Assert.AreEqual(TicketHandler.Stake, 0m);
            Assert.AreEqual(TicketHandler.Count, 0m);
            TicketActions.Verify(x => x.SaveTicket(out i, It.IsAny<TicketWS>(), It.IsAny<User>()), Times.Exactly(2));
            TicketActions.Verify(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>()), Times.Exactly(2));




        }

        [TestMethod]
        public void BuyUncheckedTicketExceptionTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var TicketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(TicketHandler).InSingletonScope();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            int i = 1;
            TicketWS newticket = new TicketWS(){ticketNbr = "2"};
            TicketWS newticket2 = new TicketWS(){ticketNbr = "1"};

            TicketActions.Setup(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>())).Returns(true);
            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);


            var tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };
            var tiv2 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = false };
            var tiv3 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = false };
            var tiv4 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            StationRepository.Setup(x => x.PrinterStatus).Returns(1);

            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 1;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            var ticket1 = TicketHandler.TicketsInBasket[0];
            TicketHandler.TicketsInBasket[1].MaxBet = 50;
            TicketHandler.TicketsInBasket[1].Stake = 5;
            TicketHandler.TicketsInBasket[1].TipItems.Add(tiv2);

            TicketHandler.TicketsInBasket[2].MaxBet = 50;
            TicketHandler.TicketsInBasket[2].Stake = 5;
            TicketHandler.TicketsInBasket[2].TipItems.Add(tiv3);

            TicketHandler.TicketsInBasket[3].MaxBet = 50;
            TicketHandler.TicketsInBasket[3].Stake = 2;
            TicketHandler.TicketsInBasket[3].TipItems.Add(tiv4);
            var ticket2 = TicketHandler.TicketsInBasket[3];

            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            var model = new BasketViewModel();

            TicketActions.Setup(x => x.CreateNewTicketWS(ticket1)).Returns(newticket);
            TicketActions.Setup(x => x.CreateNewTicketWS(ticket2)).Returns(newticket2);
            TicketActions.Setup(x => x.SaveTicket(out i, newticket, It.IsAny<User>())).Returns(newticket);


            TicketActions.Setup(x => x.SaveTicket(out i, newticket2, It.IsAny<User>())).Throws(new System.ServiceModel.FaultException<HubServiceException>(new HubServiceException()));



            model.PleaseWaitSaveTicket();
            //Thread.Sleep(10000);

            TicketActions.Verify(x => x.SaveTicket(out i, newticket,  It.IsAny<User>()), Times.Exactly(1));
            TicketActions.Verify(x => x.SaveTicket(out i, newticket2,  It.IsAny<User>()), Times.Exactly(1));
            TicketActions.Verify(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>()), Times.Exactly(1));

            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 2m);
            Assert.AreEqual(TicketHandler.Count, 1);




        }



        [TestMethod]
        public void SinglesFirtsUncheckedTest()
        {
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            int i = 1;
            TicketWS newticket = new TicketWS();
            Ticket ticket = new Ticket();
            User user = new LoggedInUser(0, "", 150.00m, 10, 10, 10);

            TicketActions.Setup(x => x.SaveTicket(out i, It.IsAny<TicketWS>(), It.IsAny<User>())).Returns(newticket);
            TicketActions.Setup(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>())).Returns(true);
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            var model = new BasketViewModel();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());

            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = false };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv3 = new TipItemVw(TestOdd.CreateOdd(3, 2.3m, true)) { IsChecked = true };

            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[1].TipItems.Add(tiv2);
            TicketHandler.TicketsInBasket[1].Stake = 10;
            TicketHandler.TicketsInBasket[1].MaxBet = 100;
            TicketHandler.TicketsInBasket[1].MaxWin = 100;
            TicketHandler.TicketsInBasket[2].TipItems.Add(tiv3);
            TicketHandler.TicketsInBasket[2].Stake = 10;
            TicketHandler.TicketsInBasket[2].MaxBet = 100;
            TicketHandler.TicketsInBasket[2].MaxWin = 100;
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150m);
            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(0, "", 150.00m, 10, 10, 10));
            TicketActions.Setup(x => x.CreateNewTicketWS(It.IsAny<Ticket>())).Returns(newticket);
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);

            model.PleaseWaitSaveTicket();
            Thread.Sleep(10000);

            TicketActions.Verify(x => x.SaveTicket(out i, It.IsAny<TicketWS>(),  It.IsAny<User>()), Times.Exactly(2));
            TicketActions.Verify(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>()), Times.Exactly(2));



        }


        [TestMethod]
        public void NotEnoughtMoneyTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            int i = 1;
            TicketWS newticket = new TicketWS();
            TicketActions.Setup(x => x.SaveTicket(out i, It.IsAny<TicketWS>(), It.IsAny<User>())).Returns(newticket);
            TicketActions.Setup(x => x.PrintTicket(It.IsAny<TicketWS>(), It.IsAny<bool>())).Returns(true);
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(10);
            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });


            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 50;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketState = TicketStates.Multy;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.00m, 10, 10, 10);
            var model = new BasketViewModel();

            var returnValue = model.PleaseWaitSaveTicket();
            Thread.Sleep(10000);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void BelowMinStakeTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            TranslationProvider.Setup(x => x.Translate(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE)).Returns("below min stake");
            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0.50m;
            TicketHandler.UpdateTicket();
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 150.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            var model = new BasketViewModel();

            var returnvalue = model.PleaseWaitSaveTicket();
            Thread.Sleep(10000);
            Assert.IsFalse(returnvalue);
            Assert.AreEqual(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 0.5m);




        }

        [TestMethod]
        public void SystemBankerTest()
        {
            IoCContainer.Kernel.Unbind<IStationRepository>();
            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<StationRepository>(stationRepository).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            ChangeTracker.CurrentUser = new LoggedInUser(0, "1", 1000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            stationRepository.PrinterStatus = 1;
            stationRepository.MaxSystemBet = 15;
            stationRepository.MaxCombination = 10;
            stationRepository.MaxWinSystemBet = 1000;
            stationRepository.MaxStakeSystemBet = 100;
            stationRepository.MinStakeSystemBet = 1;
            stationRepository.MaxOdd = 1000;
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0.50m;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.85M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.85M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.TicketState = TicketStates.System;

            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(TicketStates.System, TicketHandler.TicketState);

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(7, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(" 2/3  + 1  + 2 ", model.SystemBanker);


            TicketHandler.TicketsInBasket[0].TipItems[1].IsChecked = false;
            model.CheckBetCommand.Execute(TicketHandler.TicketsInBasket[0].TipItems[1]);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[1].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[2].IsBank);

            Assert.AreEqual(" 2/4  + 2 ", model.SystemBanker);





        }

        [TestMethod]
        public void DeleteOdFromWayBankerFromBasketTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IStationRepository>();
            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<StationRepository>(stationRepository).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            ChangeTracker.CurrentUser = new LoggedInUser(0, "1", 1000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            stationRepository.PrinterStatus = 1;
            stationRepository.MaxSystemBet = 15;
            stationRepository.MaxCombination = 10;
            stationRepository.MaxWinSystemBet = 1000;
            stationRepository.MaxStakeSystemBet = 100;
            stationRepository.MinStakeSystemBet = 1;
            stationRepository.MaxOdd = 1000;
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0.50m;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.85M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.85M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.TicketState = TicketStates.System;

            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(TicketStates.System, TicketHandler.TicketState);

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(7, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(" 2/3  + 1  + 2 ", model.SystemBanker);


            TicketHandler.TicketsInBasket[0].TipItems[1].IsChecked = false;
            model.OnDeleteBetCommand.Execute(TicketHandler.TicketsInBasket[0].TipItems[1]);
            Assert.AreEqual(6, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[0].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[1].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[2].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[3].IsBank);
            Assert.AreEqual(true, TicketHandler.TicketsInBasket[0].TipItems[4].IsBank);
            Assert.AreEqual(true, TicketHandler.TicketsInBasket[0].TipItems[5].IsBank);

            Assert.AreEqual(" 2/4  + 2 ", model.SystemBanker);

        }
        [TestMethod]
        public void DeleteOdFromWayBankerFromMainViewTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IStationRepository>();
            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<StationRepository>(stationRepository).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            ChangeTracker.CurrentUser = new LoggedInUser(0, "1", 1000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            stationRepository.IsReady = true;
            stationRepository.PrinterStatus = 1;
            stationRepository.MaxSystemBet = 15;
            stationRepository.MaxCombination = 10;
            stationRepository.MaxWinSystemBet = 1000;
            stationRepository.MaxStakeSystemBet = 100;
            stationRepository.MinStakeSystemBet = 1;
            stationRepository.MaxOdd = 1000;
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0.50m;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.85M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.85M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.TicketState = TicketStates.System;

            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(TicketStates.System, TicketHandler.TicketState);

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(7, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(" 2/3  + 1  + 2 ", model.SystemBanker);

            //TicketHandler.TicketsInBasket[0].TipItems[1].IsChecked = false;
            model.OnDeleteTipItem(TicketHandler.TicketsInBasket[0].TipItems[1]);
            //model = new BasketViewModel();
            Assert.AreEqual(TicketStates.System, TicketHandler.TicketState);

            Assert.AreEqual(6, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[0].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[1].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[2].IsBank);
            Assert.AreEqual(false, TicketHandler.TicketsInBasket[0].TipItems[3].IsBank);
            Assert.AreEqual(true, TicketHandler.TicketsInBasket[0].TipItems[4].IsBank);
            Assert.AreEqual(true, TicketHandler.TicketsInBasket[0].TipItems[5].IsBank);

            Assert.AreEqual(" 2/4  + 2 ", model.SystemBanker);

        }






        [TestMethod]
        public void MinusAvailableCashTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            WsdlRepository.Setup(x => x.LoadProfile(It.IsAny<uid>())).Returns(new profileForm() { fields = new formField[0] });

            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_MONEY)).Returns("money");
            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 50m;
            TicketHandler.UpdateTicket();
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            ErrorWindowService.Setup(x => x.ShowError("money", null, false, 0, ErrorLevel.Critical));
            var model = new BasketViewModel();

            var returnvalue = model.PleaseWaitSaveTicket();
            Thread.Sleep(10000);
            Assert.IsFalse(returnvalue);
            //Assert.AreEqual("40.00", model.NotificationText);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 50m);
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)); ;
            //TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_MONEY)); ;

            //ErrorWindowService.Verify(x => x.ShowError("money", null, false, 0, ErrorLevel.Critical));



        }

        [TestMethod]
        public void ClearMinusAvailableCashTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);

            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("{0}");
            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0m;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            var model = new BasketViewModel();
            model.ChangeStake.Execute("+50");
            //var returnvalue = model.PleaseWaitSaveTicket();
            // Assert.IsFalse(returnvalue);
            //            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            // Assert.AreEqual("40.00", model.NotificationText);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 10m);

            model.ChangeStake.Execute("clear");
            Assert.AreEqual(0m, TicketHandler.Stake);
            Assert.AreEqual(null, ChangeTracker.LastNotificationTag);

            Assert.AreEqual(null, model.NotificationText);
            //            TranslationProvider.Verify(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE), Times.Exactly(2));

        }

        [TestMethod]
        public void ClearMinusAvailableCashSinglesToMultyTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Unbind<IDataBinding>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(50);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(50);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.IsReady).Returns(true);


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));

            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("{0}");
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 10m;

            TicketHandler.TicketState = TicketStates.Multy;
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.00m, 10, 10, 10);
            TicketHandler.UpdateTicket();
            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(model.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Multy);

            model.OnSpinWheel("0");
            Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, ChangeTracker.LastNotificationTag);
            //Assert.AreEqual("10.00", model.NotificationText);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 2);
            Assert.AreEqual(TicketHandler.Stake, 20m);
            Assert.AreEqual(TicketHandler.TicketState, TicketStates.Single);


            model.OnSpinWheel("1");

            Assert.AreEqual(TicketHandler.TicketState, TicketStates.Multy);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 10m);


            Assert.AreEqual(null, ChangeTracker.LastNotificationTag);
            Assert.AreEqual(null, model.NotificationText);



        }

        [TestMethod]
        public void SpinSinglesToMultyTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Unbind<IDataBinding>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(50);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(50);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.IsReady).Returns(true);


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.85M } }));

            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("{0}");
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0m;

            TicketHandler.TicketState = TicketStates.Multy;
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000.00m, 10, 10, 10);
            var model = new BasketViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(model.WheelLine1.TicketState, TicketStates.Single);
            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Multy);

            model.OnSpinWheel("0");

            model.ChangeStakeSingle.Execute(TicketHandler.TicketsInBasket[0]);
            model.ChangeStake.Execute("5");

            Assert.AreEqual(5, TicketHandler.TicketsInBasket[0].Stake);

            model.ChangeStakeSingle.Execute(TicketHandler.TicketsInBasket[1]);
            model.ChangeStake.Execute("10");

            Assert.AreEqual(10, TicketHandler.TicketsInBasket[1].Stake);

            model.ChangeStakeSingle.Execute(TicketHandler.TicketsInBasket[2]);
            model.ChangeStake.Execute("15");

            Assert.AreEqual(15, TicketHandler.TicketsInBasket[2].Stake);

            Assert.AreEqual(3, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(30m, TicketHandler.Stake);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);


            model.OnSpinWheel("1");

            Assert.AreEqual(TicketStates.Multy, TicketHandler.TicketState);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(15m, TicketHandler.Stake);


        }



        [TestMethod]
        public void BackMinusAvailableCashTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            StationRepository.Setup(x => x.GetMaxCombination(It.IsAny<Ticket>())).Returns(10);

            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            TicketHandler.TicketsInBasket[0].MinBet = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 50;
            TicketHandler.TicketsInBasket[0].Stake = 0m;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketState = TicketStates.Single;
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.00m, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            var model = new BasketViewModel();

            model.ChangeStake.Execute("+50");
            Assert.AreEqual(MultistringTags.NOT_ENOUGHT_MONEY, ChangeTracker.LastNotificationTag);
            //Assert.AreEqual("40.00", model.NotificationText);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.Stake, 10m);

            model.ChangeStake.Execute("back");
            Assert.AreEqual(1m, TicketHandler.Stake);
            Assert.AreEqual(null, ChangeTracker.LastNotificationTag);

            Assert.AreEqual(null, model.NotificationText);

        }


        [TestMethod]
        public void UncheckAllSingleTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(100);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(1000);



            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = true };
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].Stake = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 100;
            TicketHandler.TicketsInBasket[1].TipItems.Add(tiv2);
            TicketHandler.TicketsInBasket[1].Stake = 1;
            TicketHandler.TicketsInBasket[1].MaxBet = 100;
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Single;

            var model = new BasketViewModel();

            Assert.AreEqual(2, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket[1].TipItems.Count);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            tiv1.IsChecked = false;
            model.CheckBetCommand.Execute(tiv1);
            Assert.AreEqual(1, TicketHandler.Stake);
            Assert.AreEqual(0, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket[1].Stake);

            tiv2.IsChecked = false;
            model.CheckBetCommand.Execute(tiv2);

            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            Assert.AreEqual(0, TicketHandler.Stake);
            Assert.AreEqual(0, TicketHandler.TicketsInBasket[0].Stake);
            //Assert.AreEqual(0, TicketHandler.TicketsInBasket[1].Stake);
        }

        [TestMethod]
        public void UncheckToSingleBetTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(100);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(100);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(1000);

            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(100);



            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = true };
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000, 10, 10, 10);
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(150);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv2);
            TicketHandler.TicketsInBasket[0].Stake = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 100;
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Multy;
            TicketHandler.UpdateTicket();
            var model = new BasketViewModel();

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(2, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(TicketStates.Multy, TicketHandler.TicketState);
            Assert.AreEqual(5.29m, TicketHandler.TotalOddDisplay);
            tiv1.IsChecked = false;
            model.CheckBetCommand.Execute(tiv1);
            Assert.AreEqual(1, TicketHandler.Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            Assert.AreEqual(2.3m, TicketHandler.TotalOddDisplay);



        }
        [TestMethod]
        public void CheckFromSingleToMultyBetTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();


            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(100);
            StationRepository.Setup(x => x.GetMaxStakeCombi(It.IsAny<Ticket>())).Returns(100);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxWinMultiBet(It.IsAny<Ticket>())).Returns(1000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(1000);


            TipItemVw tiv1 = new TipItemVw(TestOdd.CreateOdd(1, 2.3m, true)) { IsChecked = true };
            TipItemVw tiv2 = new TipItemVw(TestOdd.CreateOdd(2, 2.3m, true)) { IsChecked = false };
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000, 10, 10, 10);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv2);
            TicketHandler.TicketsInBasket[0].Stake = 1;
            TicketHandler.TicketsInBasket[0].MaxBet = 100;
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Single;
            var model = new BasketViewModel();
            TicketHandler.UpdateTicket();

            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(1, TicketHandler.Stake);
            Assert.AreEqual(2, TicketHandler.TicketsInBasket[0].TipItems.Count);
            Assert.AreEqual(TicketStates.Single, TicketHandler.TicketState);
            tiv2.IsChecked = true;
            model.CheckBetCommand.Execute(tiv1);
            Assert.AreEqual(1, TicketHandler.Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket[0].Stake);
            Assert.AreEqual(1, TicketHandler.TicketsInBasket.Count);
            Assert.AreEqual(TicketStates.Multy, TicketHandler.TicketState);


        }

        [TestMethod]
        public void VerifyTournamentMatchLocksTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            var changeTracker = new ChangeTracker();
            IoCContainer.Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(changeTracker).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();


            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var ticketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(ticketHandler).InSingletonScope();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxSystemBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxOdd(It.IsAny<Ticket>())).Returns(5000);
            ticketHandler.TicketsInBasket.Add(new Ticket());

            ticketHandler.TicketsInBasket[0].TipItems
                .Add(
                    new TipItemVw(
                        new TestOdd()
                        {
                            BetDomain = new TestBetDomain()
                            {
                                Match = new TestMatchLn()
                                {
                                    MatchId = 11,
                                    MatchView = new TestMatchVw() { Name = "nameX", LineObject = new MatchLn(), TournamentView = new TestGroupVw() { LineObject = new GroupLn() { SvrGroupId = 1 } } },

                                }
                            },
                            Value = { Value = 1.0M },
                            OutcomeId = 1
                        }
                        )
                );

            ticketHandler.TicketsInBasket[0].TipItems
                .Add(
                    new TipItemVw(
                        new TestOdd()
                        {
                            BetDomain = new TestBetDomain()
                            {
                                Match = new TestMatchLn()
                                {
                                    MatchId = 12,
                                    MatchView = new TestMatchVw() { Name = "nameX", LineObject = new MatchLn(), TournamentView = new TestGroupVw() { LineObject = new GroupLn() { SvrGroupId = 2 } } }
                                }
                            },
                            Value = { Value = 2.0M },
                            OutcomeId = 2
                            ,
                        }
                        )
                );

            ticketHandler.TicketsInBasket[0].TipItems
                .Add(
                    new TipItemVw(
                        new TestOdd()
                        {
                            BetDomain = new TestBetDomain()
                            {
                                Match = new TestMatchLn()
                                {
                                    MatchId = 13,
                                    MatchView = new TestMatchVw() { Name = "nameX", LineObject = new MatchLn(), TournamentView = new TestGroupVw() { LineObject = new GroupLn() { SvrGroupId = 3 } } }
                                }
                            },
                            Value = { Value = 3.0M },
                            OutcomeId = 3
                        }
                        )
                );

            ticketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd()
            {
                BetDomain = new TestBetDomain()
                {
                    Match = new TestMatchLn()
                    {
                        MatchId = 14,
                        MatchView = new TestMatchVw() { Name = "nameX", LineObject = new MatchLn(), TournamentView = new TestGroupVw() { LineObject = new GroupLn() { SvrGroupId = 4 } } }
                    }
                },
                Value = { Value = 4.0M },
                OutcomeId = 4
            }
                        )
                );

            changeTracker.CurrentUser = new AnonymousUser("1", 1);
            ticketHandler.TicketState = TicketStates.Multy;
            ticketHandler.TicketsInBasket[0].MaxBet = 500;
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            TournamentMatchLocksDictionary tmldX = new TournamentMatchLocksDictionary();

            TournamentMatchLocksLn tml1 = new TournamentMatchLocksLn();
            tml1.TMKey = "1|TOURNAMENT|TOURNAMENT";
            tml1.arrlocks =  "";
            tmldX.SafelyAdd("1|TOURNAMENT|TOURNAMENT", tml1);

            TournamentMatchLocksLn tml2 = new TournamentMatchLocksLn();
            tml2.TMKey = "1|TOURNAMENT|TOURNAMENT";
            tml2.arrlocks = "6|7|2|8" ;
            tmldX.SafelyAdd("1|TOURNAMENT|TOURNAMENT", tml2);

            TournamentMatchLocksLn tml3 = new TournamentMatchLocksLn();
            tml3.TMKey = "2|TOURNAMENT|MATCH";
            tml3.arrlocks = "20|21|14|22" ;
            tmldX.SafelyAdd("2|TOURNAMENT|MATCH", tml3);
            var model = new BasketViewModel();
            model.OnNavigationCompleted();

            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Multy);
            Assert.AreEqual(ticketHandler.TicketsInBasket.Count, 1);
            model = null;

            //
            // CASE 2: TOURNAMENT 1 lockeced with TOURNAMENT 2
            //
            tml1.arrlocks = "6|7|2|8" ;
            MyLineSr.Setup(x => x.TournamentMatchLocks()).Returns(new TournamentMatchLocksDictionary() { new KeyValuePair<string, TournamentMatchLocksLn>("1|TOURNAMENT|TOURNAMENT", tml1) });


            model = new BasketViewModel();
            model.OnNavigationCompleted();

            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(ticketHandler.TicketsInBasket.Count, 4);
            model = null;

            //
            // CASE 3: TOURNAMENT 1 lockeced with MATCH 14
            //
            tml1.TMKey = "1|TOURNAMENT|MATCH";
            tml1.arrlocks = "6|7|14|8" ;

            model = new BasketViewModel();
            model.OnNavigationCompleted();

            Assert.AreEqual(model.WheelLine2.TicketState, TicketStates.Single);
            Assert.AreEqual(ticketHandler.TicketsInBasket.Count, 4);
            model = null;

        }

    }
    public static class MyExtensions
    {
        public static Ticket Clone(this Ticket inputTicket)
        {
            var ticket = new Ticket();
            foreach (var tipItemVw in inputTicket.TipItems)
            {
                ticket.TipItems.Add(tipItemVw);

            }
            ticket.Stake = inputTicket.Stake;
            ticket.MinBet = inputTicket.MinBet;
            ticket.MaxBet = inputTicket.MaxBet;
            ticket.TicketState = inputTicket.TicketState;
            return ticket;
        }
    }
}
