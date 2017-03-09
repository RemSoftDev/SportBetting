using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportBetting.WPF.Prism.Shared;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Collections.ObjectModel;
using ComboBoxItem = SportBetting.WPF.Prism.Shared.Models.ComboBoxItem;
using SportRadar.Common;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Authorization Login view model.
    /// </summary>
    [ServiceAspect]
    public class ResultsViewModel : AccountingBaseViewModel
    {
        private readonly ScrollViewerModule _ScrollViewerModule;

        #region Constructors

        public ResultsViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            Close1Command = new Command(CloseBalance);
            PrintInfoCommand = new Command(PrintInfo);

            IsEnabledPrintInfo = true;

            _cashOperations = new ObservableCollection<CashOperation>();
            setOperationTypes();

            Mediator.Register<string>(this, RefreshCashOperationsFromMessage, MsgTag.RefreshCashOperations);

            var scroller = this.GetScrollviewerForActiveWindow();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            Mediator.Register<string>(this, OnScrollDownStartExecute, MsgTag.LoadNextPage);
            Mediator.Register<string>(this, OnScrollUpStartExecute, MsgTag.LoadPrevPage);

            RefreshCashOperationsFromMessage("");
        }

        #endregion


        #region Properties
        public ObservableCollection<ComboBoxItem> OperationTypes { get; set; }
        private int _selectedOperationTypeIndex = -1;
        public int SelectedOperationTypeIndex
        {
            get { return _selectedOperationTypeIndex; }
            set
            {
                if (_selectedOperationTypeIndex != value)
                    OperationType_SelectionChanged(value);
                _selectedOperationTypeIndex = value;
            }
        }

        public decimal CashInOperationsNum { get; set; }
        public decimal CashOutOperationsNum { get; set; }
        public List<OperationTypeSelection> OperationType { get; set; }
        /// <summary>
        /// Gets the selected start date.
        /// </summary>
        private DateTime StartDate
        {
            get { return ChangeTracker.StartDateAccounting; }
        }

        /// <summary>
        /// Gets the selected end date.
        /// </summary>
        private DateTime EndDate
        {
            get { return ChangeTracker.EndDateAccounting; }
        }

        /// <summary>
        /// Gets if it should have PayIn
        /// </summary>
        private bool PayIn
        {
            get { return ChangeTracker.CashInAccounting; }
            set { ChangeTracker.CashInAccounting = value; }
        }

        /// <summary>
        /// Gets the it should have PayOut
        /// </summary>
        private bool PayOut
        {
            get { return ChangeTracker.CashOutAccounting; }
            set { ChangeTracker.CashOutAccounting = value; }
        }




        private ObservableCollection<CashOperation> _cashOperations = new ObservableCollection<CashOperation>();

        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public ObservableCollection<CashOperation> CashOperations
        {
            get { return _cashOperations; }
            set { _cashOperations = value; }
        }



        public bool IsEnabledPrintInfo
        {
            get { return _isEnabledPrintInfo; }
            set
            {
                _isEnabledPrintInfo = value;
                OnPropertyChanged("IsEnabledPrintInfo");
            }
        }


        private bool _isEnabledPrintInfo;



        /// <summary>
        /// Register the UserName property so it is known in the class.
        /// </summary>

        #endregion

        #region Commands
        public Command ScrollDownStart { get; private set; }
        /// <summary>
        /// Gets the ScrollDownStop command.
        /// </summary>
        public Command ScrollDownStop { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStart command.
        /// </summary>
        public Command ScrollUpStart { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStop command.
        /// </summary>
        public Command ScrollUpStop { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            var scroller = this.GetScrollviewerForActiveWindow();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            base.OnNavigationCompleted();
        }

        [AsyncMethod]
        public void PrintInfo()
        {
            PleaseWaitPrintInfo();
        }

        [PleaseWaitAspect]
        public void PleaseWaitPrintInfo()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage("info");
                return;
            }
            var end = EndDate;
            if (end == DateTime.MinValue)
            {
                end = DateTime.Now;
            }

            Dictionary<Decimal, int> cashinNotes = GetNotesValuesAndCountFromCollection(CashOperations.ToList());

            PrinterHandler.PrintCashBalance(cashinNotes, StartDate, end, CashInOperationsNum, CashOutOperationsNum, 0, false, true, "", 0);

        }

        private void ShowPrinterErrorMessage(string type)
        {
            int status = PrinterHandler.currentStatus;
            string errorMessage = "";

            if (type == "info")
                errorMessage = TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_INFO_NOTE).ToString() + ", ";
            else
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

        public void setOperationTypes()
        {
            if (OperationType == null)
                OperationType = new List<OperationTypeSelection>();
            else OperationType.Clear();
            OperationType.Add(new OperationTypeSelection() { Index = 0, Value = TranslationProvider.Translate(MultistringTags.TERMINAL_CAHSIN).ToString(), OperationType = "TERMINAL_CASHIN", CashIn = true, CashOut = false });
            OperationType.Add(new OperationTypeSelection() { Index = 1, Value = TranslationProvider.Translate(MultistringTags.TERMINAL_CASHOUT).ToString(), OperationType = "TERMINAL_CASHOUT", CashIn = false, CashOut = true });
            OperationType.Add(new OperationTypeSelection() { Index = 2, Value = TranslationProvider.Translate(MultistringTags.TERMINAL_ALL).ToString(), OperationType = "TERMINAL_ALL", CashIn = true, CashOut = true });
            if (PayIn && !PayOut) SelectedOperationTypeIndex = 0;
            if (!PayIn && PayOut) SelectedOperationTypeIndex = 1;
            if (PayIn && PayOut) SelectedOperationTypeIndex = 2;
            if (!PayIn && !PayOut) SelectedOperationTypeIndex = 2;
            OnPropertyChanged("SelectedOperationTypeIndex");
        }


        private void OperationType_SelectionChanged(int value)
        {
            OperationTypeSelection val = OperationType[value];

            PayIn = val.CashIn;
            PayOut = val.CashOut;

            RefreshCashOperationsFromMessage("");
        }

        private void RefreshCashOperationsFromMessage(string additionalString)
        {
            CashOperations = GetCashOperations(0, 1);
            OnPropertyChanged("CashOperations");
        }

        [WsdlServiceAsyncAspect]
        private ObservableCollection<CashOperation> GetCashOperations(long startId, long endId = long.MaxValue)
        {
            var cashOperations = new ObservableCollection<CashOperation>();
            decimal cashinValue = 0;
            decimal cashoutValue = 0;

            if (startId == 0 && endId == 0)
            {
                CashInOperationsNum = cashinValue;
                CashOutOperationsNum = cashoutValue;
                OnPropertyChanged("CashInOperationsNum");
                OnPropertyChanged("CashOutOperationsNum");
                return cashOperations;
            }

            try
            {
                CashOperationtData[] lStationCash;

                if (ChangeTracker.FromCheckPointsAccounting && EndDate == DateTimeUtils.DATETIME1700)
                {
                    lStationCash = WsdlRepository.GetStationCashHistory(StationRepository.StationNumber, StartDate, ChangeTracker.CalendarEndDateAccounting.AddSeconds(1));
                }
                else
                {
                    lStationCash = WsdlRepository.GetStationCashHistory(StationRepository.StationNumber, StartDate, EndDate.AddSeconds(1));
                }

                foreach (CashOperationtData stationCashBo in lStationCash)
                {
                    decimal multiplier;
                    string type = stationCashBo.operation_type;
                    string translation;
                    if (type == "CASH_IN")
                    {
                        multiplier = 1;
                        cashinValue += stationCashBo.amount;
                        translation = TranslationProvider.Translate(MultistringTags.STATION_CASH_IN).ToString();
                    }
                    else if (type == "EMPTY_BOX")
                    {
                        multiplier = -1;
                        cashoutValue += stationCashBo.amount;
                        translation = TranslationProvider.Translate(MultistringTags.STATION_CASH_OUT).ToString();
                    }
                    else
                    {
                        translation = "";
                        multiplier = 1;
                    }

                    cashOperations.Add(new CashOperation(0, stationCashBo.amount * multiplier, stationCashBo.created_at, translation,
                        !string.Equals(stationCashBo.operator_name, ChangeTracker.CurrentUser.Username) ? StationRepository.StationTyp : stationCashBo.operator_name));
                }
            }
            catch (Exception)
            {

            }
            CashInOperationsNum = cashinValue;
            CashOutOperationsNum = cashoutValue;
            OnPropertyChanged("CashInOperationsNum");
            OnPropertyChanged("CashOutOperationsNum");
            return cashOperations;
        }

        private void OnScrollDownStartExecute(string aaa)
        {
            //Mediator.SendMessage("", "ScrollDown");
            this._ScrollViewerModule.OnScrollDownStartExecute(this.GetScrollviewerForActiveWindow(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        private void OnScrollUpStartExecute(string aaa)
        {
            this._ScrollViewerModule.OnScrollUpStartExecute(this.GetScrollviewerForActiveWindow(), true);
        }

        #endregion
    }
}