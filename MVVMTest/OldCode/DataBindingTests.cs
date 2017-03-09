using System;
using System.Collections.ObjectModel;
using System.Linq;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using WsdlRepository;
using WsdlRepository.oldcode;
using DataBinding = WsdlRepository.oldcode.DataBinding;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class DataBindingTests : BaseTestClass
    {
        [TestMethod]
        public void SingleTipItemTicket()
        {

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();


            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;

            StationRepository.MaxOdd = 1000;
            StationRepository.MinStakeSingleBet = 1;
            StationRepository.MaxStakeSingleBet = 500;
            StationRepository.MaxWinSingleBet = 2000;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(18.5m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(1.85m, ticket.TotalOdd);
            Assert.AreEqual(925m, ticket.MaxWin);
            Assert.AreEqual(500m, ticket.MaxBet);
            Assert.AreEqual(false, ticket.MaxOddExceeded);

            ticket.TipItems.Clear();
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1001.00M } }));
            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(true, ticket.MaxOddExceeded);
        }

        [TestMethod]
        public void MultyTipItemTicket()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            ChangeTracker.CurrentUser = new LoggedInUser(1, "1", 100000, 10, 10, 10);

            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Rebind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1000;
            StationRepository.MinStakeCombiBet = 1;
            StationRepository.MaxStakeCombi = 50000;
            StationRepository.MaxWinSingleBet = 10000;

            var ticket = new Ticket();
            ticket.Stake = 10;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.55M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.65M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket.Add(ticket);

            TicketHandler.UpdateTicket();
            Assert.AreEqual(80.4333750m, TicketHandler.CurrentTicketPossibleWin);
            Assert.AreEqual(8.0433375m, ticket.TotalOdd);
            Assert.AreEqual(402166.8750000m, ticket.MaxWin);
            Assert.AreEqual(50000m, ticket.MaxBet);

            ticket.TipItems.Clear();
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 2.00M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 500.00M } }));
            TicketHandler.UpdateTicket();

            Assert.AreEqual(false, ticket.MaxOddExceeded);

            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.00M } }));
            TicketHandler.UpdateTicket();

            Assert.AreEqual(true, ticket.MaxOddExceeded);

        }

        [TestMethod]
        public void MultyTipItemMyltywayTicket()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1000;
            StationRepository.MinStakeCombiBet = 1;
            StationRepository.MaxStakeCombi = 50000;
            StationRepository.MaxWinSingleBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.TicketState = TicketStates.Multy;
            ticket.Stake = 10;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 12, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.2M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.55M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 22, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.65M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 32, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.6M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.7M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 42, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 3.3M } }));

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(83.16m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(133.0560m, ticket.TotalOdd);
            Assert.AreEqual(415800m, ticket.MaxWin);
            Assert.AreEqual(50000m, ticket.MaxBet);


        }

        [TestMethod]
        public void SystemTicket()
        {
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1000;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.TicketState = TicketStates.System;
            ticket.Stake = 10;
            ticket.SystemX = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.55M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.65M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.7M } }));

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(28.43750m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(2.84375m, ticket.TotalOdd);
            Assert.AreEqual(2843.75m, ticket.MaxWin);
            Assert.AreEqual(1000m, ticket.MaxBet);


        }

        [TestMethod]
        public void SystemMultywatTicket()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();


            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 10;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.TicketState = TicketStates.System;
            ticket.Stake = 10;
            ticket.SystemX = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsBank = true });
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 12, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.2M } }) { IsBank = true });
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 10.55M } }) { IsBank = true });
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 22, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }) { IsBank = true });
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.65M } }) { IsBank = true });
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 32, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.6M } }) { IsBank = true });
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.7M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 2.3M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 20.7M } }));

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(3512.7955200000000000000000000m, ticket.TotalOdd);
            Assert.AreEqual(4390.994400000000000000000000m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(9998.29424880000000000000000m, ticket.MaxWin);
            Assert.AreEqual(22.77m, ticket.MaxBet);


        }

        [TestMethod]
        public void SingleTotalOddExceededTicket()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();


            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1;
            StationRepository.MinStakeSingleBet = 1;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxStakeSingleBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxWinSingleBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 10.85M } }) { IsBank = true });


            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(10.85m, ticket.TotalOdd);
            Assert.AreEqual(108.500m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(9999.9025m, ticket.MaxWin);
            Assert.AreEqual(921.65m, ticket.MaxBet);


        }
        [TestMethod]
        public void MultyTotalOddExceededTicket()
        {
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1;
            StationRepository.MinStakeCombiBet = 1;
            StationRepository.MinStakeSingleBet = 1;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeCombi = 1000;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxStakeSingleBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxCombination = 100;
            StationRepository.MaxWinSingleBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 10.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 10.0M } }));

            ticket.TicketState = TicketStates.Multy;

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(100.0m, ticket.TotalOdd);
            Assert.AreEqual(1000.000m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(100000.00m, ticket.MaxWin);
            Assert.AreEqual(1000m, ticket.MaxBet);
            Assert.AreEqual(true, ticket.MaxOddExceeded);


        }

        [TestMethod]
        public void MultyMultyWayTotalOddExceededTicket()
        {
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1;
            StationRepository.MinStakeCombiBet = 1;
            StationRepository.MinStakeSingleBet = 1;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeCombi = 1000;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxStakeSingleBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxCombination = 100;
            StationRepository.MaxWinSingleBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 10.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 10.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 10.0M } }));

            ticket.TicketState = TicketStates.Multy;

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);

            Assert.AreEqual(100.0m, ticket.TotalOdd);
            Assert.AreEqual(500.000m, ticket.CurrentTicketPossibleWin);
            Assert.AreEqual(50000, ticket.MaxWin);
            Assert.AreEqual(1000m, ticket.MaxBet);
            Assert.AreEqual(true, ticket.MaxOddExceeded);


        }

        [TestMethod]
        public void PossibleWinPodiumOutrightWithWays()
        {
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1;
            StationRepository.MinStakeCombiBet = 1;
            StationRepository.MinStakeSingleBet = 1;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeCombi = 1000;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxStakeSingleBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxCombination = 100;
            StationRepository.MaxWinSingleBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd2, Value = { Value = 5.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = tbd2, Value = { Value = 6.0M } }));

            ticket.TicketState = TicketStates.Multy;
            ticket.Stake = 100;

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(3000.00m, ticket.CurrentTicketPossibleWin);

            TestBetDomain tbd3 = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } };
            tbd3.BetDomainNumber = new ObservableProperty<int>(tbd3, new ObservablePropertyList(), "test");
            tbd3.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = tbd3, Value = { Value = 7.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = tbd3, Value = { Value = 8.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = tbd3, Value = { Value = 9.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, BetDomain = tbd3, Value = { Value = 10.0M } }));

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(20250.00m, ticket.CurrentTicketPossibleWin);

        }

        [TestMethod]
        public void PossibleWinPodiumOutrightWithWaysAndUsualOutrightWithWays()
        {
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            StationRepository.MaxOdd = 1;
            StationRepository.MinStakeCombiBet = 1;
            StationRepository.MinStakeSingleBet = 1;
            StationRepository.MinStakeSystemBet = 1;
            StationRepository.MaxStakeCombi = 1000;
            StationRepository.MaxStakeSystemBet = 1000;
            StationRepository.MaxStakeSingleBet = 1000;
            StationRepository.MaxWinSystemBet = 10000;
            StationRepository.MaxCombination = 100;
            StationRepository.MaxWinSingleBet = 10000;
            StationRepository.MaxSystemBet = 15;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd2, Value = { Value = 5.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = tbd2, Value = { Value = 6.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = tbd2, Value = { Value = 7.0M } }));

            ticket.TicketState = TicketStates.Multy;
            ticket.Stake = 100;

            TestBetDomain tbd3 = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } };
            tbd3.BetDomainNumber = new ObservableProperty<int>(tbd3, new ObservablePropertyList(), "test");
            tbd3.BetDomainNumber.Value = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = tbd3, Value = { Value = 8.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = tbd3, Value = { Value = 9.0M } }));

            var DataBinding = new DataBinding();
            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(12150.00m, ticket.CurrentTicketPossibleWin);

        }

        [TestMethod]
        public void CheckMinStakeSingle()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(3, ticket.MinBet);

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }) { IsChecked = false });

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(3, ticket.MinBet);

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(3, ticket.MinBet);
        }

        [TestMethod]
        public void CheckMinStakeMulti()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }) { IsChecked = false });

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));

            ticket.TicketState = TicketStates.Multy;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(3, ticket.MinBet);
        }

        [TestMethod]
        public void CheckMinStakeMultiWays()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd0, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd2, Value = { Value = 5.0M } }));

            ticket.TicketState = TicketStates.Multy;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(8, ticket.MinBet);
        }

        [TestMethod]
        public void CheckMinStakeSystem()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));

            TestBetDomain tbd3 = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } };
            tbd3.BetDomainNumber = new ObservableProperty<int>(tbd3, new ObservablePropertyList(), "test");
            tbd3.BetDomainNumber.Value = 4;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd3, Value = { Value = 5.0M } }));

            TestBetDomain tbd4 = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } };
            tbd4.BetDomainNumber = new ObservableProperty<int>(tbd4, new ObservablePropertyList(), "test");
            tbd4.BetDomainNumber.Value = 5;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = tbd4, Value = { Value = 6.0M } }));

            ticket.TicketState = TicketStates.System;
            ticket.SystemX = 2;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(20, ticket.MinBet);

            ticket.SystemX = 4;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(10, ticket.MinBet);
        }

        [TestMethod]
        public void CheckMinStakeSystemWithWays()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));

            TestBetDomain tbd3 = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } };
            tbd3.BetDomainNumber = new ObservableProperty<int>(tbd3, new ObservablePropertyList(), "test");
            tbd3.BetDomainNumber.Value = 4;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd3, Value = { Value = 5.0M } }));

            TestBetDomain tbd4 = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } };
            tbd4.BetDomainNumber = new ObservableProperty<int>(tbd4, new ObservablePropertyList(), "test");
            tbd4.BetDomainNumber.Value = 5;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = tbd4, Value = { Value = 6.0M } }));

            TestBetDomain tbd5 = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } };
            tbd5.BetDomainNumber = new ObservableProperty<int>(tbd5, new ObservablePropertyList(), "test");
            tbd5.BetDomainNumber.Value = 6;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = tbd5, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = tbd5, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = tbd5, Value = { Value = 2.0M } }));

            TestBetDomain tbd6 = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } };
            tbd6.BetDomainNumber = new ObservableProperty<int>(tbd6, new ObservablePropertyList(), "test");
            tbd6.BetDomainNumber.Value = 7;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, BetDomain = tbd6, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 11, OddId = { Value = 11 }, BetDomain = tbd6, Value = { Value = 2.0M } }));

            ticket.TicketState = TicketStates.System;
            ticket.SystemX = 2;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(120, ticket.MinBet);

            ticket.SystemX = 4;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(60, ticket.MinBet);
        }

        [TestMethod]
        public void CheckMinStakeSystemWithWaysAndBank()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));

            TestBetDomain tbd3 = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } };
            tbd3.BetDomainNumber = new ObservableProperty<int>(tbd3, new ObservablePropertyList(), "test");
            tbd3.BetDomainNumber.Value = 4;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd3, Value = { Value = 5.0M } })
                {
                    IsBank = true
                });

            TestBetDomain tbd4 = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } };
            tbd4.BetDomainNumber = new ObservableProperty<int>(tbd4, new ObservablePropertyList(), "test");
            tbd4.BetDomainNumber.Value = 5;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = tbd4, Value = { Value = 6.0M } })
                {
                    IsBank = true
                });

            TestBetDomain tbd5 = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } };
            tbd5.BetDomainNumber = new ObservableProperty<int>(tbd5, new ObservablePropertyList(), "test");
            tbd5.BetDomainNumber.Value = 6;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = tbd5, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = tbd5, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = tbd5, Value = { Value = 2.0M } }));

            TestBetDomain tbd6 = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } };
            tbd6.BetDomainNumber = new ObservableProperty<int>(tbd6, new ObservablePropertyList(), "test");
            tbd6.BetDomainNumber.Value = 7;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, BetDomain = tbd6, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 11, OddId = { Value = 11 }, BetDomain = tbd6, Value = { Value = 2.0M } }));

            ticket.TicketState = TicketStates.System;
            ticket.SystemX = 2;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(36, ticket.MinBet);
        }

        [TestMethod]
        public void CheckMinStakeSystemWithThreeWaysAndTwoBanks()
        {
            StationRepository StationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MinStakePerRow = 2;
            StationRepository.MinStakeCombiBet = 3;
            StationRepository.MinStakeSingleBet = 3;
            StationRepository.MinStakeSystemBet = 3;
            StationRepository.MaxSystemBet = 10;

            var ticket = new Ticket();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(ticket);
            ticket.Stake = 10;

            var DataBinding = new DataBinding();

            TestBetDomain tbd0 = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } };
            tbd0.BetDomainNumber = new ObservableProperty<int>(tbd0, new ObservablePropertyList(), "test");
            tbd0.BetDomainNumber.Value = 1;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = tbd0, Value = { Value = 2.0M } }));

            TestBetDomain tbd1 = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } };
            tbd1.BetDomainNumber = new ObservableProperty<int>(tbd1, new ObservablePropertyList(), "test");
            tbd1.BetDomainNumber.Value = 2;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = tbd1, Value = { Value = 3.0M } }));

            TestBetDomain tbd2 = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } };
            tbd2.BetDomainNumber = new ObservableProperty<int>(tbd2, new ObservablePropertyList(), "test");
            tbd2.BetDomainNumber.Value = 1050;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = tbd2, Value = { Value = 4.0M } }));

            TestBetDomain tbd3 = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } };
            tbd3.BetDomainNumber = new ObservableProperty<int>(tbd3, new ObservablePropertyList(), "test");
            tbd3.BetDomainNumber.Value = 4;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = tbd3, Value = { Value = 5.0M } })
            {
                IsBank = true
            });

            TestBetDomain tbd5 = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } };
            tbd5.BetDomainNumber = new ObservableProperty<int>(tbd5, new ObservablePropertyList(), "test");
            tbd5.BetDomainNumber.Value = 6;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = tbd5, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = tbd5, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = tbd5, Value = { Value = 2.0M } }));

            TestBetDomain tbd6 = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } };
            tbd6.BetDomainNumber = new ObservableProperty<int>(tbd6, new ObservablePropertyList(), "test");
            tbd6.BetDomainNumber.Value = 7;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, BetDomain = tbd6, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 11, OddId = { Value = 11 }, BetDomain = tbd6, Value = { Value = 2.0M } }));

            TestBetDomain tbd7 = new TestBetDomain() { BetDomainId = 8, Match = new TestMatchLn() { MatchId = 8 } };
            tbd7.BetDomainNumber = new ObservableProperty<int>(tbd7, new ObservablePropertyList(), "test");
            tbd7.BetDomainNumber.Value = 8;
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 12, OddId = { Value = 12 }, BetDomain = tbd7, Value = { Value = 2.0M } }));
            ticket.TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 13, OddId = { Value = 13 }, BetDomain = tbd7, Value = { Value = 2.0M } }));

            ticket.TicketState = TicketStates.System;
            ticket.SystemX = 2;

            DataBinding.UpdateSystemOrCombiticket(ticket);
            Assert.AreEqual(72, ticket.MinBet);
        }
    }
}
