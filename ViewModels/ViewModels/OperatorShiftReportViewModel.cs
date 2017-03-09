using System;
using System.Collections.ObjectModel;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using System.Windows;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class OperatorShiftReportViewModel : BaseViewModel
    {

        #region Constructors
        public OperatorShiftReportViewModel()
        {
            HidePleaseWait = false;
            Mediator.Register<string>(this, PrevPage, MsgTag.LoadPrevPage);
            Mediator.Register<string>(this, NextPage, MsgTag.LoadNextPage);

            Mediator.Register<string>(this, UpdateOperatorShiftData, MsgTag.RefreshCashOperations);

            onCreateCheckpoint = new Command(StartCreatingCheckpoint);
            onPrintCheckpoint = new Command(StartPrintingCheckpoint);
            onSettlementHistory = new Command(ShowSettlementHistory);
            //onNextPageClicked = new Command(NextPage);
            //onPreviousPageClicked = new Command(PrevPage);
            BackCommand = new Command(OnBackCommand);
            GridCreated = new Command<UIElement>(OnGridCreated);
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
            onScanPaymentOrCreditNote = new Command(ScanPaymentOrCreditNote);
        }

        #endregion

        #region Properties
        private int pageSize = 0;
        private int currentPosition = 0;

        private ObservableCollection<OperatorShiftCheckpoint> globalCollection = new ObservableCollection<OperatorShiftCheckpoint>();

        public Visibility CreateCheckpointVisibility
        {
            get { return ChangeTracker.CurrentUser.OperatorShiftCheckpointWrite ? Visibility.Visible : Visibility.Collapsed; }
        }

		public bool EnabledPrintCheckpoint { get; set;}       
        
        public Visibility CreateScanPaymentOrCrediteNoteVisibility
        {
            get
            {
                if (ChangeTracker.CurrentUser.PayoutCreditNote || ChangeTracker.CurrentUser.PayoutPaymentNote)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public ObservableCollection<OperatorShiftCheckpoint> _Checkpoints;
        public ObservableCollection<OperatorShiftCheckpoint> Checkpoints
        {
            get { return _Checkpoints; }
        }

        private decimal _balance = 0;
        public decimal Balance {
            get
            {
                return _balance;
            }
            set
            {
                _balance = value;
                OnPropertyChanged("Balance");
            }
        }


       
        private OperatorShiftData osd;
        public OperatorShiftData OSD
        {
            get { return osd; }
           
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
                EnabledPrintCheckpoint = _Checkpoints.Count > 0 && SelectedCheckpoint != null && SelectedCheckpoint.id != -1000;
                OnPropertyChanged("EnabledPrintCheckpoint");
            }
        }

        private double GridHeight = 0.0;
        private double ItemHeight = 0.0;

        #endregion

        #region Commands
        public Command onCreateCheckpoint { get; private set; }
        public Command onPrintCheckpoint { get; private set; }
        public Command onSettlementHistory { get; private set; }
        public Command onNextPageClicked { get; private set; }
        public Command onPreviousPageClicked { get; private set; }
        public Command BackCommand { get; set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }
        public Command onScanPaymentOrCreditNote { get; private set; }
        #endregion

        #region Methods

        public void UpdateOperatorShiftData(string res)
        {
            CurrentShiftReport();
        }

        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_OPER_SHIFT_REPORT;
            ChangeTracker.OperatorsShiftsReporChecked = true;
            LoadOperatorShifts();

            base.OnNavigationCompleted();
        }
        [AsyncMethod]
        private void LoadOperatorShifts()
        {
            //onLoadData();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();
            OperatorShiftCheckpoint testCP = new OperatorShiftCheckpoint();
            testCP.id = -1000;
            tempCollection.Add(testCP);
            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);
            OnPropertyChanged("Checkpoints");
        }

        private void CurrentShiftReport()
        {         
            try
            {                
                osd = WsdlRepository.GetOperatorShiftReport(StationRepository.LocationID, Int16.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).account_id));
                Balance = osd.balance;
                OnPropertyChanged("OSD");
            }
            catch (Exception ex)
            {
            }
        }

        [WsdlServiceSyncAspect]
        private void onLoadData()
        {
            
            decimal tempBalance;
            globalCollection = new ObservableCollection<OperatorShiftCheckpoint>(WsdlRepository.GetOperatorShiftCheckpoints(StationRepository.LocationID, Int16.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).account_id), out tempBalance, false));

            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();
            if ((currentPosition + pageSize) < globalCollection.Count)
            {
                for (int i = currentPosition; i < pageSize; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < globalCollection.Count; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }

            //calculate balance
            //CalculateBalance();

            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);
            EnabledPrintCheckpoint = _Checkpoints.Count > 0 && SelectedCheckpoint != null && SelectedCheckpoint.id != -1000;
            OnPropertyChanged("Checkpoints");
            OnPropertyChanged("EnabledPrintCheckpoint");

            //Balance = tempBalance;
            //OnPropertyChanged("Balance");

            CurrentShiftReport();
        }

        private void CalculateBalance()
        {
            decimal ebAmount = 0;
            decimal payments = 0;
            for (int i = 0; i < globalCollection.Count; i++)
            {
                ebAmount += globalCollection[i].emptyBoxTotalAmount;
                payments += globalCollection[i].payoutTotalAmount;
            }
            Balance = ebAmount - payments;
            OnPropertyChanged("Balance");
        }

        [AsyncMethod]
        private void StartCreatingCheckpoint()
        {
            CreateCheckpoint();
        }

        private bool CheckForEmptyBox()
        {
            bool result = false;

            if (ChangeTracker.CurrentUser.ViewEmptyBox)
            {
                decimal billsAmount;
                decimal coinsamount;
                int billscount;
                int coinscount;
                var amount = WsdlRepository.GetStationCashInfo(StationRepository.StationNumber, out billsAmount, out billscount, out coinsamount, out coinscount);

                if (amount > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        [WsdlServiceSyncAspect]
        private void CreateCheckpoint()
        {

            OperatorShiftCheckpoint checkpoint = new OperatorShiftCheckpoint();

            DateTime stDate;
            DateTime enDate;
            int ebTN;
            int pTN;
            decimal ebTA;
            decimal pTA;
            decimal tempBalance;
            string operatorName;

            if (CheckForEmptyBox())
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PLEASE_EMPTY_BOX).ToString(), null, false);
                return;
            }

            try
            {
                OperatorShiftData data = WsdlRepository.CreateOperatorShiftCheckpoint(StationRepository.LocationID, Int16.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).account_id));
                stDate = data.startDate;
                enDate = data.endDate;
                ebTA = data.emptyBoxTotalAmount;
                pTA = data.payoutTotalAmount;
                ebTN = data.emptyBoxTotalNumber;
                pTN = data.payoutTotalNumber;
                tempBalance = data.balance;
                operatorName = data.operatorName;
            }
            catch (Exception ex)
            {
                if (ex is System.ServiceModel.FaultException<HubServiceException>)
                {
                    System.ServiceModel.FaultException<HubServiceException> exep = (System.ServiceModel.FaultException<HubServiceException>)ex;
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_EMPTY_BOXES).ToString(), null, false);
                }

                return;
            }

            checkpoint.startDate = stDate;
            checkpoint.endDate = enDate;
            checkpoint.emptyBoxTotalAmount = ebTA;
            checkpoint.emptyBoxTotalNumber = ebTN;
            checkpoint.payoutTotalAmount = pTA;
            checkpoint.payoutTotalNumber = pTN;
            checkpoint.operatorName = operatorName;
            checkpoint.operatorId = (int)ChangeTracker.CurrentUser.AccountId;

            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();
            globalCollection.Insert(0, checkpoint);

            if ((currentPosition + pageSize) < globalCollection.Count)
            {
                for (int i = currentPosition; i < pageSize; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < globalCollection.Count; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }

            SelectedCheckpoint = tempCollection[0];

            //recalculate balance
            //CalculateBalance();

            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);
            EnabledPrintCheckpoint = _Checkpoints.Count > 0 && SelectedCheckpoint != null && SelectedCheckpoint.id != -1000;
            OnPropertyChanged("Checkpoints");
            OnPropertyChanged("EnabledPrintCheckpoint");

            Balance = tempBalance;
            OnPropertyChanged("Balance");

            CurrentShiftReport();

            //printout
            bool isPrinted = PrinterHandler.PrintOperatorShiftReport(checkpoint, tempBalance);
            if (!isPrinted)
                ShowPrinterErrorMessage();

            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_SHIFT_REPORT_CREATED).ToString() + "\n\n" + SelectedCheckpoint.startDate.ToString("dd.MM.yyyy HH:mm") + " - " + SelectedCheckpoint.endDate.ToString("dd.MM.yyyy HH:mm"), null, false, 15);
        }

        [AsyncMethod]
        private void StartPrintingCheckpoint()
        {
            PrintReport();
        }

        [WsdlServiceSyncAspect]
        private void PrintReport()
        {
            if (SelectedCheckpoint == null)
                return;

            bool isPrinted = PrinterHandler.PrintOperatorShiftReport(SelectedCheckpoint, Balance);
            if (!isPrinted)
                ShowPrinterErrorMessage();
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = "";

            switch (status)
            {
                case 0:
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                    return;
                case 4:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_PAPER).ToString();
                    break;
                case 6:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_TONER).ToString();
                    break;
                case 7:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OPEN).ToString();
                    break;
                case 8:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_JAMMED).ToString();
                    break;
                case 9:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OFFLINE).ToString();
                    break;
            }

            ShowError(errorMessage, null, true);
        }

        [AsyncMethod]
        private void NextPage(string obj)
        {
            if (globalCollection.Count < (currentPosition + pageSize))
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

        private void LoadPage()
        {
            ObservableCollection<OperatorShiftCheckpoint> tempCollection = new ObservableCollection<OperatorShiftCheckpoint>();

            if ((currentPosition + pageSize) < globalCollection.Count)
            {
                for (int i = currentPosition; i < (currentPosition + pageSize); i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < globalCollection.Count; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }

            _Checkpoints = new ObservableCollection<OperatorShiftCheckpoint>(tempCollection);

            if (_Checkpoints.Count > 0)
            {
                SelectedCheckpoint = _Checkpoints[0];
                OnPropertyChanged("Checkpoints");
            }
        }

        private void OnBackCommand()
        {
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
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

        [AsyncMethod]
        private void ShowSettlementHistory()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.NavigateUsingViewModel<OperatorSettlementHistoryViewModel>(RegionNames.UsermanagementContentRegion);
        }

         [AsyncMethod]
        private void ScanPaymentOrCreditNote()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<PaymentViewModel>(RegionNames.UsermanagementContentRegion);
        }
        #endregion
    }
}
