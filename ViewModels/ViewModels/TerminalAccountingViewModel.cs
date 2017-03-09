using System;
using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Collections.ObjectModel;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class TerminalAccountingViewModel : AccountingBaseViewModel
    {
        #region Constructors

        public TerminalAccountingViewModel()
        {
            Mediator.Register<Tuple<string, string>>(this, ShowNoConnectionErrorMessage, MsgTag.Error);

            PrintAccountReceiptForLocation = new Command(onPrintAccountReceiptForLocation);
            PrintAccountReceiptForTerminal = new Command(onPrintAccountReceiptForTerminal);

            StartDate = ChangeTracker.StartDateAccounting;
            EndDate = ChangeTracker.EndDateAccounting;
            CalStartDate = ChangeTracker.CalendarStartDateAccounting;
            CalEndDate = ChangeTracker.CalendarEndDateAccounting;
            if (CalStartDate == CalEndDate)
            {
                StartDate = StartDate == DateTime.MinValue ? DateTime.Today.AddDays(-7) : StartDate;
                EndDate = EndDate == DateTime.MinValue ? DateTime.Today.AddDays(1).AddSeconds(-1) : EndDate;
                CalStartDate = StartDate;
                CalEndDate = EndDate;
            }
            ShowFromDateWindowCommand = new Command(OnShowFromDateWindowExecute);
            ShowToDateWindowCommand = new Command(OnShowToDateWindowCommandExecute);

            CheckDates();

            ChooseFromCheckpoints = false;
            if (!ChooseFromCheckpoints)
            {
                CalStartDate = StartDate;
                CalEndDate = EndDate;
            }
        }

        #endregion


        #region Properties

        private DateTime curStartdate;
        private DateTime _startdate = DateTime.MinValue;
        private DateTime _enddate = DateTime.MinValue;
        private DateTime oldStartdate = DateTime.MinValue;
        private DateTime oldEnddate = DateTime.MinValue;
        private DateTime oldCalStartdate = DateTime.MinValue;
        private DateTime oldCalEnddate = DateTime.MinValue;

        public ObservableCollection<DateCollection> EndDateFilters { get; set; }
        private int _selectedEndDateFilterIndex;
        public int SelectedEndDateFilterIndex
        {
            get { return _selectedEndDateFilterIndex; }
            set
            {
                int oldval = _selectedEndDateFilterIndex;
                _selectedEndDateFilterIndex = value;
                if (oldval != value && value >= 0)
                    SelectedEndDate_SelectionChanged(value);

                OnPropertyChanged("SelectedEndDateFilterIndex");
            }
        }

        public ObservableCollection<DateCollection> StartDateFilters { get; set; }
        private int _selectedStartDateFilterIndex;
        public int SelectedStartDateFilterIndex
        {
            get { return _selectedStartDateFilterIndex; }
            set
            {
                int oldval = _selectedStartDateFilterIndex;
                _selectedStartDateFilterIndex = value;
                if (oldval != value && value >= 0)
                {
                    SelectedStartDate_SelectionChanged(StartDateFilters[value].Date, value);
                    CheckDates();
                }
                OnPropertyChanged("SelectedStartDateFilterIndex");
            }
        }

        bool _chooseFromCheckpoints;
        public bool ChooseFromCheckpoints
        {
            get
            {
                return _chooseFromCheckpoints;
            }
            set
            {
                _chooseFromCheckpoints = value;
                if (value)
                {
                    CheckDates();
                }
                else
                {
                    StartDate = CalStartDate;
                    EndDate = CalEndDate;
                }
                OnPropertyChanged("ChooseFromCheckpoints");
            }
        }

        bool _disableButtons;
        private DateTime _calStartDate = DateTime.Today;
        private DateTime _calEndDate = DateTime.Today;

        public bool DisableButtons
        {
            get
            {
                return !_disableButtons;
            }
            set
            {
                _disableButtons = value;
                OnPropertyChanged("DisableButtons");
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime StartDate
        {
            get { return _startdate; }
            set
            {
                _startdate = value;
                ChangeTracker.StartDateAccounting = value;
                CheckDates();
            }
        }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime EndDate
        {
            get { return _enddate; }
            set
            {
                _enddate = value;
                ChangeTracker.EndDateAccounting = value;
                CheckDates();
            }
        }


        public DateTime CalStartDate
        {
            get { return _calStartDate; }
            set
            {
                _calStartDate = value;
                OnPropertyChanged();
                ChangeTracker.CalendarStartDateAccounting = value;
            }
        }

        public DateTime CalEndDate
        {
            get { return _calEndDate; }
            set
            {
                _calEndDate = value;
                ChangeTracker.CalendarEndDateAccounting = value;
                OnPropertyChanged();
            }
        }

        #endregion


        #region Commands

        public Command PrintAccountReceiptForLocation { get; private set; }
        public Command PrintAccountReceiptForTerminal { get; private set; }

        public Command ShowFromDateWindowCommand { get; private set; }
        public Command ShowToDateWindowCommand { get; private set; }

        #endregion


        #region Methods

        public override void OnNavigationCompleted()
        {
            ChangeTracker.TerminalAccountingChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_ADMIN_TERMINAL_ACCOUNTING;
            base.OnNavigationCompleted();
        }
        private void OnShowFromDateWindowExecute()
        {
            CalStartDate = DateHelper.SelectDate(this.CalStartDate) ?? DateTime.Today;
            if ((!ChooseFromCheckpoints || SelectedStartDateFilterIndex == 0) || StartDate < CalStartDate)
            {
                StartDate = CalStartDate;
                SelectedStartDate_SelectionChanged(StartDate, SelectedStartDateFilterIndex);
                CheckDates();
            }
            if (CalStartDate > CalEndDate)
                CalEndDate = CalStartDate.Add(new TimeSpan(23, 59, 59));
            if (EndDate < CalEndDate)
            {
                EndDate = CalEndDate;
            }
            this.OnPropertyChanged("CalStartDate");
        }

        private void CheckDates()
        {
            if (oldStartdate != StartDate || oldEnddate != EndDate || oldCalStartdate != CalStartDate || oldCalEnddate != CalEndDate)
            {
                oldStartdate = StartDate;
                oldEnddate = EndDate;
                oldCalStartdate = CalStartDate;
                oldCalEnddate = CalEndDate;
                if (StartDateFilters == null) StartDateFilters = new ObservableCollection<DateCollection>();
                if (EndDateFilters == null) EndDateFilters = new ObservableCollection<DateCollection>();
                SetDates(null);
            }
        }


        private void OnShowToDateWindowCommandExecute()
        {
            DateTime end = DateHelper.SelectDate(this.CalEndDate, this.StartDate) ?? DateTime.Today;
            end = end.Date;
            end = end.AddHours(23);
            end = end.AddMinutes(59);
            end = end.AddSeconds(59);
            CalEndDate = end;
            if ((!ChooseFromCheckpoints || SelectedEndDateFilterIndex == 0) || this.EndDate < CalEndDate)
            {
                this.EndDate = CalEndDate;
                CheckDates();
            }
            this.OnPropertyChanged("CalEndDate");
        }


        public void SetDates(StationCashSr[] start)
        {
            var operatorUser = ChangeTracker.CurrentUser as SportBetting.WPF.Prism.Models.OperatorUser;
            if (ChangeTracker.CurrentUser.GetType() == typeof(SportBetting.WPF.Prism.Models.OperatorUser))
            {
                int startIdx = SelectedStartDateFilterIndex;
                int endIdx = SelectedEndDateFilterIndex;

                StartDateFilters.Clear();
                EndDateFilters.Clear();
                long idx = 1;
                DateCollection noDate = new DateCollection()
                {
                    Date = DateTimeUtils.DATETIME1700,
                    Index = 0,
                    Value =
                        TranslationProvider.Translate(
                            MultistringTags.TERMINAL_FROM_CALENDER).ToString()
                };
                StartDateFilters.Add(noDate);
                EndDateFilters.Add(noDate);

                if (start != null && start.Length > 0)
                {
                    foreach (StationCashSr cash in start)
                    {
                        DateCollection date = new DateCollection()
                        {
                            Date = cash.DateModified,
                            Index = idx,
                            Value = cash.DateModified + " (" + (
                            ((SportBetting.WPF.Prism.Models.OperatorUser)ChangeTracker.CurrentUser).Username == cash.OperatorID ? cash.OperatorID : StationRepository.StationTyp) + ")"
                        };
                        if (CalStartDate <= cash.DateModified) StartDateFilters.Add(date);
                        if (cash.DateModified > StartDate && CalEndDate <= cash.DateModified) EndDateFilters.Add(date);

                        idx++;
                    }
                }
                if (idx <= 1)
                {
                    DateTime creationDate = DateTimeUtils.DATETIMENULL;
                    if (StationRepository.Created_At != null)
                        creationDate = StationRepository.Created_At.Value;
                    DateCollection date = new DateCollection()
                    {
                        Date = creationDate,
                        Index = idx,
                        Value = creationDate.ToString()
                    };
                    StartDateFilters.Add(date);
                    EndDateFilters.Add(date);
                }
                if (StartDate > CalEndDate)
                {
                    CalEndDate = StartDate.Date.Add(new TimeSpan(23, 59, 59));
                }
                if (StartDateFilters.Count - 1 >= startIdx)
                    SelectedStartDateFilterIndex = startIdx;
                else SelectedStartDateFilterIndex = StartDateFilters.Count - 1;
                if (EndDateFilters.Count - 1 >= endIdx)
                    SelectedEndDateFilterIndex = endIdx;
                else SelectedEndDateFilterIndex = EndDateFilters.Count - 1;
            }
        }

        private void SelectedStartDate_SelectionChanged(DateTime startDate, int value)
        {
            //int idx = SelectedEndDateFilterIndex;
            //EndDateFilters.Clear();
            //foreach (DateCollection startDateFilter in StartDateFilters)
            //{
            //    if ((startDateFilter.Date > startDate && EndDate <= startDateFilter.Date) || startDateFilter.Date == DateTimeUtils.DATETIME1700)
            //        EndDateFilters.Add(startDateFilter);
            //}
            //if (EndDateFilters.Count - 1 >= idx)
            //    SelectedEndDateFilterIndex = idx;
            //else SelectedEndDateFilterIndex = EndDateFilters.Count - 1;
            //CheckDates();
            StartDate = value > 0 ? startDate : CalStartDate;
            if (SelectedEndDateFilterIndex == 0)
            {
                if (EndDate < StartDate) EndDate = DateTime.Today.AddDays(1).AddMinutes(-1);
                OnPropertyChanged("CalEndDate");
            }
        }

        private void SelectedEndDate_SelectionChanged(int value)
        {
            EndDate = value > 0 ? EndDateFilters[value].Date : CalEndDate;
        }

        /// <summary>
        /// Print Account Receipt For Location
        /// </summary>

        [AsyncMethod]
        private void onPrintAccountReceiptForLocation()
        {
            //ShowError("Printing the Account Receipt for Location !!!");
            PleaseWaitPrintAccountReceipt("location");
        }

        /// <summary>
        /// Print Account Receipt For Terminal
        /// </summary>

        [AsyncMethod]
        private void onPrintAccountReceiptForTerminal()
        {
            //ShowError("Printing the Account Receipt for Terminal !!!");
            PleaseWaitPrintAccountReceipt("terminal");
        }

        [WsdlServiceSyncAspect]
        private void PleaseWaitPrintAccountReceipt(string type)
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage(type);
                return;
            }

            //get data for report
            AccountingRecieptWS accReciept;
            if (type == "location")
                accReciept = WsdlRepository.GetAccountingRecieptData(StartDate, EndDate, StationRepository.StationNumber, StationRepository.LocationID);
            else
                accReciept = WsdlRepository.GetAccountingRecieptData(StartDate, EndDate, StationRepository.StationNumber, null);

            if (accReciept != null)
            {
                bool result = PrinterHandler.PrintAccountReceipt(type, accReciept, StartDate, EndDate);

                if (!result)
                    ShowPrinterErrorMessage(type);
            }
        }


        private void ShowNoConnectionErrorMessage(Tuple<string, string> ssErrorTuple)
        {
            string sErrorMsg = ssErrorTuple.Item1;
            if (sErrorMsg == "LostInternetConnection")
            {
                DisableButtons = true;
                ShowError(TranslationProvider.Translate(MultistringTags.SHOP_FORM_NO_CONNECTION_TO_SERVER).ToString());
            }
        }

        private void ShowPrinterErrorMessage(string type)
        {
            int status = PrinterHandler.currentStatus;
            string errorMessage = "";

            switch (type)
            {
                case "location":
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_ACCOUNT_RECEIPT_LOCATION).ToString() + ", ";
                    break;
                case "terminal":
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_ACCOUNT_RECEIPT_TERMINAL).ToString() + ", ";
                    break;
            }

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

            DisableButtons = true;
            ShowError(errorMessage, null, true);
        }

        #endregion
    }
}
