using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;
using SportRadar.DAL.NewLineObjects;
using MVVMTest.ViewModels;

namespace MVVMTest.TestClasses
{
    [TestClass]
    public class TicketCalculatioinsTest : BaseTestClass
    {
        [TestMethod]
        public void SystemBonusesWithWaysCheckTest1()
        {
            //
            // system 3/4 + 2ways + 1bank
            // 3 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
           var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
           var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.13M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 7.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.20M } }));//bonus
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.60M } }));//bonus
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 2.00M } }));//bonus
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 2.70M } }));//bonus

            TicketHandler.TicketsInBasket[0].TipItems[0].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;

            TicketHandler.TicketsInBasket[0].TipItems[5].IsBank = true;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 3;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, bonus);

        }

        [TestMethod]
        public void bonusSystemBankerWaysTest()
        {
            //
            // system 3/5 + 2ways + 2bank
            // 3 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
           var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(stationRepository).InSingletonScope();
            stationRepository.MaxOdd = 1000;
            stationRepository.MaxCombination = 1000;
            stationRepository.MaxStakeSystemBet = 1000;
            stationRepository.MaxSystemBet = 1000;
            stationRepository.MaxWinSystemBet = 10000;
            stationRepository.IsReady = true;
            stationRepository.BonusFromOdd = 3.0m;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 2.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 3.10M } }));


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.80M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 1.70M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 8, Match = new TestMatchLn() { MatchId = 8 } }, Value = { Value = 3.20M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = new TestBetDomain() { BetDomainId = 9, Match = new TestMatchLn() { MatchId = 8 } }, Value = { Value = 2.10M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, BetDomain = new TestBetDomain() { BetDomainId = 10, Match = new TestMatchLn() { MatchId = 9 } }, Value = { Value = 5.60M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 11, OddId = { Value = 11 }, BetDomain = new TestBetDomain() { BetDomainId = 11, Match = new TestMatchLn() { MatchId = 9 } }, Value = { Value = 3.70M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 12, OddId = { Value = 12 }, BetDomain = new TestBetDomain() { BetDomainId = 12, Match = new TestMatchLn() { MatchId = 9 } }, Value = { Value = 1.55M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].Stake = 10;
            stationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 5 });
            stationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 10 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);

            TicketHandler.TicketState = TicketStates.System;

            TicketHandler.TicketsInBasket[0].Stake = 10;


            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.UpdateTicket();

            Assert.AreEqual(10m, TicketHandler.Stake);
            Assert.AreEqual(783.9422976000000m, TicketHandler.CurrentTicketPossibleWin);
            Assert.AreEqual(10m, TicketHandler.BonusPercentage);
            Assert.AreEqual(71.2674816000000m, TicketHandler.BonusValue);

        }

        [TestMethod]
        public void SystemBonusesWithWaysCheckTest2()
        {
            //
            // system 3/4 + 2ways + 1bank
            // 3 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.13M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 7.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.60M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 2.00M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 2.70M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;

            TicketHandler.TicketsInBasket[0].TipItems[4].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[5].IsBank = true;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, bonus);

        }
        [TestMethod]
        public void SystemBonusesWithWaysNoBankCheckTest2()
        {
            //
            // system 3/4 + 2ways + 1bank
            // 3 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
           var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.13M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 7.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.60M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 2.00M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 2.70M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;


            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, bonus);

        }

        [TestMethod]
        public void SystemBonusesNoWaysCheckTest1()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() }, Value = { Value = 2.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() }, Value = { Value = 2.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() }, Value = { Value = 2.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() }, Value = { Value = 2.0M } }));

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;


            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            TicketHandler.TicketsInBasket[0].SystemX = 2;

            //
            // check all bigger values no banks
            //
            StationRepository.BonusFromOdd = 1.25M;
            var bonusValueAllBig = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, (decimal)bonusValueAllBig);

            //
            // check all bigger values with banks
            //
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            var bonusValueAllBigBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, (decimal)bonusValueAllBigBank);
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = false;

            //
            // check all smaller values no banks
            //
            StationRepository.BonusFromOdd = 2.25M;
            var bonusValueAllSmall = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueAllSmall);

            //
            // check all smaller values with banks
            //
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            var bonusValueAllSmallBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueAllSmallBank);
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = false;

            // 
            // 2 smaller, 2 bigger no banks
            //
            StationRepository.BonusFromOdd = 1.9M;
            TicketHandler.TicketsInBasket[0].TipItems[0].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[1].Odd.Value.Value = 1.0M;
            var bonusValueMix = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueMix);

            // 
            // 2 smaller, 2 bigger + smaller bank
            //
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            var bonusValueMixSmallBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, (decimal)bonusValueMixSmallBank);
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = false;

            // 
            // 2 smaller, 2 bigger + bigger bank
            //
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            var bonusValueMixBigBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, (decimal)bonusValueMixBigBank);
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = false;

            // 
            // 3 smaller, 1 bigger + bigger bank
            //
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;
            var bonusValueMixOneBigBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueMixOneBigBank);
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = false;

            // 
            // 3 smaller, 2 bigger no bank
            //
            TicketHandler.TicketsInBasket[0].SystemX = 3;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() }, Value = { Value = 2.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 1.0M;
            var bonusValueMixTwoBig = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueMixTwoBig);

            // 
            // 3 smaller, 2 bigger + bigger bank
            //
            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;
            var bonusValueMixTwoBigBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueMixTwoBigBank);
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = false;

            // 
            // 3 smaller, 2 bigger + smaller bank
            //
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            var bonusValueMixTwoSmallBank = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueMixTwoSmallBank);
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = false;

            //
            // 4/6, 4 big, 1bigB, 1smallB
            //
            StationRepository.BonusFromOdd = 1.9M;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() }, Value = { Value = 2.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() }, Value = { Value = 1.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 8, Match = new TestMatchLn() }, Value = { Value = 1.0M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[1].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[3].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[4].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[5].Odd.Value.Value = 1.0M;

            TicketHandler.TicketsInBasket[0].TipItems[6].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[7].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[6].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = true;

            TicketHandler.TicketsInBasket[0].SystemX = 4;

            var bonusValueMix1 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, (decimal)bonusValueMix1);
            TicketHandler.TicketsInBasket[0].TipItems[6].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = false;

            //
            // 3/7, 2 big, 1bigB, 2smallB
            //

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, BetDomain = new TestBetDomain() { BetDomainId = 9, Match = new TestMatchLn() }, Value = { Value = 1.0M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, BetDomain = new TestBetDomain() { BetDomainId = 10, Match = new TestMatchLn() }, Value = { Value = 1.0M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[1].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[3].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[4].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[5].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[6].Odd.Value.Value = 1.0M;

            TicketHandler.TicketsInBasket[0].TipItems[7].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[8].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[9].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[8].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[9].IsBank = true;

            TicketHandler.TicketsInBasket[0].SystemX = 3;

            var bonusValueMix2 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusValueMix2);
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[8].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[9].IsBank = false;

            //
            // 3/7, 4 big, 2bigB, 1smallB
            //
            TicketHandler.TicketsInBasket[0].TipItems[0].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[1].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[3].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[4].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[5].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[6].Odd.Value.Value = 1.0M;

            TicketHandler.TicketsInBasket[0].TipItems[7].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[8].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[9].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[8].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[9].IsBank = true;

            TicketHandler.TicketsInBasket[0].SystemX = 3;

            var bonusValueMix3 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, (decimal)bonusValueMix3);
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[8].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[9].IsBank = false;

            //
            // 3/7, 5 big, 3bigB
            //
            TicketHandler.TicketsInBasket[0].TipItems[0].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[1].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[2].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[3].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[4].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[5].Odd.Value.Value = 1.0M;
            TicketHandler.TicketsInBasket[0].TipItems[6].Odd.Value.Value = 1.0M;

            TicketHandler.TicketsInBasket[0].TipItems[7].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[8].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[9].Odd.Value.Value = 2.0M;
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[8].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[9].IsBank = true;

            TicketHandler.TicketsInBasket[0].SystemX = 3;

            var bonusValueMix4 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(40, (decimal)bonusValueMix4);
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[8].IsBank = false;
            TicketHandler.TicketsInBasket[0].TipItems[9].IsBank = false;
        }

        [TestMethod]
        public void SystemBonusesNoWaysCheckTest2()
        {
            //
            // system 4/5
            // 2 odds smaller that limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.50M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.00M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 3.60M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.45M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 4;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, bonus);

        }

        [TestMethod]
        public void SystemBonusesNoWaysCheckTest3()
        {
            //
            // system 4/6 + 2 bank
            // 4 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.65M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.60M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.65M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 1.85M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 8, Match = new TestMatchLn() { MatchId = 8 } }, Value = { Value = 2.00M } }));

            TicketHandler.TicketsInBasket[0].TipItems[4].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[5].IsBank = true;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 4;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, bonus);

        }

        [TestMethod]
        public void SystemBonusesNoWaysCheckTest4()
        {
            //
            // system 2/7
            // 1 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 3.50M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 3.30M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.TicketState = TicketStates.System;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, bonus);

        }

        [TestMethod]
        public void SystemBonusesNoWaysCheckTest5()
        {
            //
            // system 2/3 + 3 bank
            // 2 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, bonus);

        }

        [TestMethod]
        public void SystemBonusesNoWaysCheckTest6()
        {
            //
            // system 2/3 + 3 bank
            // 2 odds smaller than allowed limit
            //
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
           var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            //StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            StationRepository.BonusFromOdd = 1.7M;

            decimal bonus = 0;

            bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, bonus);

        }

        [TestMethod]
        public void SystemTicketBonusAndManipulationFeeNoWaysNoBankTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
           var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 1.7M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 3, limit = 3000, manipulationFee = 30 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 4, limit = 4000, manipulationFee = 40 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 5, limit = 5000, manipulationFee = 50 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 6, limit = 6000, manipulationFee = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 5;

            decimal manFee = StationRepository.GetManipulationFeePercentage(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(50M, manFee);

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, bonus);
        }

        [TestMethod]
        public void SystemTicketBonusAndManipulationFeeNoWaysWithBank1Test()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 1.7M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 3, limit = 3000, manipulationFee = 30 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 4, limit = 4000, manipulationFee = 40 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 5, limit = 5000, manipulationFee = 50 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 6, limit = 6000, manipulationFee = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            decimal manFee = StationRepository.GetManipulationFeePercentage(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(40M, manFee);

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, bonus);
        }

        [TestMethod]
        public void SystemTicketBonusAndManipulationFeeNoWaysWithBank2Test()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 1.7M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 3, limit = 3000, manipulationFee = 30 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 4, limit = 4000, manipulationFee = 40 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 5, limit = 5000, manipulationFee = 50 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 6, limit = 6000, manipulationFee = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 3;

            decimal manFee = StationRepository.GetManipulationFeePercentage(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(50M, manFee);

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, bonus);
        }

        [TestMethod]
        public void SystemTicketBonusAndManipulationFeeWithWaysNoBankTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
           var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 1.7M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 3, limit = 3000, manipulationFee = 30 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 4, limit = 4000, manipulationFee = 40 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 5, limit = 5000, manipulationFee = 50 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 6, limit = 6000, manipulationFee = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 3;

            decimal manFee = StationRepository.GetManipulationFeePercentage(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(50M, manFee);

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, bonus);
        }

        [TestMethod]
        public void SystemTicketBonusAndManipulationFeeWithWaysWithBankTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
        var    TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[3].IsBank = true;

            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = true;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 1.7M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 3, limit = 3000, manipulationFee = 30 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 4, limit = 4000, manipulationFee = 40 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 5, limit = 5000, manipulationFee = 50 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 6, limit = 6000, manipulationFee = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            decimal manFee = StationRepository.GetManipulationFeePercentage(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(50M, manFee);

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, bonus);
        }
        [TestMethod]
        public void SystemBonusWithWaysWithBankTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.20M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 1.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 2.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 2.55M } }));

            TicketHandler.TicketsInBasket[0].TipItems[0].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[0].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[1].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsWay = true;
            TicketHandler.TicketsInBasket[0].TipItems[2].IsBank = true;

            TicketHandler.TicketsInBasket[0].TipItems[6].IsBank = true;
            TicketHandler.TicketsInBasket[0].TipItems[7].IsBank = true;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 2.0M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 0 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, bonus);
        }


        [TestMethod]
        public void MultipleTicketBonusesNoWaysTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.Multy;

            StationRepository.BonusFromOdd = 1.25M;
            var bonusMulty1 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(40, (decimal)bonusMulty1);

            StationRepository.BonusFromOdd = 1.75M;
            var bonusMulty2 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(30, (decimal)bonusMulty2);
        }

        [TestMethod]
        public void SingleTicketBelowBonusOddTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
       var     ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));


            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            StationRepository.BonusFromOdd = 1;

            var bonusMulty1 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, (decimal)bonusMulty1);

            StationRepository.BonusFromOdd = 100.75M;
            var bonusMulty2 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(0, (decimal)bonusMulty2);
        }

        [TestMethod]
        public void MultipleTicketBonusesWithWaysTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
        var    ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.Multy;

            StationRepository.BonusFromOdd = 1.25M;
            var bonusMulty1 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(20, (decimal)bonusMulty1);

            StationRepository.BonusFromOdd = 1.75M;
            var bonusMulty2 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, (decimal)bonusMulty2);
        }


        [TestMethod]
        public void MultipleTicketBonusesWithWayTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository StationRepository = new StationRepository();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });


            var model = new BaseViewModel();
            TicketHandler.TicketsInBasket[0].MaxBet = 500;

            TicketHandler.TicketState = TicketStates.Multy;

            StationRepository.BonusFromOdd = 2.00M;
            var bonusMulty1 = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(10, (decimal)bonusMulty1);

        }


        [TestMethod]
        public void MultipleTicketPossibleWinningNoWaysNoBonusNoManFeeTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
        var    TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;

            //IoCContainer.Kernel.Unbind<IRepository>();
            //Repository Repository = new Repository();
            //Kernel.Bind<IRepository>().ToConstant<IRepository>(Repository).InSingletonScope();

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 0 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Multy;


            TicketHandler.UpdateTicket();
            decimal posWin = TicketHandler.TicketsInBasket[0].CurrentTicketPossibleWin;

            Assert.AreEqual(353.43M, posWin);

        }
        [TestMethod]
        public void SystemTicketWaysNoBonusNoManFeeTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;
            StationRepository.MaxSystemBet = 10;
            StationRepository.MaxWinSystemBet = 10000;
            //IoCContainer.Kernel.Unbind<IRepository>();
            //Repository Repository = new Repository();
            //Kernel.Bind<IRepository>().ToConstant<IRepository>(Repository).InSingletonScope();
            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainId = 8, Match = new TestMatchLn() { MatchId = 8 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 0 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeSystemBet = 2000;
            StationRepository.MinStakeSystemBet = 1;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.System;


            TicketHandler.UpdateTicket();

            Assert.AreEqual(33.3795m, TicketHandler.TicketsInBasket[0].TotalOdd);

        }

        [TestMethod]
        public void MultipleTicketPossibleWinningWithWaysNoBonusNoManFeeTest()
        {
            //
            // made for http://jira:8080/browse/BS-3179 / screenshot #1
            //
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
           var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.7M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 0, manipulationFee = 0 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Multy;


            TicketHandler.UpdateTicket();
            decimal posWin = TicketHandler.TicketsInBasket[0].CurrentTicketPossibleWin;
            Assert.AreEqual(33.3795m, TicketHandler.TicketsInBasket[0].TotalOdd);
            Assert.AreEqual(83.44875M, posWin);
        }

        [TestMethod]
        public void MultipleTicketPossibleWinningWithWaysWithBonusWithManFeeTest()
        {
            //
            // made for http://jira:8080/browse/BS-3179 / screenshot #1
            //
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.7M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 10 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 10000, manipulationFee = 20 });

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Multy;


            TicketHandler.UpdateTicket();
            decimal posWin = TicketHandler.TicketsInBasket[0].CurrentTicketPossibleWin;
            Assert.AreEqual(25.41M, posWin);
        }

        [TestMethod]
        public void MultipleTicketPossibleWinningWithWaysOneSingleNoBonusNoManFeeTest()
        {
            //
            // made for http://jira:8080/browse/BS-3179 / screenshot #1
            //
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
           var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.6M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.4M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.6M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 0, manipulationFee = 0 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.CombiLimitList = new List<CombiLimitWS>();

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Multy;


            TicketHandler.UpdateTicket();
            decimal posWin = TicketHandler.TicketsInBasket[0].CurrentTicketPossibleWin;
            Assert.AreEqual(72.93M, posWin);
        }

        [TestMethod]
        public void MultipleTicketPossibleWinningWithWaysOneSingleWithBonusWithManFeeExceedMaxWinTest()
        {
            //
            // made for http://jira:8080/browse/BS-3179 / screenshot #1
            //
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("");

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
        var    ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;

            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 1.6M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.4M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.6M } }));

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 10, manipulationFee = 0 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100000, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[0].Stake = 10000;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 20000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Multy;


            TicketHandler.UpdateTicket();
            decimal posWin = TicketHandler.TicketsInBasket[0].CurrentTicketPossibleWin;
            Assert.AreEqual(29.172M, TicketHandler.TicketsInBasket[0].TotalOdd); // 369.15
            Assert.AreEqual(2369.15M, Math.Round(posWin, 2)); // 369.15
        }

        [TestMethod]
        public void MultipleTicketBonusAndManipulationFeeNoWaysTest()
        {
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
          var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
         var   TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.55M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 3.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4 } }, Value = { Value = 2.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5 } }, Value = { Value = 1.90M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.55M } }));

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;

            StationRepository.BonusFromOdd = 1.25M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeCombi = 2000;
            StationRepository.MinStakeCombiBet = 1;

            StationRepository.BonusFromOdd = 1.7M;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 10 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 20 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 3, bonus = 30 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 4, bonus = 40 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 5, bonus = 50 });
            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 6, bonus = 60 });

            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 2, limit = 2000, manipulationFee = 20 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 3, limit = 3000, manipulationFee = 30 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 4, limit = 4000, manipulationFee = 40 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 5, limit = 5000, manipulationFee = 50 });
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 6, limit = 6000, manipulationFee = 60 });

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Multy;

            decimal manFee = StationRepository.GetManipulationFeePercentage(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(60M, manFee);

            var bonus = StationRepository.GetBonusValueForBets(TicketHandler.TicketsInBasket[0]);
            Assert.AreEqual(40, bonus);
        }

        [TestMethod]
        public void MultipleTicketManipulationFeeWithWaysTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
        var    ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
        var    TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<StationRepository>(stationRepository).InSingletonScope();
            stationRepository.IsReady = true;

            stationRepository.CombiLimitList = new List<CombiLimitWS>();
            stationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            stationRepository.MinStakeCombiBet = 1;
            stationRepository.MaxStakeCombi = 10000;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 3.5M } }));

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Multy;
            TicketHandler.UpdateTicket();
            Assert.AreEqual(0.5M, TicketHandler.ManipulationFeeValue);

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 5.5M } }));
            TicketHandler.UpdateTicket();
            Assert.AreEqual(0.25M, Math.Round(TicketHandler.ManipulationFeeValue,2));

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3 } }, Value = { Value = 2.5M } }));
            TicketHandler.UpdateTicket();
            Assert.AreEqual(0.25M, Math.Round(TicketHandler.ManipulationFeeValue, 2));
        }

        [TestMethod]
        public void MaxWinAndPossibleWinAreEqual()
        {
            // BS-4482
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository StationRepository = new StationRepository();
            Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            StationRepository.IsReady = true;


            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.17M } }));
            TicketHandler.TicketsInBasket[0].Stake = 500;
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 4.80M } }));
            TicketHandler.TicketsInBasket[1].Stake = 416.70M;

            StationRepository.BonusFromOdd = 2M;
            StationRepository.MaxOdd = 2000;
            StationRepository.MaxStakeSingleBet = 500;
            StationRepository.MaxWinSingleBet = 2000;

            StationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 1 });
            
            StationRepository.CombiLimitList = new List<CombiLimitWS>();
            StationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 1 });

            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            TicketHandler.UpdateTicket();

            Assert.AreEqual(TicketHandler.MaxWin, TicketHandler.CurrentTicketPossibleWin);
            
        }

        [TestMethod]
        public void MultipleTicketManipulationFeeWithWaysAndOutrightPodiumTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
         var   ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
          var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<IStationRepository>();
            StationRepository stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<StationRepository>(stationRepository).InSingletonScope();
            stationRepository.IsReady = true;

            StationRepository.Setup(x => x.GetManipulationFeePercentage(It.IsAny<Ticket>())).Returns(10);

            stationRepository.CombiLimitList = new List<CombiLimitWS>();
            stationRepository.CombiLimitList.Add(new CombiLimitWS() { combiSize = 1, limit = 1000, manipulationFee = 10 });
            stationRepository.MinStakeCombiBet = 1;
            stationRepository.MaxStakeCombi = 10000;

            TicketHandler.TicketsInBasket.Add(new Ticket());
            var podiumNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test");
            podiumNumber.Value = 1050;
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, BetDomainNumber = podiumNumber, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.8M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainId = 1, BetDomainNumber = podiumNumber, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.3M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainId = 1, BetDomainNumber = podiumNumber, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 3.5M } }));

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[0].Stake = 10;
            TicketHandler.TicketsInBasket[0].TicketState = TicketStates.Multy;

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2 } }, Value = { Value = 5.5M } }));
            TicketHandler.UpdateTicket();
            Assert.AreEqual(1M, TicketHandler.ManipulationFeeValue);
        }
    }
}
