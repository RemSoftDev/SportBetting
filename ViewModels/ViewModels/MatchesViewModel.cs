using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.DAL.OldLineObjects;
using TranslationByMarkupExtension;
using System.Windows;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Matches view model.
    /// </summary>
    [ServiceAspect]
    public class MatchesViewModel : BaseViewModel
    {
        //private read-only static Lazy<MatchesViewModel> _Instance = new Lazy<MatchesViewModel>(() => new MatchesViewModel(), true);
        //public static MatchesViewModel Instance { get { return _Instance.Value; } }

        #region Constructors
        private static object _itemsLock = new object();
        private static object _itemsLock2 = new object();
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        private System.Windows.Controls.ScrollViewer scrollViewerPreMatch = null;

        private readonly ScrollViewerModule _ScrollViewerModule;
        public List<string> SelectedDescriptors
        {
            get { return ChangeTracker.SelectedDescriptorsPreMatch; }
        }

        private SortableObservableCollection<SportBarItem> _sportsBarItemsPreMatch = new SortableObservableCollection<SportBarItem>();
        public SortableObservableCollection<SportBarItem> SportsBarItemsPreMatch
        {
            get
            {
                return _sportsBarItemsPreMatch;
            }
            set
            {
                _sportsBarItemsPreMatch = value;
                OnPropertyChanged("SportsBarItemsPreMatch");
            }
        }

        public MatchesViewModel(params object[] args)
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);

            BindingOperations.EnableCollectionSynchronization(_matches, _itemsLock);
            BindingOperations.EnableCollectionSynchronization(_sportsBarItemsPreMatch, _itemsLock2);
            OpenMatch = new Command<IMatchVw>(OnChoiceExecute);
            OpenOutrightMatch = new Command<IMatchVw>(OnOutrightChoiceExecute);

            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);
            Mediator.Register<bool>(this, ClearSelectedSports, MsgTag.ClearSelectedSports);
            //Mediator.Register<bool>(this, ShowSelectedTournaments, MsgTag.ShowSelectedTournaments);
            PreMatchScrollChangedCommand = new Command(PreMatchScrollChanged);
            PreMatchScrollLoadedCommand = new Command<System.Windows.Controls.ScrollViewer>(PreMatchScrollLoaded);

            PlaceBet = new Command<IOddVw>(OnBet);
            ScrollChangedCommand = new Command<double>(ScrollChanged);

            ScrollLeftStart = new Command(OnScrollLeftStart);
            ScrollRightStart = new Command(OnScrollRightStart);
            CheckedBox = new Command<SportBarItem>(OnCheckedExecute);


            if (args.Length > 0)
            {
                SelectedTournaments = args[0] as HashSet<string>;
            }


            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }
            //selected tournaments handling

            LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
        }


        #endregion

        #region Properties

        public bool CanScrollLeft
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentHorizontalOffset == 0)
                    res = false;
                else if (scrollViewerPreMatch == null)
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

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentHorizontalOffset + scrollViewerPreMatch.ViewportWidth >= scrollViewerPreMatch.ExtentWidth)
                    res = false;
                else if (scrollViewerPreMatch == null)
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

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentVerticalOffset == 0)
                    res = false;
                else if (scrollViewerPreMatch == null)
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

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentVerticalOffset + scrollViewerPreMatch.ViewportHeight >= scrollViewerPreMatch.ExtentHeight)
                    res = false;
                else if (scrollViewerPreMatch == null)
                    res = false;

                return res;
            }
        }

        //protected new static readonly log4net.ILog Log = LogManager.GetLogger(typeof(MatchesViewModel));

        private IMatchVw SelectedMatch
        {
            set { ChangeTracker.CurrentMatch = value; }
        }

        public SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }

        public Visibility SportsBarVisibility { get { return SportsBarItemsPreMatch.Count > 2 ? Visibility.Visible : Visibility.Collapsed; } }

        #endregion

        #region Commands

        public Command ScrollLeftStart { get; private set; }
        public Command ScrollRightStart { get; private set; }
        public Command<SportBarItem> CheckedBox { get; private set; }
        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<IMatchVw> OpenOutrightMatch { get; private set; }
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command PreMatchScrollChangedCommand { get; private set; }
        public Command<System.Windows.Controls.ScrollViewer> PreMatchScrollLoadedCommand { get; private set; }

        #endregion

        #region Methods

        private void PreMatchScrollLoaded(System.Windows.Controls.ScrollViewer scroller)
        {
            scrollViewerPreMatch = scroller;
            CheckSportBarButtons();
        }

        private void ClearSelectedSports(bool res)
        {
            SportsBarItemsPreMatch.Clear();
            SelectedDescriptors.Clear();
            SportsBarItemsPreMatch.Add(new SportBarItem(TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string, SportSr.ALL_SPORTS));
            SportsBarItemsPreMatch.ElementAt(0).IsChecked = true;
            SelectedDescriptors.Add(SportsBarItemsPreMatch.ElementAt(0).SportDescriptor);

            GetSportsBarScrollviewer();

            if (scrollViewerPreMatch == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                scrollViewerPreMatch.ScrollToVerticalOffset(0);
            }
            else
                scrollViewerPreMatch.ScrollToHorizontalOffset(0);
        }

        private void CheckSportBarButtons()
        {
            OnPropertyChanged("CanScrollLeft");
            OnPropertyChanged("CanScrollRight");
            OnPropertyChanged("CanScrollUp");
            OnPropertyChanged("CanScrollDown");
        }

        private void PreMatchScrollChanged()
        {
            CheckSportBarButtons();
        }

        private System.Windows.Controls.ScrollViewer GetSportsBarScrollviewer()
        {
            if (scrollViewerPreMatch == null)
                scrollViewerPreMatch = this.GetScrollviewerForActiveWindowByName("SportsBarScrollPreMatch");

            return scrollViewerPreMatch;
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
                SportBarItem allsports = SportsBarItemsPreMatch.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
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
                    for (int i = 1; i < SportsBarItemsPreMatch.Count; i++)
                        SportsBarItemsPreMatch[i].IsChecked = false;

                    SelectedDescriptors.Clear();
                }
                else //all sports should be unchecked automatically
                {
                    if (SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
                    {
                        SportsBarItemsPreMatch[0].IsChecked = false;
                        SelectedDescriptors.Remove(SportSr.ALL_SPORTS);
                    }
                }

                SelectedDescriptors.Add(barItem.SportDescriptor);
            }

            if (SelectedDescriptors.Count == 0)
            {
                SportBarItem allsports = SportsBarItemsPreMatch.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                    SelectedDescriptors.Add(allsports.SportDescriptor);
                }
            }

            Refresh(true);

            ScrollToVertivalOffset(0);
        }

        public void FillSportsBar()
        {
            SortableObservableCollection<IMatchVw> PreMatches = new SortableObservableCollection<IMatchVw>();
            Repository.FindMatches(PreMatches, "", SelectedLanguage, MatchFilterSportBar, delegate(IMatchVw m1, IMatchVw m2) { return 0; });

            try
            {
                var sports = PreMatches.Where(x => x.SportView != null).Select(x => x.SportView).Distinct().ToList();

                SportBarItem allsports = SportsBarItemsPreMatch.FirstOrDefault(x => x.SportDescriptor == SportSr.ALL_SPORTS);
                if (allsports != null)
                    allsports.SportName = TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string;
                else
                    SportsBarItemsPreMatch.Insert(0, new SportBarItem(TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string, SportSr.ALL_SPORTS));

                foreach (var group in sports)
                {
                    {
                        if (SportsBarItemsPreMatch.Count(x => x.SportDescriptor == group.LineObject.GroupSport.SportDescriptor) == 0)
                        {
                            SportsBarItemsPreMatch.Add(new SportBarItem(group.DisplayName, group.LineObject.GroupSport.SportDescriptor));
                        }
                        else
                        {
                            SportsBarItemsPreMatch.First(x => x.SportDescriptor == @group.LineObject.GroupSport.SportDescriptor).SportName = @group.DisplayName;
                        }
                    }
                }

                for (int i = 1; i < SportsBarItemsPreMatch.Count; )
                {
                    var item = SportsBarItemsPreMatch[i];

                    if (sports.Count(x => x.LineObject.GroupSport.SportDescriptor == item.SportDescriptor) == 0)
                    {
                        SportsBarItemsPreMatch.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                foreach (SportBarItem item in SportsBarItemsPreMatch)
                {
                    if (SelectedDescriptors.Contains(item.SportDescriptor))
                        item.IsChecked = true;
                    else
                        item.IsChecked = false;
                }

                SportsBarItemsPreMatch.Sort(ComparisonSportsBar);
                    
                OnPropertyChanged("SportsBarVisibility");
            }
            catch (Exception ex)
            {
            }
        }

        public int ComparisonSportsBar(SportBarItem m1, SportBarItem m2)
        {
            return m1.SortingOrder.CompareTo(m2.SortingOrder);
        }

        private void OnScrollLeftStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewerPreMatch == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollUpStartExecute(scrollViewerPreMatch, true);
            }
            else
                this._ScrollViewerModule.OnScrollLeftStartExecute(scrollViewerPreMatch, true);
        }

        private void OnScrollRightStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewerPreMatch == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollDownStartExecute(scrollViewerPreMatch, true);
            }
            else
                this._ScrollViewerModule.OnScrollRightStartExecute(scrollViewerPreMatch, true);
        }

        private void LineSr_DataSqlUpdateSucceeded(eUpdateType eut, string sproviderdescription)
        {
            if (eut == eUpdateType.PreMatches)
            {
                FillMatches();
            }
        }

        public override void OnNavigationCompleted()
        {
            FillMatches();
            if (Matches.Count == 0)
            {
                MyRegionManager.NavigatBack(RegionNames.ContentRegion);
            }
            //Mediator.SendMessage<bool>(false, MsgTag.BlockSportFilter);
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            //Mediator.SendMessage<bool>(false, MsgTag.BlockTimeFilter);
            ChangeTracker.SelectedSports = true;

            base.OnNavigationCompleted();
        }


        private void OnLanguageChosenExecute(string lang)
        {
            FillMatches();
        }
        object _locker = new object();
        private HashSet<string> _selectedTournaments = new HashSet<string>();

        private void FillMatches()
        {
            lock (_locker)
            {
                FillSportsBar();
                Matches = Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, Comparison);

                long oldSportId = 0;
                for (int i = 0; i < Matches.Count; i++)
                {
                    long currentSportId = Matches.ElementAt(i).TournamentView.LineObject.GroupId;


                    if (currentSportId != oldSportId)
                    {
                        Matches.ElementAt(i).IsHeaderForPreMatch = true;
                        oldSportId = currentSportId;
                    }
                    else
                        Matches.ElementAt(i).IsHeaderForPreMatch = false;
                }
            }
        }

        private static int Comparison(IMatchVw m1, IMatchVw m2)
        {
            if (m1.TournamentView.LineObject.GroupId == m2.TournamentView.LineObject.GroupId)
            {
                if (m1.StartDate == m2.StartDate)
                {
                    if (m2.TournamentView.LineObject.GroupId == m1.TournamentView.LineObject.GroupId)
                        return m2.Name.CompareTo(m1.Name);
                    return m2.TournamentView.LineObject.GroupId.CompareTo(m1.TournamentView.LineObject.GroupId);
                }

                return m1.StartDate.CompareTo(m2.StartDate);
            }
            return m2.TournamentView.LineObject.GroupId.CompareTo(m1.TournamentView.LineObject.GroupId);
        }

        public bool MatchFilter(IMatchLn match)
        {
            if (!match.Active.Value)
                return false;

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            if (match.IsLiveBet.Value)
                return false;

            if (SportsBarItemsPreMatch.Count > 1 && !SportsBarItemsPreMatch.ElementAt(0).IsChecked)
            {
                if (!SelectedDescriptors.Contains(match.MatchView.SportView.LineObject.GroupSport.SportDescriptor))
                    return false;
            }

            if (match.MatchView.CategoryView == null)
                return false;

            string id = (match.MatchView.TournamentView.LineObject.GroupId.ToString());
            string tourId = match.outright_type == eOutrightType.Outright ? id + "*1" : id + "*0";

            if (SelectedTournaments.Count > 0 && !SelectedTournaments.Contains(tourId))
                return false;


            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;


            if (ChangeTracker.PreMatchSelectedMode == 1)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate >= DateTime.Now.AddDays(1).Date)
                    return false;
            }
            if (ChangeTracker.PreMatchSelectedMode == 2)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate > DateTime.Now.AddMinutes(90))
                    return false;
            }

            if (match.outright_type == eOutrightType.Outright && SelectedTournaments.Contains(match.MatchView.TournamentView.LineObject.GroupId.ToString() + "*1"))
                return true;
            else if (match.outright_type == eOutrightType.None && SelectedTournaments.Contains(match.MatchView.TournamentView.LineObject.GroupId.ToString() + "*0"))
                return true;

            if (match.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(match.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;

        }

        public bool MatchFilterSportBar(IMatchLn match)
        {
            if (!match.Active.Value)
                return false;

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            if (match.IsLiveBet.Value)
                return false;

            if (match.MatchView.CategoryView == null)
                return false;

            string id = (match.MatchView.TournamentView.LineObject.GroupId.ToString());
            string tourId = match.outright_type == eOutrightType.Outright ? id + "*1" : id + "*0";

            if (SelectedTournaments.Count > 0 && !SelectedTournaments.Contains(tourId))
                return false;


            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;


            if (ChangeTracker.PreMatchSelectedMode == 1)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate >= DateTime.Now.AddDays(1).Date)
                    return false;
            }
            if (ChangeTracker.PreMatchSelectedMode == 2)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate > DateTime.Now.AddMinutes(180))
                    return false;
            }

            if (match.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(match.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;

        }

        public HashSet<string> SelectedTournaments
        {
            get { return _selectedTournaments; }
            set { _selectedTournaments = value; }
        }

        private void OnChoiceExecute(IMatchVw chosenMatch)
        {
            SelectedMatch = chosenMatch;
            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);
        }

        private void OnOutrightChoiceExecute(IMatchVw chosenMatch)
        {
            SelectedMatch = chosenMatch;
            MyRegionManager.NavigateUsingViewModel<OutrightViewModel>(RegionNames.ContentRegion);
        }

        public void Refresh(bool state)
        {
            Matches.Clear();
            FillMatches();
        }



        public override void Close()
        {
            LineSr.UnsubscribeFromEnent(LineSr_DataSqlUpdateSucceeded);
            base.Close();
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.IsPrematchEnabled)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}