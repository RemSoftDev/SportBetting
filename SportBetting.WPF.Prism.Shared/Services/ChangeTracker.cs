using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using IocContainer;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Annotations;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using SportBetting.WPF.Prism.OldCode;
using System.Collections.Generic;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using BalanceCheckpoint = SportBetting.WPF.Prism.Shared.Models.BalanceCheckpoint;
using System.Configuration;

namespace SportBetting.WPF.Prism.Shared.Services
{


    public class ChangeTracker : IChangeTracker, INotifyPropertyChanged
    {
        private const string TERMS_AND_COND_VERSION = "1";

        private object _itemsLock = new object();
        private object _itemsLock2 = new object();
        private object _itemsLock3 = new object();
        private object _itemsLock4 = new object();
        private object _itemsLock5 = new object();
        private object _itemsLock6 = new object();
        public ChangeTracker()
        {
            BindingOperations.EnableCollectionSynchronization(_matches, _itemsLock);
            BindingOperations.EnableCollectionSynchronization(_allResults, _itemsLock2);
            BindingOperations.EnableCollectionSynchronization(_searchTournaments, _itemsLock3);
            BindingOperations.EnableCollectionSynchronization(_searchSports, _itemsLock4);
            BindingOperations.EnableCollectionSynchronization(_sportFilters, _itemsLock5);
            BindingOperations.EnableCollectionSynchronization(_timeFilters, _itemsLock6);

        }

        public bool NewTermsAccepted { get; set; }

        public decimal TruncateDecimal(decimal valueToTruncate)
        {
            decimal multiplied = valueToTruncate * 100;
            decimal tempBonus = decimal.Truncate(multiplied);
            tempBonus = tempBonus / 100;

            return tempBonus;
        }

        public string TermsAndConditionsVersion
        {
            get { 
                string version = "1.0";
                if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["terms_version"]))
                {
                    version = ConfigurationManager.AppSettings["terms_version"].Trim();
                }

