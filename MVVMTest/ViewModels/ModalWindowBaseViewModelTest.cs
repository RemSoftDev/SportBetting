using System.Collections.Generic;
using System.Collections.ObjectModel;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class BaseViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void CreditNoteAnonymousUserCashpoolEnabledTest()
        {
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            StationRepository.Setup(x => x.SetCashInDefaultState(It.IsAny<decimal>()));
            StationRepository.Setup(x => x.DisableCashIn());

            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(10m);

            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 1000;
            WsdlRepository.Setup(x => x.DepositByCreditNote(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("1",1));
            var model = new BaseViewModel();

            model.GetMoneyFromCreditNote(new CreditNoteWS() { amount = 10 });


            Assert.AreEqual(ChangeTracker.Object.CurrentUser.Cashpool, 10);
            Assert.AreEqual(TicketHandler.Object.TicketsInBasket[0].Stake, 0);
        }

        [TestMethod]
        public void CreditNoteAnonymousUserCashpoolDisabledTest()
        {
            //StationRepository.BackToRecord();
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());

            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            StationRepository.Setup(x => x.SetCashInDefaultState(It.IsAny<decimal>()));
            StationRepository.Setup(x => x.DisableCashIn());
            decimal reserved;
            decimal factor;
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out reserved, out factor)).Returns(10m);


            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            TicketHandler.Object.TicketsInBasket[0] = new Ticket();
            TicketHandler.Object.TicketsInBasket[0].MaxBet = 1000;
            TicketHandler.Object.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.9M } }));
            WsdlRepository.Setup(x => x.DepositByCreditNote(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            //StationRepository.Replay();
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            var model = new BaseViewModel();

            model.GetMoneyFromCreditNote(new CreditNoteWS() { amount = 10 });


            Assert.AreEqual(ChangeTracker.CurrentUser.Cashpool, 10);
            Assert.AreEqual(TicketHandler.Object.TicketsInBasket[0].Stake, 0);
        }

        [TestMethod]
        public void CreditNoteLoggedInUserCashpoolEnabledTest()
        {

            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            WsdlRepository.Setup(x => x.DepositByCreditNote(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(1, "1", 0,10,10,10));
            var model = new BaseViewModel();

            model.GetMoneyFromCreditNote(new CreditNoteWS() { amount = 10 });


            Assert.AreEqual(ChangeTracker.Object.CurrentUser.Cashpool, 0);
            Assert.AreEqual(TicketHandler.Object.TicketsInBasket[0].Stake, 0);
        }

        [TestMethod]
        public void CreditNoteLoggedInUserCashpoolDisabledTest()
        {

            decimal amount = 0;
            decimal factor;
            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);
            StationRepository.Setup(x => x.GetUid(It.IsAny<User>())).Returns(new uid());
            BusinessPropsHelper.Setup(x => x.GetNextTransactionId()).Returns("123");
            TicketHandler.Setup(x => x.TicketsInBasket).Returns(new SyncObservableCollection<Ticket>());
            TicketHandler.Object.TicketsInBasket.Add(new Ticket());
            WsdlRepository.Setup(x => x.DepositByCreditNote(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            WsdlRepository.Setup(x => x.GetBalance(It.IsAny<uid>(), out amount, out factor)).Returns(1010);
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(1, "1", 1000,10,10,10));
            string error = "";
            TransactionQueueHelper.Setup(x => x.TryDepositByCreditNoteMoneyOnHub(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>(), ref error)).Returns(true);
            var model = new BaseViewModel();

            model.GetMoneyFromCreditNote(new CreditNoteWS() { amount = 10 });


            WsdlRepository.Verify(x => x.GetBalance(It.IsAny<uid>(), out amount, out factor));;

            Assert.AreEqual(ChangeTracker.Object.CurrentUser.Cashpool, 1010);
            Assert.AreEqual(TicketHandler.Object.TicketsInBasket[0].Stake, 0);
        }

        [TestMethod]
        public void TipItems4stake1eurTest()
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
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);
            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            TicketHandler.TicketsInBasket[2].MaxBet = 500;
            TicketHandler.TicketsInBasket[3].MaxBet = 500;

            TicketHandler.OnChangeStake("10", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);


            Assert.AreEqual(TicketHandler.Stake, 10);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10M);
        }

        [TestMethod]
        public void TipItems4stakeUncheckedMaxBetTest()
        {

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
           var  TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();
           TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(5000);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(5000);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10,10,10,10);
            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            TicketHandler.TicketsInBasket[2].MaxBet = 500;

            TicketHandler.TicketsInBasket[0].TipItems[0].IsChecked = false;

            var returnString = TicketHandler.OnChangeStake("max", TicketHandler.TicketsInBasket[1], ChangeTracker.CurrentUser.Cashpool);


            //Assert.AreEqual(MultistringTags.ADD_XX_TO_STAKE, returnString.Item1);
            //Assert.AreEqual("490", returnString.Item2[0]);
            Assert.AreEqual(TicketHandler.Stake, 10m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 0M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[1].Stake, 10M);
            Assert.AreEqual(TicketHandler.TicketsInBasket[2].Stake, 0M);
        }



       

        [TestMethod]
        public void SingleToMultyTest()
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

            var testOdd = new TestOdd()
            {
                OddId = { Value = 1 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 1 } },
                Value = { Value = 1 },
                OutcomeId = 1
            };


            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);

            var model = new BaseViewModel();
            model.PlaceBetMethod(new TipItemVw(testOdd));
            Assert.AreEqual(TicketHandler.TicketState, TicketStates.Single);
            model.PlaceBetMethod(new TipItemVw(new TestOdd()
            {
                OddId = { Value = 2 },
                BetDomain = new TestBetDomain() { Match = new TestMatchLn() { MatchId = 2 } },
                Value = { Value = 2 },
                OutcomeId = 2

            }));

            Assert.AreEqual(TicketHandler.TicketState, TicketStates.Multy);
            Assert.AreEqual(TicketHandler.TicketsInBasket.Count, 1);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].TipItems.Count, 2);

        }

        
     

      


       
        [TestMethod]
        public void TipItems3stake1eurTest()
        {
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
           var  ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            TicketHandler.TicketsInBasket[2].MaxBet = 500;

            TicketHandler.OnChangeStake("10", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);


            Assert.AreEqual(TicketHandler.Stake, 10m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10M);
        }

        [TestMethod]
        public void TipItems3stake1eur3TimesTest()
        {
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            var TicketHandler = new TicketHandler();
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));


            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 1000,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            TicketHandler.TicketsInBasket[2].MaxBet = 500;
            TicketHandler.OnChangeStake("*1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake);

            Assert.AreEqual(TicketHandler.Stake, 1m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1M);

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.OnChangeStake("*1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake);

            Assert.AreEqual(TicketHandler.Stake, 11m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 11M);

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.OnChangeStake("*1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake);

            Assert.AreEqual(TicketHandler.Stake, 111m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 111M);
        }

        [TestMethod]
        public void TipItems8stake1eurTest()
        {

            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[4].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[5].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[6].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[7].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 500;
            TicketHandler.TicketsInBasket[1].MaxBet = 500;
            TicketHandler.TicketsInBasket[2].MaxBet = 500;
            TicketHandler.TicketsInBasket[3].MaxBet = 500;
            TicketHandler.TicketsInBasket[4].MaxBet = 500;
            TicketHandler.TicketsInBasket[5].MaxBet = 500;
            TicketHandler.TicketsInBasket[6].MaxBet = 500;
            TicketHandler.TicketsInBasket[7].MaxBet = 500;

            TicketHandler.OnChangeStake("1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);


            Assert.AreEqual(TicketHandler.Stake, 1m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1M);
        }

        [TestMethod]
        public void TipItems3stake1toTimeseurTest()
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
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100000,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;

            TicketHandler.TicketsInBasket[0].MaxBet = 50000;
            TicketHandler.TicketsInBasket[1].MaxBet = 50000;
            TicketHandler.TicketsInBasket[2].MaxBet = 50000;
            TicketHandler.OnChangeStake("*1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            TicketHandler.OnChangeStake("*0", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.Stake, 10m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10M);

            TicketHandler.OnChangeStake("*0", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.Stake, 100m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 100M);

            TicketHandler.OnChangeStake("*1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.Stake, 1001m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1001M);
        }
       
        [TestMethod]
        public void TipItems7stake1eurTest()
        {

            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("{0}");
            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            StationRepository.Setup(x => x.IsReady).Returns(true);
            StationRepository.Setup(x => x.GetMaxWinSingleBet(It.IsAny<Ticket>())).Returns(500);
            StationRepository.Setup(x => x.GetMaxStakeSingleBet(It.IsAny<Ticket>())).Returns(500);
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket.Add(new Ticket());


            TicketHandler.TicketsInBasket[0].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[1].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[2].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[3].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[4].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[5].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            TicketHandler.TicketsInBasket[6].TipItems.Add(new TipItemVw(new TestOdd() { BetDomain = new TestBetDomain() { Match = new TestMatchLn() }, Value = { Value = 1 } }));
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 100,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.TicketState = TicketStates.Single;
            TicketHandler.TicketsInBasket[0].MaxBet = 5000;
            TicketHandler.TicketsInBasket[1].MaxBet = 5000;
            TicketHandler.TicketsInBasket[2].MaxBet = 5000;
            TicketHandler.TicketsInBasket[3].MaxBet = 5000;
            TicketHandler.TicketsInBasket[4].MaxBet = 5000;
            TicketHandler.TicketsInBasket[5].MaxBet = 5000;
            TicketHandler.TicketsInBasket[6].MaxBet = 5000;

            TicketHandler.OnChangeStake("1", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("*0", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);


            Assert.AreEqual(TicketHandler.Stake, 10m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10M);


            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);


            Assert.AreEqual(TicketHandler.Stake, 0m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 0M);

            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);


            Assert.AreEqual(TicketHandler.Stake, 0m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 0m);
        }
        [TestMethod]
        public void ChangeStakeTest()
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
            TranslationProvider.Setup(x => x.Translate(MultistringTags.ADD_XX_TO_STAKE)).Returns("{0}");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_STAKE_EXCEEDED_MAXBET)).Returns("maxbet");


            TipItemVw tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            TicketHandler.TicketsInBasket[0].MaxBet = 5000;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.20m,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.OnChangeStake("max", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("max", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("max", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10.20m);
            var resultString = TicketHandler.OnChangeStake("5", TicketHandler.TicketsInBasket[0], 0);

            //Assert.AreEqual(MultistringTags.TERMINAL_STAKE_EXCEEDED_MAXBET, resultString.Item1);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].MaxBet, 5000.00m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10.20m);



        }


        [TestMethod]
        public void BackChangeStakeTest()
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


            TipItemVw tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            TicketHandler.TicketsInBasket[0].MaxBet = 5000;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10000.00m,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.OnChangeStake("max", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].MaxBet, 5000m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 5000.00m);

            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.TicketsInBasket[0].MaxBet, 5000m);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 500m);


        }


        [TestMethod]
        public void ChangeStakeBackTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();


            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            TipItemVw tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0] = new Ticket();
            TicketHandler.TicketsInBasket[0].MaxBet = 5000;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 1000.20m,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.OnChangeStake("+0.50", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 0.50m);
            TicketHandler.OnChangeStake("+0.50", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1.00m);
            TicketHandler.OnChangeStake("+100", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            TicketHandler.OnChangeStake("+0.50", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 101.50m);

            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 101.00m);
            TicketHandler.OnChangeStake("back", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);

            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 10.00m);


        }
        [TestMethod]
        public void ChangeStake50centsTest()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            IoCContainer.Kernel.Unbind<IChangeTracker>();
            IoCContainer.Kernel.Bind<IChangeTracker>().To<ChangeTracker>().InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            IoCContainer.Kernel.Unbind<ITicketHandler>();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            var TicketHandler = IoCContainer.Kernel.Get<ITicketHandler>();

            TipItemVw tiv1 = new TipItemVw(new TestOdd() { OutcomeId = 1, OddId = { Value = 1 }, BetDomain = new TestBetDomain() { BetDomainId = 1, Match = new TestMatchLn() { MatchId = 1 } }, Value = { Value = 1.85M } }) { IsChecked = true };

            TicketHandler.TicketsInBasket.Add(new Ticket());
            TicketHandler.TicketsInBasket[0].MaxBet = 5000;
            TicketHandler.TicketsInBasket[0].TipItems.Add(tiv1);
            ChangeTracker.CurrentUser = new LoggedInUser(0, "", 10.20m,10,10,10);

            var model = new BaseViewModel();
            TicketHandler.OnChangeStake("+0.50", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 0.50m);

            TicketHandler.OnChangeStake("+0.50", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1.00m);

            TicketHandler.OnChangeStake("+0.50", TicketHandler.TicketsInBasket[0], ChangeTracker.CurrentUser.Cashpool);
            Assert.AreEqual(TicketHandler.TicketsInBasket[0].Stake, 1.50m);


        }


    }


}