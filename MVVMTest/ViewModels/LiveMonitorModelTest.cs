using System;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.OldLineObjects;
using ViewModels.Helpers;
using ViewModels.ViewModels;
using System.Windows.Controls;

namespace MVVMTest.ViewModels
{
    // TODO rewrite using Entities instead of MachesGroup
    [TestClass]
    public class LiveMonitorModelTest : BaseTestClass
    {
        [TestMethod]
        public void LiveMonitorItemsSetting()
        {
            LiveMonitorViewModel lmvm = new LiveMonitorViewModel();

            //empty page
            Assert.AreEqual(false, lmvm.rotating);

            //set screen sizes from fake event
            Grid FakeGrid = new Grid();
            FakeGrid.Height = 1000;
            FakeGrid.Width = 1500;
            FakeGrid.RenderSize = new Size(1500, 1000);
            lmvm.MainGridCreated.Execute(FakeGrid);

            Assert.AreEqual(1000, lmvm.MonitorHeight);
            Assert.AreEqual(1500, lmvm.MonitorWidth);
            Assert.AreEqual(768, lmvm.BrowserHeight);
            Assert.AreEqual(1366, lmvm.BrowserWidth);

            //set items dimentions from fake event
            Label Header = new Label();
            Header.RenderSize = new Size(10, 150);
            lmvm.ItemCreated.Execute(Header);

            Assert.AreEqual(150, lmvm.HeaderItemHeight);
            Assert.AreEqual(0, lmvm.ItemHeight);

            Label NoHeader = new Label();
            NoHeader.RenderSize = new Size(10, 100);
            lmvm.ItemCreated.Execute(NoHeader);

            Assert.AreEqual(150, lmvm.HeaderItemHeight);
            Assert.AreEqual(100, lmvm.ItemHeight);

        }

