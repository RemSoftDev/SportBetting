using System.Collections.Generic;
using System.IO;
using System.Threading;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class TicketActionsTest : BaseTestClass
    {


        [TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        public void CreateTicketWsTest()
        {
            DatabaseManager.EnsureDatabase(false);
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Expect(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Rebind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Rebind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            BusinessPropsHelper.Expect(x => x.GetNextTransactionId()).Returns("123");

            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Rebind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(stationRepository).InSingletonScope();
            stationRepository.MaxOdd = 1000;
            stationRepository.MaxCombination = 1000;
            stationRepository.MaxStakeSystemBet = 1000;
            stationRepository.MaxSystemBet = 1000;
            stationRepository.MaxWinSystemBet = 10000;
            stationRepository.IsReady = true;
            stationRepository.BonusFromOdd = 3.0m;

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, OddView = new TestOddVw() { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1, MatchView = new TestMatchVw() { LineObject = new MatchLn() } } }, Value = { Value = 2.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 2, OddId = { Value = 2 }, OddView = new TestOddVw() { Value = 2 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 2, Match = new TestMatchLn() { MatchId = 2, MatchView = new TestMatchVw() { LineObject = new MatchLn() } } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 3, OddId = { Value = 3 }, OddView = new TestOddVw() { Value = 3 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 3, Match = new TestMatchLn() { MatchId = 3, MatchView = new TestMatchVw() { LineObject = new MatchLn() } } }, Value = { Value = 3.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 4, OddId = { Value = 4 }, OddView = new TestOddVw() { Value = 4 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 4, Match = new TestMatchLn() { MatchId = 4, MatchView = new TestMatchVw() { LineObject = new MatchLn() } } }, Value = { Value = 2.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 5, OddId = { Value = 5 }, OddView = new TestOddVw() { Value = 5 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 5, Match = new TestMatchLn() { MatchId = 5, MatchView = new TestMatchVw() { LineObject = new MatchLn() } } }, Value = { Value = 3.10M } }));


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 6, OddId = { Value = 6 }, OddView = new TestOddVw() { Value = 6 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 6, Match = new TestMatchLn() { MatchId = 6 } }, Value = { Value = 1.80M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 7, OddId = { Value = 7 }, OddView = new TestOddVw() { Value = 7 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 7, Match = new TestMatchLn() { MatchId = 7 } }, Value = { Value = 1.70M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 8, OddId = { Value = 8 }, OddView = new TestOddVw() { Value = 8 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 8, Match = new TestMatchLn() { MatchId = 8 } }, Value = { Value = 3.20M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 9, OddId = { Value = 9 }, OddView = new TestOddVw() { Value = 9 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 9, Match = new TestMatchLn() { MatchId = 8 } }, Value = { Value = 2.10M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 10, OddId = { Value = 10 }, OddView = new TestOddVw() { Value = 10 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 10, Match = new TestMatchLn() { MatchId = 9 } }, Value = { Value = 5.60M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 11, OddId = { Value = 11 }, OddView = new TestOddVw() { Value = 11 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 11, Match = new TestMatchLn() { MatchId = 9 } }, Value = { Value = 3.70M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 12, OddId = { Value = 12 }, OddView = new TestOddVw() { Value = 12 }, BetDomain = new TestBetDomain() { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 12, Match = new TestMatchLn() { MatchId = 9 } }, Value = { Value = 1.55M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].Stake = 10;
            stationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 1, bonus = 5 });
            stationRepository.BonusRangeList.Add(new BonusRangeWS() { tipSize = 2, bonus = 10 });

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketState = TicketStates.System;

            TicketHandler.TicketsInBasket[0].Stake = 10;


            TicketHandler.TicketsInBasket[0].SystemX = 2;
            TicketHandler.UpdateTicket();

            Assert.AreEqual(10m, TicketHandler.Stake);
            Assert.AreEqual(783.9422976000000m, TicketHandler.CurrentTicketPossibleWin);
            Assert.AreEqual(10m, TicketHandler.BonusPercentage);
            Assert.AreEqual(71.2674816000000m, TicketHandler.BonusValue);

            var ticketActions = new TicketActions();
            TicketWS ticket = ticketActions.CreateNewTicketWS(TicketHandler.TicketsInBasket[0]);
            ticketActions.SaveTicket(ref ticket, ChangeTracker.CurrentUser);
            Assert.AreEqual(10m, ticket.stake);
            Assert.AreEqual(1.1m, ticket.superBonus);
        }





    }
}