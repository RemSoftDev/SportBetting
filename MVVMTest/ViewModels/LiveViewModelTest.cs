using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using BaseObjects;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Services;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.OldLineObjects;
using ViewModels.ViewModels;
using System.Collections.Generic;
using SportBetting.WPF.Prism.Shared;

namespace MVVMTest
{
    [TestClass]
    public class LiveviewModelTest : BaseTestClass
    {
        [TestMethod]
        public void SportFiltersCheckLive()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            var MessageMediator = new MyMessageMediator();
            IoCContainer.Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator).InSingletonScope();

            ChangeTracker.Setup(x => x.SelectedDescriptorsLive).Returns(new List<string>());

            var live = new LiveViewModel();
            live.OnNavigationCompleted();

            MessageMediator.SendMessage(true, MsgTag.ClearSelectedSports);

            //at start should be only "All sports" available and checked
            Assert.AreEqual(1, live.SportsBarItemsLive.Count);
            Assert.AreEqual(1, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, live.SelectedDescriptors[0]);

            //if something else is checked, there should be no "All sports" in selected descriptors
            SportBarItem hokkey = new SportBarItem("Ice Hokkey", SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY);
            SportBarItem soccer = new SportBarItem("Soccer", SportSr.SPORT_DESCRIPTOR_SOCCER);
            SportBarItem volley = new SportBarItem("Volleyball", SportSr.SPORT_DESCRIPTOR_VOLLEYBALL);
            live.OnCheckedExecute(hokkey);

            Assert.AreEqual(1, live.SportsBarItemsLive.Count);
            Assert.AreEqual(1, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY, live.SelectedDescriptors[0]);

            //select all sport once again, other selection should be dropped
            SportBarItem allsports = new SportBarItem("All Sports", SportSr.ALL_SPORTS);
            live.OnCheckedExecute(allsports);

            Assert.AreEqual(1, live.SportsBarItemsLive.Count);
            Assert.AreEqual(1, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, live.SelectedDescriptors[0]);

            //all sports cannot be unchecked if pressed twice
            live.OnCheckedExecute(allsports);
            Assert.AreEqual(1, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, live.SelectedDescriptors[0]);

            //same item pressed twice should be unchecked and if no other item selected all sports should be checked
            live.OnCheckedExecute(soccer);
            live.OnCheckedExecute(hokkey);
            Assert.AreEqual(2, live.SelectedDescriptors.Count);
            live.OnCheckedExecute(hokkey);
            Assert.AreEqual(1, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_SOCCER, live.SelectedDescriptors[0]);
            live.OnCheckedExecute(soccer);
            Assert.AreEqual(1, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.ALL_SPORTS, live.SelectedDescriptors[0]);

