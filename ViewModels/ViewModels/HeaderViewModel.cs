using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using Nbt.Services.Scf.CashIn.Validator;
using Shared;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Modules.Aspects;
using System.Linq;
using WsdlRepository.WsdlServiceReference;
using System.Globalization;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class HeaderViewModel : BaseViewModel
    {
        #region Constructors

        private static ILog Log = LogFactory.CreateLog(typeof(HeaderViewModel));


        public HeaderViewModel()
        {

            //FillComboBoxes();



            Mediator.Register<string>(this, OpenLogin, MsgTag.OpenLogin);
            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);
            Mediator.Register<string>(this, ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
            //Mediator.Register<string>(this, ResetFilters, MsgTag.ResetFilters);
            //Mediator.Register<bool>(this, BlockTimeFilter, MsgTag.BlockTimeFilter);
            Mediator.Register<long>(this, LockStation, MsgTag.LockStation);
            Mediator.Register<bool>(this, ShowResultFilter, MsgTag.ShowResultFilters);
            Mediator.Register<bool>(this, ActivateForwardSelected, MsgTag.ActivateForwardSelected);
            Mediator.Register<bool>(this, ActivateShowSelected, MsgTag.ActivateShowSelected);
            Mediator.Register<bool>(this, NavigateBack, MsgTag.NavigateBack);


            IsSportFilterEnabled = true;
            ShowMenuCommand = new Command(OnShowMenu);
            OpenPlaceBetCommand = new Command(OpenPlaceBetWindow);
            OpenAuthorizationCommand = new Command(OpenAuthorizationWindow);
            ShowCashViewCommand = new Command(ShowCash);
            PrintCreditNoteCommand = new Command(AskPrintCreditNote);
            InsertCreditNoteCommand = new Command(InsertCreditNote);
            RestoreTicketCommand = new Command(RestoreTicket);
            DoSearchCommand = new Command(DoSearch);
            ShowResultsViewCommand = new Command(ShowResultsViewModel);
            ShowCategoriesViewCommand = new Command(ShowSports);
            ShowLiveViewCommand = new Command(ShowLiveView);
            PrevView = new Command(OnPrevViewExecute);
            NextView = new Command(OnNextViewExecute);
            ShowEntertainmentCommand = new Command(ShowEntertainment);
            ExitTestMode = new Command(onExitTestMode);
            _forwardButtonText = TranslationProvider.Translate(MultistringTags.TERMINAL_NAVIGATION_FORWARD) as string;
            _forwardButtonImagePath = "BreadcrumbsCenter.png";
            _forwardButtonLeftImagePath = "BreadcrumbsButtonLeft.png";
            _forwardButtonRightImagePath = "BreadcrumbsRight.png";
            LogoutCommand = new Command(OnLogoutQuestion);
            OpenAnonymousSessionFromHeader();
            ResultsTimeCommand = new Command<string>(OnResultsTimeCommand);
            LiveModeCommand = new Command<string>(OnLiveModeCommand);

            TopTournamentsCommand = new Command(OnTopTournamentsCommand);
            LastMinuteCommand = new Command(OnLastMinuteCommand);
            TodaysOfferCommand = new Command(OnTodaysOfferCommand);
            AllSportsCommand = new Command(OnAllSportsCommand);
            IsTimeFilterEnabled = true;
            InitCashin();

            LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);
        }

        #endregion

        #region Properties

        private bool _todayResultsChecked;
        public bool TodayResultsChecked
        {
            get
            {
                return _todayResultsChecked;
            }
            set
            {
                _todayResultsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool _liveBySportChecked;
        public bool LiveBySportChecked
        {
            get
            {
                return _liveBySportChecked;
            }
            set
            {
                _liveBySportChecked = value;
                OnPropertyChanged();
            }
        }

        private bool _topTournamentsChecked;
        public bool TopTournamentsChecked
        {
            get
            {
                return _topTournamentsChecked;
            }
            set
            {
                _topTournamentsChecked = value;
                OnPropertyChanged();
            }
        }

        private string _forwardButtonText;
        public string ForwardButtonText
        {
            get
            {
                return _forwardButtonText;
            }
            set
            {
                _forwardButtonText = value;
                OnPropertyChanged();
            }
        }

        private string _forwardButtonImagePath;
        public string ForwardButtonImagePath
        {
            get
            {
                return _forwardButtonImagePath;
            }
            set
            {
                _forwardButtonImagePath = value;
                OnPropertyChanged();
            }
        }


        private string _forwardButtonLeftImagePath;
        public string ForwardButtonLeftImagePath
        {
            get
            {
                return _forwardButtonLeftImagePath;
            }
            set
            {
                _forwardButtonLeftImagePath = value;
                OnPropertyChanged();
            }
        }

        private string _forwardButtonRightImagePath;
        public string ForwardButtonRightImagePath
        {
            get
            {
                return _forwardButtonRightImagePath;
            }
            set
            {
                _forwardButtonRightImagePath = value;
                OnPropertyChanged();
            }
        }

        public bool ShowResultFilters
        {
            get { return _showResultFilters; }
            set
            {
                _showResultFilters = value;
                OnPropertyChanged();
            }
        }

        private bool _showResultFilters;
        private bool _isTimeFilterEnabled;

        public bool IsTimeFilterEnabled
        {
            get { return _isTimeFilterEnabled; }
            set
            {
                _isTimeFilterEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isSportFilterEnabled;
        public bool IsSportFilterEnabled
        {
            get { return _isSportFilterEnabled; }
            set
            {
                _isSportFilterEnabled = value;
                OnPropertyChanged();
            }
        }



        private bool _showMenu = true;
        public bool ShowMenu
        {
            get { return _showMenu; }
            private set
            {
                _showMenu = value;
                OnPropertyChanged();
            }
        }

        //protected SortableObservableCollection<MatchResultVw> ResultMatches
        //{
        //    get { return _resultMatches; }
        //    set { _resultMatches = value; }
        //}


        #endregion

        #region Commands

        public Command<string> ResultsTimeCommand { get; private set; }
        public Command<string> LiveModeCommand { get; private set; }
        public Command RestoreTicketCommand { get; private set; }
        public Command ShowCashViewCommand { get; private set; }
        public Command OpenPlaceBetCommand { get; private set; }
        public Command OpenAuthorizationCommand { get; private set; }
        public Command DoSearchCommand { get; private set; }
        public Command PrintCreditNoteCommand { get; private set; }
        public Command InsertCreditNoteCommand { get; private set; }
        public Command ShowResultsViewCommand { get; private set; }
        public Command ShowCategoriesViewCommand { get; private set; }
        public Command ShowLiveViewCommand { get; private set; }
        public Command LogoutCommand { get; private set; }
        public Command PrevView { get; private set; }
        public Command NextView { get; private set; }
        public Command ShowEntertainmentCommand { get; private set; }
        public Command ShowMenuCommand { get; set; }
        public Command ExitTestMode { get; set; }

        public Command LastMinuteCommand { get; set; }
        public Command TodaysOfferCommand { get; set; }
        public Command AllSportsCommand { get; set; }
        public Command TopTournamentsCommand { get; set; }

        #endregion

        #region Methods
        public const int RESTART_WIN_NOW = 1;
        public const int RESTART_WIN_SOON = 2;
        public const int RESTART_APP_NOW = 3;
        public const int RESTART_APP_SOON = 4;




        [AsyncMethod]
        public void OpenAnonymousSessionFromHeader()
        {
            while (!StationRepository.IsReady)
            {
                Thread.Sleep(1000);
            }
            while (ChangeTracker.CurrentUser == null)
            {
                try
                {
                    SessionWS sessid = WsdlRepository.OpenSession(StationRepository.StationNumber, true, string.Empty, string.Empty, false);
                    if (sessid == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    ChangeTracker.CurrentUser = new AnonymousUser(sessid.session_id, sessid.account_id) { Currency = StationRepository.Currency };
                    ChangeTracker.NewTermsAccepted = true;
                }
                catch (Exception ex)
                {

                    Log.Error("Error while trying to open anonymous session:" + ex.Message, ex);
                    Thread.Sleep(1000);
                }
            }
        }

        public void ShowSports()
        {
            ChangeTracker.SelectedTournaments.Clear();
            ActivateForwardSelected(true);
            OnTopTournamentsCommand();
        }

        //[PleaseWaitAspect]
        public void OnTopTournamentsCommand()
        {
            TopTournamentsChecked = true;

            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            ChangeTracker.PreMatchSelectedMode = 0; //default
            MyRegionManager.NavigateUsingViewModel<TopTournamentsViewModel>(RegionNames.ContentRegion);

            if (ChangeTracker.SelectedTournaments.Count > 0)
                Mediator.SendMessage(true, MsgTag.ActivateShowSelected);
        }

        [PleaseWaitAspect]
        public void OnLastMinuteCommand()
        {
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            ChangeTracker.PreMatchSelectedMode = 2;
            MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion);
            Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
        }

        [PleaseWaitAspect]
        public void OnTodaysOfferCommand()
        {
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            ChangeTracker.PreMatchSelectedMode = 1;
            MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion);
            Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
        }

        [PleaseWaitAspect]
        public void OnAllSportsCommand()
        {
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            ChangeTracker.PreMatchSelectedMode = -1;
            MyRegionManager.NavigateUsingViewModel<CategoriesViewModel>(RegionNames.ContentRegion);
        }

        private void OnResultsTimeCommand(string day)
        {
            ChangeTracker.ResultsSelectedDay = Int16.Parse(day, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
            LoadResults();
        }

        private void OnLiveModeCommand(string day)
        {
            ChangeTracker.LiveSelectedMode = Int16.Parse(day, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
            ChangeTracker.LiveSelectedAllSports = true;
            Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
            Mediator.SendMessage(true, MsgTag.Refresh);
            ScrollToVertivalOffset(0);
        }

        private void NavigateBack(bool obj)
        {
            OnPrevViewExecute();
        }

        void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            SyncHashSet<ITipItemVw> shsToRemove = new SyncHashSet<ITipItemVw>();

            var tipitems = new SortableObservableCollection<ITipItemVw>();
            foreach (var ticket1 in TicketHandler.TicketsInBasket.ToSyncList())
            {
                foreach (var tipItemVw in ticket1.TipItems.ToSyncList())
                {
                    tipitems.Add(tipItemVw);
                }
            }

            LineSr.VerifySelectedOdds(tipitems, shsToRemove);

            // remove hidden odds (disabled in web admin) and not caught by LineSr.VerifySelectedOdds()
            foreach (var ticket1 in TicketHandler.TicketsInBasket.ToSyncList())
            {
                foreach (var tipItemVw in ticket1.TipItems.ToSyncList())
                {
                    if (tipItemVw.OddView.Visibility == Visibility.Hidden)
                    {
                        shsToRemove.Add(tipItemVw);
                    }
                }
            }

            if (shsToRemove.Count > 0)
            {
                foreach (ITipItemVw tipItemVw in shsToRemove)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_REMOVE_FROM_BASKET_MATCH, tipItemVw.BetDomain.Match.MatchView.Name), null, false, 5);
                    PlaceBetMethod(tipItemVw, true);
                }

            }

            var TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
            foreach (var ticket1 in TicketsInBasket)
            {
                if (ticket1.TipItems.ToSyncList().Count == 0)
                {
                    var ticket = ticket1;
                    Dispatcher.Invoke(() =>
                    {
                        TicketHandler.TicketsInBasket.Remove(ticket);
                    });

                }
            }


            TicketsInBasket = TicketHandler.TicketsInBasket.ToSyncList();
            if (TicketsInBasket.Count() > 0)
            {
                TicketHandler.CopyValues();
            }
            TicketHandler.UpdateTicket();

            if (ChangeTracker.IsBasketOpen && TicketHandler.Count == 0 && !ChangeTracker.IsLandscapeMode)
            {
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
            }
        }

        [AsyncMethod]
        private void InitCashin()
        {
            if (StationRepository.TurnOffCashInInit)
                return;
            while (!StationSettings.IsCashinOk)
            {
                Thread.Sleep(100);
            }
            StationSettings.SubscribeCashin(AsyncAddMoney);
            StationSettings.SubscribeLimitExceeded(LimitExceeded);
        }

        private void LimitExceeded(object sender, EventArgs eventArgs)
        {
            if (ChangeTracker.CurrentUser is AnonymousUser)
                return; //no limits for AnonymousUsers

            var minLimit = ChangeTracker.CurrentUser.DailyLimit;
            var multistring = MultistringTags.DAILYLIMIT;
            if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
            {
                minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
                multistring = MultistringTags.WEEKLYLIMIT;
            }
            if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
            {
                minLimit = ChangeTracker.CurrentUser.MonthlyLimit;
                multistring = MultistringTags.MONTLYLIMIT;
            }
            var str = TranslationProvider.Translate(multistring, minLimit, StationRepository.Currency);
            ShowError(str,null,false,5);
        }


        [AsyncMethod]
        public void AsyncAddMoney(object sender, CashInEventArgs e)
        {
            AddMoney(sender, e);
        }

        [PleaseWaitAspect]
        public void AddMoney(object sender, CashInEventArgs e)
        {
            ChangeTracker.CurrentUser.DailyLimit -= e.MoneyIn;
            ChangeTracker.CurrentUser.MonthlyLimit -= e.MoneyIn;
            ChangeTracker.CurrentUser.WeeklyLimit -= e.MoneyIn;
            Exception error = null;
            var balance = TransactionQueueHelper.TryDepositMoneyOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(ChangeTracker.CurrentUser), e.MoneyIn, true, ref error, e.IsCoin ? CashAcceptorType.CoinAcceptor : CashAcceptorType.BillValidator);
            AddMoneyToTerminal(e.MoneyIn, error == null ? "" : error.ToString(), balance);
        }



        [AsyncMethod]
        public void OpenPlaceBetWindow()
        {
            if (ChangeTracker.IsBasketOpen)
                return;
            if (ChangeTracker.CurrentUser is OperatorUser)
            {
                return;
            }

            OpenPlaceBetWindowPleaseWait();
        }

        public void OpenPlaceBetWindowPleaseWait()
        {
            if (StationRepository.Active == 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString(), null, false, 3);
                return;
            }
            if (TicketHandler.Count <= 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_SELECT_BET).ToString());
            }
            else
            {
                MyRegionManager.NavigateUsingViewModel<BasketViewModel>(RegionNames.ContentRegion);
            }
        }

        public void InsertCreditNote()
        {
            ChangeTracker.LoadedTicket = "";
            ChangeTracker.LoadedTicketcheckSum = "";
            ChangeTracker.IsUserProfile = false;

            MyRegionManager.NavigateUsingViewModel<TicketCheckerViewModel>(RegionNames.ContentRegion);
            //ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.CREDIT_NOTE;
        }

        private void LoadResults()
        {
            Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
            Mediator.SendMessage<long>(0, MsgTag.RefreshResults);
        }

        private void OnPrevViewExecute()
        {

            if (ChangeTracker.CurrentUser is OperatorUser)
            {
                MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
            }
            else
            {
                MyRegionManager.NavigatBack(RegionNames.ContentRegion);
            }

            var tournamentsViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;
            var topTournamentsViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TopTournamentsViewModel;

            var liveViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as LiveViewModel;

            if ((tournamentsViewModel != null || topTournamentsViewModel != null) && ChangeTracker.SelectedTournaments.Count > 0)
            {
                ActivateShowSelected(true);
            }

            if (liveViewModel != null)
                Mediator.SendMessage(true, MsgTag.Refresh);
        }

        private void OnNextViewExecute()
        {

            var tournamentsViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;
            var topTournamentsViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TopTournamentsViewModel;


            if (ChangeTracker.CurrentUser is OperatorUser)
            {
                MyRegionManager.NavigateForvard(RegionNames.UsermanagementContentRegion);
            }
            if (ForwardButtonText == TranslationProvider.Translate(MultistringTags.TERMINAL_NAVIGATION_FORWARD).ToString())
            {
                MyRegionManager.NavigateForvard(RegionNames.ContentRegion);
            }
            else
            {
                bool outrights = false;
                if (tournamentsViewModel != null)
                {
                    if (tournamentsViewModel.SelectedTournament != null)
                        outrights = tournamentsViewModel.SelectedTournament.IsOutrightGroup;

                    MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, ChangeTracker.SelectedTournaments);
                    Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
                }
                else if (topTournamentsViewModel != null)
                {
                    MyRegionManager.NavigateUsingViewModel<MatchesViewModel>(RegionNames.ContentRegion, ChangeTracker.SelectedTournaments);
                    Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
                }
            }
        }

        private void ActivateShowSelected(bool value)
        {

            if (MyRegionManager.CurrentViewModelType(RegionNames.ContentRegion) != typeof(MatchesViewModel))
            {
                ForwardButtonText = TranslationProvider.Translate(MultistringTags.SHOW_SELECTED).ToString();
                ForwardButtonImagePath = "HeaderButtonGreenCenter.png";
                ForwardButtonLeftImagePath = "HeaderButtonGreenLeft.png";
                ForwardButtonRightImagePath = "HeaderButtonGreenRight.png";
            }
        }

        private void ActivateForwardSelected(bool value)
        {
            ForwardButtonText = TranslationProvider.Translate(MultistringTags.TERMINAL_NAVIGATION_FORWARD) as string;
            ForwardButtonImagePath = "BreadcrumbsButtonCenter.png";
            ForwardButtonLeftImagePath = "BreadcrumbsButtonLeft.png";
            ForwardButtonRightImagePath = "BreadcrumbsRight.png";
        }


        private void OnLogoutQuestion()
        {
            StationRepository.DisableCashIn();
            if (ChangeTracker.CurrentUser.IsLoggedInWithIDCard && StationRepository.IsIdCardEnabled)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.REMOVE_CARD));
                return;
            }

            var text = TranslationProvider.Translate(MultistringTags.TERMINAL_LOGOUT_CONFIRMATION);

            QuestionWindowService.ShowMessage(text, null, null, questionViewModel_YesClick, questionViewModel_NoClick, true, 5);
        }

        void questionViewModel_YesClick(object sender, EventArgs e)
        {
            OnLogout();
        }

        void questionViewModel_NoClick(object sender, EventArgs e)
        {
            var minLimit = ChangeTracker.CurrentUser.DailyLimit;
            if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
            if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.MonthlyLimit;

            StationRepository.SetCashInDefaultState(minLimit);
        }

        [PleaseWaitAspect]
        public void PleaseWaitLogout()
        {
            ClearAndOpenAnonymousSession();
        }




        [AsyncMethod]
        private void OnLogout()
        {
            PleaseWaitLogout();
            ShowMenu = true;
        }

        private void OpenLogin(object obj)
        {
            OpenAuthorizationWindow();
        }


        private void ShowCash()
        {
            // WaitOverlayProvider.ShowWaitOverlay();
            if (StationRepository.Active == 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString(), null, false, 3);
                return;
            }

            ChangeTracker.LoadedTicket = "";
            ChangeTracker.LoadedTicketcheckSum = "";
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            MyRegionManager.NavigateUsingViewModel<TicketCheckerViewModel>(RegionNames.ContentRegion);
            ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.TICKET;
        }



        private void ShowFirstViewAndResetFilters()
        {
            ShowFirstViewAndResetFiltersPleaseWait();
        }

        private void ShowFirstViewAndResetFiltersPleaseWait()
        {
            //ResetFilters("");

            ChangeTracker.CurrentMatchOrRaceDay = null;
            ChangeTracker.CurrentSeasonOrRace = null;
            ChangeTracker.VhcSelectedType = null;
            ChangeTracker.SourceType = null;
            if (StationRepository.IsLiveMatchEnabled)
            {
                if (!(MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) is LiveViewModel))
                    ShowLiveView();
                else if (ChangeTracker.LiveSelectedMode != 0)
                    ShowLiveView();
                else
                {
                    Mediator.SendMessage<bool>(true, MsgTag.ClearSelectedSports);
                    Mediator.SendMessage(true, MsgTag.Refresh);
                    ScrollToVertivalOffset(0);
                }
            }
            else if (StationRepository.IsPrematchEnabled)
            {
                ShowCategories();
            }
            else if (StationRepository.AllowVfl || StationRepository.AllowVhc)
            {
                ShowEntertainment();
            }
            else
            {
                ShowNoContentViewModel();
            }
            Dispatcher.Invoke((Action)(() =>
            {
                var window = (Window)GetActiveWindow();
                if (window != null)
                    window.Focus();
            }));
        }

        public override void OnNavigationCompleted()
        {
            //ResetFilters("");
            ShowMenu = true;

            IBaseViewModel viewModel = null;
            if (StationRepository.IsLiveMatchEnabled)
            {
                LiveBySportChecked = true;
                viewModel = MyRegionManager.NavigateUsingViewModel<LiveViewModel>(RegionNames.ContentRegion);
                Mediator.SendMessage(true, MsgTag.ClearSelectedSports);

            }
            else if (StationRepository.IsPrematchEnabled)
            {
                TopTournamentsChecked = true;
                viewModel = MyRegionManager.NavigateUsingViewModel<TopTournamentsViewModel>(RegionNames.ContentRegion);
            }
            else if (StationRepository.AllowVfl || StationRepository.AllowVhc)
            {
                viewModel = MyRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
            }
            else
            {
                viewModel = MyRegionManager.NavigateUsingViewModel<NoContentViewModel>(RegionNames.ContentRegion);
            }
            if (viewModel != null)
                HidePleaseWait = viewModel.IsReady;

            base.OnNavigationCompleted();
        }





        private void ShowFirstViewAndResetFilters(string obj)
        {
            ShowFirstViewAndResetFilters();
        }


        private void ShowCategories()
        {
            //WaitOverlayProvider.ShowWaitOverlay();

            ShowSports();

            //TicketHandler.UpdateTicket();
        }

        private void ShowLiveView()
        {
            ChangeTracker.SelectedDescriptorsLive.Clear();
            LiveBySportChecked = true;
            //WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            ChangeTracker.LiveSelectedMode = 0;
            MyRegionManager.NavigateUsingViewModel<LiveViewModel>(RegionNames.ContentRegion);
            Mediator.SendMessage(true, MsgTag.ClearSelectedSports);
            //Mediator.SendMessage(true, MsgTag.Refresh);
            //TicketHandler.UpdateTicket();
        }

        private void ShowResultsViewModel()
        {
            //WaitOverlayProvider.ShowWaitOverlay();
            Log.Debug("show results Please wait");
            ChangeTracker.SelectedDescriptors.Clear();
            TodayResultsChecked = true;
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            MyRegionManager.NavigateUsingViewModel<MatchResultsViewModel>(RegionNames.ContentRegion);

            LoadResults();

        }

        private void ShowNoContentViewModel()
        {
            // WaitOverlayProvider.ShowWaitOverlay();
            Log.Debug("show no content Please wait");

            //Mediator.SendMessage<bool>(true, MsgTag.CloseBasketView);
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            MyRegionManager.NavigateUsingViewModel<NoContentViewModel>(RegionNames.ContentRegion);

        }


        private void ShowEntertainment()
        {
            ChangeTracker.CurrentSeasonOrRace = null;
            ChangeTracker.CurrentMatchOrRaceDay = null;
            ChangeTracker.VhcSelectedType = null;
            ChangeTracker.SourceType = null;
            WaitOverlayProvider.ShowWaitOverlay();
            Log.Debug("show entertaiment Please wait");



            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            MyRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
        }

        private void onExitTestMode()
        {
            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFY_EMPTY_BOX).ToString(), onExitOkClick);
        }

        void onExitOkClick(object sender, EventArgs e)
        {
            Mediator.SendMessage<long>(123, MsgTag.RestartApplication);
        }

        private void OpenAuthorizationWindow()
        {
            if (ChangeTracker.IsUserProfile) return;

            // WaitOverlayProvider.ShowWaitOverlay();
            if (ChangeTracker.CurrentUser is LoggedInUser)
            {
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);

                ChangeTracker.IsUserProfile = true;

                ChangeTracker.HeaderVisible = false;
                ChangeTracker.FooterVisible = true;
                ChangeTracker.FooterArrowsVisible = false;

                ChangeTracker.SelectedLive = false;
                ChangeTracker.SelectedResults = false;
                ChangeTracker.SelectedTicket = false;
                ChangeTracker.SelectedSports = false;
                ChangeTracker.SelectedVirtualSports = false;

                if (MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion).GetType() != typeof(UserProfileViewModel))
                    MyRegionManager.NavigateUsingViewModel<UserProfileViewModel>(RegionNames.ContentRegion);

            }
            else
            {

                Dispatcher.Invoke((Action)(() =>
                {
                    if (ChangeTracker.LoginWindow != null && ChangeTracker.LoginWindow.IsActive)
                        return;

                    ChangeTracker.LoginWindow = MyRegionManager.FindWindowByViewModel<AuthViewModel>();

                    ChangeTracker.LoginWindow.Show();

                }));

            }
        }


        private decimal amount;
        public void AskPrintCreditNote()
        {

            amount = ChangeTracker.CurrentUser.Cashpool;

            var text = TranslationProvider.Translate(MultistringTags.CASHOUT_TOCREDITNOTE, string.Format("{0:f2}", amount), Currency);
            var yesButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_OK) as string;
            var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, PrintCreditNoteYes, null);
        }

        private void PrintCreditNoteYes(object sender, EventArgs e)
        {
            PrintCreditNote();
        }

        [WsdlServiceSyncAspect]
        private void PrintCreditNote()
        {
            foreach (var ticket1 in TicketHandler.TicketsInBasket.ToSyncList())
            {
                ticket1.Stake = 0;
            }
            TicketHandler.UpdateTicket();
            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;

            if ((ChangeTracker.CurrentUser.Cashpool > 0) && ChangeTracker.CurrentUser is AnonymousUser)
            {
                SaveCreditNote(amount);
            }

        }



        private void DoSearch()
        {
            // WaitOverlayProvider.ShowWaitOverlay();

            ChangeTracker.SearchString = "";
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);

            ChangeTracker.SearchMatches.Clear();
            ChangeTracker.SearchTournaments = new ObservableCollection<ComboBoxItem>();
            ChangeTracker.SearchSports = new ObservableCollection<ComboBoxItem>();

            MyRegionManager.NavigateUsingViewModel<SearchViewModel>(RegionNames.ContentRegion);
        }

        private void ShowResultFilter(bool show)
        {
            ShowResultFilters = show;
        }

        private void LanguageChosen(string lang)
        {
            //ChangeTracker.SportFilters[0].Name = TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string;

            var tournamentsViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;

            if (tournamentsViewModel != null && ChangeTracker.SelectedTournaments.Count != 0)
            {
                ForwardButtonText = TranslationProvider.Translate(MultistringTags.SHOW_SELECTED) as string;
            }
            else
            {
                ForwardButtonText = TranslationProvider.Translate(MultistringTags.TERMINAL_NAVIGATION_FORWARD) as string;

            }

            if (StationRepository.Active == 0) //locked
            {
                LockText = TranslationProvider.Translate(MultistringTags.TERMINAL_STATION_LOCKED).ToString();
            }
            else if (StationRepository.Active == 4) //cash locked
            {
                LockText = TranslationProvider.Translate(MultistringTags.TERMINAL_CASH_LOCKED).ToString();
            }

        }

        private DateTime currentTime = DateTime.Now;

        [AsyncMethod]
        private void RestoreTicket()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            RestoreTicketPleaseWait();
        }
        private void RestoreTicketPleaseWait()
        {
            if (StationRepository.Active == 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString(), null, false, 3);
                return;
            }

            ChangeTracker.LoadedTicket = "";
            ChangeTracker.LoadedTicketcheckSum = "";

            MyRegionManager.NavigateUsingViewModel<TicketCheckerViewModel>(RegionNames.ContentRegion);
            ChangeTracker.LoadedTicketType = BarCodeConverter.BarcodeType.STORED_TICKET;
        }

        private bool _isStationLocked = false;
        public bool IsStationLocked
        {
            get
            {
                return _isStationLocked;
            }
            set
            {
                if (_isStationLocked != value)
                {
                    _isStationLocked = value;
                    OnPropertyChanged("IsStationLocked");
                }
            }
        }

        private string _lockText = "";
        public string LockText
        {
            get
            {
                return _lockText;
            }
            set
            {
                if (_lockText != value)
                {
                    _lockText = value;
                    OnPropertyChanged("LockText");
                }
            }
        }

        private void LockStation(long obj)
        {
            if (obj == 2 && !(ChangeTracker.CurrentUser is OperatorUser))
            {
                ShowFirstViewAndResetFilters();
                if (StationRepository.IsReady)
                    OnLogout();
            }

            if (StationRepository.Active == 0) //locked
            {
                LockText = TranslationProvider.Translate(MultistringTags.TERMINAL_STATION_LOCKED).ToString();
                IsStationLocked = true;
            }
            else if (StationRepository.Active == 4) //cash locked
            {
                LockText = TranslationProvider.Translate(MultistringTags.TERMINAL_CASH_LOCKED).ToString();
                IsStationLocked = true;
            }
            else
            {
                LockText = "";
                IsStationLocked = false;
            }

        }

        private void OnShowMenu()
        {
            ShowMenu = !ShowMenu;
            Mediator.SendMessage<bool>(ShowMenu, MsgTag.ShowMenu);
        }

        #endregion
    }
}