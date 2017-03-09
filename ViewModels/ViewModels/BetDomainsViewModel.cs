using System;
using System.Timers;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.CommonObjects;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// BetDomains view model.
    /// </summary>
    [ServiceAspect]
    public sealed class BetDomainsViewModel : BaseViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BetDomainsViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>

        Timer timer = new Timer(1000);
        public BetDomainsViewModel()
        {
            PlaceBet = new Command<IOddVw>(OnBet);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockTimeFilter);
            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            ScrollChangedCommand = new Command<double>(ScrollChanged);

            if (StationRepository.IsStatisticsEnabled && MatchInfo == null && !ChangeTracker.CurrentMatch.IsLiveBet)
            {
                MatchInfo = new MatchStatistic(ChangeTracker.CurrentMatch.LineObject.BtrMatchId, ChangeTracker.CurrentMatch.TournamentView.LineObject.GroupTournament.BtrTournamentId);
            }

            if (ChangeTracker.CurrentMatch != null)
            {
                ChangeTracker.CurrentMatch.IsStartUp = true;
            }
            LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);
            timer.Elapsed += timer_Tick;
            timer.Start();
            ChangeTracker.IsBetdomainViewOpen = true;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!ChangeTracker.CurrentMatch.IsLiveBet && ChangeTracker.CurrentMatch.ExpiryDate < DateTime.Now)
            {
                timer.Stop();
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_BETDOMAINS) as string, null, true, 3);
                MyRegionManager.NavigatBack(RegionNames.ContentRegion);
            }
        }

        #endregion

        #region Properties

        //private new static readonly ILog Log = LogManager.GetLogger(typeof(BetDomainsViewModel));

        public string BigEndDate
        {
            get
            {
                DateTime date = (ChangeTracker.CurrentMatch != null) ? ChangeTracker.CurrentMatch.ExpiryDate : DateTime.MinValue;
                if (date.Date.CompareTo(DateTime.Today) == 0)
                {
                    string text = date.Hour.ToString();
                    return text;
                }
                else
                {
                    string text = date.Date.Day.ToString() + "  " + date.Hour.ToString();
                    return text;
                }
            }
        }



        private MatchStatistic _matchInfo;

        public MatchStatistic MatchInfo
        {
            get { return _matchInfo; }
            set
            { _matchInfo = value; }
        }

        public string TournamentName
        {
            get
            {
                return ChangeTracker.CurrentMatch.TournamentNameToShow;
            }
        }

        #endregion

        #region Commands
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }

        #endregion

        #region Methods

        public override void Close()
        {
            if (ChangeTracker.CurrentMatch != null)
            {
                ChangeTracker.CurrentMatch.IsStartUp = true;

                if (ChangeTracker.CurrentMatch.GoalsTimer != null)
                    ChangeTracker.CurrentMatch.GoalsTimer.Stop();
            }
            LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
            timer.Stop();
            timer.Elapsed -= timer_Tick;
            ChangeTracker.IsBetdomainViewOpen = false;
            base.Close();

        }

        public override void OnNavigationCompleted()
        {
            //if (ChangeTracker.CurrentMatch != null)
            //{
            //    ChangeTracker.CurrentMatch.IsStartUp = true;
            //}

            base.OnNavigationCompleted();
        }

        private void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (ChangeTracker.CurrentMatch.VisibleBetDomainCount == 0)
            {
                LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
                Mediator.SendMessage(true, MsgTag.NavigateBack);
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_BETDOMAINS).ToString(), null, true, 3);

            }
        }




        private void LanguageChosen(string lang)
        {
            if (!ChangeTracker.CurrentMatch.IsLiveBet)
            {
                MatchInfo = new MatchStatistic(ChangeTracker.CurrentMatch.LineObject.BtrMatchId, ChangeTracker.CurrentMatch.TournamentView.LineObject.GroupTournament.BtrTournamentId);
                OnPropertyChanged("MatchInfo");
            }
        }

        private void HeaderShowFirstView(string obj)
        {
            var source = ChangeTracker.CurrentMatch.MatchSource;
            if (source == null) return;
            if (source == eServerSourceType.BtrPre && !StationRepository.IsPrematchEnabled
                || source == eServerSourceType.BtrLive && !StationRepository.IsLiveMatchEnabled
                || source == eServerSourceType.BtrVfl && !StationRepository.AllowVfl
                || source == eServerSourceType.BtrVhc && !StationRepository.AllowVhc)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion

    }

}