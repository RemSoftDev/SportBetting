using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System;
using SportRadar.Common.Windows;
using SportRadar.Common.Logs;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class VFLViewModel : BaseViewModel
    {
        private static ILog _logger = LogFactory.CreateLog(typeof(EntertainmentViewModel));
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        private Dictionary<DateTime, SortableObservableCollection<IMatchVw>> pages = new Dictionary<DateTime, SortableObservableCollection<IMatchVw>>();
        private int currentPage = 0;
        private int totalPages = 0;
        private DispatcherTimer _timer;

        public VFLViewModel()
        {
            NextPageCmd = new Command(NextPage);
            PreviousPageCmd = new Command(PrevPage);
            OpenVflPage = new Command<VFLMatchButton>(OnOpenVflPage);
            OpenMatch = new Command<IMatchVw>(OnChoiceExecute);
            PlaceBet = new Command<IOddVw>(OnBet);
            ScrollChangedCommand = new Command<double>(ScrollChanged);

            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += Timer_Elapsed;
            _timer.Start();
            LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);

        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (ChangeTracker.Is34Mode || ChangeTracker.IsLandscapeMode)
            {
                var foundMatchdayMatches = new SortableObservableCollection<IMatchVw>();
                Repository.FindMatches(foundMatchdayMatches, "", SelectedLanguage, PrevMatchFilter, Comparison);
                if (foundMatchdayMatches.Count > 0)
                {
                    var previousMatchdayMatches = new SortableObservableCollection<IMatchVw>();
                    PrevMatchDay = foundMatchdayMatches.Last().VirtualDay;
                    PrevSeason = foundMatchdayMatches.Last().VirtualSeason;
                    for (int i = foundMatchdayMatches.Count - 1; i >= 0; i--)
                    {
                        if (foundMatchdayMatches[i].VirtualDay == PrevMatchDay && foundMatchdayMatches[i].VirtualSeason == PrevSeason)
                            previousMatchdayMatches.Add(foundMatchdayMatches[i]);
                        else
                        {
                            break;
                        }
                    }


                    ConvertMatchesToButtons(previousMatchdayMatches, VflMatchButtons);
                }
            }
        }

        private void OnOpenVflPage(VFLMatchButton obj)
        {
            Mediator.SendMessage<long>(obj.Channel, MsgTag.ShowVFL);

        }

        public override void OnNavigationCompleted()
        {
            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LiveLanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            Update();


            base.OnNavigationCompleted();

        }

        private void OnChoiceExecute(IMatchVw chosenEntity)
        {
            SelectedMatch = chosenEntity;

            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);
        }

        private void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.VirtualBet)
                Update();
        }

        private void Refresh(bool obj)
        {
            Update();
        }

        private void OnLanguageChosenExecute(string lang)
        {
            Update();
        }

        private void NextPage()
        {
            if (currentPage >= totalPages - 1)
                return;

            currentPage++;
            if (pages.ElementAt(currentPage).Value.ElementAt(0) != null)
            {
                CurrentSeason = pages.ElementAt(currentPage).Value.ElementAt(0).VirtualSeason.ToString();
                CurrentPageToShow = pages.ElementAt(currentPage).Value.ElementAt(0).VirtualDay.ToString();

            }
            Update();
            OnPropertyChanged("CurrentPageToShow");
            OnPropertyChanged("PrevPageToShow");
            OnPropertyChanged("NextPageToShow");
        }

        private void PrevPage()
        {
            if (currentPage == 0)
                return;

            currentPage--;
            if (pages.ElementAt(currentPage).Value.ElementAt(0) != null)
            {
                CurrentSeason = pages.ElementAt(currentPage).Value.ElementAt(0).VirtualSeason.ToString();
                CurrentPageToShow = pages.ElementAt(currentPage).Value.ElementAt(0).VirtualDay.ToString();

            }
            Update();

            OnPropertyChanged("CurrentPageToShow");
            OnPropertyChanged("PrevPageToShow");
            OnPropertyChanged("NextPageToShow");
        }

        private string _currentPageToShow;
        public string CurrentPageToShow
        {
            get
            {
                //if (currentPage == 0 && totalPages == 0)
                //    return "";

                //return (currentPage + 1).ToString();
                return _currentPageToShow;
            }
            set
            {
                _currentPageToShow = value;
                ChangeTracker.CurrentMatchOrRaceDay = _currentPageToShow;
                OnPropertyChanged("CurrentPageToShow");
            }
        }

        public string PrevPageToShow
        {
            get
            {
                if (currentPage == 0)
                    return "";
                else
                    return currentPage.ToString();
            }
        }

        public string NextPageToShow
        {
            get
            {
                if (currentPage >= totalPages - 1)
                    return "";
                else
                    return (currentPage + 2).ToString();
            }
        }

        public override void Close()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Elapsed;

            Mediator.UnregisterRecipientAndIgnoreTags(this);
            Matches.Clear();
            LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
            base.Close();
        }

        public Command<VFLMatchButton> OpenVflPage { get; private set; }
        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command NextPageCmd { get; private set; }
        public Command PreviousPageCmd { get; private set; }
        public Command ShowVFLCommand { get; private set; }
        public Command ShowVHCCommand { get; private set; }
        public int PrevMatchDay
        {
            get { return _prevMatchDay; }
            set
            {
                _prevMatchDay = value;
                OnPropertyChanged();
            }
        }
        public int PrevSeason
        {
            get { return _prevSeason; }
            set
            {
                _prevSeason = value;
                OnPropertyChanged();
            }
        }


        private void Update()
        {
            Dispatcher.Invoke(() =>
            {
                Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, Comparison);
                SplitCollection();

                OnPropertyChanged("CurrentPageToShow");
                OnPropertyChanged("PrevPageToShow");
                OnPropertyChanged("NextPageToShow");




            });




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

            if ((m1.IsLiveBet && m1.LiveBetStatus != eMatchStatus.NotStarted) && (m2.IsLiveBet && m2.LiveBetStatus != eMatchStatus.NotStarted))
            {
                if ((m1.LiveBetStatus != eMatchStatus.NotStarted && m2.LiveBetStatus != eMatchStatus.NotStarted) || m1.LiveBetStatus.Equals(m2.LiveBetStatus))
                {
                    if (sSportName1.Equals(sSportName2))
                    {
                        //if (m1.LivePeriodInfo.Equals(m2.LivePeriodInfo) || sSportDescroptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                        //{
                        if (m1.LiveMatchMinuteEx == m2.LiveMatchMinuteEx)
                        {
                            if (m1.LivePeriodInfo.Equals(m2.LivePeriodInfo))
                            {
                                if (m1.StartDate == m2.StartDate)
                                    return m1.Name.CompareTo(m2.Name);

                                return m1.StartDate.CompareTo(m2.StartDate);
                            }
                            return m2.LivePeriodInfo.CompareTo(m1.LivePeriodInfo);
                        }
                        return m2.LiveMatchMinuteEx.CompareTo(m1.LiveMatchMinuteEx);
                    }
                    return sSportName1.CompareTo(sSportName2);

                }
            }
            if ((!m1.IsLiveBet || m1.LiveBetStatus == eMatchStatus.NotStarted) && (!m2.IsLiveBet || m2.LiveBetStatus == eMatchStatus.NotStarted))
            {
                if (sSportName1.Equals(sSportName2))
                {
                    if (m1.StartDate == m2.StartDate)
                        return m1.Name.CompareTo(m2.Name);

                    return m1.StartDate.CompareTo(m2.StartDate);
                }

                return sSportName1.CompareTo(sSportName2);
            }
            if (!m1.IsLiveBet || m1.LiveBetStatus == eMatchStatus.NotStarted)
                return 1;
            if (!m2.IsLiveBet || m2.LiveBetStatus == eMatchStatus.NotStarted)
                return -1;
            var dd = m1.LiveBetStatus.CompareTo(m2.LiveBetStatus);
            return dd;
        }

        private void SplitCollection()
        {
            CheckTime ct = new CheckTime("SplitCollection() entered.");

            pages.Clear();

            ct.AddEvent("Cleared");

            SyncList<IMatchVw> lMatches = Matches.ToSyncList();
            foreach (var match in lMatches)
            {
                if (!pages.ContainsKey(match.StartDate))
                {
                    pages.Add(match.StartDate, new SortableObservableCollection<IMatchVw>());
                    match.IsHeaderForPreMatch = true;
                    pages[match.StartDate].Add(match);
                }
                else
                {
                    match.IsHeaderForPreMatch = false;
                    pages[match.StartDate].Add(match);
                }
            }

            lMatches.Clear();

            ct.AddEvent("Cycle completed");

            totalPages = pages.Count;
            if (pages.Count == 0)
            {
                SampleMatches.Clear();
                return;
            }

            if (currentPage > pages.Count - 1)
                currentPage = pages.Count - 1;

            if (currentPage == 0)
                currentPage = GetSavedPage();

            if (pages.ElementAt(currentPage).Value.ElementAt(0) != null)
            {
                CurrentSeason = pages.ElementAt(currentPage).Value.ElementAt(0).VirtualSeason.ToString();
                CurrentPageToShow = pages.ElementAt(currentPage).Value.ElementAt(0).VirtualDay.ToString();

            }

            SampleMatches.ApplyChanges(pages.ElementAt(currentPage).Value);


            ct.AddEvent("ApplyChanges completed");
            ct.Info(_logger);
        }

        private SortableObservableCollection<IMatchVw> _sampleMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> SampleMatches
        {
            get { return _sampleMatches; }
            set
            {
                _sampleMatches = value;
                OnPropertyChanged("SampleMatches");
            }
        }


        private bool PrevMatchFilter(IMatchLn match)
        {
            if (match.SourceType != SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                return false;

            var date = match.StartDate.Value.LocalDateTime;

            if (date.AddMinutes(-1) >= DateTime.Now)
                return false;

            return true;
        }
        private bool MatchFilter(IMatchLn match)
        {
            if (match.SourceType != SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                return false;

            if (match.LiveMatchInfo.Status.Value == eMatchStatus.Started)
                return false;

            //if (match.MatchView.BaseBetDomainView.Status != eBetDomainStatus.Visible)
            //    return false;

            if (match.StartDate.Value.LocalDateTime < DateTime.Now)
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

        private string _currentSeason;

        public string CurrentSeason
        {
            get
            {
                return _currentSeason;
            }
            set
            {
                _currentSeason = value;
                ChangeTracker.CurrentSeasonOrRace = _currentSeason;
                OnPropertyChanged("CurrentSeason");
            }
        }

        protected SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }


        private SortableObservableCollection<VFLMatchButton> _vflMatchButtons = new SortableObservableCollection<VFLMatchButton>();
        private int _prevMatchDay;
        private int _prevSeason;

        public SortableObservableCollection<VFLMatchButton> VflMatchButtons
        {
            get { return _vflMatchButtons; }
            set { _vflMatchButtons = value; OnPropertyChanged("VflMatchButtons"); }
        }

        private IMatchVw SelectedMatch
        {
            set { ChangeTracker.CurrentMatch = value; }
        }

        private int GetSavedPage()
        {
            if (ChangeTracker.CurrentMatchOrRaceDay == null || ChangeTracker.CurrentSeasonOrRace == null)
                return 0;
            for (int i = 0; i < pages.Count; i++)
                if (pages.ElementAt(i).Value.ElementAt(0) != null
                    && pages.ElementAt(i).Value.ElementAt(0).VirtualSeason.ToString() == ChangeTracker.CurrentSeasonOrRace
                    && pages.ElementAt(i).Value.ElementAt(0).VirtualDay.ToString() == ChangeTracker.CurrentMatchOrRaceDay)
                {
                    return i;
                }
            return 0;
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.AllowVfl && StationRepository.AllowVhc)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        private void ConvertMatchesToButtons(SortableObservableCollection<IMatchVw> matches, SortableObservableCollection<VFLMatchButton> vflMatchButtons)
        {
            SortableObservableCollection<VFLMatchButton> buttons = new SortableObservableCollection<VFLMatchButton>();
            int channel = 0;
            foreach (MatchVw match in matches.OrderBy(x => x.LineObject.BtrMatchId))
            {
                VFLMatchButton button = new VFLMatchButton(match.HomeCompetitorName, match.AwayCompetitorName, match.LineObject.BtrMatchId, match);
                button.Channel = channel++;
                buttons.Add(button);
            }

            if (vflMatchButtons.Count != 0)
            {
                if (buttons.Count == 0)
                    vflMatchButtons.Clear();

                if (vflMatchButtons[0].MatchVw.VirtualDay != buttons[0].MatchVw.VirtualDay)
                    vflMatchButtons.Clear();
            }
            if (vflMatchButtons.Count == 0)
            {
                foreach (var vflMatchButton in buttons)
                {
                    vflMatchButtons.Add(vflMatchButton);
                }
                vflMatchButtons.ElementAt(0).IsSelected = true;
                OnOpenVflPage(vflMatchButtons.ElementAt(0));

            }


        }
    }
}
