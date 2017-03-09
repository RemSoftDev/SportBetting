using System.Linq;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class VHCViewModel : BaseViewModel
    {

        #region classVariables

        private int totalRaces = 0;
        private int currentRaceNumber = 0;

        private int lastRaceIndex = 0;

        #endregion

        #region Commands

        public Command onWinPlacePressed { get; private set; }
        public Command onForecastPressed { get; private set; }
        public Command onTricastPressed { get; private set; }

        public Command PreviousPageCmd { get; private set; }
        public Command NextPageCmd { get; private set; }

        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }

        public Command OpenVHCHelpCommand { get; private set; }

        #endregion

        #region methods

        public VHCViewModel()
        {
            if (ChangeTracker.VhcSelectedType == null)
                ChangeTracker.VhcSelectedType = VHCType.WinPlace;
            _winVisibility = Visibility.Visible;
            _invertedVisibility = Visibility.Collapsed;

            onWinPlacePressed = new Command(OpenWinPlace);
            onForecastPressed = new Command(OpenForecast);
            onTricastPressed = new Command(OpenTricast);

            PlaceBet = new Command<IOddVw>(OnBet);

            PreviousPageCmd = new Command(PrevPage);
            NextPageCmd = new Command(NextPage);

            OpenVHCHelpCommand = new Command(OpenVHCHelp);

            ScrollChangedCommand = new Command<double>(ScrollChanged);
            LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);

        }

        private void OpenVHCHelp()
        {
            Mediator.SendMessage("VHC", MsgTag.ShowTermsAndConditions);
        }

        private void NextPage()
        {
            if (currentRaceNumber >= totalRaces - 1 || totalRaces <= 0)
                return;

            currentRaceNumber++;

            OnPropertyChanged("Race");
            OnPropertyChanged("RaceDay");
            OnPropertyChanged("Competitors");
            OnPropertyChanged("VisibleBetdomains");

        }

        private void PrevPage()
        {
            if (currentRaceNumber <= 0 || totalRaces <= 0)
                return;

            currentRaceNumber--;

            OnPropertyChanged("Race");
            OnPropertyChanged("RaceDay");
            OnPropertyChanged("Competitors");
            OnPropertyChanged("VisibleBetdomains");

        }

        public override void OnNavigationCompleted()
        {
            Update();
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            switch (ChangeTracker.VhcSelectedType)
            {
                case VHCType.Forecast:
                    onForecastPressed.Execute(this);
                    break;
                case VHCType.Tricast:
                    onTricastPressed.Execute(this);
                    break;
            }

            base.OnNavigationCompleted();
        }

        private void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.VirtualBet)
                Update();
        }

        private void Update()
        {
            Dispatcher.Invoke(() =>
            {
                Repository.FindMatches(Races, "", SelectedLanguage, MatchFilter, Comparison);
                totalRaces = Races.Count;
                if (currentRaceNumber == 0)
                    currentRaceNumber = GetSavedPage();
                if (Races != null && Races.Count > 0)
                {
                    OnPropertyChanged("Race");
                    OnPropertyChanged("RaceDay");

                    if (ChangeTracker.VhcSelectedType == VHCType.WinPlace)
                        OnPropertyChanged("Competitors");
                    else
                        FillBetdomains();
                }
            });
        }

        private void FillBetdomains()
        {
        }

        private bool MatchFilter(IMatchLn match)
        {

            if (match.SourceType != SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                return false;

            if (match.LiveMatchInfo.Status.Value == eMatchStatus.Started)
                return false;

            if (match.StartDate.Value.LocalDateTime < DateTime.Now)
                return false;


            if (match.VhcChannelId != 0)
                return false;


            if (match.MatchView.Visibility == Visibility.Collapsed)
                return false;

            if (!match.Active.Value)
                return false;

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            return true;
        }

        public int Comparison(IMatchVw m1, IMatchVw m2)
        {
            //if (ChangeTracker.SelectedSportFilter.Id == "999")
            //{
            //    if (m1.StartDate == m2.StartDate)
            //        return m1.Name.CompareTo(m2.Name);

            //    return m1.StartDate.CompareTo(m2.StartDate);
            //}

            var sSportName1 = m1.SportView.LineObject.Sort.Value;
            var sSportName2 = m2.SportView.LineObject.Sort.Value;

            if ((!m1.IsLiveBet || m1.LiveBetStatus == eMatchStatus.NotStarted) && (!m2.IsLiveBet || m2.LiveBetStatus == eMatchStatus.NotStarted))
            {
                if (m1.LineObject.MatchExternalState.VirtualSeason.Value == m2.LineObject.MatchExternalState.VirtualSeason.Value)
                {
                    if (m1.StartDate == m2.StartDate)
                    {
                        return m1.Name.CompareTo(m2.Name);
                    }
                    return m1.StartDate.CompareTo(m2.StartDate);
                }

                return m1.LineObject.MatchExternalState.VirtualSeason.Value.CompareTo(m2.LineObject.MatchExternalState.VirtualSeason.Value);
            }
            if (!m1.IsLiveBet || m1.LiveBetStatus == eMatchStatus.NotStarted)
                return 1;
            if (!m2.IsLiveBet || m2.LiveBetStatus == eMatchStatus.NotStarted)
                return -1;
            var dd = m1.LiveBetStatus.CompareTo(m2.LiveBetStatus);
            return dd;
        }

        public override void Close()
        {
            ChangeTracker.IsForecastOpen = false;
            Mediator.UnregisterRecipientAndIgnoreTags(this);
            Races.Clear();
            LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
            base.Close();
        }

        public void OpenWinPlace()
        {
            ChangeTracker.IsForecastOpen = false;
            WinVisibility = Visibility.Visible;
            InvertedWinVisibility = Visibility.Collapsed;
            ChangeTracker.VhcSelectedType = VHCType.WinPlace;
            ScrollToTop();
            OnPropertyChanged("Competitors");
        }

        public void OpenForecast()
        {
            ChangeTracker.IsForecastOpen = true;
            WinVisibility = Visibility.Collapsed;
            InvertedWinVisibility = Visibility.Visible;
            ChangeTracker.VhcSelectedType = VHCType.Forecast;
            ScrollToTop();
            OnPropertyChanged("VisibleBetdomains");
        }

        public void OpenTricast()
        {
            ChangeTracker.IsForecastOpen = true;
            ChangeTracker.VhcSelectedType = VHCType.Tricast;
            WinVisibility = Visibility.Collapsed;
            InvertedWinVisibility = Visibility.Visible;
            ScrollToTop();
            OnPropertyChanged("VisibleBetdomains");
        }

        private int GetSavedPage()
        {
            if (ChangeTracker.CurrentMatchOrRaceDay == null || ChangeTracker.CurrentSeasonOrRace == null)
                return 0;
            for (int i = 0; i < Races.Count; i++)
                if (Races.ElementAt(i).LineObject.MatchExternalState.VirtualSeason.Value.ToString() == ChangeTracker.CurrentMatchOrRaceDay
                    && GetRaceByRaceNumber(i) == ChangeTracker.CurrentSeasonOrRace)
                {
                    return i;
                }
            return 0;
        }

        #endregion

        #region Properties

        private SortableObservableCollection<IMatchVw> _races = new SortableObservableCollection<IMatchVw>();

        protected SortableObservableCollection<IMatchVw> Races
        {
            get { return _races; }
            set { _races = value; }
        }

        private SyncObservableCollection<OutrightCompetitorVw> _competitors = new SyncObservableCollection<OutrightCompetitorVw>();

        public SyncObservableCollection<OutrightCompetitorVw> Competitors
        {
            get
            {
                return Races.Count <= 0 ? _competitors : Races.ElementAt(currentRaceNumber).OutrightCompetitorsVHC;
            }
        }



        private Visibility _winVisibility;
        public Visibility WinVisibility
        {
            get { return _winVisibility; }
            set
            {
                if (_winVisibility != value)
                    _winVisibility = value;
                OnPropertyChanged("WinVisibility");
            }
        }

        private Visibility _invertedVisibility;
        public Visibility InvertedWinVisibility
        {
            get { return _invertedVisibility; }
            set
            {
                if (_invertedVisibility != value)
                    _invertedVisibility = value;
                OnPropertyChanged("InvertedWinVisibility");
            }
        }

        public string Raceday
        {
            get
            {
                if (totalRaces <= 0)
                    return "";

                if (totalRaces - 1 < currentRaceNumber)
                    currentRaceNumber = totalRaces - 1;
                var raceday = Races.ElementAt(currentRaceNumber).LineObject.MatchExternalState.VirtualSeason.Value.ToString();
                ChangeTracker.CurrentMatchOrRaceDay = raceday;
                return raceday;
            }
        }

        public string Race
        {
            get
            {
                if (totalRaces <= 0)
                    return "";

                if (totalRaces - 1 < currentRaceNumber)
                    currentRaceNumber = totalRaces - 1;
                var race = GetRaceByRaceNumber(currentRaceNumber);
                ChangeTracker.CurrentSeasonOrRace = race;
                return race;
            }
        }

        private string GetRaceByRaceNumber(int raceNumber)
        {
            int currentseasonRaces = 0;
            long currentSeason = Races.ElementAt(raceNumber).LineObject.MatchExternalState.VirtualSeason.Value;

            currentseasonRaces =
                Races.Where(x => x.LineObject.MatchExternalState.VirtualSeason.Value == currentSeason)
                     .Max(x => x.LineObject.MatchExternalState.VirtualDay.Value);

            //foreach (IMatchVw match in Races)
            //{
            //    if (match.LineObject.MatchExternalState.VirtualSeason.Value == currentSeason)
            //        currentseasonRaces = match.LineObject.MatchExternalState.VirtualDay.Value;
            //}
            return Races.ElementAt(raceNumber).LineObject.MatchExternalState.VirtualDay.Value.ToString() + "/" +
                   currentseasonRaces.ToString();
        }

        public SyncObservableCollection<IBetDomainVw> VisibleBetdomains
        {
            get
            {
                SyncObservableCollection<IBetDomainVw> betdomains = null;

                if (Races.Count > 0 && Races.ElementAt(currentRaceNumber) != null)
                {
                    if (ChangeTracker.VhcSelectedType == VHCType.Forecast)
                        betdomains = new SyncObservableCollection<IBetDomainVw>((Races.ElementAt(currentRaceNumber).VisibleBetDomainViews.Where(x => x.BetTag == "VHCSF" || x.BetTag == "VHCDF")).ToList());
                    else if (ChangeTracker.VhcSelectedType == VHCType.Tricast)
                        betdomains = new SyncObservableCollection<IBetDomainVw>((Races.ElementAt(currentRaceNumber).VisibleBetDomainViews.Where(x => x.BetTag == "VHCST" || x.BetTag == "VHCDT")).ToList());
                }

                if (betdomains == null)
                    betdomains = new SyncObservableCollection<IBetDomainVw>();

                return betdomains;
            }
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.AllowVhc && StationRepository.AllowVfl)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}
