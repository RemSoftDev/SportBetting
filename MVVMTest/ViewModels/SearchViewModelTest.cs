using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TranslationByMarkupExtension;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System;
using ViewModels.ViewModels;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class SearchViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void LessThan3SymbolsTest()
        {
            var model = new SearchViewModel();
            ChangeTracker.Setup(x => x.SearchMatches).Returns(new SortableObservableCollection<IMatchVw>());
            ChangeTracker.Setup(x => x.SearchString).Returns("2");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_NO_MATCH_FOUND)).Returns("not found");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_SEARCH_STRING_TOO_SHORT)).Returns("not found");
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>()));
            model.PleaseWaitSearch();
            Repository.Verify(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>()), Times.Never);

            TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_FORM_NO_MATCH_FOUND), Times.Never);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_FORM_SEARCH_STRING_TOO_SHORT), Times.Once);

        }

        [TestMethod]
        public void NoResultsTest()
        {
            var model = new SearchViewModel();
            ChangeTracker.Setup(x => x.SearchMatches).Returns(new SortableObservableCollection<IMatchVw>());
            ChangeTracker.Setup(x => x.AllResults).Returns(new SortableObservableCollection<IMatchVw>());
            ChangeTracker.Setup(x => x.SearchString).Returns("2222");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_NO_MATCH_FOUND)).Returns("not found");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_SEARCH_STRING_TOO_SHORT)).Returns("not found");
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) };
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(collection);
            model.PleaseWaitSearch();
            Repository.Verify(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>()), Times.Once);

            TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_FORM_NO_MATCH_FOUND), Times.Once);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_FORM_SEARCH_STRING_TOO_SHORT), Times.Never);

        }

        [TestMethod]
        public void SearchViewModelSortingTest()
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                TournamentView = TestGroupVw.CreateGroup(1, false),
                LiveBetStatus = eMatchStatus.Started,
                LiveMatchMinute = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test1",
                SportView = new TestGroupVw() { DisplayName = "b", LineObject = new GroupLn() { Sort = { Value = 3 } } },
                StartDate = new DateTime(2013, 1, 1)
            });
            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                TournamentView = TestGroupVw.CreateGroup(1, false),
                LiveBetStatus = eMatchStatus.Started,
                LiveMatchMinute = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test2",
                SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } },
                StartDate = new DateTime(2013, 1, 2)
            });
            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                TournamentView = TestGroupVw.CreateGroup(1, false),
                LiveBetStatus = eMatchStatus.Started,
                LiveMatchMinute = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test3",
                SportView = new TestGroupVw() { DisplayName = "h", LineObject = new GroupLn() { Sort = { Value = 4 } } },
                StartDate = new DateTime(2013, 1, 3)
            });
            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                TournamentView = TestGroupVw.CreateGroup(1, false),
                LiveBetStatus = eMatchStatus.Started,
                LiveMatchMinute = 1,
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test4",
                SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } },
                StartDate = new DateTime(2013, 1, 4)
            });


            collection.Sort(SearchViewModel.Comparison);

            Assert.AreEqual("test1", collection[0].Name);
            Assert.AreEqual("test2", collection[1].Name);
            Assert.AreEqual("test3", collection[2].Name);
            Assert.AreEqual("test4", collection[3].Name);

        }
    }
}