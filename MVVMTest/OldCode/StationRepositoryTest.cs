using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Preferences.Services.Preference;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using ViewModels.ViewModels;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class StationRepositoryTest : BaseTestClass
    {



        [TestMethod]
        public void StationAppconfigTest()
        {
            var stationRepository = new StationRepository();
            stationRepository.AllowVfl = true;
            stationRepository.AllowVhc = true;

            Assert.IsTrue(stationRepository.AllowVfl);
            Assert.IsTrue(stationRepository.AllowVhc);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AnonynousNoCashpoolNoBets()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);
            StationSettings StationSettings = new StationSettings();
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(StationSettings).InSingletonScope();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            ChangeTracker.Setup(x=>x.CurrentUser).Returns(new AnonymousUser("1",1));
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",1),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });

            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, true);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AnonynousCashpoolNoBets()
        {
          
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);
            StationSettings StationSettings = new StationSettings();
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(StationSettings).InSingletonScope();

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            ChangeTracker.Setup(x=>x.CurrentUser).Returns(new AnonymousUser("1", 1));
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",1),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });

            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, true);
           
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void LoggedInNoCashpoolNoBets()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);
            StationSettings StationSettings = new StationSettings();
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(StationSettings).InSingletonScope();

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            ChangeTracker.Setup(x=>x.CurrentUser).Returns(new LoggedInUser(1, "1", 10,10,10,10));
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",1),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });

            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, true);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void LoggedInCashpoolNoBets()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);
            StationSettings StationSettings = new StationSettings();
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(StationSettings).InSingletonScope();

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            ChangeTracker.Setup(x=>x.CurrentUser).Returns(new LoggedInUser(1, "1", 10,10,10,10));
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",1),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });
            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, true);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AnonymousNoCashpoolHaveBets()
        {
            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
           var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            StationSettings StationSettings = new StationSettings();
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(StationSettings).InSingletonScope();

            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }));
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",1),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });
            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, true);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AnonymousNoCashpoolHaveBetsNotAlwaysActive()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            ChangeTracker.Object.CurrentUser = new AnonymousUser("1", 1);
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",0),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });

            StationSettings StationSettings = new StationSettings();
            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, false);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AnonymousNoCashpoolHaveBetsNotAlwaysActiveInBasket()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);
            StationSettings StationSettings = new StationSettings();
            IoCContainer.Kernel.Rebind<IStationSettings>().ToConstant(StationSettings).InSingletonScope();

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            ChangeTracker.Setup(x=>x.CurrentUser).Returns(new LoggedInUser(1, "1", 10,10,10,10));
            ChangeTracker.Setup(x=>x.IsBasketOpen).Returns(true);
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",0),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });
            var basket = new BasketViewModel();

            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, true);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AnonymousbettingDisabled()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            ChangeTracker.Object.CurrentUser = new AnonymousUser("1", 1);
            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",1),
                    new StationAppConfigSr("AllowAnonymousBetting",0),
                    
                });

            StationSettings StationSettings = new StationSettings();
            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, false);
        }

        [TestMethod]
        [DeploymentItem("PrefFile.txt")]
        public void AlwaysActiveDisabled()
        {
            var stationRepository = new StationRepository();
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(),out reserved,out factor)).Returns(10m);

            Repository.Setup(x => x.GetAllStationAppConfigs()).Returns(new List<StationAppConfigSr>()
                {
                    new StationAppConfigSr("CashAcceptorAlwayActive",0),
                    new StationAppConfigSr("AllowAnonymousBetting",1),

                });

            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 500;
            ChangeTracker.Object.CurrentUser = new AnonymousUser("1", 1);

            StationSettings StationSettings = new StationSettings();
            StationSettings.PrefFileName = "PrefFile.txt";
            StationSettings.Init();
            StationSettings.GetCashInManagerInstance();
           

            stationRepository.SetCashInDefaultState(10);
            System.Threading.Thread.Sleep(500);
            Assert.AreEqual(StationSettings.IsCashInEnabled, false);
        }

    }
}