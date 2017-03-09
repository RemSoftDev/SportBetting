using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportRadar.Common.Collections;
using SportBetting.WPF.Prism.Shared.Models;
using SharedInterfaces;
using ViewModels.ViewModels;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using Moq;
using System;
using System.Collections.Generic;
using SportRadar.DAL.CommonObjects;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class TournamentsViewModelTest : BaseTestClass
    {



        [TestMethod]
        public void SortTest()
        {
            SortableObservableCollection<TournamentVw> Tournaments = new SortableObservableCollection<TournamentVw>();
            Tournaments.Add(new TournamentVw(1, 1, "AMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(3, 1, "CMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(4, 1, "AMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(2, 1, "AMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(5, 1, "DMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(6, 1, "GMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(8, 1, "ZMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(7, 1, "KMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(10, 1, "LMyTournament", 0, 0, null, ""));
            Tournaments.Add(new TournamentVw(9, 1, "AMyTournament", 0, 0, null, ""));

            Tournaments.Sort(TournamentsViewModel.Comparison);

            Assert.AreEqual(1, Tournaments[0].Id);
            Assert.AreEqual(2, Tournaments[1].Id);
            Assert.AreEqual(4, Tournaments[2].Id);
            Assert.AreEqual(9, Tournaments[3].Id);
            Assert.AreEqual(3, Tournaments[4].Id);
            Assert.AreEqual(5, Tournaments[5].Id);
            Assert.AreEqual(6, Tournaments[6].Id);
            Assert.AreEqual(7, Tournaments[7].Id);
            Assert.AreEqual(10, Tournaments[8].Id);
            Assert.AreEqual(8, Tournaments[9].Id);
        }


        [TestMethod]
        public void FilterTournamentsWithCategoryTest()
        {
            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "0"));
            TournamentsViewModel model = new TournamentsViewModel(1l);


            var match = TestMatchLn.CreateMatch(1, false, true);

            var value = model.MatchFilter(match);

            Assert.IsTrue(value);

            model = new TournamentsViewModel(2l);

            match = TestMatchLn.CreateMatch(1, false, true);

            value = model.MatchFilter(match);

            Assert.IsFalse(value);


        }

        [TestMethod]
        public void FilterOutrightsTournamentsWithCategoryTest()
        {
            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            ChangeTracker.Setup(x=>x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "0"));
            TournamentsViewModel model = new TournamentsViewModel(1l,new TournamentVw(1));


            var match = TestMatchLn.CreateMatch(1, false, true);

            var value = model.MatchFilter(match);

            Assert.IsTrue(value);

            model = new TournamentsViewModel(2l,new TournamentVw(1));

            match = TestMatchLn.CreateMatch(1, false, true);

            value = model.MatchFilter(match);

            Assert.IsFalse(value);


        }

        [TestMethod]
        public void FillTournamentsTest()
        {
            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());

            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            //fill up collection
            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament1", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);
            TournamentsViewModel model = new TournamentsViewModel(1);
            model.OnNavigationCompleted();

            Assert.AreEqual(1, model.Tournaments.Count);                       

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament1", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament1", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament1", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            model.Refresh(true);
            Assert.AreEqual(1, model.Tournaments.Count);
            Assert.AreEqual(4, model.Tournaments[0].MatchesCount);

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 0 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament2", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 0 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            model.Refresh(true);
            Assert.AreEqual(2, model.Tournaments.Count);
            Assert.AreEqual(1, model.Tournaments[0].MatchesCount);
            Assert.AreEqual(4, model.Tournaments[1].MatchesCount);

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 3 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament3", LineObject = new GroupLn() { GroupId = 3, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 3 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 3 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() { DisplayName = "tournament3", LineObject = new GroupLn() { GroupId = 3, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 3 } } },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            model.Refresh(true);
            Assert.AreEqual(3, model.Tournaments.Count);
            Assert.AreEqual(1, model.Tournaments[0].MatchesCount);
            Assert.AreEqual(4, model.Tournaments[1].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[2].MatchesCount);
            Assert.AreEqual("tournament2", model.Tournaments[0].Name);
            Assert.AreEqual("tournament1", model.Tournaments[1].Name);
            Assert.AreEqual("tournament3", model.Tournaments[2].Name);
        }
    }
}
