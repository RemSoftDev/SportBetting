using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;
using SportRadar.DAL.OldLineObjects;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class ResultViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void DoubleClickOnResultTest()
        {
            ChangeTracker.Setup(x=>x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>());
            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>());
            ChangeTracker.Setup(x => x.SelectedDescriptors).Returns(new List<string>());
            ChangeTracker.Setup(x => x.SelectedTimeFilter).Returns(new ComboBoxItem("0",0));
            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("0","0"));
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("");
            StationRepository.Setup(x=>x.TurnOffCashInInit).Returns(true);
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<IMediator>().InSingletonScope();
            IoCContainer.Kernel.Unbind<IMediator>();
            IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();
            ChangeTracker.Setup(x => x.CurrentUser).Returns(new AnonymousUser("1", 1));
            var model = new HeaderViewModel();
            model.OnNavigationCompleted();
            var result1 = new MatchResultsViewModel();
            result1.OnNavigationCompleted();
            Assert.IsTrue(model.ShowResultFilters);
            result1.Close();
            Assert.IsFalse(model.ShowResultFilters);

            var result2 = new MatchResultsViewModel();
            result2.OnNavigationCompleted();
            Assert.IsTrue(model.ShowResultFilters);

        }


        [TestMethod]
        public void SortingTest()
        {

            var icehockeyresult = new MatchResultVw(new MatchResultLn(){MatchId = 2}); 

            SortableObservableCollection<MatchResultVw> icehockey = new SortableObservableCollection<MatchResultVw>();
            icehockey.Add(icehockeyresult);
           

            SyncList<MatchResultVw> basketballIcehockey = new SyncList<MatchResultVw>();
            basketballIcehockey.Add(new MatchResultVw(new MatchResultLn(){MatchId = 1}));
            basketballIcehockey.Add(icehockeyresult);

            icehockey.ApplyChanges(basketballIcehockey);

            Assert.AreEqual(2,icehockey.Count);

        }

        [TestMethod]
        public void SportFiltersCheck()
        {
            var changeTracker = new ChangeTracker();
            Kernel.Rebind<IChangeTracker>().ToConstant(changeTracker);

            var result = new MatchResultsViewModel();
            result.OnNavigationCompleted();
            //at start should be only "All sports" available and checked
            Assert.AreEqual(1, result.SportsBarItems.Count);
            Assert.AreEqual(1, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, changeTracker.SelectedDescriptors[0]);

            //if something else is checked, there should be no "All sports" in selected descriptors
            SportBarItem hokkey = new SportBarItem("Ice Hokkey", SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY);
            SportBarItem soccer = new SportBarItem("Soccer", SportSr.SPORT_DESCRIPTOR_SOCCER);
            result.OnCheckedExecute(hokkey);

            Assert.AreEqual(1, result.SportsBarItems.Count);
            Assert.AreEqual(1, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY, changeTracker.SelectedDescriptors[0]);

            //select all sport once again, other selection should be dropped
            SportBarItem allsports = new SportBarItem("All Sports", SportSr.ALL_SPORTS);
            result.OnCheckedExecute(allsports);

            Assert.AreEqual(1, result.SportsBarItems.Count);
            Assert.AreEqual(1, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, changeTracker.SelectedDescriptors[0]);

            //all sports cannot be unchecked if pressed twice
            result.OnCheckedExecute(allsports);
            Assert.AreEqual(1, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, changeTracker.SelectedDescriptors[0]);

            //same item pressed twice should be unchecked and if no other item selected all sports should be checked
            result.OnCheckedExecute(soccer);
            result.OnCheckedExecute(hokkey);
            Assert.AreEqual(2, changeTracker.SelectedDescriptors.Count);
            result.OnCheckedExecute(hokkey);
            Assert.AreEqual(1, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_SOCCER, changeTracker.SelectedDescriptors[0]);
            result.OnCheckedExecute(soccer);
            Assert.AreEqual(1, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, changeTracker.SelectedDescriptors[0]);
            
            //two items can be selected in same time
            result.OnCheckedExecute(hokkey);
            result.OnCheckedExecute(soccer);
            Assert.AreEqual(2, changeTracker.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY, changeTracker.SelectedDescriptors[0]);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_SOCCER, changeTracker.SelectedDescriptors[1]);
        }
    }
}