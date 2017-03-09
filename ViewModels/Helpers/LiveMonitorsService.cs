using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BaseObjects.ViewModels;
using Newtonsoft.Json;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using TranslationByMarkupExtension;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SharedInterfaces;
using IocContainer;
using Ninject;
using System;
using System.Diagnostics;
using ViewModels.ViewModels;
using WsdlRepository;
using SportRadar.DAL.OldLineObjects;
using System.Timers;
using System.Windows.Forms;

namespace ViewModels.Helpers
{
    public class LiveMonitorsService : IClosable
    {
        System.Timers.Timer aTimer;
        private const double ITEM_DEFAULT_HEIGHT = 91;
        private const double ITEM_WITH_HEADER_DEFAULT_HEIGHT = 136;
        int currentMatchForRotation = 0;
        int rotationCounter = 0;
        private bool _offsetChanged = false;

        SyncList<SyncObservableCollection<IMatchVw>> LiveMonitorsCollectionList = new SyncList<SyncObservableCollection<IMatchVw>>();
        List<double> LiveMonitorHeights = new List<double>();
        List<int> LiveMonitorRotationCounters = new List<int>();
        SyncList<LiveMonitorViewModel> lmViewModels = new SyncList<LiveMonitorViewModel>();
        SyncList<LiveMonitorViewModel> lmViewModelsDefault = new SyncList<LiveMonitorViewModel>();
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();

        private IDispatcher _dispatcher;
        public IDispatcher Dispatcher
        {
            get
            {
                return _dispatcher ?? (_dispatcher = IoCContainer.Kernel.Get<IDispatcher>());
            }
        }
        public IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        public LiveMonitorsService()
        {
            Mediator = new MessageStorage();
            SetCollectionsAndStartMonitors();
            Mediator.Register<bool>(this, Refresh, MsgTag.RefreshLiveMonitor);
            Mediator.Register<bool>(this, SetMonitors, MsgTag.UpdateLiveMonitorTemplates);
            Mediator.Register<long>(this, ShowVFL, MsgTag.ShowVFL);
            Mediator.Register<bool>(this, ShowVHC, MsgTag.ShowVHC);
            Mediator.Register<bool>(this, ChangeOffset, MsgTag.OffsetChanged);
            Mediator.ApplyRegistration();
            LineSr.DataSqlUpdateSucceeded += DataCopy_DataSqlUpdateSucceeded;

            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += aTimer_Elapsed;

            SetMonitors(true);

            aTimer.Start();
        }

        void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Rotate();
            //throw new NotImplementedException();
        }

        private void ChangeOffset(bool changeOffset)
        {
            _offsetChanged = changeOffset;
        }

