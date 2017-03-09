using System;
using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Windows;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class ShowReportViewModel : AccountingBaseViewModel
    {
        public ShowReportViewModel()
        {
            onPrintReportForLocation = new Command(StartPrintReportForLocation);
            onPrintReportForTerminal = new Command(StartPrintReportForTerminal);
        }

        public override void OnNavigationCompleted()
        {
            //load data
            if (ChangeTracker.CreateFromLastCheckpoint)
            {
                SelectedCheckpoint = WsdlRepository.GetProfitAccountingReport(Int32.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).location_id.ToString()), StationRepository.StationNumber, null, null);
            }
            else
            {
                if (ChangeTracker.ProfitReportStartDate == ChangeTracker.ProfitReportEndDate)
                {
                    DateTime dtX = new DateTime(
                        ChangeTracker.ProfitReportStartDate.Year,
                        ChangeTracker.ProfitReportStartDate.Month,
                        ChangeTracker.ProfitReportStartDate.Day,
                        0,0,0);
                    ChangeTracker.ProfitReportStartDate = dtX;
                }
                SelectedCheckpoint = WsdlRepository.GetProfitAccountingReport(Int32.Parse(StationRepository.GetUid(ChangeTracker.CurrentUser).location_id.ToString()), StationRepository.StationNumber, ChangeTracker.ProfitReportStartDate, ChangeTracker.ProfitReportEndDate);
            }
            loadProperties(SelectedCheckpoint);

            try
            {
                decimal locationCashPosition, totalStationCash, totalLocationCash, totalLocationPaymentBalance;
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

        #region Properties

        public Visibility FranchisorShareVisibility
        {
            get
            {
                return ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private ProfitAccountingCheckpoint _selectedCheckpoint = null;
        public ProfitAccountingCheckpoint SelectedCheckpoint
        {
            get { return _selectedCheckpoint; }
            set
            {
                if (value == null)
                    return;

                _selectedCheckpoint = value;
            }
        }

        #endregion

        #region Methods

        private void loadProperties(ProfitAccountingCheckpoint checkpoint)
        {
            string specifier = "F";
            if (checkpoint == null)
                return;

            if (checkpoint.location.totalCashIn != null)
                totalCashIn = checkpoint.location.totalCashIn.Value.ToString(specifier);
            else
                totalCashIn = "";

            if (checkpoint.location.totalCashOut != null)
                totalCashOut = checkpoint.location.totalCashOut.Value.ToString(specifier);
            else
                totalCashOut = "";

            if (checkpoint.location.totalStake != null)
                totalStake = checkpoint.location.totalStake.Value.ToString(specifier);
            else
                totalStake = "";

            if (checkpoint.location.totalWinnings != null)
                totalWinning = checkpoint.location.totalWinnings.Value.ToString(specifier);
            else
                totalWinning = "";

            if (checkpoint.location.tax != null)
                tax = checkpoint.location.tax.Value.ToString(specifier);
            else
                tax = "";

            if (checkpoint.location.basis != null)
                basisForProfitSharing = checkpoint.location.basis.Value.ToString(specifier);
            else
                basisForProfitSharing = "";

            if (checkpoint.location.fixStakeCommission != null)
                fixStakeCommission = checkpoint.location.fixStakeCommission.Value.ToString(specifier);
            else
                fixStakeCommission = "";

            if (checkpoint.location.flexCommission != null)
                flexCommission = checkpoint.location.flexCommission.Value.ToString(specifier);
            else
                flexCommission = "";

            if (checkpoint.location.shopOwnerShare != null)
                shopOwnerShare = checkpoint.location.shopOwnerShare.Value.ToString(specifier);
            else
                shopOwnerShare = "";

            if (checkpoint.location.subFranchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                subFranchisorShare = checkpoint.location.subFranchisorShare.Value.ToString(specifier);
            else
                subFranchisorShare = "";

            if (checkpoint.location.franchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                franchisorShare = checkpoint.location.franchisorShare.Value.ToString(specifier);
            else
                franchisorShare = "";

            if (checkpoint.location.mainOwnerShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                mainOwnerShare = checkpoint.location.mainOwnerShare.Value.ToString(specifier);
            else
                mainOwnerShare = "";

            /**/

            if (checkpoint.location.totalCreditToShop != null)
                creditToShop = checkpoint.location.totalCreditToShop.Value.ToString(specifier);
            else
                creditToShop = "";

            if (checkpoint.location.totalPaymentToShop != null)
                paymentFromShop = checkpoint.location.totalPaymentToShop.Value.ToString(specifier);
            else
                paymentFromShop = "";

            if (checkpoint.location.cashTransfer != null)
                cashTransfer = checkpoint.location.cashTransfer.Value.ToString(specifier);
            else
                cashTransfer = "";



            //station part

            if (checkpoint.station.totalCashIn != null)
                stationTotalCashIn = checkpoint.station.totalCashIn.Value.ToString(specifier);
            else
                stationTotalCashIn = "";

            if (checkpoint.station.totalCashOut != null)
                stationTotalCashOut = checkpoint.station.totalCashOut.Value.ToString(specifier);
            else
                stationTotalCashOut = "";

            if (checkpoint.station.totalStake != null)
                stationTotalStake = checkpoint.station.totalStake.Value.ToString(specifier);
            else
                stationTotalStake = "";

            if (checkpoint.station.totalWinnings != null)
                stationTotalWinning = checkpoint.station.totalWinnings.Value.ToString(specifier);
            else
                stationTotalWinning = "";

            if (checkpoint.station.tax != null)
                stationTax = checkpoint.station.tax.Value.ToString(specifier);
            else
                stationTax = "";

            if (checkpoint.station.basis != null)
                stationBasisForProfitSharing = checkpoint.station.basis.Value.ToString(specifier);
            else
                stationBasisForProfitSharing = "";

            if (checkpoint.station.fixStakeCommission != null)
                stationFixStakeCommission = checkpoint.station.fixStakeCommission.Value.ToString(specifier);
            else
                stationFixStakeCommission = "";

            if (checkpoint.station.flexCommission != null)
                stationFlexCommission = checkpoint.station.flexCommission.Value.ToString(specifier);
            else
                stationFlexCommission = "";

            if (checkpoint.station.shopOwnerShare != null)
                stationShopOwnerShare = checkpoint.station.shopOwnerShare.Value.ToString(specifier);
            else
                stationShopOwnerShare = "";

            if (checkpoint.station.subFranchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                stationSubFranchisorShare = checkpoint.station.subFranchisorShare.Value.ToString(specifier);
            else
                stationSubFranchisorShare = "";

            if (checkpoint.station.franchisorShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                stationFranchisorShare = checkpoint.station.franchisorShare.Value.ToString(specifier);
            else
                stationFranchisorShare = "";

            if (checkpoint.station.mainOwnerShare != null && !ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner)
                stationMainOwnerShare = checkpoint.station.mainOwnerShare.Value.ToString(specifier);
            else
                stationMainOwnerShare = "";

            /**/

            if (checkpoint.station.totalCreditToShop != null)
                stationCreditToShop = checkpoint.station.totalCreditToShop.Value.ToString(specifier);
            else
                stationCreditToShop = "";

            if (checkpoint.station.totalPaymentToShop != null)
                stationPaymentFromShop = checkpoint.station.totalPaymentToShop.Value.ToString(specifier);
            else
                stationPaymentFromShop = "";

            if (checkpoint.station.cashTransfer != null)
                stationCashTransfer = checkpoint.station.cashTransfer.Value.ToString(specifier);
            else
                stationCashTransfer = "";

            if (checkpoint.location.model != null)
            {
                DisplayComissions = !checkpoint.location.model.Equals("A");
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

            bool isPrinted = PrinterHandler.PrintChechpointForLocation(SelectedCheckpoint, ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner);
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

            bool isPrinted = PrinterHandler.PrintChechpointForTerminal(SelectedCheckpoint, ChangeTracker.CurrentUser.ShopPaymentsReadLocationOwner);
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

        #endregion

        #region Properties

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

        #endregion

        #region Commands

        public Command onPrintReportForLocation { get; private set; }
        public Command onPrintReportForTerminal { get; private set; }

        #endregion
    }
}