            //two items can be selected in same time
            live.OnCheckedExecute(hokkey);
            live.OnCheckedExecute(soccer);
            live.OnCheckedExecute(volley);
            Assert.AreEqual(3, live.SelectedDescriptors.Count);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY, live.SelectedDescriptors[0]);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_SOCCER, live.SelectedDescriptors[1]);
            Assert.AreEqual(SportSr.SPORT_DESCRIPTOR_VOLLEYBALL, live.SelectedDescriptors[2]);
        }

        [TestMethod]
        public void DifferentSportsSort()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("1", "1");
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("1", 0);
            ChangeTracker.Object.TimeFilters = new ObservableCollection<ComboBoxItem>()
                {
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                };
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();
            var LiveViewModel = new LiveViewModel();

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
                    StartDate = new DateTime(2013, 1, 2)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 2,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test2",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 2,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test3",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } },
                    StartDate = new DateTime(2013, 1, 2)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 2,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Stopped,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test4",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 1,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Stopped,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test5",
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    StartDate = new DateTime(2013, 1, 2)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 1,
                    IsLiveBet = false,
                    LiveBetStatus = eMatchStatus.Undefined,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_3rd_Quarter,
                    Name = "test6",
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 2,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.NotStarted,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test7",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } },
                    StartDate = new DateTime(2013, 1, 3)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 1,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Stopped,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test8",
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 1,
                    IsLiveBet = false,
                    LiveBetStatus = eMatchStatus.Undefined,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test9",
                    SportView = new TestGroupVw()
                        {
                            DisplayName = "s",
                            LineObject = new GroupLn()
                                {
                                    GroupId = 1,
                                    Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 }
                                }
                        },
                    StartDate = new DateTime(2013, 1, 4)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 2,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Stopped,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test10",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } },
                    StartDate = new DateTime(2013, 1, 2)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 2,
                    IsLiveBet = false,
                    LiveBetStatus = eMatchStatus.Undefined,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test11",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 2 } } },
                    StartDate = new DateTime(2013, 1, 5)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 1,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test12",
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 1,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinuteEx = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test13",
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1, Sort = new ObservableProperty<int>(new GroupLn(), new ObservablePropertyList(), "test") { Value = 1 } } },
                    StartDate = new DateTime(2013, 1, 2)
                });

            var groupln = new GroupLn { Sort = { Value = 1 } };
            collection.Sort(LiveViewModel.Comparison);
            LiveViewModel.UpdateHeaders(collection);

            Assert.AreEqual("test12", collection[0].Name);//s
            Assert.AreEqual("test8", collection[1].Name);//s
            Assert.AreEqual("test13", collection[2].Name);//s
            Assert.AreEqual("test5", collection[3].Name);//s
            Assert.AreEqual("test2", collection[4].Name);//t
            Assert.AreEqual("test4", collection[5].Name);//t
            Assert.AreEqual("test10", collection[6].Name);//t
            Assert.AreEqual("test3", collection[7].Name);//t
            Assert.AreEqual("test6", collection[8].Name);//s
            Assert.AreEqual("test1", collection[9].Name);//s
            Assert.AreEqual("test9", collection[10].Name);//s
            Assert.AreEqual("test7", collection[11].Name);//t
            Assert.AreEqual("test11", collection[12].Name);//t

            Assert.AreEqual(true, collection[0].IsHeader);
            Assert.AreEqual(false, collection[1].IsHeader);
            Assert.AreEqual(false, collection[2].IsHeader);
            Assert.AreEqual(false, collection[3].IsHeader);
            Assert.AreEqual(true, collection[4].IsHeader);
            Assert.AreEqual(false, collection[5].IsHeader);
            Assert.AreEqual(false, collection[6].IsHeader);
            Assert.AreEqual(false, collection[7].IsHeader);
            Assert.AreEqual(true, collection[8].IsHeader);
            Assert.AreEqual(false, collection[9].IsHeader);
            Assert.AreEqual(false, collection[10].IsHeader);
            Assert.AreEqual(true, collection[11].IsHeader);
            Assert.AreEqual(false, collection[12].IsHeader);

            collection = new SortableObservableCollection<IMatchVw>(collection.OrderByDescending(x => x.Name).ToList());

            collection.Sort(LiveViewModel.Comparison);
            LiveViewModel.UpdateHeaders(collection);

            Assert.AreEqual("test12", collection[0].Name);//s
            Assert.AreEqual("test8", collection[1].Name);//s
            Assert.AreEqual("test13", collection[2].Name);//s
            Assert.AreEqual("test5", collection[3].Name);//s
            Assert.AreEqual("test2", collection[4].Name);//t
            Assert.AreEqual("test4", collection[5].Name);//t
            Assert.AreEqual("test10", collection[6].Name);//t
            Assert.AreEqual("test3", collection[7].Name);//t
            Assert.AreEqual("test6", collection[8].Name);//s
            Assert.AreEqual("test1", collection[9].Name);//s
            Assert.AreEqual("test9", collection[10].Name);//s
            Assert.AreEqual("test7", collection[11].Name);//t
            Assert.AreEqual("test11", collection[12].Name);//t

            Assert.AreEqual(true, collection[0].IsHeader);
            Assert.AreEqual(false, collection[1].IsHeader);
            Assert.AreEqual(false, collection[2].IsHeader);
            Assert.AreEqual(false, collection[3].IsHeader);
            Assert.AreEqual(true, collection[4].IsHeader);
            Assert.AreEqual(false, collection[5].IsHeader);
            Assert.AreEqual(false, collection[6].IsHeader);
            Assert.AreEqual(false, collection[7].IsHeader);
            Assert.AreEqual(true, collection[8].IsHeader);
            Assert.AreEqual(false, collection[9].IsHeader);
            Assert.AreEqual(false, collection[10].IsHeader);
            Assert.AreEqual(true, collection[11].IsHeader);
            Assert.AreEqual(false, collection[12].IsHeader);

        }

        [TestMethod]
        public void SortPausedSoccer()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("1", "1");
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("1", 0);
            ChangeTracker.Object.TimeFilters = new ObservableCollection<ComboBoxItem>()
                {
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                }; SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 45, LivePeriodInfo = eLivePeriodInfo.Soccer_1st_Period, Name = "test1", SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 2) });//should be third
            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 45, LivePeriodInfo = eLivePeriodInfo.Paused, Name = "test2", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1) });//should be second
            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 20, LivePeriodInfo = eLivePeriodInfo.Soccer_1st_Period, Name = "test3", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 2) });//should be forth
            collection.Add(new TestMatchVw() { SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER, IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinuteEx = 50, LivePeriodInfo = eLivePeriodInfo.Soccer_2nd_Period, Name = "test4", SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { Sort = { Value = 2 } } }, StartDate = new DateTime(2013, 1, 1) });//should be first

            var LiveViewModel = new LiveViewModel();
            var groupln = new GroupLn { Sort = { Value = 1 } };
            collection.Sort(LiveViewModel.Comparison);

            Assert.AreEqual("test4", collection[0].Name);//s
            Assert.AreEqual("test2", collection[1].Name);//s
            Assert.AreEqual("test1", collection[2].Name);//s
            Assert.AreEqual("test3", collection[3].Name);//s
        }


        [TestMethod]
        public void SortSports()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("1", "1");
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("1", 0);
            ChangeTracker.Object.TimeFilters = new ObservableCollection<ComboBoxItem>()
                {
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                };
            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("1", "1");
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_SOCCER",
                    DefaultSorting = 3,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinute = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test1",
                    SportView = new TestGroupVw() { DisplayName = "b", LineObject = new GroupLn() { GroupId = 3 } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_TENNIS",
                    DefaultSorting = 1,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinute = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test2",
                    SportView = new TestGroupVw() { DisplayName = "s", LineObject = new GroupLn() { GroupId = 1 } },
                    StartDate = new DateTime(2013, 1, 1)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "SPRT_ICEHOCKEY",
                    DefaultSorting = 4,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinute = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test3",
                    SportView = new TestGroupVw() { DisplayName = "h", LineObject = new GroupLn() { GroupId = 4 } },
                    StartDate = new DateTime(2013, 1, 2)
                });

            collection.Add(new TestMatchVw()
                {
                    SportDescriptor = "RUGBY",
                    DefaultSorting = 2,
                    IsLiveBet = true,
                    LiveBetStatus = eMatchStatus.Started,
                    LiveMatchMinute = 1,
                    LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                    Name = "test4",
                    SportView = new TestGroupVw() { DisplayName = "t", LineObject = new GroupLn() { GroupId = 2 } },
                    StartDate = new DateTime(2013, 1, 1)
                });


            var LiveViewModel = new LiveViewModel();

            collection.Sort(LiveViewModel.Comparison);
            LiveViewModel.UpdateHeaders(collection);

            Assert.AreEqual("s", collection[0].SportView.DisplayName);//s
            Assert.AreEqual("t", collection[1].SportView.DisplayName);//s
            Assert.AreEqual("b", collection[2].SportView.DisplayName);//s
            Assert.AreEqual("h", collection[3].SportView.DisplayName);//s


            Assert.AreEqual(true, collection[0].IsHeader);
            Assert.AreEqual(true, collection[1].IsHeader);
            Assert.AreEqual(true, collection[2].IsHeader);
            Assert.AreEqual(true, collection[3].IsHeader);


        }

        [TestMethod]
        public void SameSport()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("1", "1");
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("1", 0);
            ChangeTracker.Object.TimeFilters = new ObservableCollection<ComboBoxItem>()
                {
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                };
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();
            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test1", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 5) });

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = true, LiveBetStatus = eMatchStatus.Started, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test2", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 2) });

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = false, LiveBetStatus = eMatchStatus.Undefined, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test3", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 3) });

            collection.Add(new TestMatchVw() { SportDescriptor = "SPRT_SOCCER", IsLiveBet = true, LiveBetStatus = eMatchStatus.Stopped, LiveMatchMinute = 1, LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter, Name = "test4", SportView = new TestGroupVw() { DisplayName = "s" }, StartDate = new DateTime(2013, 1, 4) });


            var LiveViewModel = new LiveViewModel();
            collection.Sort(LiveViewModel.Comparison);
            LiveViewModel.UpdateHeaders(collection);

            Assert.AreEqual("test2", collection[0].Name);//s
            Assert.AreEqual("test4", collection[1].Name);//s
            Assert.AreEqual("test3", collection[2].Name);//s
            Assert.AreEqual("test1", collection[3].Name);//s


            Assert.AreEqual(true, collection[0].IsHeader);
            Assert.AreEqual(false, collection[1].IsHeader);
            Assert.AreEqual(true, collection[2].IsHeader);
            Assert.AreEqual(false, collection[3].IsHeader);


            collection = new SortableObservableCollection<IMatchVw>(collection.OrderByDescending(x => x.Name).ToList());

            collection.Sort(LiveViewModel.Comparison);
            LiveViewModel.UpdateHeaders(collection);

            Assert.AreEqual("test2", collection[0].Name);//s
            Assert.AreEqual("test4", collection[1].Name);//s
            Assert.AreEqual("test3", collection[2].Name);//s
            Assert.AreEqual("test1", collection[3].Name);//s

            Assert.AreEqual(true, collection[0].IsHeader);
            Assert.AreEqual(false, collection[1].IsHeader);
            Assert.AreEqual(true, collection[2].IsHeader);
            Assert.AreEqual(false, collection[3].IsHeader);


        }

        [TestMethod]
        public void TopGames()
        {
            Dispatcher dispatcher = null;


            var newWindowThread = new Thread(() =>
            {
                Dispatcher.Run();
            });
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
            while (dispatcher == null)
            {
                Thread.Sleep(10);
                dispatcher = Dispatcher.FromThread(newWindowThread);

            }

            var dispatchermy = new MyDispatcher(dispatcher);

            IoCContainer.Kernel.Unbind<IDispatcher>();
            IoCContainer.Kernel.Bind<IDispatcher>().ToConstant<IDispatcher>(dispatchermy).InSingletonScope();

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            string jsonfeed = @"{""tournamentData"":[{""id"":-1230,""sportId"":5,""sort"":1},{""id"":-1225,""sportId"":5,""sort"":2},{""id"":-1221,""sportId"":5,""sort"":3},{""id"":-1161,""sportId"":1,""sort"":4},{""id"":-1080,""sportId"":1,""sort"":5},{""id"":-1011,""sportId"":1,""sort"":6},{""id"":-229,""sportId"":2,""sort"":7}],""sportData"":[{""id"":2,""sort"":1},{""id"":1,""sort"":2},{""id"":5,""sort"":3}],""matchData"":[{""tournamentId"":-1225,""id"":-10683559350109352,""sort"":1},{""tournamentId"":-1161,""id"":-10558683175978152,""sort"":2},{""tournamentId"":-1080,""id"":-10542096012281000,""sort"":3},{""tournamentId"":-1221,""id"":-10683645249455272,""sort"":4},{""tournamentId"":-1011,""id"":-10491028851131560,""sort"":5},{""tournamentId"":-1225,""id"":-10683572235011240,""sort"":6},{""tournamentId"":-1230,""id"":-10683980256904360,""sort"":7},{""tournamentId"":-229,""id"":-9781233965860008,""sort"":8}]}
";

            WsdlRepository.Setup(x => x.Matchsorting(It.IsAny<string>())).Returns(jsonfeed);


            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "1"));
            ChangeTracker.Setup(x => x.SelectedTimeFilter).Returns(new ComboBoxItem("1", 4));
            ChangeTracker.Setup(x => x.LiveSelectedMode).Returns(4);
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>() { new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), });
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn()
                {
                    StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-1)) }
                },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test5",
                TournamentView = new TestGroupVw()
                {
                    Sort = 3,

                },
                SportView = new TestGroupVw()
                    {
                        Sort = 2,
                    },
                StartDate = new DateTime(2013, 1, 5)
            });

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-1)) }, MatchId = 10, Sort = 2 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test1",
                TournamentView = new TestGroupVw()
                {
                    Sort = 3,

                },
                SportView = new TestGroupVw()
                    {
                        Sort = 1,
                    },
                StartDate = new DateTime(2013, 1, 5)
            });
            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-2)) }, MatchId = 11 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test8",
                TournamentView = new TestGroupVw()
                {
                    Sort = 4,

                },
                SportView = new TestGroupVw()
                    {
                        Sort = 1,
                    },
                StartDate = new DateTime(2013, 1, 5)
            });

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.Started,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-2)) }, Sort = 1 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test2",
                TournamentView = new TestGroupVw()
                {
                    Sort = 1,

                },
                SportView = new TestGroupVw()
                {
                    Sort = 1,
                },
                StartDate = new DateTime(2013, 1, 2)
            });


            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-3)) }, MatchId = 2 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test3",
                TournamentView = new TestGroupVw()
                {
                    Sort = 1,

                },
                SportView = new TestGroupVw()
                {
                    DisplayName = "s",
                    Sort = 1,
                },
                StartDate = new DateTime(2013, 1, 3)
            });

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.Stopped,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-3)) }, MatchId = 1 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test4",
                TournamentView = new TestGroupVw()
                    {
                        Sort = 1,
                    },
                SportView = new TestGroupVw()
                {
                    DisplayName = "s",
                    Sort = 1,
                },
                StartDate = new DateTime(2013, 1, 4)
            });
            var LiveViewModel = new LiveViewModel();
            LiveViewModel.OnNavigationCompleted();
            LiveViewModel.Update();
            Thread.Sleep(2000);
            collection.Sort(LiveViewModel.Comparison);

            foreach (var matchVw in collection)
            {
                Console.WriteLine(matchVw.Name + " s sort: " + matchVw.SportView.Sort + " t sort: " + matchVw.TournamentView.Sort + " m sort: " + matchVw.LineObject.Sort + " start: (" + matchVw.LineObject.StartDate.Value.UtcDateTime + ") m id: " + matchVw.LineObject.MatchId);
            }


            Console.WriteLine();
            Assert.AreEqual("test3", collection[0].Name);//s
            Assert.AreEqual("test4", collection[1].Name);//s
            Assert.AreEqual("test2", collection[2].Name);//s
            Assert.AreEqual("test1", collection[3].Name);//s
            Assert.AreEqual("test8", collection[4].Name);//s
            Assert.AreEqual("test5", collection[5].Name);//s


            collection = new SortableObservableCollection<IMatchVw>(collection.OrderByDescending(x => x.Name).ToList());

            collection.Sort(LiveViewModel.Comparison);


            foreach (var matchVw in collection)
            {
                Console.WriteLine(matchVw.Name + " s sort: " + matchVw.SportView.Sort + " t sort: " + matchVw.TournamentView.Sort + " m sort: " + matchVw.LineObject.Sort + " start: (" + matchVw.LineObject.StartDate.Value.UtcDateTime + ") m id: " + matchVw.LineObject.MatchId);
            }

            Assert.AreEqual("test3", collection[0].Name);//s
            Assert.AreEqual("test4", collection[1].Name);//s
            Assert.AreEqual("test2", collection[2].Name);//s
            Assert.AreEqual("test1", collection[3].Name);//s
            Assert.AreEqual("test8", collection[4].Name);//s
            Assert.AreEqual("test5", collection[5].Name);//s





        }


        [TestMethod]
        public void TopGamesInvalidjson()
        {
            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            string jsonfeed = "{\"tournamentData\":[],\"sportData\":[],\"matchData\":[]}";

            WsdlRepository.Expect(x => x.Matchsorting(It.IsAny<string>())).Returns(jsonfeed);


            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "1"));
            ChangeTracker.Setup(x => x.SelectedTimeFilter).Returns(new ComboBoxItem("1", 4));
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>()
                {
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                });
            SortableObservableCollection<IMatchVw> collection = new SortableObservableCollection<IMatchVw>();

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn()
                {
                    StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-1)) }
                },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test5",
                TournamentView = new TestGroupVw()
                {
                    Sort = 3,

                },
                SportView = new TestGroupVw()
                    {
                        Sort = 2,
                    },
                StartDate = new DateTime(2013, 1, 5)
            });

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-1)) }, MatchId = 10, Sort = 2 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test1",
                TournamentView = new TestGroupVw()
                {
                    Sort = 3,

                },
                SportView = new TestGroupVw()
                    {
                        Sort = 1,
                    },
                StartDate = new DateTime(2013, 1, 5)
            });
            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-2)) }, MatchId = 11 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test8",
                TournamentView = new TestGroupVw()
                {
                    Sort = 4,

                },
                SportView = new TestGroupVw()
                    {
                        Sort = 1,
                    },
                StartDate = new DateTime(2013, 1, 5)
            });

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.Started,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-2)) }, Sort = 1 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test2",
                TournamentView = new TestGroupVw()
                {
                    Sort = 1,

                },
                SportView = new TestGroupVw()
                {
                    Sort = 1,
                },
                StartDate = new DateTime(2013, 1, 2)
            });


            collection.Add(new TestMatchVw()
            {
                IsLiveBet = false,
                LiveBetStatus = eMatchStatus.Undefined,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-3)) }, MatchId = 2 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test3",
                TournamentView = new TestGroupVw()
                {
                    Sort = 1,

                },
                SportView = new TestGroupVw()
                {
                    DisplayName = "s",
                    Sort = 1,
                },
                StartDate = new DateTime(2013, 1, 3)
            });

            collection.Add(new TestMatchVw()
            {
                IsLiveBet = true,
                LiveBetStatus = eMatchStatus.Stopped,
                LiveMatchMinute = 1,
                LineObject = new MatchLn() { StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(-3)) }, MatchId = 1 },
                LivePeriodInfo = eLivePeriodInfo.Basketball_1st_Quarter,
                Name = "test4",
                TournamentView = new TestGroupVw()
                    {
                        Sort = 1,
                    },
                SportView = new TestGroupVw()
                {
                    DisplayName = "s",
                    Sort = 1,
                },
                StartDate = new DateTime(2013, 1, 4)
            });
            var LiveViewModel = new LiveViewModel();
            LiveViewModel.OnNavigationCompleted();
            Thread.Sleep(2000);
            collection.Sort(LiveViewModel.Comparison);

            foreach (var matchVw in collection)
            {
                Console.WriteLine(matchVw.Name + " s sort: " + matchVw.SportView.Sort + " t sort: " + matchVw.TournamentView.Sort + " m sort: " + matchVw.LineObject.Sort + " start: (" + matchVw.LineObject.StartDate.Value.UtcDateTime + ") m id: " + matchVw.LineObject.MatchId);
            }







        }


        [TestMethod]
        public void PreLiveMatchesOnlyFor24Hours()
        {

            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(1) });

            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "1"));
            ChangeTracker.Setup(x => x.SelectedTimeFilter).Returns(new ComboBoxItem("1", 0));
            ChangeTracker.Setup(x => x.TimeFilters).Returns(new ObservableCollection<ComboBoxItem>() { new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), new ComboBoxItem("", 1), });

            ChangeTracker.Setup(x => x.SportFilters).Returns(new ObservableCollection<ComboBoxItemStringId>() { new ComboBoxItemStringId("1", "1"), new ComboBoxItemStringId("2", "2") });
            ChangeTracker.Setup(x => x.SelectedSportFilter).Returns(new ComboBoxItemStringId("1", "1"));
            StationRepository.Setup(x => x.AllowFutureMatches).Returns(true);
            var matchLiveNotActive = TestMatchLn.CreateMatch(1, true);
            matchLiveNotActive.Active.Value = false;
            ((TestMatchVw)matchLiveNotActive.MatchView).LiveBetStatus = eMatchStatus.NotStarted;

            Repository.Expect(x => x.GetByBtrMatchId(1, true)).Returns(matchLiveNotActive);


            var LiveViewModel = new LiveViewModel();
            var match = TestMatchLn.CreateMatch(1, false);
            match.StartDate.Value = new DateTimeSr(DateTime.Now.AddHours(1));
            var result = LiveViewModel.MatchFilter(match);

            Assert.IsTrue(result);



            match = TestMatchLn.CreateMatch(1, false);
            match.StartDate.Value = new DateTimeSr(DateTime.Now.AddHours(24).AddMinutes(1));
            ((TestMatchVw)match.MatchView).LiveBetStatus = eMatchStatus.NotStarted;
            result = LiveViewModel.MatchFilter(match);

            Assert.IsFalse(result);



        }

        [TestMethod]
        [Ignore]
        public void JsonLiveStreamFeedSport()
        {

            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("1", "1");
            ChangeTracker.Object.SelectedTimeFilter = new ComboBoxItem("1", 0);
            ChangeTracker.Object.TimeFilters = new ObservableCollection<ComboBoxItem>()
                {
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                    new ComboBoxItem("",1),
                };
            ChangeTracker.Object.SportFilters = new ObservableCollection<ComboBoxItemStringId>() { new ComboBoxItemStringId("", "0"), new ComboBoxItemStringId("", "0") };
            ChangeTracker.Object.SelectedSportFilter = new ComboBoxItemStringId("", "0");

            string jsonfeed = @"
{
  ""queryUrl"": ""matches\/0"",
  ""doc"": [
    {
      ""event"": ""matches"",
      ""_dob"": 1389249397,
      ""_maxage"": 5,
      ""_factor"": 1,
      ""data"": [
        {
          ""_key"": ""event"",
          ""eventid"": 3948943,
          ""betradarid"": 7451782,
          ""eventinfo"": {
            ""name"": ""Soccer - Ligue 2"",
            ""description"": ""N\/A""
          },
          ""sportinfo"": {
            ""sport"": ""Soccer"",
            ""liga"": ""Ligue 2"",
            ""competition"": ""Ligue 2"",
            ""season"": ""Ligue 2 13\/14"",
            ""hometeam"": ""SCO ANGERS"",
            ""awayteam"": ""AS NANCY LORRAINE""
          },
          ""schedule"": {
            ";
            var startDate = DateTime.Now;
            startDate = startDate.AddSeconds(-startDate.Second);

            jsonfeed += "\"day\": \"" + startDate.ToShortDateString() + "\",\"start\": {\"time\": \"" + startDate.ToShortTimeString() + "\",\"offset\": \"+" + TimeZone.CurrentTimeZone.GetUtcOffset(startDate).Hours + "\"},";
            jsonfeed += @"""end"": {
              ""time"": ""16:00"",
              ""offset"": ""+1""
            }
          },
          ""status"": ""N\/A"",
          ""georestriction"": [
            {
              ""_key"": ""disallow"",
              ""_value"": """"
            }
          ],
          ""stream"": {
            ""streamid"": ""348"",
            ""streamstatus"": ""N\/A"",
            ""streamstatustext"": ""N\/A""
          }
        },
        {
          ""_key"": ""event"",
          ""eventid"": 3948947,
          ""betradarid"": 7451784,
          ""eventinfo"": {
            ""name"": ""Soccer - Ligue 2"",
            ""description"": ""N\/A""
          },
          ""sportinfo"": {
            ""sport"": ""Soccer"",
            ""liga"": ""Ligue 2"",
            ""competition"": ""Ligue 2"",
            ""season"": ""Ligue 2 13\/14"",
            ""hometeam"": ""STADE BRESTOIS 29"",
            ""awayteam"": ""RC LENS""
          },
          ""schedule"": {";

            jsonfeed += "\"day\": \"" + DateTime.Now.AddHours(2).ToShortDateString() + "\",\"start\": {\"time\": \"" + DateTime.Now.AddHours(2).ToShortTimeString() + "\",\"offset\": \"+" + TimeZone.CurrentTimeZone.GetUtcOffset(startDate).Hours + "\"},";

            jsonfeed += @"
            ""end"": {
              ""time"": ""22:30"",
              ""offset"": ""+1""
            }
          },
          ""status"": ""N\/A"",
          ""georestriction"": [
            {
              ""_key"": ""disallow"",
              ""_value"": """"
            }
          ],
          ""stream"": {
            ""streamid"": ""348"",
            ""streamstatus"": ""N\/A"",
            ""streamstatustext"": ""N\/A""
          }
        },
        {
          ""_key"": ""event"",
          ""eventid"": 4879446,
          ""betradarid"": 7605263,
          ""eventinfo"": {
            ""name"": ""Tennis - Germany F1, Doubles"",
            ""description"": ""N\/A""
          },
          ""sportinfo"": {
            ""sport"": ""Tennis"",
            ""liga"": ""Germany F1, Doubles"",
            ""competition"": ""EMPTY"",
            ""season"": null,
            ""hometeam"": ""RASPUDIC F \/ VEGER F"",
            ""awayteam"": ""ARNEODO R \/ GROEN S""
          },
          ""schedule"": {
            ""day"": ""2014-01-09"",
            ""start"": {
              ""time"": ""18:00"",
              ""offset"": ""+1""
            },
            ""end"": {
              ""time"": ""20:00"",
              ""offset"": ""+1""
            }
          },
          ""status"": ""N\/A"",
          ""georestriction"": [
            {
              ""_key"": ""disallow"",
              ""_value"": """"
            }
          ],
          ""stream"": {
            ""streamid"": ""660"",
            ""streamstatus"": ""N\/A"",
            ""streamstatustext"": ""N\/A""
          }
        }
      ]
    }
  ]
}
";
            LiveStreamService.Setup(x => x.GetLiveStreamFeed()).Returns(jsonfeed);


            Repository.Setup(x => x.FindMatches(It.IsAny<SortableObservableCollection<IMatchVw>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LineSr.DelegateFilterMatches>(), It.IsAny<Comparison<IMatchVw>>())).Returns(new SortableObservableCollection<IMatchVw>() { TestMatchVw.CreateMatch(3948943), TestMatchVw.CreateMatch(0), TestMatchVw.CreateMatch(3948947) });

            StationRepository.Object.EnableLiveStreaming = true;

            var model = new LiveViewModel();
            model.OnNavigationCompleted();
            Assert.AreEqual(3, LiveViewModel.StreamData.Count);
            Assert.AreEqual(3948943, LiveViewModel.StreamData.First().Value.BtrMatchId);
            Assert.AreEqual(startDate.ToString(), LiveViewModel.StreamData.First().Value.StartDate.ToString());

            model.Update();
            Assert.AreEqual(3, model.Matches.Count);
            Assert.AreEqual(3948943, model.Matches[0].LineObject.BtrMatchId);
            Assert.AreEqual(0, model.Matches[1].LineObject.BtrMatchId);
            Assert.AreEqual(3948947, model.Matches[2].LineObject.BtrMatchId);


            Assert.AreEqual(true, model.Matches[0].HaveStream);
            //Assert.AreEqual(true, model.Matches[0].StreamStarted);
            Assert.AreEqual(false, model.Matches[1].HaveStream);
            //Assert.AreEqual(false, model.Matches[1].StreamStarted);
            Assert.AreEqual(true, model.Matches[2].HaveStream);
            //Assert.AreEqual(false, model.Matches[2].StreamStarted);




        }

    }
}