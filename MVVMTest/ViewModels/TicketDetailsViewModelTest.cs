using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportBetting.WPF.Prism.Models;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;
using WsdlRepository.WsdlServiceReference;
using Nbt.Station.Design;

namespace MVVMTest
{
    [TestClass]
    public class TicketDetailsViewModelTest : BaseTestClass
    {
        Random r = new Random();
        string station;
        decimal odds;
        int ways;
        int bankers;

        [TestInitialize]
        public void SetUp()
        {
            odds = (decimal)r.NextDouble() * 10 + 1;
            station = r.Next(10000).ToString("0000");
            StationRepository.Object.FranchisorID = r.Next(100);
            StationRepository.Setup(x => x.LocationID).Returns(r.Next(100));

            ways = 0;
            bankers = 0;

            StationRepository.Setup(x => x.AllowAnonymousBetting).Returns(true);

            // multistrings
            TranslationProvider.Setup(x => x.Translate(MultistringTags.SINGLES)).Returns(Bet.BET_TYPE_SINGLE);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.Multiple)).Returns(Bet.BET_TYPE_COMBI);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.Multiple_and_Ways)).Returns(Bet.BET_TYPE_COMBIPATH);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.Full_Cover)).Returns(Bet.BET_TYPE_SYSTEM);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_PRINT_SYSTEM)).Returns(Bet.BET_TYPE_SYSTEMPATH);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.VERSUS)).Returns("singles");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_TICKETOPEN)).Returns("open");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_CANCELLED)).Returns("cancelled");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_TICKETWON)).Returns("won");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_TICKETLOST)).Returns("lost");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.terminal_yes)).Returns("Yes");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.terminal_no)).Returns("No");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TRANSFER_TO_CASHPOOL)).Returns("cashpool");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TRANSFER_TO_ACCOUNT)).Returns("account");

            // common ticket details
            ChangeTracker.Setup(x => x.CurrentTicket).Returns(new TicketWS());
            ChangeTracker.Object.CurrentTicket.ticketNbr = station + r.Next(100000000).ToString("00000000");
            ChangeTracker.Object.CurrentTicket.checkSum = r.Next(10000).ToString("0000");
            ChangeTracker.Object.CurrentTicket.stake = r.Next(100) / 10m + 1;
            ChangeTracker.Object.CurrentTicket.franchisorId = StationRepository.Object.FranchisorID;
            ChangeTracker.Object.CurrentTicket.locationId = StationRepository.Object.LocationID;
            ChangeTracker.Object.CurrentTicket.wonAmount = 0;
            ChangeTracker.Object.CurrentTicket.wonExpireTime = DateTime.Now.AddDays(5);
            ChangeTracker.Object.CurrentTicket.acceptedTime = System.DateTime.Now;
            ChangeTracker.Object.CurrentTicket.paid = false;
            ChangeTracker.Object.CurrentTicket.manipulationFee = 0;
            ChangeTracker.Object.CurrentTicket.manipulationFeeValue = 0;
            ChangeTracker.Object.CurrentTicket.superBonus = 1;
            ChangeTracker.Object.CurrentTicket.superBonusValue = 0;
        }


        /// <summary>
        /// Simple Single Open ticket
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_SingleOpen()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.calculated = false;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        HashSet<string> raisedProperties = new HashSet<string>();
       
        [TestMethod]
        public void TicketDetailsView_SingleOpenStakeperRow()
        {
            raisedProperties.Clear();
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.calculated = false;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;

            var model = new TicketDetailsViewModel();

            model.PropertyChanged += model_PropertyChanged;
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
            Assert.IsTrue(raisedProperties.Contains("StakePerRow"));
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!raisedProperties.Contains(e.PropertyName))
                raisedProperties.Add(e.PropertyName);
        }


        /// <summary>
        /// Multi ticket and Manipulation Fee, Lost ticket
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_MultiFee()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = false;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_COMBI } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[] { new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            ChangeTracker.Object.CurrentTicket.manipulationFee = 0.1m;
            ChangeTracker.Object.CurrentTicket.manipulationFeeValue = ChangeTracker.Object.CurrentTicket.stake * ChangeTracker.Object.CurrentTicket.manipulationFee;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// System and Super Bonus
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_SystemBonus()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.cancelled = false;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SYSTEM } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].systemX = 2;
            ChangeTracker.Object.CurrentTicket.bets[0].systemY = 3;
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 3;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            ChangeTracker.Object.CurrentTicket.superBonus = 1.1m;
            ChangeTracker.Object.CurrentTicket.superBonusValue = ChangeTracker.Object.CurrentTicket.stake * odds * ChangeTracker.Object.CurrentTicket.superBonus;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// System with Banker, Canceled Ticket
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_SystemBankers()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            bankers = 1;
            ChangeTracker.Object.CurrentTicket.cancelled = true;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SYSTEM } };
            ChangeTracker.Object.CurrentTicket.bets[0].systemX = 2;
            ChangeTracker.Object.CurrentTicket.bets[0].systemY = 3;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].bank = true;

            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS.betDomainName = "{0}sdg";

            ChangeTracker.Object.CurrentTicket.bets[0].rows = 3;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// Multi with Ways, Won expired Ticket
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_MultiWays()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ways = 1;
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Object.CurrentTicket.wonExpireTime = DateTime.Now.AddSeconds(-1);
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_COMBIPATH } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[] { new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].bankGroupID = 1;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].bankGroupID = 1;

            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 2;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            ChangeTracker.Object.CurrentTicket.superBonus = 1.1m;
            ChangeTracker.Object.CurrentTicket.superBonusValue = ChangeTracker.Object.CurrentTicket.stake * odds * ChangeTracker.Object.CurrentTicket.superBonus;
            ChangeTracker.Object.CurrentTicket.manipulationFee = 0.1m;
            ChangeTracker.Object.CurrentTicket.manipulationFeeValue = ChangeTracker.Object.CurrentTicket.stake * ChangeTracker.Object.CurrentTicket.manipulationFee;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// System with Ways, view by Logged user
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_SystemWays()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 100));
            ways = 2;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SYSTEMPATH } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].bankGroupID = 1;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].bankGroupID = 1;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[2].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[2].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[2].bankGroupID = 2;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].bankGroupID = 2;

            ChangeTracker.Object.CurrentTicket.bets[0].systemX = 2;
            ChangeTracker.Object.CurrentTicket.bets[0].systemY = 4;
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[3].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[3].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 24;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// System with Ways and Bankers
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_SystemBankerWays()
        {
            ways = 1;
            bankers = 1;
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 100));
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SYSTEMPATH } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[0].bankGroupID = 1;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[1].bankGroupID = 1;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[2].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[2].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[2].bankGroupID = 1;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].bankGroupID = 2;
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti[3].bank = true;

            ChangeTracker.Object.CurrentTicket.bets[0].systemX = 2;
            ChangeTracker.Object.CurrentTicket.bets[0].systemY = 3;
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[] { new TipWS() { }, new TipWS() { }, new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[1].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[2].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 9;
            ChangeTracker.Object.CurrentTicket.isAnonymous = false;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// Won anonymous ticket from current location
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_Won()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.wonAmount = ChangeTracker.Object.CurrentTicket.stake * odds;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }
        [TestMethod]
        public void TicketDetailsView_WonExpired()
        {
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_TICKETEXPIRED)).Returns("expired");
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.wonAmount = ChangeTracker.Object.CurrentTicket.stake * odds;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            ChangeTracker.Object.CurrentTicket.wonExpired = true;
            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// Won anonymous ticket from another franchisor
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_WonAnotherFran()
        {
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.wonAmount = ChangeTracker.Object.CurrentTicket.stake * odds;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            ChangeTracker.Object.CurrentTicket.franchisorId++;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// Won anonymous ticket from another location
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_WonAnotherLoc()
        {
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("211", 211));
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.wonAmount = ChangeTracker.Object.CurrentTicket.stake * odds;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;
            ChangeTracker.Object.CurrentTicket.locationId++;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// Won anonymous ticket open by Logged user
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_WonViewLogged()
        {
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(1, "211", 100, 10, 10, 10));
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.wonAmount = ChangeTracker.Object.CurrentTicket.stake * odds;
            ChangeTracker.Object.CurrentTicket.isAnonymous = true;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        /// <summary>
        /// Won user ticket open by Logged user
        /// </summary>
        [TestMethod]
        public void TicketDetailsView_WonLogged()
        {
            ChangeTracker.Object.CurrentTicket.calculated = true;
            ChangeTracker.Object.CurrentTicket.won = true;
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new LoggedInUser(1, "211", 100, 10, 10, 10));
            ChangeTracker.Object.CurrentTicket.bets = new BetWS[1] { new BetWS() { betType = Bet.BET_TYPE_SINGLE } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips = new TipWS[1] { new TipWS() { } };
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS = new TipDetailsWS();
            ChangeTracker.Object.CurrentTicket.bets[0].bankTips[0].tipDetailsWS.betDomainName = "{0}sdg";
            ChangeTracker.Object.CurrentTicket.bets[0].tips2BetMulti = new TipWS[0];
            ChangeTracker.Object.CurrentTicket.bets[0].rows = 1;
            ChangeTracker.Object.CurrentTicket.wonAmount = ChangeTracker.Object.CurrentTicket.stake * odds;
            ChangeTracker.Object.CurrentTicket.isAnonymous = false;

            var model = new TicketDetailsViewModel();
            model.OnNavigationCompleted();
            ValidateTicketDetailsView(model);
        }

        private void ValidateTicketDetailsView(TicketDetailsViewModel model)
        {
            PrintTestTicketInfo();
            var ticket = ChangeTracker.Object.CurrentTicket;
            string betType = ticket.bets[0].betType;
            if (ticket.bets[0].betType == Bet.BET_TYPE_SYSTEM || ticket.bets[0].betType == Bet.BET_TYPE_SYSTEMPATH)
            {
                betType += " " + ticket.bets[0].systemX + "/" + ticket.bets[0].systemY;
                if (bankers != 0)
                    betType += "+" + bankers + "B";
                if (ways != 0)
                    betType += "+" + ways + "W";
            }

            string status = "open";
            if (ticket.calculated && ticket.won)
                status = "won";
            else if (ticket.calculated)
                status = "lost";
            else if (ticket.cancelled)
                status = "cancelled";
            if (ticket.wonExpired)
                status = "EXPIRED";

            string showStakePerRow = "";
            if (ticket.bets[0].rows > 1)
                showStakePerRow = "1";

            Assert.AreEqual(status, model.TabText);
            Assert.AreEqual(station, model.Station);
            Assert.AreEqual(ticket.acceptedTime, model.Date);
            Assert.AreEqual(ticket.paid ? "Yes" : "No", model.Paid);
            Assert.AreEqual(ticket.ticketNbr, model.TicketNumber);
            Assert.AreEqual(betType, model.BetType);
            Assert.AreEqual(ticket.paid ? string.Format("{0:dd.MM.yyyy HH:mm}", ticket.paidTime) : "-", model.PaidAt);
            Assert.AreEqual(ticket.stake, model.Stake);
            Assert.AreEqual(ticket.manipulationFee != 0, model.ShowManipulationFee);
            Assert.AreEqual(ticket.superBonusValue != 0, model.ShowSuperBonus);
            decimal bonusPercent = (ticket.superBonus - 1) * 100m;
            Assert.AreEqual(bonusPercent, model.SuperBonusPercent);
            Assert.AreEqual(showStakePerRow, model.ShowStakePerRow);
            Assert.AreEqual(ticket.stake / ticket.bets[0].rows, model.StakePerRow);

            bool showTransferButton = !ticket.wonExpired && ticket.won && ticket.isAnonymous && !ticket.paid && (StationRepository.Object.LocationID == ticket.locationId) && (StationRepository.Object.FranchisorID == ticket.franchisorId) && (ticket.wonExpireTime > DateTime.Now);
            Assert.AreEqual(showTransferButton, model.ShowTranferToAccountButton);
            bool showCreditButton = showTransferButton && (ChangeTracker.Object.CurrentUser is AnonymousUser);
            Assert.AreEqual(showCreditButton, model.ShowCreditNoteButton);
        }

        private void PrintTestTicketInfo()
        {
            Console.WriteLine("Test ticket details:");
            Console.WriteLine("Ticket Nbr: " + ChangeTracker.Object.CurrentTicket.ticketNbr);
            Console.WriteLine("Ticket Checksum: " + ChangeTracker.Object.CurrentTicket.checkSum);
            Console.WriteLine("Ticket stake: " + ChangeTracker.Object.CurrentTicket.stake);
            Console.WriteLine("Ticket franchisorId: " + ChangeTracker.Object.CurrentTicket.franchisorId);
            Console.WriteLine("Ticket locationId: " + ChangeTracker.Object.CurrentTicket.locationId);
            Console.WriteLine("Ticket wonAmount: " + ChangeTracker.Object.CurrentTicket.wonAmount);
            Console.WriteLine("Ticket wonExpireTime: " + ChangeTracker.Object.CurrentTicket.wonExpireTime);
            Console.WriteLine("Ticket acceptedTime: " + ChangeTracker.Object.CurrentTicket.acceptedTime);
            Console.WriteLine("Ticket paid: " + ChangeTracker.Object.CurrentTicket.paid);
            Console.WriteLine("Ticket manipulationFee: " + ChangeTracker.Object.CurrentTicket.manipulationFee);
            Console.WriteLine("Ticket manipulationFeeValue: " + ChangeTracker.Object.CurrentTicket.manipulationFeeValue);
            Console.WriteLine("Ticket superBonus: " + ChangeTracker.Object.CurrentTicket.superBonus);
            Console.WriteLine("Ticket superBonusValue: " + ChangeTracker.Object.CurrentTicket.superBonusValue);
            Console.WriteLine("Ticket rows: " + ChangeTracker.Object.CurrentTicket.bets[0].rows);
            Console.WriteLine("Ticket StakePerRow: " + ChangeTracker.Object.CurrentTicket.stake / ChangeTracker.Object.CurrentTicket.bets[0].rows);
        }

    }
}