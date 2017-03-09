using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    [ServiceAspect]
    public class UserHistoryViewModel : BaseViewModel
    {
        #region Variables

        private long _currentPageIndex;
        private long _allPages;
        private ComboBoxItem _selectedGroup;
        private int _selectedHistoryRecordTypeIndex;
        private ObservableCollection<ComboBoxItem> dateFilters;
        private ObservableCollection<ComboBoxItem> operations;
        private DateTime? startDate = null;
        private DateTime? endDate = null;
        private IList<BalanceOperation> _balanceOperations;
        private int pagesize = 100;
        private readonly Dictionary<string, MultistringTag> alloperations = new Dictionary<string, MultistringTag>()
			{
				{"0", MultistringTags.TERMINAL_ALL_TYPES},
				{"1", MultistringTags.UPDATE_PROFILE},
				{"2", MultistringTags.LOAD_PROFILE},
				{"3", MultistringTags.REGISTER_ACCOUNT},
				{"4", MultistringTags.LOAD_STORED_TICKETS},
				{"5", MultistringTags.CHANGE_PASSWORD_FROM_SHOP},
				{"6", MultistringTags.CHANGE_PASSWORD_FROM_TERMINAL},
				{"7", MultistringTags.RESTORE_PASSWORD},
				{"8", MultistringTags.END_USER_VERIFICATION},
				{"9", MultistringTags.GET_BALANCE},
				{"10", MultistringTags.STORE_SESSION},
				{"11", MultistringTags.CLOSE_SESSION},
				{"12", MultistringTags.OPEN_SESSION},
				{"13", MultistringTags.DEPOSIT},
				{"14", MultistringTags.TERMINAL_FORM_WITHDRAW},
				{"15", MultistringTags.RESERVE},
				{"16", MultistringTags.CANCEL_RESERVE},
				{"21", MultistringTags.CANCEL_TICKET},
                {"22", MultistringTags.SYSTEM_DEPOSIT},
				{"23", MultistringTags.SYSTEM_WITHDRAW},
				{"24", MultistringTags.PAYMENT_NOTE_REGISTERED},
				{"25", MultistringTags.PAYMENT_NOTE_PAID},
				{"26", MultistringTags.CREDITNOTE},
				{"27", MultistringTags.SYSTEM_RECALCULATION},
				{"28", MultistringTags.DEPOSIT_BY_TICKET},
				{"29", MultistringTags.SYSTEM_EXPIRED_PAYMENT_NOTE_PAYOUT},
                {"33", MultistringTags.TERMINAL_STAKE_LIVE}, 
                {"34", MultistringTags.TERMINAL_STAKE_PREMATCH},
                {"35", MultistringTags.TERMINAL_STAKE_MIXED},
                {"36", MultistringTags.TERMINAL_STAKE_VFL},
                {"31", MultistringTags.ACCOUNT_BLOCKED},
                {"32", MultistringTags.ACCOUNT_UNBLOCKED},
                {"38", MultistringTags.TERMINAL_STAKE_VHC},
			};

        private Dictionary<long, Dictionary<int, MultistringTag>> operationsDict = new Dictionary
            <long, Dictionary<int, MultistringTag>>
			{
				{
					1, new Dictionary<int, MultistringTag>
						{
							{0, MultistringTags.TERMINAL_ALL_TYPES},
							{1, MultistringTags.UPDATE_PROFILE},
							{2, MultistringTags.LOAD_PROFILE},
							{3, MultistringTags.REGISTER_ACCOUNT},
							{4, MultistringTags.LOAD_STORED_TICKETS},
							{5, MultistringTags.CHANGE_PASSWORD_FROM_SHOP},
							{6, MultistringTags.CHANGE_PASSWORD_FROM_TERMINAL},
							{7, MultistringTags.RESTORE_PASSWORD},
							{8, MultistringTags.END_USER_VERIFICATION},
							{9, MultistringTags.GET_BALANCE},
							{10, MultistringTags.STORE_SESSION},
							{11, MultistringTags.CLOSE_SESSION},
							{12, MultistringTags.OPEN_SESSION},
						}
				},
				{
					2, new Dictionary<int, MultistringTag>
						{
							{0, MultistringTags.TERMINAL_ALL_TYPES},
							{13, MultistringTags.DEPOSIT},
							{14, MultistringTags.STAKE_ON_TICKET},
							{22, MultistringTags.SYSTEM_DEPOSIT},
							{23, MultistringTags.SYSTEM_WITHDRAW},
							{24, MultistringTags.PAYMENT_NOTE_REGISTERED},
							{25, MultistringTags.PAYMENT_NOTE_PAID},
							{26, MultistringTags.CREDITNOTE_PAYOUT},
							{27, MultistringTags.SYSTEM_RECALCULATION},
							{28, MultistringTags.DEPOSIT_BY_TICKET},
							{29, MultistringTags.SYSTEM_EXPIRED_PAYMENT_NOTE_PAYOUT},
                            {33, MultistringTags.TERMINAL_STAKE_LIVE}, 
                            {34, MultistringTags.TERMINAL_STAKE_PREMATCH},
                            {35, MultistringTags.TERMINAL_STAKE_MIXED},
                            {36, MultistringTags.TERMINAL_STAKE_VFL},
                            {38, MultistringTags.TERMINAL_STAKE_VHC},
						}
				},

				{
					3, new Dictionary<int, MultistringTag>
						{
							{0, MultistringTags.ALL_OPERATIONS},
							{21, MultistringTags.CANCEL_TICKET},
						}
				},
				{
					0, new Dictionary<int, MultistringTag>
						{
							{0, MultistringTags.TERMINAL_ALL_TYPES},
							{1, MultistringTags.UPDATE_PROFILE},
							{2, MultistringTags.LOAD_PROFILE},
							{3, MultistringTags.REGISTER_ACCOUNT},
							{4, MultistringTags.LOAD_STORED_TICKETS},
							{5, MultistringTags.CHANGE_PASSWORD_FROM_SHOP},
							{6, MultistringTags.CHANGE_PASSWORD_FROM_TERMINAL},
							{7, MultistringTags.RESTORE_PASSWORD},
							{8, MultistringTags.END_USER_VERIFICATION},
							{9, MultistringTags.GET_BALANCE},
							{10, MultistringTags.STORE_SESSION},
							{11, MultistringTags.CLOSE_SESSION},
							{12, MultistringTags.OPEN_SESSION},
							{13, MultistringTags.DEPOSIT},
							{14, MultistringTags.STAKE_ON_TICKET},
							{15, MultistringTags.RESERVE},
							{16, MultistringTags.CANCEL_RESERVE},
						}
				}
			};

        private bool _showDatePickers;


        #endregion

        #region Constructor & destructor
        public UserHistoryViewModel()
        {


            StartPage = 1;
            GoCommand = new Command(OnGoClick);
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
            GridCreated = new Command<UIElement>(OnGridCreated);
            this.SelectStartDateCommand = new Command(OnSelectStartDateExecute);
            this.SelectEndDateCommand = new Command(OnSelectEndDateExecute);
            Mediator.Register<long>(this, UpdateHistory, MsgTag.UpdateHistory);

            operations = new ObservableCollection<ComboBoxItem>();
            operations.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.BALANCE_OPERATIONS).ToString(), 2));
            operations.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.PROFILE_OPERATIONS).ToString(), 1));
            //operations.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.TICKET_OPERATIONS).ToString(), 3));
            operations.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.ALL_OPERATIONS).ToString(), 0));

            dateFilters = new ObservableCollection<ComboBoxItem>();
            dateFilters.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.Last_Week).ToString(), 0));
            dateFilters.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.Current_Month).ToString(), 1));
            dateFilters.Add(
                new ComboBoxItem(TranslationProvider.Translate(MultistringTags.Previous_and_Current_Month).ToString(), 2));
            dateFilters.Add(new ComboBoxItem(TranslationProvider.Translate(MultistringTags.Custom_data_filter).ToString(), 3));
            _selectedGroup = operations[0];
            UpdateHistoryRecordTypes();
            _selectedDateFilterIndex = 0;
            startDate = DateTime.Now.AddDays(-7);
            endDate = null;

            PreviousPage = new Command(OnPreviousPage);
            FirstPage = new Command(OnFirstPage);
            NextPage = new Command(OnNextPage);
            LastPage = new Command(OnLastPage);
            HideWindowCommand = new Command(HideWindow);
            _currentPageIndex = 1;

            _selectedHistoryRecordTypeIndex = 0;
            IsBackVisible = ChangeTracker.CurrentUser is OperatorUser;

        }

        public double gridHeight { get { return ChangeTracker.HistorygridHeight; } set { ChangeTracker.HistorygridHeight = value; } }
        public double HistoryrowHeight { get { return ChangeTracker.HistoryrowHeight; } set { ChangeTracker.HistoryrowHeight = value; } }


        #endregion

        #region Properties




        public long CurrentPageIndex
        {
            get { return AllPages > _currentPageIndex ? _currentPageIndex : AllPages; }
            set
            {
                _currentPageIndex = value;
                OpenHistoryPage(_currentPageIndex);
                OnPropertyChanged("CurrentPageIndex");
            }
        }

        public bool IsBackVisible
        {
            get { return _isBackVisible; }
            set
            {
                _isBackVisible = value;
                OnPropertyChanged("IsBackVisible");
            }
        }

        public long AllPages
        {
            get { return _allPages; }
            set
            {
                _allPages = value;
                if (_allPages == 0)
                    _allPages = 1;
                OnPropertyChanged("AllPages");
            }
        }

        protected FoundUser EditUserId
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        private ObservableCollection<ComboBoxItem> _historyRecordTypes;
        private int _selectedDateFilterIndex;
        private bool _isBackVisible;
        private DateTime? _startDateSelected;
        private DateTime? _endDateSelected;

        public DateTime? StartDateSelected
        {
            get { return _startDateSelected; }
            set
            {
                _startDateSelected = value;
                OnPropertyChanged();
                OnStartDateSelectedChanged(value);
            }
        }

        public string StartDateSelectedFormatted
        {
            get
            {
                string formatted = string.Empty;
                if (this.StartDateSelected.HasValue)
                {
                    formatted = this.StartDateSelected.Value.ToString("dd.MM.yyyy HH:mm");
                }
                return formatted;
            }
        }

        public DateTime? EndDateSelected
        {
            get { return _endDateSelected; }
            set
            {
                _endDateSelected = value;
                OnPropertyChanged();
                OnEndDateSelectedChanged(value);
            }
        }


        public string EndDateSelectedFormatted
        {
            get
            {
                string formatted = string.Empty;
                if (this.EndDateSelected.HasValue)
                {
                    formatted = this.EndDateSelected.Value.ToString("dd.MM.yyyy HH:mm");
                }
                return formatted;
            }
        }



        public ComboBoxItem SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
                UpdateHistoryRecordTypes();
                StartPage = 1;
                UpdateHistory();
            }
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public ObservableCollection<ComboBoxItem> Operations
        {
            get { return operations; }
            set { operations = value; }
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public ObservableCollection<ComboBoxItem> DateFilters
        {
            get { return dateFilters; }
            set { dateFilters = value; }
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public ObservableCollection<ComboBoxItem> HistoryRecordTypes
        {
            get { return _historyRecordTypes; }
            set
            {
                _historyRecordTypes = value;
                OnPropertyChanged("HistoryRecordTypes");
            }
        }

        public bool ShowDatePickers
        {
            get { return _showDatePickers; }
            set
            {
                _showDatePickers = value;
                OnPropertyChanged("ShowDatePickers");
                UpdateHistory();
                OnPropertyChanged("Operations");
            }
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public int SelectedDateFilterIndex
        {
            get { return _selectedDateFilterIndex; }
            set
            {
                StartPage = 1;

                _selectedDateFilterIndex = value;
                OnPropertyChanged("SelectedDateFilterIndex");

                switch (_selectedDateFilterIndex)
                {
                    case 0:
                        ShowDatePickers = false;
                        startDate = DateTime.Now.AddDays(-7);
                        endDate = null;
                        UpdateHistory();
                        break;
                    case 1:
                        ShowDatePickers = false;
                        startDate = DateTime.Now.AddMonths(-1);
                        endDate = null;
                        UpdateHistory();
                        break;
                    case 2:
                        ShowDatePickers = false;
                        startDate = DateTime.Now.AddMonths(-2);
                        endDate = null;
                        UpdateHistory();
                        break;
                    case 3:
                        ShowDatePickers = true;
                        startDate = StartDateSelected;
                        endDate = EndDateSelected;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public int SelectedHistoryRecordTypeIndex
        {
            get { return _selectedHistoryRecordTypeIndex; }
            set
            {
                _selectedHistoryRecordTypeIndex = value;
                OnPropertyChanged();
                StartPage = 1;
                UpdateHistory();
            }
        }

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public IList<BalanceOperation> BalanceOperations
        {
            get
            {
                return _balanceOperations;
            }
            set
            {
                _balanceOperations = value;
                OnPropertyChanged();
            }
        }

        public long StartPage { get; set; }
        public long Total { get; set; }


        #endregion

        #region Commands
        public Command PreviousPage { get; set; }
        public Command FirstPage { get; set; }
        public Command NextPage { get; set; }
        public Command LastPage { get; set; }
        public Command HideWindowCommand { get; private set; }
        public Command GoCommand { get; set; }
        public Command SelectStartDateCommand { get; set; }
        public Command SelectEndDateCommand { get; set; }
        public Command<UIElement> ItemCreated { get; set; }
        public Command<UIElement> GridCreated { get; set; }

        #endregion


        #region Methods


        public override void OnNavigationCompleted()
        {
            UpdateHistory(0);
            base.OnNavigationCompleted();
        }

        private void OnGridCreated(UIElement obj)
        {
            //if (gridHeight > 0)
            //    return;
            if (obj != null)
            {
                gridHeight = obj.RenderSize.Height;
            }
        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (obj.RenderSize.Height > 0 && gridHeight > 0 && HistoryrowHeight <= 0)
            {
                HistoryrowHeight = obj.RenderSize.Height;
                pagesize = (int)(gridHeight / HistoryrowHeight);
                BalanceOperations = BalanceOperations.Take(pagesize).ToList();
                UpdateHistoryTotal(Total);
            }
        }


        private void OnPreviousPage()
        {
            if (_currentPageIndex < 2)
                return;

            CurrentPageIndex--;
        }

        private void OnFirstPage()
        {
            if (_currentPageIndex < 2)
                return;

            CurrentPageIndex = 1;
        }

        private void OnNextPage()
        {
            if (_currentPageIndex < AllPages)
                CurrentPageIndex++;

        }

        private void OnLastPage()
        {
            if (_currentPageIndex < AllPages)
                CurrentPageIndex = AllPages;
        }

        private void UpdateHistoryTotal(long obj)
        {
            if (pagesize > 0)
                AllPages = (obj / pagesize) + (obj % pagesize == 0 ? 0 : 1);
            _currentPageIndex = 1;
            OnPropertyChanged("CurrentPageIndex");
        }
        [AsyncMethod]
        private void HideWindow()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitHideWindow();
        }

        private void PleaseWaitHideWindow()
        {
            if (ChangeTracker.CurrentUser is OperatorUser)
            {
                MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
            }
        }


        private void UpdateHistory(IEnumerable<historyData> obj)
        {

            var result = new ObservableCollection<BalanceOperation>();
            foreach (var historyRec in obj)
            {

                var operationName = historyRec.type.ToString();
                if (alloperations.ContainsKey(historyRec.type))
                    operationName = TranslationProvider.Translate(alloperations[historyRec.type]).ToString();
                var operation = new BalanceOperation(operationName,
                                                     historyRec.ticket_number,
                                                     historyRec.amount, historyRec.created_at,
                                                     historyRec.comment, Currency);
                //Skip adding System recalculation with zero amount
                if (historyRec.type == "27" && historyRec.amount == 0)
                {
                    continue;
                }
                result.Add(operation);
            }
            if (HistoryrowHeight == 0)
            {
                if (result.Count > 0 && result[0].DateTime == DateTime.MinValue)
                    result[0].Hidden = true;
            }
            BalanceOperations = result.Take(pagesize).ToList();
        }

        private void UpdateHistoryRecordTypes()
        {
            var recordTypes = new ObservableCollection<ComboBoxItem>();

            foreach (var operation in operationsDict[SelectedGroup.Id])
            {
                recordTypes.Add(new ComboBoxItem(TranslationProvider.Translate(operation.Value).ToString(), operation.Key));
            }
            _selectedHistoryRecordTypeIndex = 0;
            OnPropertyChanged("SelectedHistoryRecordTypeIndex");
            _historyRecordTypes = recordTypes;
            OnPropertyChanged("HistoryRecordTypes");
            OnPropertyChanged("SelectedHistoryRecordTypeIndex");

        }

        private void OpenHistoryPage(long obj)
        {
            StartPage = obj;
            UpdateHistory();
        }

        private void OnGoClick()
        {
            StartPage = 1;
            startDate = StartDateSelected;
            if (EndDateSelected != null)
                endDate = EndDateSelected.Value.AddDays(1).AddSeconds(-1);
            UpdateHistory();
        }

        private void OnSelectStartDateExecute()
        {
            this.StartDateSelected = DateHelper.SelectDate(this.StartDateSelected);
        }

        private void OnSelectEndDateExecute()
        {
            this.EndDateSelected = DateHelper.SelectDate(this.EndDateSelected);
        }

        private void OnStartDateSelectedChanged(DateTime? value)
        {
            this.startDate = value as DateTime?;
            this.OnPropertyChanged("StartDateSelectedFormatted");
        }

        private void OnEndDateSelectedChanged(DateTime? value)
        {
            this.endDate = value as DateTime?;
            this.OnPropertyChanged("EndDateSelectedFormatted");
        }

        [AsyncMethod]
        private void UpdateHistory()
        {
            PleaseWaitOnUpdateHistory();
        }

        [WsdlServiceSyncAspectSilent]
        private void UpdateHistory(long obj)
        {
            OnUpdateHistory();
        }


        [WsdlServiceSyncAspect]
        private void PleaseWaitOnUpdateHistory()
        {
            OnUpdateHistory();
        }
        private void OnUpdateHistory()
        {
            if (HistoryrowHeight == 0)
            {
                BalanceOperations = new ObservableCollection<BalanceOperation>() { new BalanceOperation("", "", 0, DateTime.Now, "", "") { Hidden = true } };
            }
            if (gridHeight > 0 && HistoryrowHeight > 0)
                pagesize = (int)(gridHeight / HistoryrowHeight);
            else
            {
                pagesize = 100;
            }


            var uid = StationRepository.GetUid(ChangeTracker.CurrentUser);
            if (!(ChangeTracker.CurrentUser is LoggedInUser))
                if (EditUserId != null) uid = StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0,0,0,0));


            historyData[] history;
            string stringtotal = WsdlRepository.History(uid,
                                                               HistoryRecordTypes[SelectedHistoryRecordTypeIndex].Id.ToString(),
                                                               SelectedGroup.Id.ToString(), ((StartPage - 1) * pagesize).ToString(), pagesize.ToString(), startDate, endDate, out history, true);

            var total = long.Parse(stringtotal);

            if (history != null)
            {

                UpdateHistory(history);

            }
            if (Total != total && pagesize > 1)
            {
                Total = total;
                UpdateHistoryTotal(Total);
            }

        }


        #endregion
    }
}