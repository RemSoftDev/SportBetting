using System;
using System.Collections.Generic;
using System.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Collections.ObjectModel;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// Authorization Login view model.
    /// </summary>
    [ServiceAspect]
    public class AccountingBaseViewModel : BaseViewModel
    {
        private bool _isEnabledCloseBalance;

        #region Constructors
        private static ILog Log = LogFactory.CreateLog(typeof(AccountingBaseViewModel));

        public AccountingBaseViewModel()
        {
            ShowCommand = new Command(ShowOverview);
            //CloseCommand = new Command(CloseWindow);
            CreateCheckpointCommand = new Command(OnCheckPoint);
            ShowCreditCommand = new Command(OnCredit);
            ShowCheckPointsCommand = new Command(OnShowCheckPoints);
            CheckPointsCommand = new Command(OnCheckPoints);
            BackCommand = new Command(OnBackCommand);

            EnabledCheckPoint = ChangeTracker.CurrentUser.CloseBalance;
            EnabledShowCheckPoints = ChangeTracker.CurrentUser.ViewStationBalance;
            EnabledCredit = ChangeTracker.CurrentUser.Credit;
            if (ChangeTracker.CurrentUser != null)
            {
                decimal billsAmount;
                decimal coinsamount;
                int billscount;
                int coinscount;
                var amount = WsdlRepository.GetStationCashInfo(StationRepository.StationNumber, out billsAmount, out billscount, out coinsamount, out coinscount);

                IsEnabledCloseBalance =amount > 0 && ChangeTracker.CurrentUser.EmptyBox;
            }

        }


        #endregion

        #region Properties

        public bool EnabledCheckPoint { get; set; }
        public bool EnabledShowCheckPoints { get; set; }
        public bool EnabledCredit { get; set; }
        public bool ShowBackButton { get; set; }
        public bool ShowCheckPointsButton { get; set; }
        public bool Show_ShowButton { get; set; }
        public bool IsEnabledCloseBalance
        {
            get { return _isEnabledCloseBalance; }
            set
            {
                _isEnabledCloseBalance = value;
                OnPropertyChanged();
            }
        }



        public ObservableCollection<BalanceCheckpoint> Balance
        {
            get { return ChangeTracker.Balance; }
            set
            {
                ChangeTracker.Balance = value;
                OnPropertyChanged();
            }
        }

        public string ErrorLabel
        {
            get { return ChangeTracker.ErrorLabel; }
            set
            {
                ChangeTracker.ErrorLabel = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get { return ChangeTracker.StartDateAccounting; }
            set
            {
                ChangeTracker.StartDateAccounting = value;
            }
        }





        public decimal CashInOperationsNum { get; set; }
        public decimal CashOutOperationsNum { get; set; }




        public bool IsEnabledPrintInfo
        {
            get { return _isEnabledPrintInfo; }
            set
            {
                _isEnabledPrintInfo = value;
                OnPropertyChanged();
            }
        }


        private bool _isEnabledPrintInfo;

        #endregion

        #region Commands
        public Command CreateCheckpointCommand { get; set; }
        public Command CheckPointsCommand { get; set; }
        public Command ShowCreditCommand { get; set; }
        public Command Close1Command { get; set; }
        public Command ShowCheckPointsCommand { get; set; }
        public Command BackCommand { get; set; }

        public Command ShowCommand { get; set; }
        public Command CloseCommand { get; set; }
        public Command PrintInfoCommand { get; set; }


        #endregion


        #region Methods



      


        public void CloseBalance()
        {
            decimal cashoutCurrentAmount = 0;

            var text = TranslationProvider.Translate(MultistringTags.TERMINAL_COLLECT_CASH, _cashinCurrentAmount, StationRepository.Currency);

            QuestionWindowService.ShowMessage(text, null, null, askWindow_YesClick, null);
        }

        [AsyncMethod]
        private void askWindow_YesClick(object sender, EventArgs e)
        {
            PleaseWaitCloseBalance();
        }

        private decimal _cashinCurrentAmount = 0;

        [PleaseWaitAspect]
        public void PleaseWaitCloseBalance()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }

            long cashoutId = GetLastCashoutId() + 1;
            Dictionary<Decimal, int> CashinNotes = GetNotesValuesAndCount(cashoutId);
            decimal cashoutCurrentAmount = 0;
            DateTime startdate;
            DateTime enddate;
            _cashinCurrentAmount = TransactionQueueHelper.TryRegisterMoneyOnHub(StationRepository.GetUid(ChangeTracker.CurrentUser), BusinessPropsHelper.GetNextTransactionId(), false, "STATION_CASH_OUT", (int)ChangeTracker.CurrentUser.AccountId, true,out startdate,out enddate);
            foreach (var cashinNote in CashinNotes)
            {
                Log.Debug("cashin notes:" + cashinNote.Key + " amount: " + cashinNote.Value);
            }
            PrinterHandler.PrintCashBalance(CashinNotes, StartDate, DateTime.Now, _cashinCurrentAmount, 0, _cashinCurrentAmount, false, false, ChangeTracker.CurrentUser.Username, GetNumberOfCheckpoints() + 1);
            IsEnabledCloseBalance = _cashinCurrentAmount - cashoutCurrentAmount > 0;

        }

        private int GetNumberOfCheckpoints()
        {
            string sQuery = string.Format("SELECT Count(*) FROM StationCash WHERE StationCash.CashCheckpoint=1");

            int count = 0;
            using (DataTable dt = DataCopy.GetDataTable(sQuery))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    count = DbConvert.ToInt32(dr, "count");
                }
            }
            return count;
        }








        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;
            string errorMessage = "";

            errorMessage = TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_BALANCE_NOTE).ToString() + ", ";

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

        private void OnBackCommand()
        {
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }


        private void OnShowCheckPoints()
        {
            Balance = null;
            ErrorLabel = string.Empty;
            if (MyRegionManager.CurrentViewModelType(RegionNames.UsermanagementContentRegion).ToString().Contains("BalanceCheckpointsViewModel"))
            {
                Mediator.SendMessage<bool>(true, MsgTag.ShowBalanceCheckpoints);
            }
            else
                MyRegionManager.NavigateUsingViewModel<BalanceCheckpointsViewModel>(RegionNames.UsermanagementContentRegion);
        }

        public static long GetLastCashoutId()
        {
            long lastCashoutId = 0;
            try
            {
                List<StationCashSr> lStationCash = StationCashSr.GetStationCashListByQuery("SELECT * From StationCash where StationCash.CashCheckPoint = 1 order by stationcashid desc", new List<IDbDataParameter>());

                if (lStationCash.Count > 0)
                {
                    lastCashoutId = lStationCash[0].StationCashID;
                }
            }
            catch (Exception ex)
            {

            }
            return lastCashoutId;
        }

        public static Dictionary<Decimal, int> GetNotesValuesAndCount(long startId, long endId = long.MaxValue)
        {

            var cashinNotes = new Dictionary<decimal, int>();
            try
            {
                List<IDbDataParameter> lParams = new List<IDbDataParameter>
                    {
                        SqlObjectFactory.CreateParameter("lastCashoutStartId", startId, string.Empty), 
                        SqlObjectFactory.CreateParameter("lastCashoutEndId", endId, string.Empty)
                    };


                List<StationCashSr> lStationCash = StationCashSr.GetStationCashListByQuery("SELECT * FROM StationCash where stationcashid >= @lastCashoutStartId and stationcashid <= @lastCashoutEndId order by stationcashid desc", lParams);



                foreach (StationCashSr stationCashBo in lStationCash)
                {
                    if (stationCashBo.MoneyIn)
                    {
                        if (cashinNotes.ContainsKey(stationCashBo.Cash))
                        {
                            cashinNotes[stationCashBo.Cash]++;
                        }
                        else
                        {
                            cashinNotes.Add(stationCashBo.Cash, 1);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return cashinNotes;
        }

        public static Dictionary<Decimal, int> GetNotesValuesAndCountFromCollection(List<CashOperation> operations)
        {
            var cashinNotes = new Dictionary<decimal, int>();
            try
            {
                foreach (CashOperation oper in operations)
                {
                    if (oper.Amount >= 0)
                    {
                        if (cashinNotes.ContainsKey(oper.Amount))
                        {
                            cashinNotes[oper.Amount]++;
                        }
                        else
                        {
                            cashinNotes.Add(oper.Amount, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return cashinNotes;
        }


        private void OnCheckPoints()
        {
            MyRegionManager.NavigateUsingViewModel<FilterViewModel>(RegionNames.UsermanagementContentRegion);
        }

        [AsyncMethod]
        private void OnCheckPoint()
        {
            CheckPoint();
        }

        [WsdlServiceSyncAspect]
        private void CheckPoint()
        {
            try
            {
                decimal amount;
                WsdlRepository.CreateCheckpoint(StationRepository.StationNumber, (int)ChangeTracker.CurrentUser.AccountId, out amount);
                Mediator.SendMessage(true, MsgTag.CreatedCheckpoint);
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    case 176: // 113, 114, 172
                        ShowError(TranslationProvider.Translate(MultistringTags.CANNOT_CREATE_CHECKPOINT).ToString());
                        return;
                    default: // 113, 114, 172
                        ShowError(exception.Detail.message);
                        return;
                }
            }
            catch (System.ServiceModel.FaultException)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR).ToString());
                return;
            }

            ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_DONE).ToString());
        }


        private void OnCredit()
        {
            MyRegionManager.NavigateUsingViewModel<CreditViewModel>(RegionNames.UsermanagementContentRegion);
        }

        [AsyncMethod]
        public void ShowOverview()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.NavigateUsingViewModel<ResultsViewModel>(RegionNames.UsermanagementContentRegion);
        }



        [PleaseWaitAspect]
        public void CloseWindow()
        {

            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
            if (ShowBackButton)
                MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);

        }

        protected void TryUpdateLocationTotals()
        {
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
        }
        #endregion
    }
}