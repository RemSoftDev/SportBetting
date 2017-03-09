using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using Newtonsoft.Json;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System;
using SportRadar.DAL.OldLineObjects;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportBetting.WPF.Prism.Shared.Models;
using System.Linq;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class LiveViewModel : BaseViewModel
    {

        //protected new static readonly log4net.ILog Log = LogManager.GetLogger(typeof(LiveViewModel));
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        public static IDictionary<long, MatchStreamData> StreamData = new Dictionary<long, MatchStreamData>();
        private bool _offsetChanged = false;

        private readonly ScrollViewerModule _ScrollViewerModule;

        private SortableObservableCollection<SportBarItem> _sportsBarItemsLive = new SortableObservableCollection<SportBarItem>();
        public SortableObservableCollection<SportBarItem> SportsBarItemsLive
        {
            get
            {
                return _sportsBarItemsLive;
            }
            set
            {
                _sportsBarItemsLive = value;
            }
        }

        #region Constructors

        public LiveViewModel()
        {
            object locker2 = new object();
            object locker3 = new object();
            //BindingOperations.EnableCollectionSynchronization(_matches, locker2);
            //BindingOperations.EnableCollectionSynchronization(_sportsBarItemsLive, locker3);
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            ListCreated = new Command<UIElement>(OnGridCreated);
            OpenMatch = new Command<IMatchVw>(OnChoiceExecute);
            PlaceBet = new Command<IOddVw>(OnBet);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            OnCameraClickedCommand = new Command<IMatchVw>(OnCameraClicked);
            LiveScrollChangedCommand = new Command(LiveScrollChanged);
            var timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            ScrollLeftStart = new Command(OnScrollLeftStart);
            ScrollRightStart = new Command(OnScrollRightStart);
            CheckedBox = new Command<SportBarItem>(OnCheckedExecute);

            Mediator.Register<bool>(this, ClearSelectedSports, MsgTag.ClearSelectedSports);
        }

        object locker = new object();
        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Update();
            lock (locker)
            {
                UpdateSorting();
            }
        }

        private void OnGridCreated(UIElement obj)
        {
            if (GridWidth <= obj.RenderSize.Width)
                GridWidth = obj.RenderSize.Width;
        }

        #endregion

        #region Properties

        public bool CanScrollLeft
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerLive != null && scrollViewerLive.ContentHorizontalOffset == 0)
                    res = false;
                else if (scrollViewerLive == null)
                    res = false;

                return res;
            }
        }

        public bool CanScrollRight
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerLive != null && scrollViewerLive.ContentHorizontalOffset + scrollViewerLive.ViewportWidth >= scrollViewerLive.ExtentWidth)
                    res = false;
                else if (scrollViewerLive == null)
                    res = false;

                return res;
            }
        }

        public bool CanScrollUp
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerLive != null && scrollViewerLive.ContentVerticalOffset == 0)
                    res = false;
                else if (scrollViewerLive == null)
                    res = false;

                return res;
            }
        }

        public bool CanScrollDown
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerLive != null && scrollViewerLive.ContentVerticalOffset + scrollViewerLive.ViewportHeight >= scrollViewerLive.ExtentHeight)
                    res = false;
                else if (scrollViewerLive == null)
                    res = false;

                return res;
            }
        }

        private System.Windows.Controls.ScrollViewer scrollViewerLive = null;

        public Visibility SportsBarVisibility { get { return SportsBarItemsLive.Count > 2 ? Visibility.Visible : Visibility.Collapsed; } }

        public SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set
            {
                _matches = value;
                OnPropertyChanged();
            }
        }

        public double GridWidth
        {
            get { return _gridWidth; }
            set
            {
                _gridWidth = value;
                OnPropertyChanged();
            }
        }


        private IMatchVw SelectedMatch
        {
            set { ChangeTracker.CurrentMatch = value; }
        }

        #endregion

        #region Commands

        public Command ScrollLeftStart { get; private set; }
        public Command ScrollRightStart { get; private set; }
        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command<IMatchVw> OnCameraClickedCommand { get; private set; }
        public Command<UIElement> ListCreated { get; set; }
        public Command<SportBarItem> CheckedBox { get; private set; }
        public Command LiveScrollChangedCommand { get; private set; }

        #endregion

        #region Methods

        private void LiveScrollChanged()
        {
            CheckSportBarButtons();
        }

        private void CheckSportBarButtons()
        {
            OnPropertyChanged("CanScrollLeft");
            OnPropertyChanged("CanScrollRight");
            OnPropertyChanged("CanScrollUp");
            OnPropertyChanged("CanScrollDown");
        }

        private System.Windows.Controls.ScrollViewer GetSportsBarScrollviewer()
        {
            if (scrollViewerLive == null)
                scrollViewerLive = this.GetScrollviewerForActiveWindowByName("SportsBarScrollLive");

            return scrollViewerLive;
        }

        public void FillSportsBar()
        {

            SortableObservableCollection<IMatchVw> LiveMatches = new SortableObservableCollection<IMatchVw>();
            Repository.FindMatches(LiveMatches, "", SelectedLanguage, MatchFilterSportBar, delegate(IMatchVw m1, IMatchVw m2) { return 0; });
            Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var sports =
                            LiveMatches.Where(x => x.SportView != null).Select(x => x.SportView).Distinct().ToList();

                        SportBarItem allsports =
                            SportsBarItemsLive.FirstOrDefault(x => x.SportDescriptor == SportSr.ALL_SPORTS);
                        if (allsports != null)
                            allsports.SportName = TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string;

                        foreach (var group in sports)
                        {
                            {
                                if (
                                    SportsBarItemsLive.Count(
                                        x => x.SportDescriptor == group.LineObject.GroupSport.SportDescriptor) == 0)
                                {


                                    SportsBarItemsLive.Add(new SportBarItem(group.DisplayName,
                                                                            group.LineObject.GroupSport.SportDescriptor));
                                }
                                else
                                {
                                    SportsBarItemsLive.First(
                                        x => x.SportDescriptor == @group.LineObject.GroupSport.SportDescriptor)
                                                      .SportName = @group.DisplayName;
                                }
                            }
                        }

                        for (int i = 1; i < SportsBarItemsLive.Count; )
                        {
                            var item = SportsBarItemsLive[i];

                            if (sports.Count(x => x.LineObject.GroupSport.SportDescriptor == item.SportDescriptor) == 0)
                            {

                                SportsBarItemsLive.RemoveAt(i);
                            }
                            else
                            {
                                i++;
                            }
                        }

                        Dispatcher.Invoke(() =>
                            {
                                SportsBarItemsLive.Sort(ComparisonSportsBar);
                            });

                        OnPropertyChanged("SportsBarVisibility");
                    }
                    catch (Exception ex)
                    {
                    }
                });
        }


        public int ComparisonSportsBar(SportBarItem m1, SportBarItem m2)
        {
            return m1.SortingOrder.CompareTo(m2.SortingOrder);
        }

        public void OnCheckedExecute(SportBarItem barItem)
        {
            if (barItem == null)
                return;

            CheckedExecute(barItem);
        }

        private void CheckedExecute(SportBarItem barItem)
        {
            if (barItem.SportDescriptor == SportSr.ALL_SPORTS && SelectedDescriptors.Count == 1 && SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
            {
                SportBarItem allsports = SportsBarItemsLive.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                }

                return;
            }
            else if (SelectedDescriptors.Contains(barItem.SportDescriptor))
                SelectedDescriptors.Remove(barItem.SportDescriptor);
            else
            {
                if (barItem.SportDescriptor == SportSr.ALL_SPORTS)
                {
                    for (int i = 1; i < SportsBarItemsLive.Count; i++)
                        SportsBarItemsLive[i].IsChecked = false;

                    SelectedDescriptors.Clear();
                    ChangeTracker.LiveSelectedAllSports = true;
                }
                else //all sports should be unchecked automatically
                {
                    if (SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
                    {
                        SportsBarItemsLive[0].IsChecked = false;
                        SelectedDescriptors.Remove(SportSr.ALL_SPORTS);
                    }
                    ChangeTracker.LiveSelectedAllSports = false;
                }

                SelectedDescriptors.Add(barItem.SportDescriptor);
            }

            if (SelectedDescriptors.Count == 0)
            {
                SportBarItem allsports = SportsBarItemsLive.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                    SelectedDescriptors.Add(allsports.SportDescriptor);
                }
            }

            Refresh(true);

            ScrollToVertivalOffset(0);
        }

        private void OnScrollLeftStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewerLive == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollUpStartExecute(scrollViewerLive, true);
            }
            else
                this._ScrollViewerModule.OnScrollLeftStartExecute(scrollViewerLive, true);
        }

        private void OnScrollRightStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewerLive == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollDownStartExecute(scrollViewerLive, true);
            }
            else
                this._ScrollViewerModule.OnScrollRightStartExecute(scrollViewerLive, true);
        }

        private void OnCameraClicked(IMatchVw match)
        {
            if (!match.StreamStarted)
                return;

            Mediator.SendMessage(match, MsgTag.ShowStream);
        }

        private void ChangeOffset(bool changeOffset)
        {
            _offsetChanged = changeOffset;
        }

        private bool disabledLive = false;
        private void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (!StationRepository.IsLiveMatchEnabled)
            {
                LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
                disabledLive = true;
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
            }
            if (eut == eUpdateType.LiveBet)
                Update();
        }

        public List<string> SelectedDescriptors { get { return ChangeTracker.SelectedDescriptorsLive; } }

        public override void OnNavigationCompleted()
        {
            ChangeTracker.SelectedLive = true;
            ChangeTracker.LiveSelectedAllSports = true;

            if (disabledLive)
            {
                LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);
                disabledLive = false;
            }

            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            Mediator.Register<bool>(this, ChangeOffset, MsgTag.OffsetChanged);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);
            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LiveLanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<bool>(this, ClearSelectedSports, MsgTag.ClearSelectedSports);

            base.OnNavigationCompleted();
            UpdateSorting();

            Dispatcher.Invoke((Action)(() =>
                {
                    var window = (Window)GetActiveWindow();
                    if (window != null)
                        window.Focus();
                }));
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);

            Update();
            //ClearSelectedSports(true);

            //FillSportsBar();

            //CheckSportBarButtons();
        }


        private volatile string matchescsv = "";
        private double _gridWidth;
        private DateTime currentTime = DateTime.Now;
        public void Update()
        {
            {
                if (_offsetChanged)
                {
                    Dispatcher.Invoke(() =>
                        {
                            Matches.Clear();
                        });
                    _offsetChanged = false;
                }
                currentTime = DateTime.Now;

                Dispatcher.Invoke(() =>
                    {
                        Matches = Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, Comparison);
                    });
                matchescsv = "";
                if (Matches != null)
                {
                    foreach (var matchVw in Matches)
                    {
                        matchescsv += matchVw.LineObject.MatchId + ";";
                        matchescsv += "-" + matchVw.LineObject.MatchId + ";";
                    }



                    if (ChangeTracker.LiveSelectedMode != 3)
                    {
                        UpdateHeaders(Matches);
                    }
                    else
                    {
                        foreach (var matchVw in Matches)
                        {
                            matchVw.IsHeader = false;
                        }
                    }
                    if (Matches != null && StationRepository.EnableLiveStreaming && StreamData != null)
                        foreach (var matchVw in Matches)
                        {
                            if (StreamData.ContainsKey(matchVw.LineObject.BtrMatchId))
                            {
                                matchVw.HaveStream = true;
                                matchVw.StreamStarted = DateTime.Now >
                                                        StreamData[matchVw.LineObject.BtrMatchId].StartDate &&
                                                        (matchVw.LiveBetStatus == eMatchStatus.Started ||
                                                         matchVw.LiveBetStatus == eMatchStatus.Stopped);
                                matchVw.StreamID = StreamData[matchVw.LineObject.BtrMatchId].StreamID;
                            }
                            else
                            {
                                matchVw.HaveStream = false;
                                matchVw.StreamStarted = false;
                                matchVw.StreamID = 0;
                            }
                        }
                }

                FillSportsBar();
            }
        }

        private static bool sortingIsWorking = false;

        private void UpdateSorting()
        {
            UpdateSortingAsync();
        }

        private void UpdateSortingAsync()
        {
            if (ChangeTracker.LiveSelectedMode == 4)
            {
                try
                {
                    if (string.IsNullOrEmpty(matchescsv))
                        return;
                    var jsonSorting = MatchsortingJson(matchescsv);
                    Log.Debug(jsonSorting);
                    if (string.IsNullOrEmpty(jsonSorting))
                        return;

                    var deserializedFeed = JsonConvert.DeserializeObject<MatshSorting>(jsonSorting);
                    foreach (var matchData in deserializedFeed.matchData)
                    {
                        var match = Repository.GetByMatchId(matchData.id);
                        if (match != null)
                        {
                            match.Sort = matchData.sort;

                            if (deserializedFeed.tournaments.ContainsKey(matchData.tournamentId))
                            {
                                var sportId = deserializedFeed.tournaments[matchData.tournamentId].sportId;

                                if (deserializedFeed.sports.ContainsKey(sportId))
                                {
                                    match.MatchView.SportView.Sort = deserializedFeed.sports[sportId];
                                }
                            }
                            var tournamentId = matchData.tournamentId;
                            if (deserializedFeed.tournaments.ContainsKey(tournamentId))
                            {
                                match.MatchView.TournamentView.Sort = deserializedFeed.tournaments[tournamentId].sort;
                            }
                        }
                    }
                    if (deserializedFeed.matchData.Length > 0)
                        sortingIsWorking = true;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                }
            }
        }

        [WsdlServiceSyncAspectSilent]
        private string MatchsortingJson(string csv)
        {
            var jsonSorting = WsdlRepository.Matchsorting(csv);
            return jsonSorting;
        }

        public static void UpdateHeaders<T>(SortableObservableCollection<T> matchesCollection) where T : IMatchVw
        {
            string oldSport = "";
            bool isPreLiveOld = false;
            if (matchesCollection == null)
                return;
            foreach (T match in matchesCollection)
            {
                string currentSport = match.SportDescriptor;
                var isPreLive = match.LiveBetStatus == eMatchStatus.NotStarted || !match.IsLiveBet;
                if (string.IsNullOrEmpty(currentSport))
                    continue;

                if (currentSport != oldSport)
                {
                    match.IsHeader = true;
                }
                else if (isPreLive && !isPreLiveOld)
                {
                    match.IsHeader = true;
                }
                else
                {
                    match.IsHeader = false;
                }
                oldSport = currentSport;
                isPreLiveOld = isPreLive;
            }
        }

        private void ClearSelectedSports(bool res)
        {
            Dispatcher.Invoke(() =>
                {
                    SportsBarItemsLive.Clear();
                });
            SelectedDescriptors.Clear();
            Dispatcher.Invoke(() =>
                {
                    SportsBarItemsLive.Add(
                        new SportBarItem(
                            TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as
                            string, SportSr.ALL_SPORTS));
                });
            SportsBarItemsLive.ElementAt(0).IsChecked = true;
            ChangeTracker.LiveSelectedAllSports = true;
            SelectedDescriptors.Add(SportsBarItemsLive.ElementAt(0).SportDescriptor);

            GetSportsBarScrollviewer();

            if (scrollViewerLive == null)
                return;

            Dispatcher.BeginInvoke(() =>
            {
                if (ChangeTracker.IsLandscapeMode)
                {
                    scrollViewerLive.ScrollToVerticalOffset(0);
                }
                else
                    scrollViewerLive.ScrollToHorizontalOffset(0);

            });
        }

        // [PleaseWaitAspect]
        private void Refresh(bool obj)
        {
            Update();
        }

        private void OnLanguageChosenExecute(string lang)
        {
            Update();
        }
        public bool MatchFilter(IMatchLn match)
        {
            if (match.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                return false;
            if (match.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                return false;

            if (SportsBarItemsLive.Count > 1 && !SportsBarItemsLive.ElementAt(0).IsChecked)
            {
                if (!SelectedDescriptors.Contains(match.MatchView.SportView.LineObject.GroupSport.SportDescriptor))
                    return false;
            }

            if (!match.Active.Value)
                return false;

            if (!match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime < currentTime)
            {
                return false;
            }

            if (!match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime > currentTime.AddHours(24))
            {
                return false;
            }


            if (!match.IsLiveBet.Value && !StationRepository.AllowFutureMatches)
            {
                return false;
            }
            if (!match.IsLiveBet.Value && ChangeTracker.LiveSelectedMode == 4)
            {
                return false;
            }
            if (match.IsLiveBet.Value && ChangeTracker.LiveSelectedMode == 4 && match.MatchView.LiveBetStatus == eMatchStatus.NotStarted)
            {
                return false;
            }

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;



            var liveMatch = Repository.GetByBtrMatchId(match.BtrMatchId, true);

            if (liveMatch == null)
            {
                return false;
            }



            if (!match.IsLiveBet.Value && MatchFilter(liveMatch))
            {
                return false;
            }
            if (!match.IsLiveBet.Value && liveMatch.MatchView.LiveBetStatus != eMatchStatus.NotStarted)
            {
                return false;
            }

            return true;
        }

        public bool MatchFilterSportBar(IMatchLn match)
        {
            if (match.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                return false;
            if (match.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                return false;

            if (!match.Active.Value)
                return false;

            if (!match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime < currentTime)
            {
                return false;
            }

            if (!match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime > currentTime.AddHours(24))
            {
                return false;
            }


            if (!match.IsLiveBet.Value && !StationRepository.AllowFutureMatches)
            {
                return false;
            }
            if (!match.IsLiveBet.Value && ChangeTracker.LiveSelectedMode == 4)
            {
                return false;
            }
            if (match.IsLiveBet.Value && ChangeTracker.LiveSelectedMode == 4 && match.MatchView.LiveBetStatus == eMatchStatus.NotStarted)
            {
                return false;
            }

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            var liveMatch = Repository.GetByBtrMatchId(match.BtrMatchId, true);

            if (liveMatch == null)
            {
                return false;
            }

            if (!match.IsLiveBet.Value && MatchFilter(liveMatch))
            {
                return false;
            }
            if (!match.IsLiveBet.Value && liveMatch.MatchView.LiveBetStatus != eMatchStatus.NotStarted)
            {
                return false;
            }

            return true;
        }

        public int Comparison(IMatchVw m1, IMatchVw m2)
        {
            if (ChangeTracker.LiveSelectedMode == 3)
            {
                if (m1.StartDate == m2.StartDate)
                    return m1.Name.CompareTo(m2.Name);

                return m1.StartDate.CompareTo(m2.StartDate);
            }
            if (ChangeTracker.LiveSelectedMode == 4 && sortingIsWorking)
            {
                if (m1.SportView.Sort == m2.SportView.Sort)
                {
                    if (m1.TournamentView.Sort == m2.TournamentView.Sort)
                    {
                        if (m1.LineObject.Sort == m2.LineObject.Sort)
                        {
                            if (m1.LineObject.StartDate.Value.UtcDateTime.Equals(m2.LineObject.StartDate.Value.UtcDateTime))
                            {
                                var id1 = -m1.LineObject.MatchId;
                                var id2 = -m2.LineObject.MatchId;
                                var value = id1.CompareTo(id2);
                                return value;
                            }
                            return m1.LineObject.StartDate.Value.UtcDateTime.CompareTo(m2.LineObject.StartDate.Value.UtcDateTime);
                        }
                        return m1.LineObject.Sort.CompareTo(m2.LineObject.Sort);
                    }
                    return m1.TournamentView.Sort.CompareTo(m2.TournamentView.Sort);

                }

                return m1.SportView.Sort.CompareTo(m2.SportView.Sort);
            }

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

        private void OnChoiceExecute(IMatchVw chosenEntity)
        {
            SelectedMatch = chosenEntity;


            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);
        }


        public override void Close()
        {
            //ChangeTracker.TimeFilters[1].Visibility = Visibility.Visible;
            //ChangeTracker.TimeFilters[2].Visibility = Visibility.Visible;
            //ChangeTracker.TimeFilters[3].Visibility = Visibility.Collapsed;
            //ChangeTracker.TimeFilters[4].Visibility = Visibility.Collapsed;
            //LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
            ChangeTracker.LiveSelectedAllSports = false;
            Mediator.SendMessage(true, MsgTag.HideStream);
            Mediator.UnregisterRecipientAndIgnoreTags(this);
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.IsLiveMatchEnabled)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion

        public IDictionary<long, MatchStreamData> ParseXmlFeed()
        {
            var jsonFeed = LiveStreamService.GetLiveStreamFeed();


            var returnValues = new Dictionary<long, MatchStreamData>();

            if (jsonFeed == null)
                return returnValues;

            var deserializedFeed = JsonConvert.DeserializeObject<MatchStreamInfo>(jsonFeed);

            foreach (var doc in deserializedFeed.doc)
            {
                foreach (var data in doc.data)
                {
                    var streamData = new MatchStreamData();
                    streamData.BtrMatchId = data.eventid;
                    streamData.StreamID = data.stream.streamid;
                    streamData.StartDate = DateTime.Parse(data.schedule.day + " " + data.schedule.start.time + data.schedule.start.offset);
                    streamData.EndDate = DateTime.Parse(data.schedule.day + " " + data.schedule.end.time + data.schedule.end.offset);
                    returnValues[streamData.BtrMatchId] = streamData;
                }
            }

            return returnValues;
        }
    }

    public class MatshSorting
    {
        public tournamentData[] tournamentData { get; set; }
        public sportData[] sportData { get; set; }
        public matchData[] matchData { get; set; }
        private IDictionary<long, tournamentData> _tournaments;
        public IDictionary<long, tournamentData> tournaments
        {
            get
            {
                if (_tournaments == null)
                {
                    _tournaments = new Dictionary<long, tournamentData>();
                    if (tournamentData != null)
                        foreach (var tournament in tournamentData)
                        {
                            _tournaments.Add(tournament.id, tournament);
                        }
                }
                return _tournaments;
            }
        }
        private IDictionary<long, int> _sports;
        public IDictionary<long, int> sports
        {
            get
            {
                if (_sports == null)
                {
                    _sports = new Dictionary<long, int>();
                    if (sportData != null)
                        foreach (var tournament in sportData)
                        {
                            _sports.Add(tournament.id, tournament.sort);
                        }
                }
                return _sports;
            }
        }
    }

    public class sportData
    {
        public long id { get; set; }
        public int sort { get; set; }
    }

    public class tournamentData
    {
        public long sportId { get; set; }
        public long id { get; set; }
        public int sort { get; set; }
    }

    public class matchData
    {
        public long id { get; set; }
        public int sort { get; set; }
        public long tournamentId { get; set; }

    }



    public class MatchStreamInfo
    {
        public doc[] doc { get; set; }
        public string queryUrl { get; set; }
    }

    public class MatchStreamData
    {

        public long BtrMatchId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long StreamID { get; set; }
    }
    public class doc
    {

        public IList<data> data { get; set; }
    }

    public class data
    {
        public long eventid { get; set; }
        public long betradarid { get; set; }

        public schedule schedule { get; set; }
        public stream stream { get; set; }
    }

    public class stream
    {
        public long streamid { get; set; }
        public string streamstatus { get; set; }
        public string streamstatustext { get; set; }
    }

    public class schedule
    {
        public string day { get; set; }
        public Time start { get; set; }
        public Time end { get; set; }
    }

    public class Time
    {
        public string time { get; set; }
        public string offset { get; set; }
    }
}