        [TestMethod]
        public void DifferentSportsSort()
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.NotStarted, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test1", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test2", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 2 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test3", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 2), DefaultSorting = 2 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Stopped, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test4", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 2 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Stopped, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test5", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 2), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test6", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 2), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.NotStarted, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test7", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 3), DefaultSorting = 2 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Stopped, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test8", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test9", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 4), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Stopped, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test10", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 2), DefaultSorting = 2 });

            collection.Add(new TestMatchVw() { IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test11", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 5), DefaultSorting = 2 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test12", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test13", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 2), DefaultSorting = 1 });

            var groupln = new GroupLn {Sort = {Value = 1}};
            collection.Sort(LiveMonitorsService.Comparison);
            LiveMonitorsService.UpdateHeaders(collection);

            Assert.AreEqual("test12", collection[0].Name);//s
            Assert.AreEqual("test8", collection[1].Name);//s
            Assert.AreEqual("test13", collection[2].Name);//s
            Assert.AreEqual("test5", collection[3].Name);//s
            Assert.AreEqual("test2", collection[4].Name);//t
            Assert.AreEqual("test4", collection[5].Name);//t
            Assert.AreEqual("test10", collection[6].Name);//t
            Assert.AreEqual("test3", collection[7].Name);//t
            Assert.AreEqual("test1", collection[8].Name);//s
            Assert.AreEqual("test6", collection[9].Name);//s
            Assert.AreEqual("test9", collection[10].Name);//s
            Assert.AreEqual("test7", collection[11].Name);//t
            Assert.AreEqual("test11", collection[12].Name);//t

             Assert.AreEqual(true, collection[0].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[1].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[2].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[3].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[4].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[5].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[6].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[7].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[8].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[9].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[10].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[11].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[12].IsHeaderForLiveMonitor);

            collection = new SortableObservableCollection<IMatchVw>(collection.OrderByDescending(x => x.Name).ToList());

            collection.Sort(LiveMonitorsService.Comparison);
            LiveMonitorsService.UpdateHeaders(collection);

            Assert.AreEqual("test12", collection[0].Name);//s
            Assert.AreEqual("test8", collection[1].Name);//s
            Assert.AreEqual("test13", collection[2].Name);//s
            Assert.AreEqual("test5", collection[3].Name);//s
            Assert.AreEqual("test2", collection[4].Name);//t
            Assert.AreEqual("test4", collection[5].Name);//t
            Assert.AreEqual("test10", collection[6].Name);//t
            Assert.AreEqual("test3", collection[7].Name);//t
            Assert.AreEqual("test1", collection[8].Name);//s
            Assert.AreEqual("test6", collection[9].Name);//s
            Assert.AreEqual("test9", collection[10].Name);//s
            Assert.AreEqual("test7", collection[11].Name);//t
            Assert.AreEqual("test11", collection[12].Name);//t

             Assert.AreEqual(true, collection[0].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[1].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[2].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[3].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[4].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[5].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[6].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[7].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[8].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[9].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[10].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[11].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[12].IsHeaderForLiveMonitor);

        }

        [TestMethod]
        public void SortPausedSoccerLiveMonitor()
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 45, LivePeriodInfo = eLivePeriodInfo.Soccer_1st_Period, Name = "test1", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 2) });//should be third
            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 45, LivePeriodInfo = eLivePeriodInfo.Paused, Name = "test2", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1) });//should be second
            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 20, LivePeriodInfo = eLivePeriodInfo.Soccer_1st_Period, Name = "test3", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 2) });//should be forth
            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 50, LivePeriodInfo = eLivePeriodInfo.Soccer_2nd_Period, Name = "test4", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1) });//should be first

            var groupln = new GroupLn { Sort = { Value = 1 } };
            collection.Sort(LiveMonitorsService.Comparison);

            Assert.AreEqual("test4", collection[0].Name);//s
            Assert.AreEqual("test2", collection[1].Name);//s
            Assert.AreEqual("test1", collection[2].Name);//s
            Assert.AreEqual("test3", collection[3].Name);//s
        }


        [TestMethod]
        public void SortSports()
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test1", SportDescriptor = "SPRT_BASKETBALL", SportView = new TestGroupVw() { DisplayName = "b", LineObject = new GroupLn() { Sort = { Value = 3 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 3 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test2", SportDescriptor = "SPRT_SOCCER", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 1 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 1 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test3", SportDescriptor = "SPRT_ICEH", SportView = new TestGroupVw() { DisplayName = "h", LineObject = new GroupLn() { Sort = { Value = 4 } } }, StartDate = new DateTime(2013, 1, 2), DefaultSorting = 4 });

            collection.Add(new TestMatchVw() { IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test4", SportDescriptor = "SPRT_TENNIS", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1), DefaultSorting = 2 });



            collection.Sort(LiveMonitorsService.Comparison);
            LiveMonitorsService.UpdateHeaders(collection);

            Assert.AreEqual("s", collection[0].SportView.DisplayName);//s
            Assert.AreEqual("t", collection[1].SportView.DisplayName);//s
            Assert.AreEqual("b", collection[2].SportView.DisplayName);//s
            Assert.AreEqual("h", collection[3].SportView.DisplayName);//s
           

            Assert.AreEqual(true, collection[0].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[1].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[2].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[3].IsHeaderForLiveMonitor);
         

        }

        [TestMethod]
        public void SameSport()
        {
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test1", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 5) });

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test2", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 2) });

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test3", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 3) });

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = true, LiveBetStatus = eMatchStatus.Stopped, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test4", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 4) });


            collection.Sort(LiveMonitorsService.Comparison);
            LiveMonitorsService.UpdateHeaders(collection);

            Assert.AreEqual("test2", collection[0].Name);//s
            Assert.AreEqual("test4", collection[1].Name);//s
            Assert.AreEqual("test3", collection[2].Name);//s
            Assert.AreEqual("test1", collection[3].Name);//s


            Assert.AreEqual(true, collection[0].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[1].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[2].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[3].IsHeaderForLiveMonitor);


            collection = new SortableObservableCollection<IMatchVw>(collection.OrderByDescending(x => x.Name).ToList());

            collection.Sort(LiveMonitorsService.Comparison);
            LiveMonitorsService.UpdateHeaders(collection);

            Assert.AreEqual("test2", collection[0].Name);//s
            Assert.AreEqual("test4", collection[1].Name);//s
            Assert.AreEqual("test3", collection[2].Name);//s
            Assert.AreEqual("test1", collection[3].Name);//s

            Assert.AreEqual(true, collection[0].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[1].IsHeaderForLiveMonitor);
            Assert.AreEqual(true, collection[2].IsHeaderForLiveMonitor);
            Assert.AreEqual(false, collection[3].IsHeaderForLiveMonitor);
        

        }

    }

}