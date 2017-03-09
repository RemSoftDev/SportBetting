using System;
using System.Linq;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using SportBetting.WPF.Prism.Shared.Models;
using System.Collections.Generic;
using TranslationByMarkupExtension;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using System.Windows;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// TEMP Only used for testing
    ///  Copyed from Results view model.
    /// </summary>
    [ServiceAspect]
    public class MatchResultsViewModel : BaseViewModel
    {

        #region Constructors
        DispatcherTimer timer;
        private SortableObservableCollection<MatchResultVw> _results = new SortableObservableCollection<MatchResultVw>();

        private readonly ScrollViewerModule _ScrollViewerModule;

        private SortableObservableCollection<SportBarItem> _sportsBarItems = new SortableObservableCollection<SportBarItem>();
        public SortableObservableCollection<SportBarItem> SportsBarItems
        {
            get
            {
                return _sportsBarItems;
            }
            set
            {
                _sportsBarItems = value;
                OnPropertyChanged("SportsBarItems");
            }
        }

        public MatchResultsViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            ResultsScrollChangedCommand = new Command(ResultsScrollChanged);
            Mediator.Register<long>(this, LoadResults, MsgTag.RefreshResults);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);
            //LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
            timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = new TimeSpan(0, 0, 1);

            CheckedBox = new Command<SportBarItem>(OnCheckedExecute);
            ScrollLeftStart = new Command(OnScrollLeftStart);
            ScrollRightStart = new Command(OnScrollRightStart);
        }

        private void CheckSportBarButtons()
        {
            OnPropertyChanged("CanScrollLeft");
            OnPropertyChanged("CanScrollRight");
            OnPropertyChanged("CanScrollUp");
            OnPropertyChanged("CanScrollDown");
        }

        #endregion

        #region Commands

        public Command<double> ScrollChangedCommand { get; private set; }
        public Command<SportBarItem> CheckedBox { get; private set; }
        public Command ScrollLeftStart { get; private set; }
        public Command ScrollRightStart { get; private set; }
        public Command ResultsScrollChangedCommand { get; private set; }

        #endregion

        #region Properties

        private System.Windows.Controls.ScrollViewer scrollViewer = null;

        public Visibility SportsBarVisibility { get { return SportsBarItems.Count > 2 ? Visibility.Visible : Visibility.Collapsed; } }

        private SyncHashSet<long> ids = new SyncHashSet<long>();

        public SortableObservableCollection<MatchResultVw> Results
        {
            get { return _results; }
            set { _results = value; }
        }

        public bool CanScrollLeft
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewer != null && scrollViewer.ContentHorizontalOffset == 0)
                    res = false;
                else if (scrollViewer == null)
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

                if (scrollViewer != null && scrollViewer.ContentHorizontalOffset + scrollViewer.ViewportWidth >= scrollViewer.ExtentWidth)
                    res = false;
                else if (scrollViewer == null)
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

                if (scrollViewer != null && scrollViewer.ContentVerticalOffset == 0)
                    res = false;
                else if (scrollViewer == null)
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

                if (scrollViewer != null && scrollViewer.ContentVerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
                    res = false;
                else if (scrollViewer == null)
                    res = false;

                return res;
            }
        }

        #endregion

        #region Methods

        private System.Windows.Controls.ScrollViewer GetSportsBarScrollviewer()
        {
            if (scrollViewer == null)
                scrollViewer = this.GetScrollviewerForActiveWindowByName("SportsBarScrollResults");

            return scrollViewer;
        }

        private void ResultsScrollChanged()
        {
            CheckSportBarButtons();
        }

        private void OnScrollLeftStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewer == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollUpStartExecute(scrollViewer, true);
            }
            else
                this._ScrollViewerModule.OnScrollLeftStartExecute(scrollViewer, true);
        }

        private void OnScrollRightStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewer == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollDownStartExecute(scrollViewer, true);
            }
            else
                this._ScrollViewerModule.OnScrollRightStartExecute(scrollViewer, true);
        }

        public void OnCheckedExecute(SportBarItem barItem)
        {
            if (barItem == null)
                return;

            CheckedExecute(barItem);
        }

        private void CheckedExecute(SportBarItem barItem)
        {
            if (barItem == null)
                return;
            if (barItem.SportDescriptor == SportSr.ALL_SPORTS && ChangeTracker.SelectedDescriptors.Count == 1 && ChangeTracker.SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
            {
                SportBarItem allsports = SportsBarItems.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                }

                return;
            }
            else if (ChangeTracker.SelectedDescriptors.Contains(barItem.SportDescriptor))
                ChangeTracker.SelectedDescriptors.Remove(barItem.SportDescriptor);
            else
            {
                if (barItem.SportDescriptor == SportSr.ALL_SPORTS)
                {
                    for (int i = 1; i < SportsBarItems.Count; i++)
                        SportsBarItems[i].IsChecked = false;

                    ChangeTracker.SelectedDescriptors.Clear();
                }
                else //all sports should be unchecked automatically
                {
                    if (ChangeTracker.SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
                    {
                        SportsBarItems[0].IsChecked = false;
                        ChangeTracker.SelectedDescriptors.Remove(SportSr.ALL_SPORTS);
                    }
                }

                ChangeTracker.SelectedDescriptors.Add(barItem.SportDescriptor);
            }

            if (ChangeTracker.SelectedDescriptors.Count == 0)
            {
                SportBarItem allsports = SportsBarItems.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                    ChangeTracker.SelectedDescriptors.Add(allsports.SportDescriptor);
                }
            }

            FillResults();

            ScrollToVertivalOffset(0);
        }

        void LineSr_DataSqlUpdateSucceeded(SportRadar.DAL.CommonObjects.eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.PreMatches)
            {
                Refresh(true);
            }
        }

        private void Refresh(bool state)
        {
            if (SportsBarItems.Count > 0)
            {
                SportsBarItems.ElementAt(0).SportName = TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string;
            }

            Results.Clear();
            FillResults();
            FillSportsBar();
        }

        void LoadResults(long obj)
        {

            Results.Clear();
            FillSportsBar();
            FillResults();

            ScrollToVertivalOffset(0);
        }

        private bool ResultMatchFilter(MatchResultLn match)
        {
            if (match.Score.Value.Contains("-1:-1"))
                return false;
            if (match.MatchLn != null)
                return false;
            if (match.CategoryGroupId.Value == null)
                return false;
            if (match.CategoryGroupId.Value == 0)
                return false;

            if (SportsBarItems.Count > 1 && !SportsBarItems.ElementAt(0).IsChecked)
            {
                if (!ChangeTracker.SelectedDescriptors.Contains(match.MatchResultView.SportView.LineObject.GroupSport.SportDescriptor))
                    return false;
            }

            if (match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime < DateTime.Now.AddDays(-ChangeTracker.ResultsSelectedDay - 1).Date)
                return false;

            var start = DateTime.Now.AddDays(-ChangeTracker.ResultsSelectedDay).Date;
            var end = DateTime.Now.AddDays(-ChangeTracker.ResultsSelectedDay).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (match.StartDate.Value.LocalDateTime > end)
                return false;
            if (match.StartDate.Value.LocalDateTime < start)
                return false;

            if (ids.Contains(match.BtrMatchId))
                return false;

            ids.Add(match.BtrMatchId);

            return true;
        }

        private bool SportBarMatchFilter(MatchResultLn match)
        {
            if (match.Score.Value.Contains("-1:-1"))
                return false;
            if (match.MatchLn != null)
                return false;
            if (match.CategoryGroupId.Value == null)
                return false;
            if (match.CategoryGroupId.Value == 0)
                return false;

            if (match.IsLiveBet.Value && match.StartDate.Value.LocalDateTime < DateTime.Now.AddDays(-ChangeTracker.ResultsSelectedDay - 1).Date)
                return false;

            var start = DateTime.Now.AddDays(-ChangeTracker.ResultsSelectedDay).Date;
            var end = DateTime.Now.AddDays(-ChangeTracker.ResultsSelectedDay).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (match.StartDate.Value.LocalDateTime > end)
                return false;
            if (match.StartDate.Value.LocalDateTime < start)
                return false;

            return true;
        }

        private void FillResults()
        {

            ids.Clear();
            Repository.FindResults(Results, ResultMatchFilter, Comparison);

            string oldSport = "";
            for (int i = 0; i < Results.Count; i++)
            {
                string currentSport = Results.ElementAt(i).SportView.DisplayName;

                if (string.IsNullOrEmpty(currentSport))
                    continue;

                if (currentSport != oldSport)
                {
                    Results.ElementAt(i).IsHeader = true;
                    oldSport = currentSport;
                }
                else
                    Results.ElementAt(i).IsHeader = false;
            }
        }

        public void FillSportsBar()
        {

            SortableObservableCollection<MatchResultVw> ResultMatches = new SortableObservableCollection<MatchResultVw>();
            Repository.FindResults(ResultMatches, SportBarMatchFilter, Comparison);
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var sports = ResultMatches.Where(x => x.SportView != null).Select(x => x.SportView).Distinct().ToList();

                    SportBarItem allsports = SportsBarItems.FirstOrDefault(x => x.SportDescriptor == SportSr.ALL_SPORTS);
                    if (allsports != null)
                        allsports.SportName = TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string;

                    foreach (var group in sports)
                    {
                        if (SportsBarItems.Count(x => x.SportDescriptor == group.LineObject.GroupSport.SportDescriptor) == 0)
                        {
                            SportsBarItems.Add(new SportBarItem(group.DisplayName, group.LineObject.GroupSport.SportDescriptor));
                        }
                        else
                        {
                            SportsBarItems.First(x => x.SportDescriptor == @group.LineObject.GroupSport.SportDescriptor).SportName = @group.DisplayName;
                        }
                    }

                    for (int i = 1; i < SportsBarItems.Count; )
                    {
                        var item = SportsBarItems[i];

                        if (sports.Count(x => x.LineObject.GroupSport.SportDescriptor == item.SportDescriptor) == 0)
                        {
                            SportsBarItems.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    SportsBarItems.Sort(ComparisonSportsBar);

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

        private int Comparison(MatchResultVw m1, MatchResultVw m2)
        {
            string sSportName1 = m1.SportView.LineObject.GetDisplayName(SelectedLanguage);
            string sSportName2 = m2.SportView.LineObject.GetDisplayName(SelectedLanguage);

            if (sSportName1.Equals(sSportName2))
            {
                if (m1.LineObject.MatchLn != null)
                {
                    if (m1.LiveMatchMinute == m2.LiveMatchMinute)
                    {
                        if (m1.StartDate == m2.StartDate)
                        {
                            if (m1.Name == m2.Name)
                            {
                                return m2.LineObject.MatchId.CompareTo(m1.LineObject.MatchId);
                            }
                            return m2.Name.CompareTo(m1.Name);
                        }

                        return m2.StartDate.CompareTo(m1.StartDate);
                    }
                }
                else
                {
                    if (m1.StartDate == m2.StartDate)
                    {
                        if (m1.Name == m2.Name)
                        {
                            return m2.LineObject.MatchId.CompareTo(m1.LineObject.MatchId);
                        }
                        return m2.Name.CompareTo(m1.Name);
                    }

                    return m2.StartDate.CompareTo(m1.StartDate);
                }
                return m2.LiveMatchMinute.CompareTo(m1.LiveMatchMinute);


            }

            return sSportName1.CompareTo(sSportName2);
        }

        void timer_Elapsed(object sender, EventArgs eventArgs)
        {
            FillResults();
        }

        private void ResetSportBar(bool res)
        {
            SportsBarItems.Clear();
            ChangeTracker.SelectedDescriptors.Clear();
            SportsBarItems.Add(new SportBarItem(TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string, SportSr.ALL_SPORTS));
            SportsBarItems.ElementAt(0).IsChecked = true;
            ChangeTracker.SelectedDescriptors.Add(SportsBarItems.ElementAt(0).SportDescriptor);

            GetSportsBarScrollviewer();

            if (scrollViewer == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                scrollViewer.ScrollToVerticalOffset(0);
            }
            else
                scrollViewer.ScrollToHorizontalOffset(0);
        }

        public override void OnNavigationCompleted()
        {
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            Mediator.SendMessage<bool>(true, MsgTag.ShowResultFilters);
            Mediator.Register<bool>(this, ResetSportBar, MsgTag.ClearSelectedSports);

            Results.Clear();
            SportsBarItems.Clear();
            FillResults();

            timer.Tick += timer_Elapsed;
            timer.Start();
            ChangeTracker.SelectedResults = true;

            SportsBarItems.Add(new SportBarItem(TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string, SportSr.ALL_SPORTS));
            FillSportsBar();
            if (ChangeTracker.SelectedDescriptors.Count == 0)
            {
                ChangeTracker.SelectedDescriptors.Add(SportsBarItems.ElementAt(0).SportDescriptor);
                SportsBarItems.ElementAt(0).IsChecked = true;
            }
            else
            {
                var arraycopy = ChangeTracker.SelectedDescriptors.ToArray();
                ChangeTracker.SelectedDescriptors.Clear();
                foreach (var selectedDescriptor in arraycopy)
                {
                    var sport = SportsBarItems.FirstOrDefault(x => x.SportDescriptor == selectedDescriptor);
                    sport.IsChecked = true;
                    CheckedExecute(sport);
                }
            }

            base.OnNavigationCompleted();
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);
            CheckSportBarButtons();
        }
        public override void Close()
        {
            timer.Stop();
            timer.Tick -= timer_Elapsed;
            Mediator.SendMessage<bool>(false, MsgTag.ShowResultFilters);
            base.Close();
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.ResultsVisible)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}