        public void ShowVFL(long channel)
        {
            try
            {
                if (!lmViewModelsDefault.Equals(lmViewModels))
                {
                    lmViewModels = new SyncList<LiveMonitorViewModel>(lmViewModelsDefault);

                    var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();

                    if (screens.Count != 0)
                    {
                        /*
                         * Index '0' correspond to first Live monitor available and is a default value. 
                         * Additional logic should be added as soon as sending Secondary screen parameter from HUB will be implemented.
                         */
                        lmViewModels[0].LiveOddsVisibility = Visibility.Collapsed;
                        lmViewModels[0].HeaderTextVisibility = Visibility.Collapsed;
                        lmViewModels[0].BannerVisibility = Visibility.Collapsed;
                        lmViewModels[0].rotating = false;
                        lmViewModels[0].rotationInterval = 10;
                        lmViewModels[0].MonitorWidth = screens[0].WorkingArea.Width;
                        lmViewModels[0].MonitorHeight = screens[0].WorkingArea.Height;

                        Dispatcher.Invoke(() =>
                        {
                            if (!lmViewModels[0].IsClosed)
                                lmViewModels[0].ViewWindow.Visibility = Visibility.Collapsed;

                        });

                        Mediator.SendMessage("0|" + Visibility.Visible.ToString(), MsgTag.ChangeVisibility);

                        Mediator.SendMessage("0|" + SetVFLAddress(screens[0], channel), MsgTag.SetUpBrowser);
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        public void ShowVHC(bool result)
        {
            try
            {
                if (!lmViewModelsDefault.Equals(lmViewModels))
                {
                    lmViewModels = new SyncList<LiveMonitorViewModel>(lmViewModelsDefault);

                    var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();

                    if (screens.Count != 0)
                    {
                        /*
                         * Index '0' correspond to first Live monitor available and is a default value. 
                         * Additional logic should be added as soon as sending Secondary screen parameter from HUB will be implemented.
                         */
                        lmViewModels[0].LiveOddsVisibility = Visibility.Collapsed;
  
                        lmViewModels[0].HeaderTextVisibility = Visibility.Collapsed;
                        lmViewModels[0].BannerVisibility = Visibility.Collapsed;
                        lmViewModels[0].rotating = false;
                        lmViewModels[0].rotationInterval = 10;
                        lmViewModels[0].MonitorWidth = screens[0].WorkingArea.Width;
                        lmViewModels[0].MonitorHeight = screens[0].WorkingArea.Height;

                        Dispatcher.Invoke(() =>
                        {
                            lmViewModels[0].ViewWindow.Visibility = Visibility.Collapsed;

                        });

                        Mediator.SendMessage("0|" + Visibility.Visible.ToString(), MsgTag.ChangeVisibility);
                        Mediator.SendMessage("0|" + SetVHCAddress(screens[0]), MsgTag.SetUpBrowser);

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        object locker = new object();
        [AsyncMethod]
        public void SetMonitors(bool result)
        {
            lock (locker)
            {


                MonitorTemplates deserializedFeed = null;
                try
                {

                    if (!lmViewModelsDefault.Equals(lmViewModels))
                    {
                        lmViewModels = new SyncList<LiveMonitorViewModel>(lmViewModelsDefault);
                    }

                    foreach (LiveMonitorViewModel lv in lmViewModels)
                    {
                        lv.liveOddsPage = 1000;
                    }

                    string number = StationRepository.StationNumber;
                    string templates = WsdlRepository.GetContentManagementData(StationRepository.StationNumber);
                    deserializedFeed = JsonConvert.DeserializeObject<MonitorTemplates>(templates);

                    var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();
                    for (int i = 0; i < ChangeTracker.LiveMonitors.Count; i++)
                    {
                        string index = screens[i].DeviceName.Substring(screens[i].DeviceName.Length - 1, 1);
                        lmViewModels[i].LiveOddsVisibility = Visibility.Collapsed;

                        lmViewModels[i].HeaderTextVisibility = Visibility.Collapsed;
                        lmViewModels[i].BannerVisibility = Visibility.Collapsed;
                        lmViewModels[i].rotating = false;
                        lmViewModels[i].rotationInterval = 10;
                        lmViewModels[i].MonitorWidth = screens[i].WorkingArea.Width;
                        lmViewModels[i].MonitorHeight = screens[i].WorkingArea.Height;                      

                        MonitorTemplate temp = deserializedFeed.monitorTemplates.Where(x => x.monitorPosition == index).FirstOrDefault();
                        if (temp == null)
                        {
                            Mediator.SendMessage(i + "|" + Visibility.Collapsed.ToString(), MsgTag.ChangeVisibility);

                            Dispatcher.Invoke(() =>
                            {
                                if (!lmViewModels[i].IsClosed)
                                    lmViewModels[i].ViewWindow.Visibility = Visibility.Visible;
                            });
                            lmViewModels[i].HeaderTextVisibility = Visibility.Collapsed;
                            lmViewModels[i].BannerVisibility = Visibility.Collapsed;
                            lmViewModels[i].rotating = false;
                            lmViewModels[i].rotationInterval = 10;
                            lmViewModels[i].MonitorWidth = screens[i].WorkingArea.Width;
                            lmViewModels[i].MonitorHeight = screens[i].WorkingArea.Height;

                            lmViewModels[i].LiveOddsRow = 0;
                            lmViewModels[i].LiveOddsVisibility = Visibility.Visible;
                            lmViewModels[i].LiveOddsHeight = screens[i].WorkingArea.Height;
                            lmViewModels[i].liveOddsPage = i + 1001;

                        }
                        else
                        {
                            string templateId = temp.templateId;
                            AvailableTemplate template = deserializedFeed.availableTemplates.Where(x => x.templateId == templateId).FirstOrDefault();
                            if (template != null)
                            {
                                for (int y = 0; y < template.content.Count(); y++)
                                {
                                    string type = template.content.ElementAt(y).contentType;
                                    if (type == "VFL")
                                    {
                                        Dispatcher.Invoke(() =>
                                            {
                                                if (!lmViewModels[i].IsClosed)
                                                    lmViewModels[i].ViewWindow.Visibility = Visibility.Collapsed;

                                            });
     
                                        Mediator.SendMessage(i + "|" + Visibility.Visible.ToString(), MsgTag.ChangeVisibility);

                                        Mediator.SendMessage(i + "|" + SetVFLAddress(screens[i], 0), MsgTag.SetUpBrowser);

                                    }
                                    else if (type == "VHC")
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            if (!lmViewModels[i].IsClosed)
                                                lmViewModels[i].ViewWindow.Visibility = Visibility.Collapsed;

                                        });

                                        Mediator.SendMessage(i + "|" + Visibility.Visible.ToString(),
                                            MsgTag.ChangeVisibility);
                                        Mediator.SendMessage(i + "|" + SetVHCAddress(screens[i]), MsgTag.SetUpBrowser);


                                    }
                                    else
                                    {
                                        Mediator.SendMessage(i + "|" + Visibility.Collapsed.ToString(), MsgTag.ChangeVisibility);


                                        Dispatcher.Invoke(() =>
                                        {
                                            if (!lmViewModels[i].IsClosed)
                                                lmViewModels[i].ViewWindow.Visibility = Visibility.Visible;
                                        });
                                        //lmViewModels[i].HeaderTextVisibility = Visibility.Collapsed;
                                        //lmViewModels[i].BannerVisibility = Visibility.Collapsed;
                                        //lmViewModels[i].rotating = false;
                                        //lmViewModels[i].rotationInterval = 10;
                                        //lmViewModels[i].MonitorWidth = screens[i].WorkingArea.Width;
                                        //lmViewModels[i].MonitorHeight = screens[i].WorkingArea.Height;

                                        if (type == "HEADER_TEXT")
                                        {
                                            lmViewModels[i].HeaderRow = y;
                                            lmViewModels[i].HeaderTextVisibility = Visibility.Visible;
                                            double dd = Int64.Parse(template.content.ElementAt(y).size);
                                            double sc = screens[i].WorkingArea.Height;
                                            double res = dd*sc/100;
                                            lmViewModels[i].HeaderTextHeight = res;
                                            lmViewModels[i].HeaderText = template.content.ElementAt(y).data;
                                        }
                                        else if (type == "LIVE_ODDS")
                                        {
                                            lmViewModels[i].LiveOddsRow = y;
                                            lmViewModels[i].LiveOddsVisibility = Visibility.Visible;
                                            double desiredHeight = Int64.Parse(template.content.ElementAt(y).size)*
                                                                   screens[i].WorkingArea.Height/100;
                                            ;
                                            if (desiredHeight != lmViewModels[i].LiveOddsHeight)
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    LiveMonitorsCollectionList[i].Clear();
                                                });

                                                lmViewModels[i].LiveOddsHeight = desiredHeight;
                                            }

                                            if (template.content.ElementAt(y).data.ToLowerInvariant().Contains("page"))
                                            {
                                                string page =
                                                    template.content.ElementAt(y)
                                                        .data.Substring(template.content.ElementAt(y).data.Length - 1, 1);
                                                lmViewModels[i].liveOddsPage = Int16.Parse(page);
                                                lmViewModels[i].rotating = false;
                                                lmViewModels[i].rotationInterval = 10;
                                            }
                                            else if (
                                                template.content.ElementAt(y)
                                                    .data.ToLowerInvariant()
                                                    .Contains("rotating"))
                                            {
                                                //if(!lmViewModels[i].rotating)
                                                //    LiveMonitorRotationCounters[i] = 0;

                                                lmViewModels[i].rotating = true;
                                                lmViewModels[i].rotationInterval =
                                                    Int16.Parse(template.content.ElementAt(y).additParams);
                                            }
                                        }
                                        else if (type == "BANNER")
                                        {
                                            double dd = Int64.Parse(template.content.ElementAt(y).size);
                                            double sc = screens[i].WorkingArea.Height;
                                            double res = dd*sc/100;

                                            lmViewModels[i].BannerRow = y;
                                            lmViewModels[i].BannerVisibility = Visibility.Visible;
                                            lmViewModels[i].BannerHeight = res;
                                            lmViewModels[i].BannerLink = template.content.ElementAt(y).data;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    LiveMonitorViewModel vm = lmViewModels.Where(x => x.liveOddsPage != 1000).FirstOrDefault();
                    LiveMonitorViewModel vmRotating = lmViewModels.Where(x => x.rotating).FirstOrDefault();
                    if (vm != null || vmRotating != null)
                    {
                        lmViewModels = new SyncList<LiveMonitorViewModel>(lmViewModels.OrderBy(x => x.liveOddsPage).ToList());
                        LiveMonitorHeights.Clear();
                        for (int i = 0; i < lmViewModels.Count; i++)
                        {
                            LiveMonitorHeights.Add(lmViewModels[i].LiveOddsHeight);
                            lmViewModels[i].MatchesCollection = LiveMonitorsCollectionList[i];
                        }
                    }
                    else
                    {
                        ResetMonitorsHeight();
                    }

                }
                catch (Exception ex)
                {
                }
            }
        }

        private string SetVFLAddress(Screen screen, long channel)
        {
            if (screen.WorkingArea.Width <= 1250) return "http://vflsrterminal.aitcloud.de/vflterminal/vleague_terminal.php?clientid=642&lang=" + SelectedLanguage.ToLowerInvariant() + "&resolution=1024&channel=" + channel;
            else if (screen.WorkingArea.Width <= 1350) return "http://vflsrterminal.aitcloud.de/vflterminal/vleague_terminal.php?clientid=642&lang=" + SelectedLanguage.ToLowerInvariant() + "&resolution=1280&channel=" + channel;
            else if (screen.WorkingArea.Width <= 1900) return "http://vflsrterminal.aitcloud.de/vflterminal/vleague_terminal.php?clientid=642&lang=" + SelectedLanguage.ToLowerInvariant() + "&resolution=1366&channel=" + channel;

            return "http://vflsrterminal.aitcloud.de/vflterminal/vleague_terminal.php?clientid=642&lang=" + SelectedLanguage.ToLowerInvariant() + "&resolution=1920&channel=" + channel;
        }

        private string SetVHCAddress(Screen screen)
        {
            if (screen.WorkingArea.Width <= 1250) return "http://vhcsrterminal.aitcloud.de/vhcshop/terminal/1024/clientid:642/lang:" + SelectedLanguage.ToLowerInvariant() + "/channel:0";
            else if (screen.WorkingArea.Width <= 1350) return "http://vhcsrterminal.aitcloud.de/vhcshop/terminal/1280/clientid:642/lang:" + SelectedLanguage.ToLowerInvariant() + "/channel:0";
            else if (screen.WorkingArea.Width <= 1900) return "http://vhcsrterminal.aitcloud.de/vhcshop/terminal/1366/clientid:642/lang:" + SelectedLanguage.ToLowerInvariant() + "/channel:0";

            return "http://vhcsrterminal.aitcloud.de/vhcshop/terminal/1920/clientid:642/lang:" + SelectedLanguage.ToLowerInvariant() + "/channel:0";
        }

        private void Refresh(bool obj)
        {
            foreach (var monitor in LiveMonitorsCollectionList)
            {
                monitor.ApplyChanges(new List<IMatchVw>());
            }

            Update();
        }

        void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.LiveBet)
                Update();
        }

        #region Methods

        private void SetCollectionsAndStartMonitors()
        {
            int index = 0;
            var screens = Screen.AllScreens.Where(s => !s.Primary).ToList();
            foreach (var liveMonitor in ChangeTracker.LiveMonitors)
            {
                LiveWindowEntry monitor = liveMonitor;
                Dispatcher.Invoke(() =>
                    {
                        monitor.Window.Show();
                        monitor.Window.WindowState = WindowState.Normal;
                        monitor.Window.WindowState = WindowState.Maximized;

                        LiveMonitorsCollectionList.Add(new SyncObservableCollection<IMatchVw>());
                        lmViewModels.Add((LiveMonitorViewModel)monitor.DataContext);
                        lmViewModels.Last().MatchesCollection = LiveMonitorsCollectionList.LastOrDefault();

                        LiveMonitorHeights.Add(screens.ElementAt(index).WorkingArea.Height);
                        LiveMonitorRotationCounters.Add(0);
                        index++;
                    });
                // Debug.Assert(monitor.threadId == LiveMonitorsCollectionList.Last().threadid);
            }

            lmViewModelsDefault = new SyncList<LiveMonitorViewModel>(lmViewModels);
        }

        private void ResetMonitorsHeight()
        {
            LiveMonitorHeights.Clear();
            foreach (var liveMonitor in ChangeTracker.LiveMonitors)
            {
                LiveWindowEntry monitor = liveMonitor;
                LiveMonitorHeights.Add(((LiveMonitorViewModel)monitor.DataContext).MonitorHeight);

            }
        }

        public MessageStorage Mediator { get; set; }

        private void Update()
        {
            if (LiveMonitorsCollectionList.Count == 0 || !StationRepository.IsLiveMatchEnabled)
                return;

            Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, Comparison);

            SplitCollection();
        }

        protected SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }

        private void SplitCollection()
        {

            if (ChangeTracker.LiveMonitors.Count <= 0)
                return;

            SyncList<IMatchVw> lMatches = Matches.ToSyncList();
            int iMatchIdx = 0;

            int page = -1;

            for (int i = 0; i < LiveMonitorsCollectionList.Count; i++)
            {
                if (lmViewModels[i].rotating || lmViewModels[i].LiveOddsVisibility == Visibility.Collapsed)
                    continue;

                LiveMonitorViewModel lmvm = null;
                Dispatcher.Invoke(() =>
                {
                    lmvm = ChangeTracker.LiveMonitors[i].Window.DataContext as LiveMonitorViewModel;
                });
                Debug.Assert(lmvm != null);

                double dblItemHeight = lmvm != null && lmvm.ItemHeight > 0.0d ? lmvm.ItemHeight : ITEM_DEFAULT_HEIGHT;
                double dblHeaderItemHeight = lmvm != null && lmvm.HeaderItemHeight > 0.0d ? lmvm.HeaderItemHeight : ITEM_WITH_HEADER_DEFAULT_HEIGHT;

                Dispatcher.Invoke(() =>
                {
                    SyncObservableCollection<IMatchVw> socMonitor = LiveMonitorsCollectionList[i];
                    double dblCurrentMonitorFilledHeight = 0;

                    SyncList<IMatchVw> lMatchesTemp = new SyncList<IMatchVw>();

                    IMatchVw matchVw = lMatches.Count > iMatchIdx ? lMatches[iMatchIdx] : null;
                    string currentSport = "";
                    bool isPreLiveOld = false;

                    while (matchVw != null)
                    {
                        var isPreLive = matchVw.LiveBetStatus == eMatchStatus.NotStarted || !matchVw.IsLiveBet;

                        if (matchVw.SportDescriptor != currentSport)
                        {
                            matchVw.IsHeaderForLiveMonitor = true;
                            currentSport = matchVw.SportDescriptor;
                        }
                        else if (page != i)
                        {
                            matchVw.IsHeaderForLiveMonitor = true;
                            page = i;
                        }
                        else if (isPreLive && !isPreLiveOld)
                        {
                            matchVw.IsHeaderForLiveMonitor = true;
                            isPreLiveOld = isPreLive;
                        }
                        else
                        {
                            if (dblCurrentMonitorFilledHeight + dblItemHeight <= LiveMonitorHeights[i])
                                matchVw.IsHeaderForLiveMonitor = false;
                        }

                        currentSport = matchVw.SportDescriptor;
                        page = i;
                        isPreLiveOld = isPreLive;

                        dblCurrentMonitorFilledHeight += matchVw.IsHeaderForLiveMonitor ? dblHeaderItemHeight : dblItemHeight;

                        if (dblCurrentMonitorFilledHeight > LiveMonitorHeights[i])
                        {
                            page = i;
                            break;
                        }
                        else
                        {
                            lMatchesTemp.Add(matchVw);
                        }

                        iMatchIdx++;
                        matchVw = lMatches.Count > iMatchIdx ? lMatches[iMatchIdx] : null;
                    }
                    //Debug.Assert(ChangeTracker.LiveMonitors[i].threadId == socMonitor.threadid);

                    //Debug.Assert(Thread.CurrentThread == socMonitor.threadid);

                    socMonitor.ApplyChanges(lMatchesTemp);
                });
            }
        }

        public void Rotate()
        {
            if (ChangeTracker.LiveMonitors.Count <= 0)
                return;

            if (this.rotationCounter >= 1000)
                rotationCounter = 0;

            this.rotationCounter++;

            LiveMonitorViewModel lmvm = null;

            SyncList<IMatchVw> lMatches = Matches.ToSyncList();
            
            for (int i = 0; i < LiveMonitorsCollectionList.Count; i++)
            {
                if (!lmViewModels[i].rotating || lmViewModels[i].LiveOddsVisibility == Visibility.Collapsed)
                    continue;

                //check interval
                if (this.rotationCounter % lmViewModels[i].rotationInterval != 0 && this.rotationCounter != 0)
                {
                    continue;
                }

                lmvm = ChangeTracker.LiveMonitors[i].DataContext as LiveMonitorViewModel;
                Debug.Assert(lmvm != null);

                double dblItemHeight = lmvm != null && lmvm.ItemHeight > 0.0d ? lmvm.ItemHeight : ITEM_DEFAULT_HEIGHT;
                double dblHeaderItemHeight = lmvm != null && lmvm.HeaderItemHeight > 0.0d ? lmvm.HeaderItemHeight : ITEM_WITH_HEADER_DEFAULT_HEIGHT;

                if (LiveMonitorRotationCounters[i] >= lMatches.Count)
                    LiveMonitorRotationCounters[i] = LiveMonitorRotationCounters[i] - lMatches.Count;

                int iMatchIdx = LiveMonitorRotationCounters[i];

                if (iMatchIdx >= lMatches.Count)
                    iMatchIdx = 0;

                IMatchVw matchVw = lMatches.Count > iMatchIdx ? lMatches[iMatchIdx] : null;



                SyncObservableCollection<IMatchVw> socMonitor = LiveMonitorsCollectionList[i];
                double dblCurrentMonitorFilledHeight = 0;

                SyncList<IMatchVw> lMatchesTemp = new SyncList<IMatchVw>();

                string oldSport = "";
                bool isPreLiveOld = false;

                while (matchVw != null)
                {
                    if (matchVw == null || lMatchesTemp.Contains(matchVw))
                        break;

                    string currentSport = matchVw.SportDescriptor;
                    var isPreLive = matchVw.LiveBetStatus == eMatchStatus.NotStarted || !matchVw.IsLiveBet;
                    if (!string.IsNullOrEmpty(currentSport))
                    {
                        if (currentSport != oldSport)
                        {
                            matchVw.IsHeaderForRotation = true;
                        }
                        else if (isPreLive && !isPreLiveOld)
                        {
                            matchVw.IsHeaderForRotation = true;
                        }
                        else
                        {
                            matchVw.IsHeaderForRotation = false;
                        }
                        oldSport = currentSport;
                        isPreLiveOld = isPreLive;
                    }

                    dblCurrentMonitorFilledHeight += matchVw.IsHeaderForRotation ? dblHeaderItemHeight : dblItemHeight;

                    if (dblCurrentMonitorFilledHeight > lmViewModels[i].LiveOddsHeight) //lmViewModels[i].LiveOddsHeight)
                    {
                        break;
                    }
                    else
                    {
                        lMatchesTemp.Add(matchVw);
                    }

                    iMatchIdx++;

                    if (iMatchIdx >= lMatches.Count)
                        iMatchIdx = 0;

                    matchVw = lMatches.Count > iMatchIdx ? lMatches[iMatchIdx] : null;

                    LiveMonitorRotationCounters[i]++;
                }

                Dispatcher.Invoke(() =>
                    {
                        socMonitor.ApplyChanges(lMatchesTemp); //damn thing does not applay normally changes for collections with duplicate matches with it...
                    });
                
            }
        }

        public static void UpdateHeaders<T>(SortableObservableCollection<T> MatchesCollection) where T : IMatchVw
        {
            string oldSport = "";
            bool isPreLiveOld = false;
            for (int i = 0; i < MatchesCollection.Count; i++)
            {
                string currentSport = MatchesCollection[i].SportDescriptor;
                var isPreLive = MatchesCollection[i].LiveBetStatus == eMatchStatus.NotStarted || !MatchesCollection[i].IsLiveBet;
                if (string.IsNullOrEmpty(currentSport))
                    continue;

                if (currentSport != oldSport)
                {
                    MatchesCollection[i].IsHeaderForLiveMonitor = true;
                }
                else if (isPreLive && !isPreLiveOld)
                {
                    MatchesCollection[i].IsHeaderForLiveMonitor = true;
                }
                else
                {
                    MatchesCollection[i].IsHeaderForLiveMonitor = false;
                }
                oldSport = currentSport;
                isPreLiveOld = isPreLive;
            }
        }


        public static int Comparison(IMatchVw m1, IMatchVw m2)
        {
            var sSportSort1 = m1.DefaultSorting;
            var sSportSort2 = m2.DefaultSorting;

            if ((m1.IsLiveBet && m1.LiveBetStatus != eMatchStatus.NotStarted) && (m2.IsLiveBet && m2.LiveBetStatus != eMatchStatus.NotStarted))
            {
                if ((m1.LiveBetStatus != eMatchStatus.NotStarted && m2.LiveBetStatus != eMatchStatus.NotStarted) || m1.LiveBetStatus.Equals(m2.LiveBetStatus))
                {
                    if (sSportSort1.Equals(sSportSort2))
                    {
                        if (m1.LiveMatchMinuteEx == m2.LiveMatchMinuteEx)
                        {
                            if (m1.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER && m2.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
                            {
                                if (m1.LivePeriodInfo.Equals(m2.LivePeriodInfo))
                                {
                                    if (m1.StartDate == m2.StartDate)
                                        return m1.Name.CompareTo(m2.Name);

                                    return m1.StartDate.CompareTo(m2.StartDate);
                                }
                                return m1.LivePeriodInfo.CompareTo(m2.LivePeriodInfo);
                            }
                            else
                            {
                                if (m1.LivePeriodInfo.Equals(m2.LivePeriodInfo))
                                {
                                    if (m1.StartDate == m2.StartDate)
                                        return m1.Name.CompareTo(m2.Name);

                                    return m1.StartDate.CompareTo(m2.StartDate);
                                }
                                return m2.LivePeriodInfo.CompareTo(m1.LivePeriodInfo);
                            }
                        }
                        return m2.LiveMatchMinuteEx.CompareTo(m1.LiveMatchMinuteEx);
                    }
                    return sSportSort1.CompareTo(sSportSort2);

                }
            }
            if ((!m1.IsLiveBet || m1.LiveBetStatus == eMatchStatus.NotStarted) && (!m2.IsLiveBet || m2.LiveBetStatus == eMatchStatus.NotStarted))
            {
                if (sSportSort1.Equals(sSportSort2))
                {
                    if (m1.StartDate == m2.StartDate)
                        return m1.Name.CompareTo(m2.Name);

                    return m1.StartDate.CompareTo(m2.StartDate);
                }

                return sSportSort1.CompareTo(sSportSort2);
            }
            if (!m1.IsLiveBet || m1.LiveBetStatus == eMatchStatus.NotStarted)
                return 1;
            if (!m2.IsLiveBet || m2.LiveBetStatus == eMatchStatus.NotStarted)
                return -1;
            var dd = m1.LiveBetStatus.CompareTo(m2.LiveBetStatus);
            return dd;

        }

        private bool MatchFilter(IMatchLn match)
        {
            if (!match.Active.Value)
                return false;

            if (match.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                return false;

            if (match.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                return false;

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            var liveMatch = LineSr.Instance.AllObjects.Matches.GetByBtrMatchId(match.BtrMatchId, true);

            if (!match.IsLiveBet.Value && match.MatchView.StartDate < DateTime.Now)
            {
                return false;
            }

            if (!match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime > DateTime.Now.AddHours(24))
            {
                return false;
            }

            if (!match.IsLiveBet.Value && !StationRepository.AllowFutureMatches)
            {
                return false;
            }

            if (liveMatch == null)
            {
                return false;
            }
            if (!match.IsLiveBet.Value && liveMatch.MatchView.LiveBetStatus != eMatchStatus.NotStarted)
            {
                return false;
            }

            if (!match.IsLiveBet.Value && MatchFilter(liveMatch))
            {
                return false;
            }


            return true;
        }

        #endregion

        #region Properties

        public static string SelectedLanguage
        {
            get { return TranslationProvider.CurrentLanguage; }
            set { TranslationProvider.CurrentLanguage = value; }
        }

        public static IRepository Repository
        {
            get { return IoCContainer.Kernel.Get<IRepository>(); }
        }

        public IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        public static ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }


        public IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }

        #endregion

        public bool IsClosed { get; set; }
    }

    public class Content
    {
        public string additParams { get; set; }
        public string data { get; set; }
        public string contentType { get; set; }
        public string contentId { get; set; }
        public string size { get; set; }
    }

    public class ScheduledTemplate
    {
        public string totalStart { get; set; }
        public string templateId { get; set; }
        public string dailyStartTime { get; set; }
        public string monitorPosition { get; set; }
        public string schedulerType { get; set; }
        public string totalEnd { get; set; }
        public string schedulerSetting { get; set; }
        public string dailyEndTime { get; set; }
    }

    public class MonitorTemplate
    {
        public string templateId { get; set; }
        public string monitorPosition { get; set; }
    }

    public class AvailableTemplate
    {
        public Content[] content { get; set; }

        public string templateId { get; set; }
        public string orientationType { get; set; }
        public string name { get; set; }
    }

    public class MonitorTemplates
    {
        public AvailableTemplate[] availableTemplates { get; set; }
        public ScheduledTemplate[] scheduledTemplates { get; set; }
        public MonitorTemplate[] monitorTemplates { get; set; }

        public string serverGmtHoursOffset { get; set; }
    }
}
