using System;
using System.Threading;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common;
using SportRadar.Common.Enums;
using TranslationByMarkupExtension;
using System.Windows;
using WsdlRepository.WsdlServiceReference;
using BaseObjects;
using System.Collections.ObjectModel;
using System.Linq;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class ProfitAccountingViewModel : AccountingBaseViewModel
    {
        #region Constructors

        public ProfitAccountingViewModel()
        {

            onCreateCheckpoint = new Command(StartCreatingCheckpoint);
            onPrintReportForLocation = new Command(StartPrintReportForLocation);
            onPrintReportForTerminal = new Command(StartPrintReportForTerminal);
            onReportButtonPressed = new Command(OnReportPressed);
            //onNextPageClicked = new Command(NextPage);
            //onPreviousPageClicked = new Command(PrevPage);

            Mediator.Register<string>(this, PrevPage, MsgTag.LoadPrevPage);
            Mediator.Register<string>(this, NextPage, MsgTag.LoadNextPage);

            GridCreated = new Command<UIElement>(OnGridCreated);
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
            _Checkpoints = new ObservableCollection<CheckpointModel>();

            if (ChangeTracker.CurrentUser.ProfitShareCheckpointWrite)
            {
                CreateCheckpointVisibility = Visibility.Visible;
            }
            else
            {
                CreateCheckpointVisibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Commands

        public Command onCreateCheckpoint { get; private set; }
        public Command onPrintReportForLocation { get; private set; }
        public Command onPrintReportForTerminal { get; private set; }
        public Command onNextPageClicked { get; private set; }
        public Command onPreviousPageClicked { get; private set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }
        public Command onReportButtonPressed { get; set; }

        #endregion

        #region properties
        private int itemsAmountPerPage = 13;
        private int currentPosition = 0;
        private long itemsTotal = 0;

        public Visibility CreateCheckpointVisibility { get; set; }

        private bool _showReportPart = false;
        public bool ShowReportPart
        {
            get
            {
                return _showReportPart;
            }
            set
            {
                _showReportPart = value;
                OnPropertyChanged("ShowReportPart");
            }
        }

        private bool _displayComissions = false;
        public bool DisplayComissions
        {
            get { return _displayComissions; }
            set
            {
                _displayComissions = value;
                OnPropertyChanged("DisplayComissions");
            }
        }

        //location part
        public string totalCashIn { get; set; }
        public string totalCashOut { get; set; }
        public string totalStake { get; set; }
        public string totalWinning { get; set; }
        public string tax { get; set; }
        public string basisForProfitSharing { get; set; }
        public string fixStakeCommission { get; set; }
        public string flexCommission { get; set; }
        public string shopOwnerShare { get; set; }
        public string subFranchisorShare { get; set; }
        public string franchisorShare { get; set; }
        public string mainOwnerShare { get; set; }
        public string creditToShop { get; set; }
        public string paymentFromShop { get; set; }
        public string cashTransfer { get; set; }

        //station part
        public string stationTotalCashIn { get; set; }
        public string stationTotalCashOut { get; set; }
        public string stationTotalStake { get; set; }
        public string stationTotalWinning { get; set; }
        public string stationTax { get; set; }
        public string stationBasisForProfitSharing { get; set; }
        public string stationFixStakeCommission { get; set; }
        public string stationFlexCommission { get; set; }
        public string stationShopOwnerShare { get; set; }
        public string stationSubFranchisorShare { get; set; }
        public string stationFranchisorShare { get; set; }
        public string stationMainOwnerShare { get; set; }
        public string stationCreditToShop { get; set; }
        public string stationPaymentFromShop { get; set; }
        public string stationCashTransfer { get; set; }


        private double GridHeight = 0.0;
        private double ItemHeight = 0.0;

        private string _accountingModel;
        public string AccountingModel { get { return _accountingModel; } set { _accountingModel = value; OnPropertyChanged("AccountingModel"); } }

        private CheckpointModel _selectedCheckpoint;
        public CheckpointModel SelectedCheckpoint
        {
            get { return _selectedCheckpoint; }
            set
            {
                if (value == null)
                    return;

                _selectedCheckpoint = value;
                LoadProperties(_selectedCheckpoint.ProfitAccountingCheckpoint);
                OnPropertyChanged("SelectedCheckpoint");
            }
        }

        public ObservableCollection<CheckpointModel> _Checkpoints;
        public ObservableCollection<CheckpointModel> Checkpoints
        {
            get { return _Checkpoints; }
        }

        public Visibility FranchisorShareVisibility
        {
            get
            {
                return ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            MyRegionManager.ClearHistory(RegionNames.ContentRegion);
            ChangeTracker.ProfitAccountingChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_PROFIT_ACCOUNTING;

            try
            {
                decimal locationCashPosition, totalStationCash, totalLocationCash, totalLocationPaymentBalance;
                LoadAccCheckpoints(true);

                ChangeTracker.TerminalBalance = WsdlRepository.GetCashInfo(StationRepository.StationNumber, out totalStationCash, out locationCashPosition,
                                           out totalLocationCash, out totalLocationPaymentBalance);

                ChangeTracker.TotalStationCash = totalStationCash;
                ChangeTracker.LocationCashPosition = locationCashPosition;
                ChangeTracker.LocationBalance = totalLocationCash;
                ChangeTracker.TotalLocationPaymentBalance = totalLocationPaymentBalance;
            }
            catch
            {//should here be message about fail or not, considering that such calls should be repeated on every view so fail has really short-term affect? 
            }
            base.OnNavigationCompleted();
        }
        [AsyncMethod]
        private void LoadAccCheckpoints(bool isNeeded)
        {
            //OnLoadData();
            LoadInitialData();
        }


        private void LoadInitialData()
        {
            var tempCollection = new ObservableCollection<CheckpointModel>();
            //ProfitAccountingCheckpoint testCP = WsdlRepository.GetProfitAccountingReport(Int32.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).location_id.ToString()), StationRepository.StationNumber, null, null);

            ProfitAccountingCheckpoint testCP = new ProfitAccountingCheckpoint();
            testCP.location = new ProfitAccounting();
            testCP.station = new ProfitAccounting();

            tempCollection.Add(new CheckpointModel { ProfitAccountingCheckpoint = testCP });
            _Checkpoints = new ObservableCollection<CheckpointModel>(tempCollection);
            OnPropertyChanged("Checkpoints");
        }

        [WsdlServiceSyncAspect]
        private void OnLoadData()
        {
            //download data from hub
            ArrayOfProfitAccountingCheckpoints checkpointsObject = WsdlRepository.GetProfitAccountingCheckpoint(StationRepository.GetUid(ChangeTracker.CurrentUser).location_id.Value, StationRepository.StationNumber, currentPosition, itemsAmountPerPage);
            _Checkpoints = new ObservableCollection<CheckpointModel>();
            if (checkpointsObject != null)
            {
                if (checkpointsObject.total != null)
                    itemsTotal = checkpointsObject.total.Value;
                if (checkpointsObject.profitAccounting != null)
                    foreach (var profitAccountingCheckpoint in checkpointsObject.profitAccounting)
                    {
                        _Checkpoints.Add(new CheckpointModel { ProfitAccountingCheckpoint = profitAccountingCheckpoint });
                    }
            }

            AddReportFromLastCheckpoint();

            if (Checkpoints.Count > 0)
            {
                SelectedCheckpoint = Checkpoints.ElementAt(0);
                //LoadProperties(Checkpoints.ElementAt(0));
            }

            OnPropertyChanged("Checkpoints");
        }

        private void AddReportFromLastCheckpoint()
        {
            ProfitAccountingCheckpoint testCP = WsdlRepository.GetProfitAccountingReport(Int32.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).location_id.ToString()), StationRepository.StationNumber, null, null);
            if (testCP != null)
            {
                testCP.general.endDate = DateTime.Now;
                Checkpoints.Insert(0, new CheckpointModel { ProfitAccountingCheckpoint = testCP, IsLastCheckpoint = true });

                AccountingModel = testCP.location.model;
            }

            if (Checkpoints.Count > 0)
            {
                SelectedCheckpoint = Checkpoints.ElementAt(0);
            }
        }

        private void OnReportPressed()
        {
            ShowReportPart = true;
            MyRegionManager.NavigateUsingViewModel<CreateReportViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void LoadProperties(ProfitAccountingCheckpoint checkpoint)
        {
            if (checkpoint.location.totalCashIn != null)
                totalCashIn = checkpoint.location.totalCashIn.Value.ToString();
            else
                totalCashIn = "";

            if (checkpoint.location.totalCashOut != null)
                totalCashOut = checkpoint.location.totalCashOut.Value.ToString();
            else
                totalCashOut = "";

            if (checkpoint.location.totalStake != null)
                totalStake = checkpoint.location.totalStake.Value.ToString();
            else
                totalStake = "";

            if (checkpoint.location.totalWinnings != null)
                totalWinning = checkpoint.location.totalWinnings.Value.ToString();
            else
                totalWinning = "";

            if (checkpoint.location.tax != null)
                tax = checkpoint.location.tax.Value.ToString();
            else
                tax = "";

            if (checkpoint.location.basis != null)
                basisForProfitSharing = checkpoint.location.basis.Value.ToString();
            else
                basisForProfitSharing = "";

            if (checkpoint.location.fixStakeCommission != null)
                fixStakeCommission = checkpoint.location.fixStakeCommission.Value.ToString();
            else
                fixStakeCommission = "";

            if (checkpoint.location.flexCommission != null)
                flexCommission = checkpoint.location.flexCommission.Value.ToString();
            else
                flexCommission = "";

            if (checkpoint.location.shopOwnerShare != null)
                shopOwnerShare = checkpoint.location.shopOwnerShare.Value.ToString();
            else
                shopOwnerShare = "";

            if (checkpoint.location.subFranchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                subFranchisorShare = checkpoint.location.subFranchisorShare.Value.ToString();
            else
                subFranchisorShare = "";

            if (checkpoint.location.franchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                franchisorShare = checkpoint.location.franchisorShare.Value.ToString();
            else
                franchisorShare = "";

            if (checkpoint.location.mainOwnerShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                mainOwnerShare = checkpoint.location.mainOwnerShare.Value.ToString();
            else
                mainOwnerShare = "";

            /**/

            if (checkpoint.location.totalCreditToShop != null)
                creditToShop = checkpoint.location.totalCreditToShop.Value.ToString();
            else
                creditToShop = "";

            if (checkpoint.location.totalPaymentToShop != null)
                paymentFromShop = checkpoint.location.totalPaymentToShop.Value.ToString();
            else
                paymentFromShop = "";

            if (checkpoint.location.cashTransfer != null)
                cashTransfer = checkpoint.location.cashTransfer.Value.ToString();
            else
                cashTransfer = "";



            //station part

            if (checkpoint.station.totalCashIn != null)
                stationTotalCashIn = checkpoint.station.totalCashIn.Value.ToString();
            else
                stationTotalCashIn = "";

            if (checkpoint.station.totalCashOut != null)
                stationTotalCashOut = checkpoint.station.totalCashOut.Value.ToString();
            else
                stationTotalCashOut = "";

            if (checkpoint.station.totalStake != null)
                stationTotalStake = checkpoint.station.totalStake.Value.ToString();
            else
                stationTotalStake = "";

            if (checkpoint.station.totalWinnings != null)
                stationTotalWinning = checkpoint.station.totalWinnings.Value.ToString();
            else
                stationTotalWinning = "";

            if (checkpoint.station.tax != null)
                stationTax = checkpoint.station.tax.Value.ToString();
            else
                stationTax = "";

            if (checkpoint.station.basis != null)
                stationBasisForProfitSharing = checkpoint.station.basis.Value.ToString();
            else
                stationBasisForProfitSharing = "";

            if (checkpoint.station.fixStakeCommission != null)
                stationFixStakeCommission = checkpoint.station.fixStakeCommission.Value.ToString();
            else
                stationFixStakeCommission = "";

            if (checkpoint.station.flexCommission != null)
                stationFlexCommission = checkpoint.station.flexCommission.Value.ToString();
            else
                stationFlexCommission = "";

            if (checkpoint.station.shopOwnerShare != null)
                stationShopOwnerShare = checkpoint.station.shopOwnerShare.Value.ToString();
            else
                stationShopOwnerShare = "";

            if (checkpoint.station.subFranchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                stationSubFranchisorShare = checkpoint.station.subFranchisorShare.Value.ToString();
            else
                stationSubFranchisorShare = "";

            if (checkpoint.station.franchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                stationFranchisorShare = checkpoint.station.franchisorShare.Value.ToString();
            else
                stationFranchisorShare = "";

            if (checkpoint.station.mainOwnerShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                stationMainOwnerShare = checkpoint.station.mainOwnerShare.Value.ToString();
            else
                stationMainOwnerShare = "";

            /**/

            if (checkpoint.station.totalCreditToShop != null)
                stationCreditToShop = checkpoint.station.totalCreditToShop.Value.ToString();
            else
                stationCreditToShop = "";

            if (checkpoint.station.totalPaymentToShop != null)
                stationPaymentFromShop = checkpoint.station.totalPaymentToShop.Value.ToString();
            else
                stationPaymentFromShop = "";

            if (checkpoint.station.cashTransfer != null)
                stationCashTransfer = checkpoint.station.cashTransfer.Value.ToString();
            else
                stationCashTransfer = "";

            if (checkpoint.location.model != null)
            {
                DisplayComissions = !checkpoint.location.model.Equals("A");
                AccountingModel = checkpoint.location.model;
            }
            else
            {
                AccountingModel = string.Empty;
            }

            OnPropertyChanged("totalCashIn");
            OnPropertyChanged("totalCashOut");
            OnPropertyChanged("totalStake");
            OnPropertyChanged("totalWinning");
            OnPropertyChanged("tax");
            OnPropertyChanged("basisForProfitSharing");
            OnPropertyChanged("fixStakeCommission");
            OnPropertyChanged("flexCommission");
            OnPropertyChanged("shopOwnerShare");
            OnPropertyChanged("subFranchisorShare");
            OnPropertyChanged("franchisorShare");
            OnPropertyChanged("mainOwnerShare");
            OnPropertyChanged("creditToShop");
            OnPropertyChanged("paymentFromShop");
            OnPropertyChanged("cashTransfer");

            //station part
            OnPropertyChanged("stationTotalCashIn");
            OnPropertyChanged("stationTotalCashOut");
            OnPropertyChanged("stationTotalStake");
            OnPropertyChanged("stationTotalWinning");
            OnPropertyChanged("stationTax");
            OnPropertyChanged("stationBasisForProfitSharing");
            OnPropertyChanged("stationFixStakeCommission");
            OnPropertyChanged("stationFlexCommission");
            OnPropertyChanged("stationShopOwnerShare");
            OnPropertyChanged("stationSubFranchisorShare");
            OnPropertyChanged("stationFranchisorShare");
            OnPropertyChanged("stationMainOwnerShare");
            OnPropertyChanged("stationCreditToShop");
            OnPropertyChanged("stationPaymentFromShop");
            OnPropertyChanged("stationCashTransfer");
            OnPropertyChanged("stationModel");
            OnPropertyChanged("DisplayComissions");
            OnPropertyChanged("AccountingModel");
        }

        [AsyncMethod]
        private void StartCreatingCheckpoint()
        {
            CreateCheckpoint();
        }

        [WsdlServiceSyncAspect]
        private void CreateCheckpoint()
        {
           WsdlRepository.CreateProfitAccountingCheckpoint(Int32.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).location_id.ToString()), StationRepository.StationNumber);

            var lastcheckpoint = _Checkpoints.FirstOrDefault(x => x.IsLastCheckpoint);



            var errorWindowSettings = new ErrorSettings();
            errorWindowSettings.ErrorLevel = ErrorLevel.Critical;
            errorWindowSettings.HideButtons = true;
            errorWindowSettings.WarningVisibility = Visibility.Collapsed;
            errorWindowSettings.TextAligment = TextAlignment.Center;
            ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.SETTLEMENT_PROCESSING), errorWindowSettings);
            while (true)
            {
                Thread.Sleep(5000);
                OnLoadData();
                var newCheckpoint = _Checkpoints.FirstOrDefault(x => x.IsLastCheckpoint);
                if (newCheckpoint != null && lastcheckpoint == null)
                    break;
                if (lastcheckpoint != null && newCheckpoint != null && newCheckpoint.ProfitAccountingCheckpoint.general.endDate > lastcheckpoint.ProfitAccountingCheckpoint.general.endDate)
                {
                    break;
                }
            }
            ErrorWindowService.Close();

            SelectedCheckpoint = _Checkpoints.ElementAt(0);

            OnPropertyChanged("Checkpoints");
            TryUpdateLocationTotals();
        }

        [AsyncMethod]
        private void StartPrintReportForLocation()
        {
            PrintReportForLocation();
        }

        [WsdlServiceSyncAspect]
        private void PrintReportForLocation()
        {
            if (SelectedCheckpoint == null)
                return;

            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }

            bool isPrinted = PrinterHandler.PrintChechpointForLocation(SelectedCheckpoint.ProfitAccountingCheckpoint, ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner);
            if (!isPrinted)
                ShowPrinterErrorMessage();
        }

        [AsyncMethod]
        private void StartPrintReportForTerminal()
        {
            PrintReportForTerminal();
        }

        [WsdlServiceSyncAspect]
        private void PrintReportForTerminal()
        {
            if (SelectedCheckpoint == null)
                return;

            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }

            bool isPrinted = PrinterHandler.PrintChechpointForTerminal(SelectedCheckpoint.ProfitAccountingCheckpoint, ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner);
            if (!isPrinted)
                ShowPrinterErrorMessage();
        }

        [AsyncMethod]
        private void NextPage(string obj)
        {
            if (itemsTotal < (currentPosition + itemsAmountPerPage))
                return;

            currentPosition += itemsAmountPerPage;
            OnLoadData();
        }

        [AsyncMethod]
        private void PrevPage(string obj)
        {
            if (currentPosition == 0)
                return;

            currentPosition -= itemsAmountPerPage;
            if (currentPosition < 0)
                currentPosition = 0;

            OnLoadData();
        }

        private void OnGridCreated(UIElement obj)
        {
            if (GridHeight > 0)
                return;

            GridHeight = obj.RenderSize.Height;

            if (ItemHeight == 0.0)
                return;

            CalculatePageSize();
        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (ItemHeight > 0)
                return;

            ItemHeight = obj.RenderSize.Height;
            OnPropertyChanged("Checkpoints");

            if (GridHeight == 0.0)
                return;

            CalculatePageSize();
        }

        [AsyncMethod]
        private void CalculatePageSize()
        {
            itemsAmountPerPage = (int)(GridHeight / ItemHeight) - 1;
            OnLoadData();
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

        public override void Close()
        {
            base.Close();
        }

        #endregion
    }
}
