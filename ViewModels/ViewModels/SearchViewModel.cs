using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using SportRadar.Common.Extensions;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class SearchViewModel : BaseViewModel
    {
        private static ILog Log = LogFactory.CreateLog(typeof(SearchViewModel));

        #region Constructors

        public SearchViewModel()
        {



            OpenMatch = new Command<IMatchVw>(OnChoiceExecute);
            OpenOutrightMatch = new Command<IMatchVw>(OnOutrightChoiceExecute);
            PlaceBet = new Command<IOddVw>(OnBet);
            SearchCommand = new Command<string>(OnSearchExecute);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            UnfocusComand = new Command(OnUnfocus);
            Mediator.Register<bool>(this, OnLanguageChosen, MsgTag.Refresh);
            Mediator.Register<string>(this, OnSearchExecute, MsgTag.EnterCommand);
            Mediator.Register<string>(this, OnSearchExecute, MsgTag.GetSearchResults);


        }



        private void OnUnfocus()
        {
            IsFocused = false;
        }

        #endregion

        #region Properties
        public ObservableCollection<ComboBoxItem> Sports
        {
            get { return ChangeTracker.SearchSports; }
            set
            {
                ChangeTracker.SearchSports = value;
                OnPropertyChanged();
            }
        }

        private bool _isFocused;
        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                if (_isFocused)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ComboBoxItem> Tournaments
        {
            get { return ChangeTracker.SearchTournaments; }
            set
            {
                ChangeTracker.SearchTournaments = value;
                OnPropertyChanged();
            }
        }

        public IMatchVw SelectedMatch
        {
            get { return ChangeTracker.CurrentMatch; }
            set { ChangeTracker.CurrentMatch = value; }
        }

        public ComboBoxItem SelectedTournament
        {
            get { return ChangeTracker.SearchSelectedTournament; }
            set
            {
                if (value == null)
                    return;
                ChangeTracker.SearchSelectedTournament = value;
                OnPropertyChanged();
                FillMatches();
            }
        }

        public ComboBoxItem SelectedSport
        {
            get { return ChangeTracker.SearchSelectedSport; }
            set
            {
                if (value == null)
                    return;

                ChangeTracker.SearchSelectedSport = value;
                OnPropertyChanged();

                var sportscb = new List<ComboBoxItem>();
                var tournaments = AllResults.Where(x => x.SportView.LineObject.GroupId == ChangeTracker.SearchSelectedSport.Id).Where(x => x.TournamentView != null).Select(x => x.TournamentView).Distinct().ToList();
                foreach (var group in tournaments)
                {
                    if (sportscb.Count(x => x.Id == group.LineObject.GroupId) == 0)
                        sportscb.Add(new ComboBoxItem(group.DisplayName, group.LineObject.GroupId) { OrderId = AllResults.Count(x => x.SportView.LineObject.GroupId == @group.LineObject.GroupId) });
                }
                for (int i = 1; i < sportscb.Count; )
                {
                    var comboBoxItem = sportscb[i];

                    if (tournaments.Count(x => x.LineObject.GroupId != comboBoxItem.Id) == 0)
                    {
                        sportscb.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                Tournaments = new ObservableCollection<ComboBoxItem>(sportscb.OrderByDescending(x => x.OrderId).ToList());
                ChangeTracker.SearchSelectedTournament = Tournaments[0];
                OnPropertyChanged("SelectedTournament");
                FillMatches();

            }
        }

        public SortableObservableCollection<IMatchVw> Matches
        {
            get { return ChangeTracker.SearchMatches; }
            set { ChangeTracker.SearchMatches = value; }
        }




        private int _id;

        #endregion

        #region Commands
        /// <summary>
        /// Gets the Choice command.
        /// </summary>
        public Command<string> SearchCommand { get; private set; }
        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<IMatchVw> OpenOutrightMatch { get; private set; }
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command UnfocusComand { get; private set; }


        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.KeyboardRegion);
            if (string.IsNullOrEmpty(ChangeTracker.SearchString))
            {
                IsFocused = true;
            }
            ChangeTracker.IsSearchOpen = true;
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);

            //Mediator.SendMessage<bool>(true, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockTimeFilter);

            base.OnNavigationCompleted();
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);

        }

        private void OnLanguageChosen(bool obj)
        {
            ChangeTracker.SearchString = (ChangeTracker.SearchString == null) ? "" : ChangeTracker.SearchString.Trim();

            if (ChangeTracker.SearchString.Length < 3)
            {
                return;
            }
            PleaseWaitSearch();
        }

        private void ClearSearchResults(string obj)
        {
            SelectedSport = null;
            SelectedTournament = null;
            Sports = new ObservableCollection<ComboBoxItem>();
            Tournaments = new ObservableCollection<ComboBoxItem>();
            ChangeTracker.SearchString = "";
            IsFocused = false;
            Matches.Clear();
        }

        private void OnSetFocus(string obj)
        {
            IsFocused = true;
        }



        private void OnChoiceExecute(IMatchVw chosenMatch)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            SelectedMatch = chosenMatch;
            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);
        }

        private void OnOutrightChoiceExecute(IMatchVw chosenMatch)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            SelectedMatch = chosenMatch;
            MyRegionManager.NavigateUsingViewModel<OutrightViewModel>(RegionNames.ContentRegion);
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.KeyboardRegion);
            ChangeTracker.IsSearchOpen = false;
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            IsFocused = false;

            //Mediator.SendMessage<bool>(false, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(false, MsgTag.BlockTimeFilter);
            base.Close();
        }


        private void OnSearchExecute(string searchString)
        {
            Log.Info(String.Format("Search string: {0}", ChangeTracker.SearchString));
            var sport = ChangeTracker.SearchSelectedSport;
            var tournament = ChangeTracker.SearchSelectedTournament;
            PleaseWaitSearch();
            if (searchString == "Back")
            {
                SelectedSport = Sports.FirstOrDefault(x => x.Id.Equals(sport.Id));
                SelectedTournament = Tournaments.FirstOrDefault(x => x.Id.Equals(tournament.Id));
            }
        }

        private bool MatchFilter(IMatchLn match)
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

            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;

            if (ChangeTracker.SearchSelectedTournament.Id != 0)
            {
                if (match.MatchView.TournamentView.LineObject.GroupId != ChangeTracker.SearchSelectedTournament.Id)
                    return false;
            }
            if (ChangeTracker.SearchSelectedSport.Id != 0)
            {
                if (match.MatchView.SportView.LineObject.GroupId != ChangeTracker.SearchSelectedSport.Id)
                    return false;
            }

            if (match.Code.Value.ToString("G").Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (match.outright_type == eOutrightType.None)
            {
                if (match.HomeCompetitor.GetDisplayName(SelectedLanguage).Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (match.AwayCompetitor.GetDisplayName(SelectedLanguage).Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (match.outright_type == eOutrightType.Outright)
            {
                if (
                    match.OutrightCompetitors.Any(
                        c =>
                        c.Value.GetCompetitor()
                         .GetDisplayName(SelectedLanguage)
                         .Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            if (match.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(match.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return false;

        }
        private bool SimpleFilter(IMatchLn match)
        {
            if (match.IsLiveBet.Value)
            {
                return false;
            }
            if (!match.Active.Value)
                return false;

            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
            {
                return false;
            }
            if (match.MatchView.VisibleBetDomainCount == 0)
            {
                return false;
            }

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            return true;

        }

        private bool MatchFilter2(IMatchLn match)
        {
            if (match.IsLiveBet.Value)
            {
                return false;
            }
            if (!match.Active.Value)
                return false;

            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
            {
                return false;
            }
            if (match.MatchView.VisibleBetDomainCount == 0)
            {
                return false;
            }

            if (match.MatchView.CategoryView == null)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            if (match.Code.Value.ToString("G").Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (match.outright_type == eOutrightType.None)
            {
                if (match.HomeCompetitor.GetDisplayName(SelectedLanguage).Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (match.AwayCompetitor.GetDisplayName(SelectedLanguage).Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (match.outright_type == eOutrightType.Outright)
            {
                if (
                    match.OutrightCompetitors.Any(
                        c => c.Value.GetCompetitor() != null &&
                        c.Value.GetCompetitor()
                         .GetDisplayName(SelectedLanguage)
                         .Contains(ChangeTracker.SearchString, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return false;

        }

        public static int Comparison(IMatchVw m1, IMatchVw m2)
        {
            if (m1.StartDate == m2.StartDate)
            {
                return m2.TournamentView.LineObject.GroupId.CompareTo(m1.TournamentView.LineObject.GroupId);
            }

            return m1.StartDate.CompareTo(m2.StartDate);
        }


        private void FillMatches()
        {
            Matches.Clear();
            Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, Comparison);

            string oldSport = "";
            for (int i = 0; i < Matches.Count; i++)
            {
                string currentSport = Matches.ElementAt(i).SportView.DisplayName;

                if (string.IsNullOrEmpty(currentSport))
                    continue;

                if (currentSport != oldSport)
                {
                    Matches.ElementAt(i).IsHeaderForPreMatch = true;
                    oldSport = currentSport;
                }
                else
                    Matches.ElementAt(i).IsHeaderForPreMatch = false;
            }
        }


        [PleaseWaitAspect]
        public void PleaseWaitSearch()
        {
            ChangeTracker.SearchString = (ChangeTracker.SearchString == null) ? "" : ChangeTracker.SearchString.Trim();

            if (ChangeTracker.SearchString.Length < 3)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_SEARCH_STRING_TOO_SHORT).ToString());
                ClearSearchResults("");
                return;
            }

            Repository.FindMatches(AllResults, "", SelectedLanguage, MatchFilter2, delegate(IMatchVw m1, IMatchVw m2) { return 0; });

            if (AllResults.Count == 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_NO_MATCH_FOUND).ToString());
                ClearSearchResults("");
            }
            else
            {


                var sportscb = new List<ComboBoxItem>();
                var sports = AllResults.Where(x => x.SportView != null).Select(x => x.SportView).Distinct().ToList();
                foreach (var group in sports)
                {
                    if (sportscb.Count(x => x.Id == group.LineObject.GroupId) == 0)
                        sportscb.Add(new ComboBoxItem(group.DisplayName, group.LineObject.GroupId) { OrderId = AllResults.Count(x => x.SportView.LineObject.GroupId == @group.LineObject.GroupId) });
                }
                for (int i = 1; i < sportscb.Count; )
                {
                    var comboBoxItem = sportscb[i];

                    if (sports.Count(x => x.LineObject.GroupId != comboBoxItem.Id) == 0)
                    {
                        sportscb.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                Sports = new ObservableCollection<ComboBoxItem>(sportscb.OrderByDescending(x => x.OrderId).ToList());
                ChangeTracker.SearchSelectedSport = Sports[0];
                OnPropertyChanged("SelectedSport");


                var tournaments = new List<ComboBoxItem>();
                var groups = AllResults.Where(x => x.SportView.LineObject.GroupId == ChangeTracker.SearchSelectedSport.Id).Where(x => x.TournamentView != null).Select(x => x.TournamentView).Distinct().ToList();
                foreach (var group in groups)
                {
                    if (tournaments.Count(x => x.Id == group.LineObject.GroupId) == 0)
                        tournaments.Add(new ComboBoxItem(group.DisplayName, group.LineObject.GroupId) { OrderId = AllResults.Count(x => x.TournamentView.LineObject.GroupId == @group.LineObject.GroupId) });
                }
                for (int i = 1; i < tournaments.Count; )
                {
                    var comboBoxItem = tournaments[i];

                    if (groups.Count(x => x.LineObject.GroupId != comboBoxItem.Id) == 0)
                    {
                        tournaments.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                Tournaments = new ObservableCollection<ComboBoxItem>(tournaments.OrderByDescending(x => x.OrderId).ToList());
                ChangeTracker.SearchSelectedTournament = Tournaments[0];
                OnPropertyChanged("SelectedTournament");

                FillMatches();
            }


        }

        public SortableObservableCollection<IMatchVw> AllResults
        {
            get { return ChangeTracker.AllResults; }
            set
            {
                ChangeTracker.AllResults = value;
                OnPropertyChanged();
            }
        }






        #endregion
    }
}