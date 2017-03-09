using System;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
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
    public sealed class OutrightViewModel : BaseViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OutrightViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public OutrightViewModel()
        {
            PlaceBet = new Command<IOddVw>(OnBet);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockTimeFilter);
            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);

            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            ScrollChangedCommand = new Command<double>(ScrollChanged);


            if (MatchInfo == null && !ChangeTracker.CurrentMatch.IsLiveBet)
            {
                MatchInfo = new MatchStatistic(ChangeTracker.CurrentMatch.LineObject.BtrMatchId, ChangeTracker.CurrentMatch.TournamentView.LineObject.GroupTournament.BtrTournamentId);
            }

            if (ChangeTracker.CurrentMatch != null)
            {
                ChangeTracker.CurrentMatch.IsStartUp = true;
            }
            LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);
            ChangeTracker.IsBetdomainViewOpen = true;

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
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_BETDOMAINS).ToString(), null, true, 3);
                MyRegionManager.NavigatBack(RegionNames.ContentRegion);
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

        #endregion

        #region Properties

        #endregion
    }

}