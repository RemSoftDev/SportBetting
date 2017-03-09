using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportRadar.Common.Collections;
using SportBetting.WPF.Prism.Shared.Models;
using SharedInterfaces;
using ViewModels.ViewModels;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using Moq;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class TopTournamentsViewModelTest : BaseTestClass
    {
        [TestMethod]
        public void TopTournamentsSortTest()
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

            Tournaments.Sort(TopTournamentsViewModel.Comparison);

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
        public void FilterTournamentsTest()
        {
            TestLineSr LineSr = new TestLineSr();
            Kernel.Rebind<ILineSr>().ToConstant<ILineSr>(LineSr);

            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "0"));
            TopTournamentsViewModel model = new TopTournamentsViewModel();


            var match = TestMatchLn.CreateMatch(1, false, true);

            var value = model.MatchFilter(match);

            Assert.IsTrue(value);

            model = new TopTournamentsViewModel();

            match = TestMatchLn.CreateMatch(1, true, true);

            value = model.MatchFilter(match);

            Assert.IsFalse(value);


        }

        [TestMethod]
        public void FillTopTournamentsTest()
        {
            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("Outrights");

            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            //fill up collection
            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                CategoryView = new TestGroupVw() { DisplayName = "soccer category", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw() 
                { 
                    DisplayName = "tournament1",
                    TournamentSportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } 
                },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);
            TopTournamentsViewModel model = new TopTournamentsViewModel();
            model.OnNavigationCompleted();

            //no outrights, no "all tournaments"
            Assert.AreEqual(1, model.Categories.Count);
            Assert.AreEqual("soccer category", model.Categories[0].SportName);
            Assert.AreEqual(1, model.Categories[0].Tournaments.Count);
            Assert.AreEqual("tournament1", model.Categories[0].Tournaments[0].Name);
            Assert.AreEqual(1, model.Categories[0].Tournaments[0].MatchesCount);
        }

        [TestMethod]
        public void FillTournamentsWithOutrights()
        {
            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("Outrights");

            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            TestGroupVw soccerCategory = new TestGroupVw() { DisplayName = "soccer category", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } };

            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.NotStarted,
                LiveMatchMinuteEx = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_4th_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                CategoryView = soccerCategory,
                StartDate = new DateTime(2013, 1, 2),
                TournamentView = new TestGroupVw()
                {
                    DisplayName = "tournament1",
                    TournamentSportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } }
                },
                ExpiryDate = DateTime.Now.AddDays(1)
            });

            //add outright tournament to category
            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = false,
                IsOutright = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                Name = "outright1",
                SportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                CategoryView = soccerCategory,
                StartDate = new DateTime(2015, 1, 2),
                TournamentView = new TestGroupVw()
                {
                    DisplayName = "outright1",
                    TournamentSportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } }
                },
                ExpiryDate = DateTime.Now.AddDays(1000)
            });

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);
            TopTournamentsViewModel model = new TopTournamentsViewModel();
            model.OnNavigationCompleted(); 

            //with outrights, no "all tournaments"
            Assert.AreEqual(1, model.Categories.Count);
            Assert.AreEqual("soccer category", model.Categories[0].SportName);
            Assert.AreEqual(2, model.Categories[0].Tournaments.Count);
            Assert.AreEqual("tournament1", model.Categories[0].Tournaments[0].Name);
            Assert.AreEqual("Outrights", model.Categories[0].Tournaments[1].Name);            
            Assert.AreEqual(1, model.Categories[0].Tournaments[1].MatchesCount);
        }

        [TestMethod]
        public void FillPlentyTournamentsWithOutrights()
        {
            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("Outrights");

            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            TestGroupVw soccerCategory = new TestGroupVw() { DisplayName = "soccer category", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } };
                                    
            //get category with more tournaments that fits into it
            collection = FillCategoryWithTournaments(soccerCategory, 9);

            //add outright tournament to category
            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = false,
                IsOutright = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                Name = "outright1",
                SportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                CategoryView = soccerCategory,
                StartDate = new DateTime(2015, 1, 2),
                TournamentView = new TestGroupVw()
                {
                    DisplayName = "outright1",
                    TournamentSportView = new TestGroupVw() { DisplayName = "lasttournament", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } }
                },
                ExpiryDate = DateTime.Now.AddDays(1000)
            });

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);
            TopTournamentsViewModel model = new TopTournamentsViewModel();
            model.OnNavigationCompleted();

            //no outrights, with "all tournaments"
            Assert.AreEqual(1, model.Categories.Count);
            Assert.AreEqual("soccer category", model.Categories[0].SportName);
            Assert.AreEqual(8, model.Categories[0].Tournaments.Count);
            Assert.AreEqual("tournament0", model.Categories[0].Tournaments[0].Name);
            Assert.AreEqual("tournament6", model.Categories[0].Tournaments[6].Name);
            Assert.AreEqual("All tournaments", model.Categories[0].Tournaments[7].Name);
        }

        [TestMethod]
        public void EightTournamentsInCategory()
        {
            TestGroupVw soccerCategory = new TestGroupVw() { DisplayName = "soccer category", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } };
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();
            collection = FillCategoryWithTournaments(soccerCategory, 8);

            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("Outrights");
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);
                        
            TopTournamentsViewModel model = new TopTournamentsViewModel();
            model.OnNavigationCompleted();

            //no outrights, without "all tournaments"
            Assert.AreEqual(1, model.Categories.Count);
            Assert.AreEqual("soccer category", model.Categories[0].SportName);
            Assert.AreEqual(8, model.Categories[0].Tournaments.Count);
            Assert.AreEqual("tournament0", model.Categories[0].Tournaments[0].Name);
            Assert.AreEqual("tournament7", model.Categories[0].Tournaments[7].Name);
        }

        [TestMethod]
        public void FillSeveralCategories()
        {
            TestGroupVw soccerCategory = new TestGroupVw() { DisplayName = "soccer category", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } };
            TestGroupVw hockeyCategory = new TestGroupVw() { DisplayName = "hockey category", LineObject = new GroupLn() { GroupId = 3, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 3 } } };
            TestGroupVw someCategory = new TestGroupVw() { DisplayName = "some category", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } };

            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();
            collection = FillCategoryWithTournaments(soccerCategory, 8);

            SortableObservableCollection<IMatchVw> collection2 = FillCategoryWithTournaments(hockeyCategory, 15);
            SortableObservableCollection<IMatchVw> collection3 = FillCategoryWithTournaments(someCategory, 2);

            foreach (IMatchVw m in collection2)
                collection.Add(m);

            foreach (IMatchVw m in collection3)
                collection.Add(m);

            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("Outrights");
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);

            TopTournamentsViewModel model = new TopTournamentsViewModel();
            model.OnNavigationCompleted();

            Assert.AreEqual(3, model.Categories.Count);
            Assert.AreEqual("soccer category", model.Categories[0].SportName);
            Assert.AreEqual("hockey category", model.Categories[2].SportName);
            Assert.AreEqual("some category", model.Categories[1].SportName);
            Assert.AreEqual(8, model.Categories[0].Tournaments.Count);
            Assert.AreEqual(8, model.Categories[2].Tournaments.Count);
            Assert.AreEqual(2, model.Categories[1].Tournaments.Count);
            Assert.AreEqual("tournament0", model.Categories[0].Tournaments[0].Name);
            Assert.AreEqual("tournament7", model.Categories[0].Tournaments[7].Name);
            Assert.AreEqual("All tournaments", model.Categories[2].Tournaments[7].Name);
            Assert.AreEqual("tournament0", model.Categories[0].Tournaments[0].Name);
            Assert.AreEqual("tournament1", model.Categories[0].Tournaments[1].Name);
        }

        [TestMethod]
        public void CetagoriesWithOnlyOutrights()
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();
            TestGroupVw soccerCategory = new TestGroupVw() { DisplayName = "soccer category", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } };

            ChangeTracker.Setup(x => x.SelectedTournaments).Returns(new HashSet<string>());
            TranslationProvider.Setup(x => x.Translate(It.IsAny<MultistringTag>())).Returns("Outrights");
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);

            //add outright tournament to category
            collection.Add(new TestMatchVw()
            {
                SportDescriptor = "SPRT_SOCCER",
                DefaultSorting = 1,
                IsLiveBet = false,
                IsOutright = true,
                LiveBetStatus = eMatchStatus.NotStarted,
                Name = "outright1",
                SportView = new TestGroupVw() { DisplayName = "soccer", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                CategoryView = soccerCategory,
                StartDate = new DateTime(2015, 1, 2),
                TournamentView = new TestGroupVw()
                {
                    DisplayName = "outright1",
                    TournamentSportView = new TestGroupVw() { DisplayName = "lasttournament", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } }
                },
                ExpiryDate = DateTime.Now.AddDays(1000)
            });

            TopTournamentsViewModel model = new TopTournamentsViewModel();
            model.OnNavigationCompleted();

            //we do not show categories with outrights only
            Assert.AreEqual(0, model.Categories.Count);
        }

        private SortableObservableCollection<IMatchVw> FillCategoryWithTournaments(TestGroupVw category, int amount)
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            for (int i = 0; i < amount; i++)
            {
                collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = i,
                    IsLiveBet = false,
                    IsOutright = false,
                    Name = "test1",
                    CategoryView = category,
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 3 } } },
                    StartDate = new DateTime(2015, 1, 2),
                    TournamentView = new TestGroupVw() { DisplayName = "tournament" + i.ToString(), LineObject = new GroupLn() { GroupId = i, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = i } } },
                    ExpiryDate = DateTime.Now.AddDays(365)
                });
            }

            return collection;
        }
    }
}
