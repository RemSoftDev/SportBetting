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


    /// <summary>
    /// Tournaments view model.
    /// </summary>
    [ServiceAspect]
    public class TournamentsViewModel : BaseViewModel
    {
        private SortableObservableCollection<TournamentVw> _tournaments = new SortableObservableCollection<TournamentVw>();
        private static object _itemsLock = new object();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentsViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public TournamentsViewModel(params object[] args)
        {
            LayoutUpdatedCommand = new Command<double>(LayoutUpdated);
            Choice = new Command<TournamentVw>(OnChoiceExecute);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            CheckedBox = new Command<TournamentVw>(OnCheckedExecute);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            BindingOperations.EnableCollectionSynchronization(_tournaments, _itemsLock);

            if (args.Length > 0)
            {
                if (args[0] is long)
                    SelectedCategoryId = (long)args[0];

                if (args.Length > 1 && args[1] is TournamentVw)
                    SelectedTournament = (TournamentVw)args[1];

            }

        }



        #endregion

        #region Properties

        public TournamentVw SelectedTournament { get; set; }

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

        public SortableObservableCollection<TournamentVw> Tournaments
        {
            get { return _tournaments; }
            set
            {
                _tournaments = value;
                OnPropertyChanged();
            }
        }
        public HashSet<string> SelectedTournaments
        {
            get { return _selectedTournaments; }
            set { _selectedTournaments = value; }
        }
        /// <summary>
        /// Register the Tournaments property so it is known in the class.
        /// </summary>

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

        #region Methods

        public void LayoutUpdated(double width)
        {
            ColumnsAmount = (Int32)(width / 259);
        }

        void LineSr_DataSqlUpdateSucceeded(SportRadar.DAL.CommonObjects.eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.PreMatches)
                FillTournaments();
        }


        public override void OnNavigationCompleted()
        {
            SelectedTournaments = ChangeTracker.SelectedTournaments;
            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            //Mediator.SendMessage<bool>(false, MsgTag.BlockTimeFilter);
            FillTournaments();
            LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
            ChangeTracker.SelectedSports = true;
            base.OnNavigationCompleted();
        }

        object _lockerTimer = new object();
        private HashSet<string> _selectedTournaments = new HashSet<string>();

        public override void Close()
        {
            LineSr.UnsubscribeFromEnent(LineSr_DataSqlUpdateSucceeded);

            base.Close();
        }

        public int Sort(IMatchVw m1, IMatchVw m2)
        {
            return 0;
        }

        private void FillTournaments()
        {

            lock (_lockerTimer)
            {
                SortableObservableCollection<IMatchVw> matches = new SortableObservableCollection<IMatchVw>();

                matches = Repository.FindMatches(matches, "", SelectedLanguage, MatchFilter, Sort);

                bool showSeparateOutrights = SelectedTournament != null && SelectedTournament.IsOutrightGroup;

                if (!showSeparateOutrights)
                {
                    var outrights = matches.Where(x => x.IsOutright).OrderBy(o => o.TournamentView.LineObject.GroupId).ToList();

                    if (outrights.Count > 0)
                    {
                        TournamentVw tournamentVw = null;

                        if (Tournaments.Count > 0 && Tournaments[0].IsOutrightGroup)
                        {
                            tournamentVw = Tournaments[0];
                            tournamentVw.Name = TranslationProvider.Translate(MultistringTags.OUTRIGHTS).ToString();
                        }
                        else
                        {
                            tournamentVw = new TournamentVw(int.MinValue, 0, TranslationProvider.Translate(MultistringTags.OUTRIGHTS).ToString(), 0, int.MinValue, null, "");
                            tournamentVw.IsOutrightGroup = true;

                            Tournaments.Insert(0, tournamentVw);
                        }
                        tournamentVw.TemporaryMatchesCount = outrights.Count;

                    }
                }

                var prematches = matches.Where(x => x.IsOutright == showSeparateOutrights).OrderBy(o => o.TournamentView.LineObject.GroupId).ToList();

                foreach (var matchVw in prematches)
                {
                    if (matchVw.TournamentView != null && matchVw.ExpiryDate > DateTime.Now)
                    {
                        TournamentVw tournamentVw = null;

                        int iTrmtIndex = Tournaments.IndexOf(new TournamentVw(matchVw.TournamentView.LineObject.GroupId));

                        if (iTrmtIndex >= 0)
                        {
                            tournamentVw = Tournaments[iTrmtIndex];
                            tournamentVw.Name = matchVw.TournamentView.DisplayName;
                        }
                        else
                        {
                            long countryId = -1;
                            string country = "";
                            if (matchVw.CountryView != null)
                            {
                                countryId = matchVw.CountryView.LineObject.SvrGroupId;
                                country = matchVw.CountryView.DisplayName;
                            }
                            //mv.TournamentView.LineObject.

                            string mincombination = null;
                            if (matchVw.TournamentView.LineObject.GroupTournament != null && matchVw.TournamentView.LineObject.GroupTournament.MinCombination.Value > 0)
                            {
                                mincombination = TranslationProvider.Translate(MultistringTags.TERMINAL_X_COMB, matchVw.TournamentView.LineObject.GroupTournament.MinCombination.Value);
                            }
                            tournamentVw = new TournamentVw(matchVw.TournamentView.LineObject.GroupId,
                                                    matchVw.TournamentView.LineObject.SvrGroupId,
                                                    matchVw.TournamentView.DisplayName,
                                                    countryId,
                                                    matchVw.TournamentView.LineObject.Sort.Value,
                                                    mincombination,
                                                    country);
                            
                            if (matchVw.IsOutright)
                                tournamentVw.ContainsOutrights = true;

                            string id = tournamentVw.Id.ToString();
                            string tourId = matchVw.IsOutright ? id + "*1" : id + "*0";

                            if (ChangeTracker.SelectedTournaments.Contains(tourId))
                            {
                                tournamentVw.IsSelected = true;
                            }

                            Tournaments.Add(tournamentVw);
                        }

                        tournamentVw.AddMatch();
                    }
                }





                foreach (TournamentVw tournament in Tournaments)
                {
                    tournament.ApplayTemporaryMatchesCount();
                }


                Tournaments.Sort(Comparison);
            }

            if (ChangeTracker.SelectedTournaments.Count == 0)
                Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            else
                Mediator.SendMessage(true, MsgTag.ActivateShowSelected);
        }

        public static int Comparison(TournamentVw t1, TournamentVw t2)
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

            if (SelectedCategoryId != 0)
            {
                if (matchLn.MatchView.CategoryView == null || matchLn.MatchView.CategoryView.LineObject.GroupId != SelectedCategoryId)
                    return false;
            }

            if (matchLn.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(matchLn.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;
        }

        protected long SelectedCategoryId { get; set; }


        private void OnChoiceExecute(TournamentVw tournamentVw)
        {

            WaitOverlayProvider.ShowWaitOverlay();

            if (tournamentVw.IsOutrightGroup)
            {
                MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion, SelectedCategoryId, tournamentVw);
            }
            else
            {
                if (SelectedTournament == null)
                    MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, new HashSet<string>() { tournamentVw.Id.ToString() + "*0" });
                else
                    MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, new HashSet<string>() { tournamentVw.ContainsOutrights ? tournamentVw.Id.ToString() + "*1" : tournamentVw.Id.ToString() + "*0" });

                Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
            }

        }


        private void OnLanguageChosenExecute(string lang)
        {
            Refresh(true);
        }

        public void Refresh(bool state)
        {
            FillTournaments();
        }

        private void OnCheckedExecute(TournamentVw tournamentVw)
        {
            string id = tournamentVw.Id.ToString();
            string tourId = tournamentVw.ContainsOutrights ? id + "*1" : id + "*0";

            if (tournamentVw.ContainsOutrights && ChangeTracker.SelectedTournaments.Contains(id + "*1"))
            {
                ChangeTracker.SelectedTournaments.Remove(id + "*1");

                if (ChangeTracker.SelectedTournaments.Count == 0)
                {
                    MyRegionManager.ClearForwardHistory(RegionNames.ContentRegion);
                    Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
                }
            }
            else if (!tournamentVw.ContainsOutrights && ChangeTracker.SelectedTournaments.Contains(id + "*0"))
            {
                ChangeTracker.SelectedTournaments.Remove(id + "*0");

                if (ChangeTracker.SelectedTournaments.Count == 0)
                {
                    MyRegionManager.ClearForwardHistory(RegionNames.ContentRegion);
                    Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
                }
            }
            else
                ChangeTracker.SelectedTournaments.Add(tourId);


            if (ChangeTracker.SelectedTournaments.Count == 1)
                Mediator.SendMessage(true, MsgTag.ActivateShowSelected);

            SelectedTournaments = ChangeTracker.SelectedTournaments;
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.IsPrematchEnabled)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}