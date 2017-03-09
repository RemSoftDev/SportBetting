using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportBetting.WPF.Prism.Shared.Models;
using MVVMTest.ViewModels;
using SharedInterfaces;
using ViewModels.ViewModels;
using SportRadar.DAL.OldLineObjects;
using Moq;
using SportRadar.Common.Collections;
using SportRadar.DAL.ViewObjects;
using System;
using SportRadar.DAL.NewLineObjects;
using IocContainer;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared;

namespace MVVMTest
{
    [TestClass]
    public class MatchesViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void SportFiltersCheckPreMatch()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            var MessageMediator = new MyMessageMediator();
            IoCContainer.Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator).InSingletonScope();
            ChangeTracker.Setup(x => x.SelectedDescriptorsPreMatch).Returns(new List<string>());

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>());

            var prematch = new MatchesViewModel();
            prematch.OnNavigationCompleted();

            MessageMediator.SendMessage(true, MsgTag.ClearSelectedSports);

            //at start should be only "All sports" available and checked
            Assert.AreEqual(1, prematch.SportsBarItemsPreMatch.Count);
            Assert.AreEqual(1, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, prematch.SelectedDescriptors[0]);

            //if something else is checked, there should be no "All sports" in selected descriptors
            SportBarItem hokkey = new SportBarItem("Ice Hokkey", SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY);
            SportBarItem soccer = new SportBarItem("Soccer", SportSr.SPORT_DESCRIPTOR_SOCCER);
            SportBarItem volley = new SportBarItem("Volleyball", SportSr.SPORT_DESCRIPTOR_VOLLEYBALL);
            prematch.OnCheckedExecute(hokkey);

            Assert.AreEqual(1, prematch.SportsBarItemsPreMatch.Count);
            Assert.AreEqual(1, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY, prematch.SelectedDescriptors[0]);

            //select all sport once again, other selection should be dropped
            SportBarItem allsports = new SportBarItem("All Sports", SportSr.ALL_SPORTS);
            prematch.OnCheckedExecute(allsports);

            Assert.AreEqual(1, prematch.SportsBarItemsPreMatch.Count);
            Assert.AreEqual(1, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, prematch.SelectedDescriptors[0]);

            //all sports cannot be unchecked if pressed twice
            prematch.OnCheckedExecute(allsports);
            Assert.AreEqual(1, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, prematch.SelectedDescriptors[0]);

            //same item pressed twice should be unchecked and if no other item selected all sports should be checked
            prematch.OnCheckedExecute(soccer);
            prematch.OnCheckedExecute(hokkey);
            Assert.AreEqual(2, prematch.SelectedDescriptors.Count);
            prematch.OnCheckedExecute(hokkey);
            Assert.AreEqual(1, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_SOCCER, prematch.SelectedDescriptors[0]);
            prematch.OnCheckedExecute(soccer);
            Assert.AreEqual(1, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, prematch.SelectedDescriptors[0]);

            //two items can be selected in same time
            prematch.OnCheckedExecute(hokkey);
            prematch.OnCheckedExecute(soccer);
            prematch.OnCheckedExecute(volley);
            Assert.AreEqual(3, prematch.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY, prematch.SelectedDescriptors[0]);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_SOCCER, prematch.SelectedDescriptors[1]);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_VOLLEYBALL, prematch.SelectedDescriptors[2]);
        }

        [TestMethod]
        public void FilterOutrights()
        {
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("", 0);
            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("", "0");
            
            var model = new MatchesViewModel(new HashSet<string> { "1*1" });

            var accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, false));
            Assert.IsFalse(accepted);

            accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, true));
            Assert.IsTrue(accepted);


        }
        [TestMethod]
        public void FilterPrematches()
        {
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("", 0);
            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("", "0");

            var model = new MatchesViewModel(new HashSet<string> { "1*0" });

            var accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, false));
            Assert.IsTrue(accepted);

            accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, true));

            Assert.IsFalse(accepted);

        }


        [TestMethod]
        public void EmptyTournamentsPrematches()
        {

            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("", 0);
            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("", "0");

            var model = new MatchesViewModel();

            var accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, false));
            Assert.IsTrue(accepted);

            accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, true));

            Assert.IsTrue(accepted);

        }


        [TestMethod]
        public void FilterOutrightsMultyTournaments()
        {
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("", 0);
            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("", "0");
            var model = new MatchesViewModel(new HashSet<string> { "1*1" });

            var accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, false));
            Assert.IsFalse(accepted);

            accepted = model.MatchFilter(TestMatchLn.CreateMatch(1, false, true));

            Assert.IsTrue(accepted);

            accepted = model.MatchFilter(TestMatchLn.CreateMatch(3, false, true));

            Assert.IsFalse(accepted);

        }


    }
}