using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using System.Collections.ObjectModel;
using System.Windows;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class OperatorShiftReportsViewModel : BaseViewModel
    {
        
        #region Constructors
        public OperatorShiftReportsViewModel()
        {
            Mediator.Register<bool>(this, LoadOperShiftReports, MsgTag.LoadOperShiftReports);
            Mediator.Register<string>(this, PrevPage, MsgTag.LoadPrevPage);
            Mediator.Register<string>(this, NextPage, MsgTag.LoadNextPage);

            BackCommand = new Command(OnBackCommand);
            //onNextPageClicked = new Command(NextPage);
            //onPreviousPageClicked = new Command(PrevPage);
            onSearchClicked = new Command(onSearch);
            onShowAllPressed = new Command(ShowAll);
            onOperatorSettlementPressed = new Command(OperatorSettlement);
            GridCreated = new Command<UIElement>(OnGridCreated);
            ItemCreated = new Command<UIElement>(OnRowItemCreated);

            Search = "";

            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
        }
        #endregion

        #region Properties

        private int pageSize = 0;
        private int currentPosition = 0;

        private ObservableCollection<OperatorShiftCheckpoint> globalCollection = new ObservableCollection<OperatorShiftCheckpoint>();
        private ObservableCollection<OperatorShiftCheckpoint> visibleCollection = new ObservableCollection<OperatorShiftCheckpoint>();

        public Visibility CreateSettlementVisibility
        {
            get { return ChangeTracker.CurrentUser.OperatorShiftSettlementWrite ? Visibility.Visible : Visibility.Collapsed; }
        }

        public ObservableCollection<OperatorShiftCheckpoint> _Checkpoints;
        public ObservableCollection<OperatorShiftCheckpoint> Checkpoints
        {
            get { return _Checkpoints; }
        }

        private OperatorShiftCheckpoint _selectedCheckpoint;
        public OperatorShiftCheckpoint SelectedCheckpoint
        {
            get { return _selectedCheckpoint; }
            set
            {
                if (value == null)
                    return;

                _selectedCheckpoint = value;
                OnPropertyChanged("SelectedCheckpoint");
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
                OnPropertyChanged("IsFocused");
            }

        }

        private string _searchString;
        public string Search
        {
            get { return _searchString; }
            set { _searchString = value; OnPropertyChanged("Search");}
        }

        private double GridHeight = 0.0;
        private double ItemHeight = 0.0;

        #endregion

        #region Commands

        public Command BackCommand { get; set; }
        public Command onNextPageClicked { get; private set; }
        public Command onPreviousPageClicked { get; private set; }
        public Command onSearchClicked { get; private set; }
        public Command onShowAllPressed { get; private set; }
        public Command onOperatorSettlementPressed { get; private set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_LO_SHIFT_REPORT;
            ChangeTracker.OpenShiftsChecked = true;
            LoadOperShiftReports(true);
            base.OnNavigationCompleted();
        }
        private void OnBackCommand()
        {
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        [AsyncMethod]
        private void LoadOperShiftReports(bool isNeeded)
        {
            //onLoadData();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();
            OperatorShiftCheckpoint testCP = new OperatorShiftCheckpoint();
            tempCollection.Add(testCP);
            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);
            OnPropertyChanged("Checkpoints");
        }

        [WsdlServiceSyncAspect]
        private void onLoadData()
        {
            decimal balance;
            globalCollection = new ObservableCollection<OperatorShiftCheckpoint>(WsdlRepository.GetOperatorShiftCheckpoints(StationRepository.LocationID, Int16.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).account_id), out balance, true));
            if (globalCollection.Count == 0)
                return;

            visibleCollection = new ObservableCollection<OperatorShiftCheckpoint>(globalCollection);

            //load first part
            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();
            if ((currentPosition + pageSize) < visibleCollection.Count)
            {
                for (int i = currentPosition; i < pageSize; i++)
                {
                    tempCollection.Add(visibleCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < visibleCollection.Count; i++)
                {
                    tempCollection.Add(visibleCollection[i]);
                }

            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);
            OnPropertyChanged("Checkpoints");  
        }

        private void LoadPage()
        {
            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();

            if ((currentPosition + pageSize) < visibleCollection.Count)
            {
                for (int i = currentPosition; i < (currentPosition + pageSize); i++)
                {
                    tempCollection.Add(visibleCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < visibleCollection.Count; i++)
                {
                    tempCollection.Add(visibleCollection[i]);
                }

            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);
            OnPropertyChanged("Checkpoints");
        }

        [AsyncMethod]
        private void NextPage(string obj)
        {
            if (visibleCollection.Count < (currentPosition + pageSize))
                return;

            currentPosition += pageSize;
            LoadPage();
        }

        [AsyncMethod]
        private void PrevPage(string obj)
        {
            if (currentPosition == 0)
                return;

            currentPosition -= pageSize;
            if (currentPosition < 0)
                currentPosition = 0;

            LoadPage();
        }

        [AsyncMethod]
        private void ShowAll()
        {
            if (globalCollection.Count == 0)
                return;

            currentPosition = 0;
            Search = "";

            visibleCollection = globalCollection;
            LoadPage();
        }

        [AsyncMethod]
        private void onSearch()
        {
            if (globalCollection.Count == 0)
                return;

            if (string.IsNullOrEmpty(Search))
                LoadOperShiftReports(true);

            currentPosition = 0;

            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();

            for(int i=0; i<globalCollection.Count; i++)
                if(globalCollection[i].operatorName.Contains(Search))
                    tempCollection.Add(globalCollection[i]);

            visibleCollection = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);

            LoadPage();
        }

        private void OperatorSettlement()
        {
            //open operator settlement
            MyRegionManager.NavigateUsingViewModel<OperatorSettlementViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnGridCreated(UIElement obj)
        {
            if (GridHeight > 0)
                return;

            GridHeight = obj.RenderSize.Height - 12;

            if (ItemHeight == 0.0)
                return;

            CalculatePageSize();
        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (ItemHeight > 0)
                return;

            ItemHeight = obj.RenderSize.Height;
            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>();
            OnPropertyChanged("Checkpoints");

            if (GridHeight == 0.0)
                return;

            CalculatePageSize();
        }

        [AsyncMethod]
        private void CalculatePageSize()
        {
            pageSize = (int)(GridHeight / ItemHeight);
            onLoadData();
        }

        public override void Close()
        {
            globalCollection.Clear();
            visibleCollection.Clear();
            base.Close();
        }

        #endregion
    }
}
