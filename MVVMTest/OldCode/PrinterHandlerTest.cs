using System.Globalization;
using System.Threading;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class PrinterHandlerTest : BaseTestClass
    {
        [TestMethod]
        [Ignore]
        public void PrintCreditNoteReceptTest()
        {
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();

            var rep = IoCContainer.Kernel.Get<IStationRepository>();
            rep.Init();
            var PrinterHandler = new PrinterHandler();
            PrinterHandler.PrintPaymentRecept("999933333333333333", "2222", 200, true);
        }

        [TestMethod]
        [Ignore]
        public void PrintPaymentNoteReceptTest()
        {
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();
            var PrinterHandler = new PrinterHandler();

            var rep = IoCContainer.Kernel.Get<IStationRepository>();
            rep.Init();
            rep.UsePrinter = true;
            PrinterHandler.PrintPaymentRecept("999933333333333333", "", 200, false);
        }


        [TestMethod]
        public void PrintdublicateTest()
        {
            IoCContainer.Kernel.Rebind<IStationRepository>().To<StationRepository>().InSingletonScope();

            var rep = IoCContainer.Kernel.Get<IStationRepository>();
            //rep.Init();
            rep.UsePrinter = true;

            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            var ChangeTracker = new ChangeTracker();
            IoCContainer.Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(ChangeTracker).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var TicketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(TicketHandler).InSingletonScope();

            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(stationRepository).InSingletonScope();
            stationRepository.MaxOdd = 1000;
            stationRepository.MaxCombination = 1000;
            stationRepository.MaxStakeSystemBet = 1000;
            stationRepository.MaxStakeCombi = 1000;
            stationRepository.MaxSystemBet = 1000;
            stationRepository.MaxWinSystemBet = 10000;
            stationRepository.MaxWinSingleBet = 10000;
            stationRepository.IsReady = true;
            stationRepository.BonusFromOdd = 1.30m;
            stationRepository.UsePrinter = true;
            StationSettings.Setup(x => x.UsePrinter).Returns(true);
            long[] tipLock;
            long[] tournamentlock;
            TicketHandler.TicketsInBasket.Add(new Ticket());
            WsdlRepository.Setup(x => x.SaveTicket(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<TicketWS>(), It.IsAny<bool>(), It.IsAny<string>(), out tipLock, out tournamentlock)).Returns("1");
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 1, OddId = { Value = 1 }, OddView = new TestOddVw() { Value = 1 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 1, Match = new TestMatchLn { MatchId = 1, Code = { Value = 1 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.14M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 2, OddId = { Value = 2 }, OddView = new TestOddVw() { Value = 2 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 2, Match = new TestMatchLn { MatchId = 2, Code = { Value = 2 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 3, OddId = { Value = 3 }, OddView = new TestOddVw() { Value = 3 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 3, Match = new TestMatchLn { MatchId = 3, Code = { Value = 3 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.50M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 4, OddId = { Value = 4 }, OddView = new TestOddVw() { Value = 4 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 4, Match = new TestMatchLn { MatchId = 4, Code = { Value = 4 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.25M } }));


            stationRepository.BonusRangeList.Add(new BonusRangeWS {tipSize = 1, bonus = 3});


            TicketHandler.TicketsInBasket[0].Stake = 10;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            TicketHandler.TicketsInBasket[0].Stake = 5;


            TicketHandler.UpdateTicket();


            var ticketActions = new TicketActions();
            TicketWS ticket = ticketActions.CreateNewTicketWS(TicketHandler.TicketsInBasket[0]);
            ticketActions.SaveTicket(ref ticket, ChangeTracker.CurrentUser);

            var PrinterHandler = new PrinterHandler();
            stationRepository.UsePrinter = true;
            PrinterHandler.PrintTicket(ticket, false);

            Assert.AreEqual(PrinterHandler.NotPrintedItemsCount, 1);
            PrinterHandler.PrintTicket(ticket, false);
            Assert.AreEqual(PrinterHandler.NotPrintedItemsCount, 2);

            PrinterHandler.DeleteAllPrinterObjects();
            Assert.AreEqual(0, PrinterHandler.NotPrintedItemsCount);
        }


        [TestMethod]
        public void PrinterLanguageDefaultStationLanguageTest()
        {
            var rep = IoCContainer.Kernel.Get<IStationRepository>();
            IoCContainer.Kernel.Rebind<ITranslationProvider>().To<DBTranslationProvider>().InSingletonScope();
            var translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>();

            translationProvider.CurrentLanguage = "Fr";
            StationRepository.Setup(x => x.PrintingLanguageSetting).Returns(1);
            StationRepository.Setup(x => x.DefaultDisplayLanguage).Returns("It");
            rep.Init();
            rep.UsePrinter = true;
            var PrinterHandler = new PrinterHandler();
            var printer = PrinterHandler.InitPrinter(true);
            Assert.AreEqual("It", translationProvider.PrintingLanguage);
        }


        [TestMethod]
        public void CreateTicketWsAndPrintTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            var ChangeTracker = new ChangeTracker();
            IoCContainer.Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(ChangeTracker).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var TicketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(TicketHandler).InSingletonScope();

            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(stationRepository).InSingletonScope();
            stationRepository.MaxOdd = 1000;
            stationRepository.MaxCombination = 1000;
            stationRepository.MaxStakeSystemBet = 1000;
            stationRepository.MaxSystemBet = 1000;
            stationRepository.MaxWinSystemBet = 10000;
            stationRepository.IsReady = true;
            stationRepository.BonusFromOdd = 3.0m;
            long[] tipLock;
            long[] tournamentlock;
            WsdlRepository.Setup(x => x.SaveTicket(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<TicketWS>(), It.IsAny<bool>(), It.IsAny<string>(), out tipLock, out tournamentlock)).Returns("1");
            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 1, OddId = { Value = 1 }, OddView = new TestOddVw() { Value = 1 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 1, Match = new TestMatchLn { MatchId = 1, Code = { Value = 1 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 2.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 2, OddId = { Value = 2 }, OddView = new TestOddVw() { Value = 2 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 2, Match = new TestMatchLn { MatchId = 2, Code = { Value = 2 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 3, OddId = { Value = 3 }, OddView = new TestOddVw() { Value = 3 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 3, Match = new TestMatchLn { MatchId = 3, Code = { Value = 3 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 3.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 4, OddId = { Value = 4 }, OddView = new TestOddVw() { Value = 4 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 4, Match = new TestMatchLn { MatchId = 4, Code = { Value = 4 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 2.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 5, OddId = { Value = 5 }, OddView = new TestOddVw() { Value = 5 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 5, Match = new TestMatchLn { MatchId = 5, Code = { Value = 5 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 3.10M } }));


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 6, OddId = { Value = 6 }, OddView = new TestOddVw() { Value = 6 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 6, Match = new TestMatchLn { MatchId = 6, Code = { Value = 6 } } }, Value = { Value = 1.80M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 7, OddId = { Value = 7 }, OddView = new TestOddVw() { Value = 7 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 7, Match = new TestMatchLn { MatchId = 7, Code = { Value = 7 } } }, Value = { Value = 1.70M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 8, OddId = { Value = 8 }, OddView = new TestOddVw() { Value = 8 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 8, Match = new TestMatchLn { MatchId = 8, Code = { Value = 8 } } }, Value = { Value = 3.20M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 9, OddId = { Value = 9 }, OddView = new TestOddVw() { Value = 9 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 9, Match = new TestMatchLn { MatchId = 8, Code = { Value = 8 } } }, Value = { Value = 2.10M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd {OutcomeId = 10, OddId = {Value = 10},OddView = new TestOddVw(){Value = 10},BetDomain = new TestBetDomain {BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 10, Match = new TestMatchLn {MatchId = 9, Code = {Value = 9}}}, Value = {Value = 5.60M}}) {IsBank = true});
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 11, OddId = { Value = 11 }, OddView = new TestOddVw() { Value = 11 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 11, Match = new TestMatchLn { MatchId = 9, Code = { Value = 9 } } }, Value = { Value = 3.70M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 12, OddId = { Value = 12 }, OddView = new TestOddVw() { Value = 12 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 12, Match = new TestMatchLn { MatchId = 9, Code = { Value = 9 } } }, Value = { Value = 1.55M } }) { IsBank = true });

            TicketHandler.TicketsInBasket[0].Stake = 10;
            stationRepository.BonusRangeList.Add(new BonusRangeWS {tipSize = 1, bonus = 5});
            stationRepository.BonusRangeList.Add(new BonusRangeWS {tipSize = 2, bonus = 10});

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
            stationRepository.UsePrinter = true;
            Repository.Expect(x => x.GetOddBySvrId(It.IsAny<long>())).Returns(new OddVw(new OddLn {NameTag = {Value = "test"}}));
            TranslationProvider.Object.PrintingLanguage = "EN";
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            PrinterHandler printerHandler = new PrinterHandler();
            var xml = printerHandler.CreateTicketXmlForPrinting(ticket, false, null);
            var stringxml = xml.ToString();
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                Assert.IsTrue(stringxml.Contains("71.2"));
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                Assert.IsTrue(stringxml.Contains("71,2"));
        }

        [TestMethod]
        public void CreateTicketWsTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Expect(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            var ChangeTracker = new ChangeTracker();
            IoCContainer.Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(ChangeTracker).InSingletonScope();

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            IoCContainer.Kernel.Unbind<ITicketHandler>();
            var TicketHandler = new TicketHandler();
            IoCContainer.Kernel.Bind<ITicketHandler>().ToConstant<ITicketHandler>(TicketHandler).InSingletonScope();

            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(stationRepository).InSingletonScope();
            stationRepository.MaxOdd = 1000;
            stationRepository.MaxCombination = 1000;
            stationRepository.MaxStakeSystemBet = 1000;
            stationRepository.MaxStakeCombi = 1000;
            stationRepository.MaxSystemBet = 1000;
            stationRepository.MaxWinSystemBet = 10000;
            stationRepository.MaxWinSingleBet = 10000;
            stationRepository.IsReady = true;
            stationRepository.BonusFromOdd = 1.30m;
            long[] tipLock;
            long[] tournamentlock;
            TicketHandler.TicketsInBasket.Add(new Ticket());
            WsdlRepository.Setup(x => x.SaveTicket(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<TicketWS>(), It.IsAny<bool>(), It.IsAny<string>(), out tipLock, out tournamentlock)).Returns("1");
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 1, OddId = { Value = 1 }, OddView = new TestOddVw() { Value = 1 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 1, Match = new TestMatchLn { MatchId = 1, Code = { Value = 1 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.14M } }) { IsBank = true });
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 2, OddId = { Value = 2 }, OddView = new TestOddVw() { Value = 2 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 2, Match = new TestMatchLn { MatchId = 2, Code = { Value = 2 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.40M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 3, OddId = { Value = 3 }, OddView = new TestOddVw() { Value = 3 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 3, Match = new TestMatchLn { MatchId = 3, Code = { Value = 3 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.50M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 4, OddId = { Value = 4 }, OddView = new TestOddVw() { Value = 4 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 4, Match = new TestMatchLn { MatchId = 4, Code = { Value = 4 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 1.25M } }));


            stationRepository.BonusRangeList.Add(new BonusRangeWS {tipSize = 1, bonus = 3});


            TicketHandler.TicketsInBasket[0].Stake = 10;

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketState = TicketStates.System;
            TicketHandler.TicketsInBasket[0].SystemX = 2;

            TicketHandler.TicketsInBasket[0].Stake = 5;

            TicketWS newticket = new TicketWS();
            TicketActions.Setup(x => x.CreateNewTicketWS(It.IsAny<Ticket>())).Returns(newticket);

            TicketHandler.UpdateTicket();

            Assert.AreEqual(2, TicketHandler.TicketsInBasket[0].SystemX);
            Assert.AreEqual(3, TicketHandler.TicketsInBasket[0].SystemY);
            Assert.AreEqual(5m, TicketHandler.Stake);
            Assert.AreEqual(11.203824999999999999999999999m, TicketHandler.CurrentTicketPossibleWin);


            var ticketActions = new TicketActions();
            TicketWS ticket = ticketActions.CreateNewTicketWS(TicketHandler.TicketsInBasket[0]);
            ticketActions.SaveTicket(ref ticket, ChangeTracker.CurrentUser);


            Assert.AreEqual(1.03m, ticket.superBonus);
            Assert.AreEqual(5m, ticket.stake);
            Assert.AreEqual(2.17m, ticket.bets[0].maxOdd);
            Assert.AreEqual(11.203824999999999999999999999m, TicketHandler.CurrentTicketPossibleWin);
            Assert.AreEqual(11.203824999999999999999999999m, ticket.bets[0].maxWin);
            Assert.AreEqual(3m, TicketHandler.BonusPercentage);
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                Assert.AreEqual("0.32", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(TicketHandler.BonusValue, 2));
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                Assert.AreEqual("0,32", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(TicketHandler.BonusValue, 2));
        }


        [TestMethod]
        public void MultyTicketWsAndPrintTest()
        {
            IoCContainer.Kernel.Unbind<IConfidenceFactor>();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor.Object).InSingletonScope();
            ConfidenceFactor.Setup(x => x.CalculateFactor(It.IsAny<Ticket>())).Returns(1000000);

            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");

            IoCContainer.Kernel.Unbind<IDataBinding>();
            var DataBinding = new DataBinding();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();

            var stationRepository = new StationRepository();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(stationRepository).InSingletonScope();
            stationRepository.MaxOdd = 1000;
            stationRepository.MaxCombination = 1000;
            stationRepository.MaxStakeSystemBet = 1000;
            stationRepository.MaxStakeCombi = 1000;
            stationRepository.MaxSystemBet = 1000;
            stationRepository.MaxWinSystemBet = 10000;
            stationRepository.IsReady = true;
            stationRepository.BonusFromOdd = 3.0m;
            long[] tipLock;
            long[] tournamentlock;
            WsdlRepository.Setup(x => x.SaveTicket(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<TicketWS>(), It.IsAny<bool>(), It.IsAny<string>(), out tipLock, out tournamentlock)).Returns("1");

            TicketHandler.TicketsInBasket.Add(new Ticket());

            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 1, OddId = { Value = 1 }, OddView = new TestOddVw() { Value = 1 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 1, Match = new TestMatchLn { MatchId = 1, Code = { Value = 1 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 2.80M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 2, OddId = { Value = 2 }, OddView = new TestOddVw() { Value = 2 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 2, Match = new TestMatchLn { MatchId = 2, Code = { Value = 2 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 2.10M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 3, OddId = { Value = 3 }, OddView = new TestOddVw() { Value = 3 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 3, Match = new TestMatchLn { MatchId = 3, Code = { Value = 3 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 3.30M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 4, OddId = { Value = 4 }, OddView = new TestOddVw() { Value = 4 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 4, Match = new TestMatchLn { MatchId = 4, Code = { Value = 4 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 2.70M } }));
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd { OutcomeId = 5, OddId = { Value = 5 }, OddView = new TestOddVw() { Value = 5 }, BetDomain = new TestBetDomain { BetDomainNumber = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "test"), BetDomainId = 5, Match = new TestMatchLn { MatchId = 5, Code = { Value = 5 }, MatchView = new TestMatchVw { LineObject = new MatchLn() } } }, Value = { Value = 3.10M } }));


            TicketHandler.TicketsInBasket[0].Stake = 10;
            stationRepository.BonusRangeList.Add(new BonusRangeWS {tipSize = 1, bonus = 5});
            stationRepository.BonusRangeList.Add(new BonusRangeWS {tipSize = 2, bonus = 10});

            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100, 10, 10, 10);

            TicketHandler.TicketState = TicketStates.Multy;

            TicketHandler.TicketsInBasket[0].Stake = 10;


            TicketHandler.UpdateTicket();

            Assert.AreEqual(10m, TicketHandler.Stake);
            Assert.AreEqual(1786.52628000000m, TicketHandler.CurrentTicketPossibleWin);
            Assert.AreEqual(10m, TicketHandler.BonusPercentage);
            Assert.AreEqual(162.41148000000m, TicketHandler.BonusValue);

            var ticketActions = new TicketActions();
            TicketWS ticket = ticketActions.CreateNewTicketWS(TicketHandler.TicketsInBasket[0]);
            ticketActions.SaveTicket(ref ticket, ChangeTracker.CurrentUser);
            Assert.AreEqual(5, ticket.bets[0].bankTips.Length);
            Assert.AreEqual(10m, ticket.stake);
            Assert.AreEqual(1.1m, ticket.superBonus);
            stationRepository.UsePrinter = true;
            Repository.Expect(x => x.GetOddBySvrId(It.IsAny<long>())).Returns(new OddVw(new OddLn {NameTag = {Value = "test"}}));
            TranslationProvider.Object.PrintingLanguage = "EN";
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            PrinterHandler printerHandler = new PrinterHandler();
            var xml = printerHandler.CreateTicketXmlForPrinting(ticket, false, null);
            var stringxml = xml.ToString();
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                Assert.IsTrue(stringxml.Contains("162.4"));
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                Assert.IsTrue(stringxml.Contains("162,4"));
        }

        [TestMethod]
        public void PrinterLanguageDefaultSelectedLanguageTest()
        {
            var rep = IoCContainer.Kernel.Get<IStationRepository>();
            IoCContainer.Kernel.Rebind<ITranslationProvider>().To<DBTranslationProvider>().InSingletonScope();
            var translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>();

            translationProvider.CurrentLanguage = "Fr";
            StationRepository.Setup(x => x.PrintingLanguageSetting).Returns(2);
            StationRepository.Setup(x => x.DefaultDisplayLanguage).Returns("It");
            rep.Init();
            rep.UsePrinter = true;
            var PrinterHandler = new PrinterHandler();
            var printer = PrinterHandler.InitPrinter(true);
            Assert.AreEqual("Fr", translationProvider.PrintingLanguage);
        }
        
        [Ignore]
        [TestMethod]
        public void PrintBarcodeTest()
        {
            StationSettings settings = new StationSettings();
            Kernel.Rebind<IStationSettings>().ToConstant(settings);
            settings.ReadPrefFileData();
            var PrinterHandler = new PrinterHandler();
            var printer = PrinterHandler.InitPrinter(true);
            TranslationProvider.Setup(x => x.TranslateForPrinter(It.IsAny<MultistringTag>())).Returns("123");
            var str = "123456789012345";
            for (int i = 15; i < 25; i++)
            {
                str = str + i.ToString();
                PrinterHandler.WriteBarcodeCardNumber(str);
                
            }
            //Thread.Sleep(1000000);
        }

        [TestMethod]
        public void CheckDecimal2StringConversion()
        {
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
            {
                Assert.AreEqual("15.50", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(15.5M, 2));
                Assert.AreEqual("25.30", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(25.3M, 2));
                Assert.AreEqual("220.00", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(220, 2));
                Assert.AreEqual("9.00", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(9, 2));
                Assert.AreEqual("0.00", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(0, 2));
                Assert.AreEqual("74.39", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(74.39M, 2));
            }
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
            {
                Assert.AreEqual("15,50", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(15.5M, 2));
                Assert.AreEqual("25,30", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(25.3M, 2));
                Assert.AreEqual("220,00", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(220, 2));
                Assert.AreEqual("9,00", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(9, 2));
                Assert.AreEqual("0,00", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(0, 2));
                Assert.AreEqual("74,39", SportBetting.WPF.Prism.OldCode.PrinterHandler.Decimal2String(74.39M, 2));
            }
        }
    }
}