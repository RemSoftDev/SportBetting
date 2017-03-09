using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class TopTournamentsViewModel : BaseViewModel
    {
        private static object _itemsLock = new object();

        #region Constructors
        
        public TopTournamentsViewModel()
        {
            LayoutUpdatedCommand = new Command<double>(LayoutUpdated);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            CheckedBox = new Command<TournamentVw>(OnCheckedExecute);
            Choice = new Command<TournamentVw>(OnChoiceExecute);
            LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
        }

        #endregion

        #region Properties

        private SortableObservableCollection<TournamentVw> _tournaments = new SortableObservableCollection<TournamentVw>();
        public SortableObservableCollection<TournamentVw> Tournaments
        {
            get { return _tournaments; }
            set
            {
                _tournaments = value;
                OnPropertyChanged();
            }
        }

        private SyncObservableCollection<SportCategory> _categories = new SyncObservableCollection<SportCategory>();
        public SyncObservableCollection<SportCategory> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
                OnPropertyChanged("Categories");
            }
        }

        private int _columnsAmount = 4;
        public int ColumnsAmount
        {
            get
            {
                return _columnsAmount;
            }
            set
            {
                if (_columnsAmount != value)
                {
                    _columnsAmount = value;
                    OnPropertyChanged("ColumnsAmount");
                }
            }
        }

        #endregion

        #region Methods

        private void OnChoiceExecute(TournamentVw tournamentVw)
        {

            WaitOverlayProvider.ShowWaitOverlay();

            if (tournamentVw.IsOutrightGroup)
            {
                if(tournamentVw.SvrId == -999)
                    MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion, tournamentVw.SportId);
                else
                    MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion, tournamentVw.SportId, tournamentVw);
            }
            else
            {
                MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, new HashSet<string>() { tournamentVw.IsOutrightGroup ? tournamentVw.Id.ToString() + "*1" : tournamentVw.Id.ToString() + "*0" });
                Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
                //if (!tournamentVw.IsOutrightGroup)
                //    MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, new Dictionary<long, bool>() { { tournamentVw.Id, false } });
                //else
                //    MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, new Dictionary<long, bool>() { { tournamentVw.Id, tournamentVw.IsOutrightGroup } });
            }

        }

        private void OnCheckedExecute(TournamentVw tournamentVw)
        {
            if (ChangeTracker.SelectedTournaments.Contains(tournamentVw.Id.ToString() + "*0"))
            {
                ChangeTracker.SelectedTournaments.Remove(tournamentVw.Id.ToString() + "*0");
                if (ChangeTracker.SelectedTournaments.Count == 0)
                {
                    MyRegionManager.ClearForwardHistory(RegionNames.ContentRegion);
                    Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
                }
            }
            else
                ChangeTracker.SelectedTournaments.Add(tournamentVw.Id.ToString() + "*0");

            if (ChangeTracker.SelectedTournaments.Count == 1)
                Mediator.SendMessage(true, MsgTag.ActivateShowSelected);

        }

        void LineSr_DataSqlUpdateSucceeded(SportRadar.DAL.CommonObjects.eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.PreMatches)
                FillTournaments();
        }

        public override void OnNavigationCompleted()
        {
            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);

            FillTournaments();
            ChangeTracker.SelectedSports = true;
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);

            if (ChangeTracker.SelectedTournaments.Count > 0)
                Mediator.SendMessage(true, MsgTag.ActivateShowSelected);

            base.OnNavigationCompleted();
        }

        private void OnLanguageChosenExecute(string lang)
        {
            Refresh(true);
        }

        public void Refresh(bool state)
        {
            FillTournaments();
        }

        private void FillTournaments()
        {
            lock (_itemsLock)
            {
                SortableObservableCollection<IMatchVw> matches = new SortableObservableCollection<IMatchVw>();
                int tournamentsAmount = ChangeTracker.IsLandscapeMode ? 10 : 8;
                SyncObservableCollection<SportCategory> tempCategories = new SyncObservableCollection<SportCategory>();

                matches = Repository.FindMatches(matches, "", SelectedLanguage, MatchFilter, Sort);

                //lets see if we can start from categories, not tournaments
                var groups = matches.Where(x => !x.IsOutright && x.CategoryView != null).Select(x => x.CategoryView).Distinct().ToList();

                foreach (var group in groups)
                {
                    SportCategory temp = new SportCategory(group.DisplayName, new SyncObservableCollection<TournamentVw>(), group.LineObject.GroupId);
                    temp.Sort = group.LineObject.Sort.Value;
                    tempCategories.Add(temp);
                }
                tempCategories = new SyncObservableCollection<SportCategory>(tempCategories.OrderBy(x=>x.Sort).ToList());

                foreach (SportCategory category in tempCategories)
                {
                    //fill tournaments - not outrights
                    List<TournamentVw> Tournaments = new List<TournamentVw>();
                    var tours = matches.Where(x => !x.IsOutright && x.TournamentView != null && x.CategoryView != null && x.CategoryView.LineObject.GroupId == category.CategoryID).Select(x => x.TournamentView).Distinct().ToList();
                    int allTournamentsCount = tours.Count;

                    for (int i = 0; i < tours.Count; i++)
                    { 
                        long id = tours[i].LineObject.GroupId;
                        long svrId = tours[i].LineObject.SvrGroupId;
                        string tournamentName = tours[i].DisplayName;
                        long countryId = 0;
                        string country = "";

                        int sort = tours[i].LineObject.Sort.Value;
                        string minCombination = null;
                        
                        if (tours[i].LineObject.GroupTournament != null && tours[i].LineObject.GroupTournament.MinCombination.Value > 0)
                        {
                            minCombination = TranslationProvider.Translate(MultistringTags.TERMINAL_X_COMB, tours[i].LineObject.GroupTournament.MinCombination.Value);
                        }

                        if (tours[i].TournamentCountryView != null)
                        {
                            countryId = tours[i].TournamentCountryView.LineObject.SvrGroupId;
                            country = tours[i].TournamentCountryView.DisplayName;
                        }
                        
                        long sportId = 0;
                        string sportName = "";

                        if (tours[i].TournamentSportView != null)
                        {
                            sportId = tours[i].TournamentSportView.LineObject.SvrGroupId;
                            sportName = tours[i].TournamentSportView.DisplayName;
                        }

                        bool isOutright = false;
                        long categoryId = 0;                        

                        TournamentVw tour = new TournamentVw(id, svrId, tournamentName, countryId, sort, minCombination, country, sportId, isOutright, sportName, categoryId);
                        tour.TemporaryMatchesCount = matches.Where(x => !x.IsOutright && x.TournamentView != null && x.TournamentView.LineObject.GroupId == tour.Id).Count();
                        tour.ApplayTemporaryMatchesCount();

                        if (ChangeTracker.SelectedTournaments.Contains(tour.Id.ToString() + "*0"))
                        {
                            tour.IsSelected = true;
                        }

                        Tournaments.Add(tour);
                    }

                    Tournaments = new List<TournamentVw>(Tournaments.OrderBy(x=>x.Sort));
                    Tournaments.Sort(Comparison);
                    //now i have all category tournaments

                    for (int i = 0; i < tournamentsAmount; i++)
                    {
                        if (i >= Tournaments.Count)
                            break;

                        category.Tournaments.Add(Tournaments[i]);
                    }

                    //if there is only outrights, add them to category
                    int outrightCount = matches.Where(x => x.IsOutright && x.CategoryView != null && x.CategoryView.LineObject.GroupId == category.CategoryID).Count();
                    if(outrightCount>0)
                        allTournamentsCount++;

                    if (category.Tournaments.Count == 0 || category.Tournaments.Count < tournamentsAmount)
                    {                        
                        if (outrightCount > 0)
                        {
                            TournamentVw outright = new TournamentVw(int.MinValue, 0, TranslationProvider.Translate(MultistringTags.OUTRIGHTS).ToString(), 0, int.MinValue, null, "", category.CategoryID, true, category.SportName);
                            outright.MatchesCount = outrightCount;
                            category.Tournaments.Add(outright);                            
                        }
                    }

                    //add all tournaments button
                    if (allTournamentsCount > category.Tournaments.Count)
                    {
                        category.Tournaments.RemoveAt(category.Tournaments.Count - 1);

                        TournamentVw allTournaments = new TournamentVw(int.MinValue, -999, "All tournaments", 0, int.MinValue, null, "", category.CategoryID, true, category.SportName);
                        allTournaments.MatchesCount = allTournamentsCount;
                        category.Tournaments.Add(allTournaments);
                    }
                }



                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Categories.ApplyChanges(tempCategories);
                }));
            }
        }

        public static int Comparison(TournamentVw t1, TournamentVw t2)
        {
            if (t1.SportId == t2.SportId)
            {
                if (t1.Sort == t2.Sort)
                {
                    if (t1.Name == t2.Name)
                    {
                        return t1.Id.CompareTo(t2.Id);
                    }
                    return t1.Name.CompareTo(t2.Name);
                }
                return t1.Sort.CompareTo(t2.Sort);
            }
            return t1.SportId.CompareTo(t2.SportId);
        }

        public bool MatchFilter(IMatchLn matchLn)
        {

            if (!matchLn.Active.Value)
                return false;

            if (matchLn.IsLiveBet.Value)
                return false;

            if (matchLn.MatchView.CategoryView == null)
                return false;

            if (matchLn.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (matchLn.MatchView.AllVisibleOddCount == 0)
                return false;

            if (matchLn.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;

            if (matchLn.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(matchLn.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;
        }

        public int Sort(IMatchVw m1, IMatchVw m2)
        {
            return 0;
        }

        public void LayoutUpdated(double width)
        {
            ColumnsAmount = (Int32)(width / 259);
        }

        #endregion

        #region Commands
        /// <summary>
        /// Gets the Choice command.
        /// </summary>
        public Command<TournamentVw> Choice { get; private set; }
        public Command<TournamentVw> CheckedBox { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command<double> LayoutUpdatedCommand { get; set; }

        #endregion
    }
}