                return version;
            }
        }

        public bool NeedVerticalRegistrationFields { get; set; }

        public bool ShowVFLVirtualButtons { get { return this.IsLandscapeMode || this.Is34Mode; } }

        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get { return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>()); }
        }
        public IErrorWindowService ErrorWindowService
        {
            get { return IoCContainer.Kernel.Get<IErrorWindowService>(); }
        }
        private ITranslationProvider _translationProvider;
        public ITranslationProvider TranslationProvider
        {
            get
            {
                return _translationProvider ?? (_translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>());
            }
        }
        public IQuestionWindowService QuestionWindowService
        {
            get { return IoCContainer.Kernel.Get<IQuestionWindowService>(); }
        }
        public bool IsCashAcceporDatasetValid
        {
            get { return StationRepository.IsCashDatasetValid(); }
        }

        private List<string> _selectedDescriptorsLive = new List<string>();
        public List<string> SelectedDescriptorsLive
        {
            get
            {
                return _selectedDescriptorsLive;
            }
            set
            {
                _selectedDescriptorsLive = value;
            }
        }

        private List<string> _selectedDescriptorsPrematch = new List<string>();
        public List<string> SelectedDescriptorsPreMatch
        {
            get
            {
                return _selectedDescriptorsPrematch;
            }
            set
            {
                _selectedDescriptorsPrematch = value;
            }
        }

        public bool TestInputActive { get; set; }
        public IList<string> SelectedDescriptors
        {
            get { return _selectedDescriptors; }
            set { _selectedDescriptors = value; }
        }

        public bool RedirectToTicketDetails
        {
            get { return _redirectToTicketDetails; }
            set { _redirectToTicketDetails = value; OnPropertyChanged(); }
        }
        public bool SelectedSports
        {
            get { return _selectedSports; }
            set
            {
                if (value.Equals(_selectedSports))
                    return;

                _selectedSports = value;

                OnPropertyChanged();
            }
        }

        public bool SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {

                if (value.Equals(_selectedTicket))
                    return;
                _selectedTicket = value;


                OnPropertyChanged();
            }
        }

        public bool SelectedLive
        {
            get { return _selectedLive; }
            set
            {
                if (value.Equals(_selectedLive))
                    return;
                _selectedLive = value;

                OnPropertyChanged();
            }
        }

        private bool _isSearchOpen = false;
        public bool IsSearchOpen
        {
            get { return _isSearchOpen; }
            set
            {
                if (value.Equals(_isSearchOpen))
                    return;
                _isSearchOpen = value;

                OnPropertyChanged();
            }
        }

        public bool SelectedResults
        {
            get { return _selectedResults; }
            set
            {
                if (value.Equals(_selectedResults))
                    return;

                _selectedResults = value;

                OnPropertyChanged();
            }
        }

        public MultistringTag LastNotificationTag { get; set; }
        public MultistringTag AdminTitle2
        {
            get { return _adminTitle2; }
            set
            {
                _adminTitle2 = value;
                OnPropertyChanged();
            }
        }

        public MultistringTag AdminTitle1
        {
            get { return _adminTitle1; }
            set
            {
                _adminTitle1 = value;
                OnPropertyChanged();
            }
        }

        public bool LockTournamentAgainstAll { get; set; }

        public long TournamentLockId { get; set; }
        public long CategoryLockId { get; set; }


        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                CashInDefaultState();
                OnPropertyChanged();
                OnPropertyChanged("SearchVisibility");
            }
        }

        public Visibility SearchVisibility
        {
            get
            {
                if (CurrentUser is SportBetting.WPF.Prism.Models.OperatorUser)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        [AsyncMethod]
        private void CashInDefaultState()
        {
            var minLimit = _currentUser.DailyLimit;
            if (_currentUser.WeeklyLimit < minLimit)
                minLimit = _currentUser.WeeklyLimit;
            if (_currentUser.MonthlyLimit < minLimit)
                minLimit = _currentUser.MonthlyLimit;

            StationRepository.SetCashInDefaultState(minLimit);
        }

        public Window MainWindow { get; set; }


        public DateTime MouseClickLastTime
        {
            get { return _mouseClickLastTime; }
            set { _mouseClickLastTime = value; }
        }


        public string ErrorLabel
        {
            get { return _errorLabel; }
            set
            {
                _errorLabel = value;
                OnPropertyChanged();
            }
        }


        public DateTime? BirthDate { get; set; }
        public DateTime? minDate { get; set; }
        public DateTime? maxDate { get; set; }
        public DateTime? initDate { get; set; }
        public DateTime LastCashoutDate { get; set; }

        public bool IsForecastOpen { get; set; }




        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    OnPropertyChanged();
                }
            }
        }
        public ComboBoxItemStringId SelectedSportFilter { get; set; }

        public long SelectedSportId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public BarCodeConverter.BarcodeType LoadedTicketType
        {
            get { return _loadedTicketType; }
            set
            {
                _loadedTicketType = value;
                OnPropertyChanged();
            }
        }

        private string _loadedTicket = "";

        public string LoadedTicket
        {
            get { return _loadedTicket; }
            set
            {
                _loadedTicket = value;
                OnPropertyChanged();
            }
        }

        private string _loadedTicketcheckSum = "";
        private BarCodeConverter.BarcodeType _loadedTicketType;
        private ObservableCollection<FoundUser> _foundUsers = new ObservableCollection<FoundUser>();
        private string _cardNumber;

        public string LoadedTicketcheckSum
        {
            get { return _loadedTicketcheckSum; }
            set
            {
                _loadedTicketcheckSum = value;
                OnPropertyChanged();
            }
        }

        public TicketWS CurrentTicket { get; set; }

        public string TicketNumber { get; set; }


        public DateTime StartDateAccounting { get; set; }
        public DateTime EndDateAccounting { get; set; }
        public bool CashInAccounting { get; set; }
        public bool CashOutAccounting { get; set; }
        public bool FromCheckPointsAccounting { get; set; }
        public DateTime CalendarStartDateAccounting
        {
            get { return _calendarStartDateAccounting; }
            set
            {
                _calendarStartDateAccounting = value;
                OnPropertyChanged();
            }
        }

        public DateTime CalendarEndDateAccounting
        {
            get { return _calendarEndDateAccounting; }
            set
            {
                _calendarEndDateAccounting = value;
                OnPropertyChanged();
            }
        }

        public Window UserProfileWindow { get; set; }
        public FoundUser EditableUser
        {
            get { return _editableUser; }
            set { _editableUser = value; }
        }

        public ObservableCollection<FoundUser> FoundUsers
        {
            get { return _foundUsers; }
            set
            {
                _foundUsers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FoundOperator> FoundOperators
        {
            get { return _foundOperators; }
            set
            {
                _foundOperators = value;
                OnPropertyChanged();
            }
        }

        public string CardNumber
        {
            get { return _cardNumber; }
            set
            {
                _cardNumber = value;
            }
        }



        private string _errorLabel;
        private TicketWS _ticket;
        private ObservableCollection<FoundOperator> _foundOperators;
        private FoundOperator _foundOperator;




        public bool MaxOddFactorExceeded { get; set; }
        public ComboBoxItem SelectedTimeFilter { get; set; }
        public bool VerifivationCancelled { get; set; }
        public bool VerificationRestart { get; set; }
        public TicketWS Ticket
        {
            get { return _ticket; }
            set { _ticket = value; }
        }
        public int RestartPending { get; set; }
        public ObservableCollection<BalanceCheckpoint> Balance { get; set; }
        public FoundOperator FoundOperator
        {
            get { return _foundOperator; }
            set
            {
                _foundOperator = value;
                OnPropertyChanged("FoundOperator");
            }
        }

        public bool IsBindingCard { get; set; }
        public bool PrinterErrorChecked { get; set; }
        public SyncList<LiveWindowEntry> LiveMonitors { get; set; }
        public double HeaderHeight { get; set; }
        public double MatchHeight { get; set; }
        public string ResultsSelectedSport { get; set; }
        public ComboBoxItem ResultsSelectedTime { get; set; }
        public int ResultsSelectedDay { get; set; }
        public int LiveSelectedMode { get; set; }
        public bool LiveSelectedAllSports { get; set; }
        public int PreMatchSelectedMode { get; set; }

        public ObservableCollection<TicketView> Tickets
        {
            get { return _tickets; }
            set { _tickets = value; }
        }

        public ComboBoxItem SelectedType { get; set; }
        public int SelectedTycketType { get; set; }
        public long AllPages { get; set; }
        public long CurrentPageIndex { get; set; }
        public bool AutoLogoutActive { get; set; }
        public bool DoLogout { get; set; }



        public Window LoginWindow { get; set; }
        public SortableObservableCollection<IMatchVw> AllResults
        {
            get { return _allResults; }
            set { _allResults = value; }
        }

        public ObservableCollection<ComboBoxItem> SearchSports
        {
            get { return _searchSports; }
            set { _searchSports = value; }
        }

        public ObservableCollection<ComboBoxItem> SearchTournaments
        {
            get { return _searchTournaments; }
            set { _searchTournaments = value; }
        }

        private HashSet<string> _SelectedTournaments = new HashSet<string>();
        private bool _errorWindowActive;
        private ObservableCollection<ComboBoxItemStringId> _sportFilters = new ObservableCollection<ComboBoxItemStringId>();
        private bool _isBasketOpen;
        private bool _isUserProfile;
        private bool _redirectToTicketDetails;
        private bool _selectedSports;
        private bool _selectedTicket;
        private bool _selectedLive;
        private bool _selectedResults;

        public HashSet<string> SelectedTournaments
        {
            get { return _SelectedTournaments; }
            set { _SelectedTournaments = value; }
        }

        public double HistoryrowHeight { get; set; }
        public double HistorygridHeight { get; set; }

        public bool ErrorWindowActive
        {
            get { return _errorWindowActive; }
            set { _errorWindowActive = value; }
        }


        public string PaymentFlowOperationType { get; set; }
        public ComboBoxItem SearchSelectedTournament { get; set; }
        public ComboBoxItem SearchSelectedSport { get; set; }
        public int UserPinSetting { get; set; }

        public bool LockCashAcceptors { get; set; }

        public bool OperatorPaymentViewOpen { get; set; }

        public int BasketWheelPosition { get; set; }
        public bool BetDomainViewFromBasket { get; set; }
        public bool BasketItemsChanged { get; set; }



        public ObservableCollection<ComboBoxItemStringId> SportFilters
        {
            get { return _sportFilters; }
            set
            {
                if (Equals(value, _sportFilters))
                    return;
                _sportFilters = value;
                OnPropertyChanged();
            }
        }


        private bool _selectedVirtualSports;
        public bool SelectedVirtualSports
        {
            get
            {
                return _selectedVirtualSports;
            }
            set
            {
                if (value.Equals(_selectedVirtualSports))
                    return;

                _selectedVirtualSports = value;

                OnPropertyChanged();
            }
        }

        private bool _isUserManagementWindowVisible = false;

        public bool IsUserManagementWindowVisible
        {
            get { return _isUserManagementWindowVisible; }
            set
            {
                _isUserManagementWindowVisible = value;
                OnPropertyChanged();
            }
        }
        public bool TicketChecked
        {
            get { return _ticketChecked; }
            set
            {
                _ticketChecked = value;
                OnPropertyChanged();
            }
        }

        private bool _userChecked;
        public bool UserChecked
        {
            get { return _userChecked; }
            set
            {
                _userChecked = value;
                OnPropertyChanged();
            }
        }


        public bool CashHistoryChecked
        {
            get { return _cashHistoryChecked; }
            set
            {
                _cashHistoryChecked = value;
                OnPropertyChanged();
            }
        }

        public bool SearchOperatorUsersChecked
        {
            get { return _searchOperatorUsersChecked; }
            set
            {
                _searchOperatorUsersChecked = value;
                OnPropertyChanged();
            }
        }

        public bool RestartChecked
        {
            get { return _restartChecked; }
            set
            {
                _restartChecked = value;
                OnPropertyChanged();
            }
        }

        public bool SystemInfoNetworkChecked
        {
            get { return _systemInfoNetworkChecked; }
            set
            {
                _systemInfoNetworkChecked = value;
                OnPropertyChanged();
            }
        }

        public bool SystemInfoMonitorsChecked
        {
            get { return _systemInfoMonitorsChecked; }
            set
            {
                _systemInfoMonitorsChecked = value;
                OnPropertyChanged();
            }
        }

        public bool SystemInfoChecked
        {
            get { return _systemInfoChecked; }
            set
            {
                _systemInfoChecked = value;
                OnPropertyChanged();
            }
        }

        public bool VerifyStationChecked
        {
            get { return _verifyStationChecked; }
            set
            {
                _verifyStationChecked = value;
                OnPropertyChanged();
            }
        }

        public bool RegisterUserChecked
        {
            get { return _registerUserChecked; }
            set
            {
                _registerUserChecked = value;
                OnPropertyChanged();
            }
        }

        public bool SearchUsersChecked
        {
            get { return _searchUsersChecked; }
            set
            {
                _searchUsersChecked = value;
                OnPropertyChanged();
            }
        }

        public bool ShopPaymentsChecked
        {
            get { return _shopPaymentsChecked; }
            set
            {
                _shopPaymentsChecked = value;
                OnPropertyChanged();
            }
        }

        public bool ProfitAccountingChecked
        {
            get { return _profitAccountingChecked; }
            set
            {
                _profitAccountingChecked = value;
                OnPropertyChanged();
            }
        }

        public bool CashOperationsChecked
        {
            get { return _cashOperationsChecked; }
            set
            {
                _cashOperationsChecked = value;
                OnPropertyChanged();
            }
        }

        public bool OperatorsShiftsReporChecked
        {
            get { return _operatorsShiftsReporChecked; }
            set
            {
                _operatorsShiftsReporChecked = value;
                OnPropertyChanged();
            }
        }

        public bool OpenShiftsChecked
        {
            get { return _openShiftsChecked; }
            set
            {
                _openShiftsChecked = value;
                OnPropertyChanged();
            }
        }

        public bool RegisterOperatorChecked
        {
            get { return _registerOperatorChecked; }
            set
            {
                _registerOperatorChecked = value;
                OnPropertyChanged();
            }
        }

        public bool CardAndPinChecked
        {
            get { return _cardAndPinChecked; }
            set { _cardAndPinChecked = value; OnPropertyChanged(); }
        }


        public bool BalanceOperationsChecked
        {
            get { return _balanceOperationsChecked; }
            set
            {
                _balanceOperationsChecked = value;
                OnPropertyChanged();
            }
        }

        public bool TerminalAccountingChecked
        {
            get { return _terminalAccountingChecked; }
            set
            {
                _terminalAccountingChecked = value;
                OnPropertyChanged();
            }
        }

        public bool AdministrationHiddenChecked
        {
            get { return _administrationHiddenChecked; }
            set
            {
                _administrationHiddenChecked = value;
                OnPropertyChanged();
            }
        }

        private bool _emptyBoxChecked = false;
        public bool EmptyBoxChecked
        {
            get { return _emptyBoxChecked; }
            set
            {
                _emptyBoxChecked = value;
                OnPropertyChanged();
            }
        }

        private bool _printTestPageChecked = false;
        public bool PrintTestPageChecked
        {
            get { return _printTestPageChecked; }
            set
            {
                _printTestPageChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsBasketOpen
        {
            get { return _isBasketOpen; }
            set
            {
                if (value.Equals(_isBasketOpen))
                    return;
                _isBasketOpen = value;
                OnPropertyChanged();
            }
        }

        private bool _isBetdomainViewOpen;
        public bool IsBetdomainViewOpen
        {
            get { return _isBetdomainViewOpen; }
            set
            {
                if (value.Equals(_isBetdomainViewOpen))
                    return;
                _isBetdomainViewOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserProfile
        {
            get { return _isUserProfile; }
            set
            {
                if (value.Equals(_isUserProfile))
                    return;
                _isUserProfile = value;
                OnPropertyChanged();
            }
        }

        private DateTime _mouseClickLastTime = DateTime.Now;
        private FoundUser _editableUser;
        private MultistringTag _adminTitle2;
        private MultistringTag _adminTitle1;
        private bool _ticketChecked;
        private bool _cashHistoryChecked;
        private bool _searchOperatorUsersChecked;
        private bool _restartChecked;
        private bool _systemInfoNetworkChecked;
        private bool _systemInfoMonitorsChecked;
        private bool _systemInfoChecked;
        private bool _verifyStationChecked;
        private bool _registerUserChecked;
        private bool _searchUsersChecked;
        private bool _shopPaymentsChecked;
        private bool _profitAccountingChecked;
        private bool _cashOperationsChecked;
        private bool _operatorsShiftsReporChecked;
        private bool _openShiftsChecked;
        private bool _registerOperatorChecked;
        private bool _balanceOperationsChecked;
        private bool _terminalAccountingChecked;
        private bool _administrationHiddenChecked;
        private bool _cardAndPinChecked;

        private decimal _terminalBalance;
        public decimal TerminalBalance
        {
            get
            {
                return _terminalBalance;
            }
            set
            {
                _terminalBalance = value;
                OnPropertyChanged();
            }
        }

        private decimal _locationBalance;
        public decimal LocationBalance
        {
            get
            {
                return _locationBalance;
            }
            set
            {
                _locationBalance = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalLocationPaymentBalance;
        public decimal TotalLocationPaymentBalance
        {
            get
            {
                return _totalLocationPaymentBalance;
            }
            set
            {
                _totalLocationPaymentBalance = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalStationCash;
        public decimal TotalStationCash
        {
            get
            {
                return _totalStationCash;
            }
            set
            {
                _totalStationCash = value;
                OnPropertyChanged();
            }
        }

        private decimal _locationCashPosition;
        public decimal LocationCashPosition
        {
            get
            {
                return _locationCashPosition;
            }
            set
            {
                _locationCashPosition = value;
                OnPropertyChanged();
            }
        }

        private DateTime _profitReportStartDate;
        public DateTime ProfitReportStartDate
        {
            get
            {
                return _profitReportStartDate;
            }
            set
            {
                _profitReportStartDate = value;
                OnPropertyChanged();
            }
        }
        private DateTime _profitReportEndDate;
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        private SortableObservableCollection<IMatchVw> _allResults = new SortableObservableCollection<IMatchVw>();
        private ObservableCollection<ComboBoxItem> _searchSports = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> _searchTournaments = new ObservableCollection<ComboBoxItem>();

        public DateTime ProfitReportEndDate
        {
            get
            {
                return _profitReportEndDate;
            }
            set
            {
                _profitReportEndDate = value;
                OnPropertyChanged();
            }
        }
        public bool CreateFromLastCheckpoint { get; set; }
        public SortableObservableCollection<IMatchVw> SearchMatches
        {
            get { return _matches; }
            set { _matches = value; }
        }

        public List<criteria> SearchRequest { get; set; }

        private int _itemsPerPage = 0;
        private ObservableCollection<TicketView> _tickets = new ObservableCollection<TicketView>();
        private ObservableCollection<ComboBoxItem> _timeFilters = new ObservableCollection<ComboBoxItem>();

        public int ItemsAmmountPerPage
        {
            get { return _itemsPerPage; }
            set { _itemsPerPage = value; OnPropertyChanged(); }
        }
        public bool BindingCardCancelled { get; set; }
        public IMatchVw CurrentMatch { get; set; }
        public int TicketsStartPage { get; set; }
        public bool OperatorSearchUserViewOpen { get; set; }
        private int _videoTimePeriodMin = 45;
        public int VideoTimePeriodMin { get { return _videoTimePeriodMin; } set { _videoTimePeriodMin = value; } }
        private int _videoTimePeriodMax = 50;
        public int VideoTimePeriodMax { get { return _videoTimePeriodMax; } set { _videoTimePeriodMax = value; } }

        public int _videoBlockedPeriod = 30;
        public int VideoBlockedPeriod { get { return _videoBlockedPeriod; } set { _videoBlockedPeriod = value; } }

        public int _videoWarningBefore = 5;
        public int VideoWarningBefore { get { return _videoWarningBefore; } set { _videoWarningBefore = value; } }
        public ObservableCollection<ComboBoxItem> TimeFilters
        {
            get { return _timeFilters; }
            set { _timeFilters = value; }
        }

        private bool _headerVisible = true;
        public bool HeaderVisible
        {
            get { return _headerVisible; }
            set
            {
                _headerVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _footerVisible = true;
        public bool FooterVisible
        {
            get { return _footerVisible; }
            set
            {
                _footerVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _footerArrowsVisible = true;
        public bool FooterArrowsVisible
        {
            get { return _footerArrowsVisible; }
            set
            {
                _footerArrowsVisible = value;
                OnPropertyChanged();
            }
        }

        public string TicketLogo { get; set; }
        public bool AdministratorWindowLoading { get; set; }

        private bool _betSelected = false;
        public bool BetSelected
        {
            get
            {
                return _betSelected;
            }
            set
            {
                _betSelected = value;
                OnPropertyChanged();
            }
        }

        private bool _isLandscapeMode = false;

        public bool IsLandscapeMode
        {
            get
            {
                return _isLandscapeMode;
            }
            set
            {
                _isLandscapeMode = value;
                OnPropertyChanged();
            }
        }

        private bool _is34Mode = false;
        public bool Is34Mode
        {
            get { return _is34Mode; }
            set
            {
                _is34Mode = value;
                OnPropertyChanged();
            }
        }

        private double _screen2WindowScale = 1.0;
        public double Screen2WindowScale
        {
            get
            {
                return _screen2WindowScale;
            }
            set
            {
                _screen2WindowScale = value;
                OnPropertyChanged();
            }
        }

        public String CurrentMatchOrRaceDay { get; set; }
        public String CurrentSeasonOrRace { get; set; }
        public eServerSourceType? SourceType { get; set; }
        public VHCType? VhcSelectedType { get; set; }

        private bool _isCalibration = false;
        private bool _printTicketChecked;
        private DateTime _calendarStartDateAccounting;
        private DateTime _calendarEndDateAccounting;
        private IList<string> _selectedDescriptors = new List<string>();

        public bool IsCalibration
        {
            get { return _isCalibration; }
            set
            {
                _isCalibration = value;
                OnPropertyChanged();
            }
        }

        public bool CanScanTaxNumber { get; set; }

        public bool PrintTicketChecked
        {
            get { return _printTicketChecked; }
            set
            {
                _printTicketChecked = value;
                OnPropertyChanged();
            }
        }

        public bool TicketBuyActive { get; set; }
    }
}