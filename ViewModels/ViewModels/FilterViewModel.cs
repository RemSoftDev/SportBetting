using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.Common;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using System.Collections.ObjectModel;
using SportBetting.WPF.Prism.Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Authorization Login view model.
    /// </summary>
    [ServiceAspect]
    public class FilterViewModel : AccountingBaseViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public FilterViewModel()
        {
            var startdate = ChangeTracker.StartDateAccounting;
            _startdate = ChangeTracker.StartDateAccounting;
            var enddate = ChangeTracker.EndDateAccounting;
            _enddate = ChangeTracker.EndDateAccounting;
            if (ChangeTracker.CalendarStartDateAccounting == ChangeTracker.CalendarEndDateAccounting)
            {
                _startdate = StartDate == DateTime.MinValue ? DateTime.Today.AddDays(-7) : StartDate;
                _enddate = EndDate == DateTime.MinValue ? DateTime.Today.AddDays(1).AddSeconds(-1) : EndDate;
                ChangeTracker.CalendarStartDateAccounting = StartDate;
                ChangeTracker.CalendarEndDateAccounting = EndDate;
            }
            ShowFromDateWindowCommand = new Command(OnShowFromDateWindowExecute);
            ShowToDateWindowCommand = new Command(OnShowToDateWindowCommandExecute);
            IsEnabledPrintInfo = false;
            dates = GetStartDates();

            bool cashin = ChangeTracker.CashInAccounting;
            bool cashout = ChangeTracker.CashOutAccounting;
            if (cashin && !cashout)
            {
                cashOutChecked = false;
                allChecked = false;
                CashInChecked = true;
            }
            else if (!cashin && cashout)
            {
                cashInChecked = false;
                allChecked = false;
                CashOutChecked = true;
            }
            else
            {
                cashInChecked = false;
                cashOutChecked = false;
                AllChecked = true;
            }

            CheckDates(true);
            startdate = startdate == DateTime.MinValue ? StartDate : startdate;
            enddate = enddate == DateTime.MinValue ? EndDate : enddate;
            ChooseFromCheckpoints = ChangeTracker.FromCheckPointsAccounting;
            if (ChooseFromCheckpoints)
            {
                for (int i = 0; i < StartDateFilters.Count; i++)
                {
                    DateCollection startDateFilter = StartDateFilters[i];
                    if (startDateFilter.Date == startdate)
                    {
                        SelectedStartDateFilterIndex = startDateFilter;
                        break;
                    }
                }
                for (int i = 0; i < EndDateFilters.Count; i++)
                {
                    DateCollection dateCollection = EndDateFilters[i];
                    if (dateCollection.Date == enddate)
                    {
                        SelectedEndDateFilterIndex = dateCollection;
                        break;
                    }
                }
            }
            else
            {
                ChangeTracker.CalendarStartDateAccounting = StartDate;
                ChangeTracker.CalendarEndDateAccounting = EndDate;
            }

            Show_ShowButton = true;
        }


        #endregion


        #region Properties

        private IList<StationCashSr> dates;
        private bool cashInChecked;
        private bool cashOutChecked;
        private bool allChecked;
        private bool _isEnabledPrintInfo;
        private DateTime _startdate = DateTime.MinValue;
        private DateTime _enddate = DateTime.MinValue;

        public bool ShowResults { get; set; }


        public ObservableCollection<DateCollection> EndDateFilters
        {
            get { return _endDateFilters; }
            set { _endDateFilters = value; }
        }

        private DateCollection _selectedEndDateFilterIndex;
        public DateCollection SelectedEndDateFilterIndex
        {
            get { return _selectedEndDateFilterIndex; }
            set
            {
                _selectedEndDateFilterIndex = value;
                if (value != null)
                    SelectedEndDate_SelectionChanged(value);

                OnPropertyChanged("SelectedEndDateFilterIndex");
            }
        }
        public ObservableCollection<DateCollection> StartDateFilters
        {
            get { return _startDateFilters; }
            set { _startDateFilters = value; }
        }

        private DateCollection _selectedStartDateFilterIndex;
        public DateCollection SelectedStartDateFilterIndex
        {
            get { return _selectedStartDateFilterIndex; }
            set
            {
                _selectedStartDateFilterIndex = value;
                {
                    if (value != null)
                    {
                        SelectedStartDate_SelectionChanged(value.Date, value);
                        CheckDates(false);
                    }
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
                ChangeTracker.FromCheckPointsAccounting = value;
                if (value)
                {
                    CheckDates(true);
                }
                else
                {
                    StartDate = ChangeTracker.CalendarStartDateAccounting;
                    EndDate = ChangeTracker.CalendarEndDateAccounting;
                }

                if (ChooseFromCheckpoints)
                {
                    foreach (DateCollection startDateFilter in StartDateFilters)
                    {
                        if (startDateFilter.Date == StartDate)
                        {
                            SelectedStartDateFilterIndex = startDateFilter;
                            break;
                        }
                    }
                    foreach (DateCollection dateCollection in EndDateFilters)
                    {
                        if (dateCollection.Date == EndDate)
                        {
                            SelectedEndDateFilterIndex = dateCollection;
                            break;
                        }
                    }
                }

                OnPropertyChanged("ChooseFromCheckpoints");
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
                CheckDates(false);
            }
        }


        public DateTime EndDate
        {
            get { return _enddate; }
            set
            {
                _enddate = value;
                ChangeTracker.EndDateAccounting = value;
            }
        }


        public bool CashInChecked
        {
            get { return cashInChecked; }
            set
            {
                cashInChecked = value;
                Pay();
            }
        }
        public bool CashOutChecked
        {
            get { return cashOutChecked; }
            set
            {
                cashOutChecked = value;
                Pay();
            }
        }
        public bool AllChecked
        {
            get { return allChecked; }
            set
            {
                allChecked = value;
                Pay();
            }
        }

        public void Pay()
        {
            ChangeTracker.CashInAccounting = cashInChecked || allChecked;
            ChangeTracker.CashOutAccounting = cashOutChecked || allChecked;
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




        private ObservableCollection<DateCollection> _startDateFilters = new ObservableCollection<DateCollection>();
        private ObservableCollection<DateCollection> _endDateFilters = new ObservableCollection<DateCollection>();


        #endregion

        #region Commands
        public Command ShowFromDateWindowCommand { get; private set; }
        public Command ShowToDateWindowCommand { get; private set; }

        #endregion

        #region Methods
        /// <summary>
        /// TODO get values (start & end date) from FromToDateWindow
        /// </summary>
        private void OnShowFromDateWindowExecute()
        {
            ChangeTracker.CalendarStartDateAccounting = DateHelper.SelectDate(this.ChangeTracker.CalendarStartDateAccounting) ?? DateTime.Today;
            if ((!ChooseFromCheckpoints || SelectedStartDateFilterIndex.Date == DateTime.MinValue) || StartDate < ChangeTracker.CalendarStartDateAccounting)
            {
                StartDate = ChangeTracker.CalendarStartDateAccounting;
                SelectedStartDate_SelectionChanged(StartDate, SelectedStartDateFilterIndex);
                CheckDates(true);
            }
            if (ChangeTracker.CalendarStartDateAccounting > ChangeTracker.CalendarEndDateAccounting)
                ChangeTracker.CalendarEndDateAccounting = ChangeTracker.CalendarStartDateAccounting.Add(new TimeSpan(23, 59, 59));
            if (EndDate < ChangeTracker.CalendarEndDateAccounting)
            {
                EndDate = ChangeTracker.CalendarEndDateAccounting;
            }
        }

        public override void OnNavigationCompleted()
        {
            ChangeTracker.CashOperationsChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.CASH_OPERATIONS;
            base.OnNavigationCompleted();
        }

        private void CheckDates(bool setStartDates)
        {
            if (StartDateFilters == null) StartDateFilters = new ObservableCollection<DateCollection>();
            if (EndDateFilters == null) EndDateFilters = new ObservableCollection<DateCollection>();
            if (setStartDates)
                SetStartDates(dates);
            SetEndDates(dates);
        }

        public void SetStartDates(IList<StationCashSr> dates)
        {
            StartDateFilters.Clear();
            long idx = 1;
            DateCollection noDate = new DateCollection()
            {
                Date = DateTime.MinValue,
                Index = 0,
                Value = TranslationProvider.Translate(MultistringTags.TERMINAL_FROM_CALENDER).ToString()
            };
            StartDateFilters.Add(noDate);

            if (dates != null && dates.Count > 0)
            {
                foreach (StationCashSr cash in dates)
                {
                    DateCollection date = new DateCollection()
                    {
                        Date = cash.DateModified,
                        Index = idx,
                        Value = cash.DateModified.ToString("dd.MM.yyyy HH:mm") + " (" + (
                        ((OperatorUser)ChangeTracker.CurrentUser).AccountId.ToString() == cash.OperatorID ? ((OperatorUser)ChangeTracker.CurrentUser).Username : StationRepository.StationTyp) + ")"
                    };
                    if (ChangeTracker.CalendarStartDateAccounting <= cash.DateModified)
                        StartDateFilters.Add(date);

                    idx++;
                }
            }
            if (idx <= 0)
            {
                DateTime creationDate = DateTimeUtils.DATETIMENULL;
                if (StationRepository.Created_At != null)
                    creationDate = StationRepository.Created_At.Value;
                DateCollection date = new DateCollection()
                {
                    Date = creationDate,
                    Index = idx,
                    Value = creationDate.ToString("dd.MM.yyyy HH:mm")
                };
                StartDateFilters.Add(date);
            }
            if (StartDate > ChangeTracker.CalendarEndDateAccounting)
            {
                ChangeTracker.CalendarEndDateAccounting = StartDate.Date.Add(new TimeSpan(23, 59, 59));
            }
            SelectedStartDateFilterIndex = StartDateFilters.First();

        }

        public void SetEndDates(IList<StationCashSr> dates)
        {

            EndDateFilters.Clear();
            long idx = 1;
            DateCollection noDate = new DateCollection()
            {
                Date = DateTimeUtils.DATETIME1700,
                Index = 0,
                Value = TranslationProvider.Translate(MultistringTags.TERMINAL_FROM_CALENDER).ToString()
            };
            EndDateFilters.Add(noDate);

            if (dates != null && dates.Count > 0)
            {
                foreach (StationCashSr cash in dates)
                {
                    DateCollection date = new DateCollection()
                    {
                        Date = cash.DateModified,
                        Index = idx,
                        Value = cash.DateModified.ToString("dd.MM.yyyy HH:mm") + " (" + (
                        ((OperatorUser)ChangeTracker.CurrentUser).AccountId.ToString() == cash.OperatorID ? ((OperatorUser)ChangeTracker.CurrentUser).Username : StationRepository.StationTyp) + ")"
                    };
                    if (cash.DateModified > StartDate && cash.DateModified <= ChangeTracker.CalendarEndDateAccounting)
                        EndDateFilters.Add(date);

                    idx++;
                }
            }
            if (idx <= 0)
            {
                DateTime creationDate = DateTimeUtils.DATETIMENULL;
                if (StationRepository.Created_At != null)
                    creationDate = StationRepository.Created_At.Value;
                DateCollection date = new DateCollection()
                {
                    Date = creationDate,
                    Index = idx,
                    Value = creationDate.ToString("dd.MM.yyyy HH:mm")
                };
                EndDateFilters.Add(date);
            }
            if (StartDate > ChangeTracker.CalendarEndDateAccounting)
            {
                ChangeTracker.CalendarEndDateAccounting = StartDate.Date.Add(new TimeSpan(23, 59, 59));
            }
            SelectedEndDateFilterIndex = EndDateFilters.First();
        }



        private void OnShowToDateWindowCommandExecute()
        {
            DateTime end = DateHelper.SelectDate(this.ChangeTracker.CalendarEndDateAccounting, this.StartDate) ?? DateTime.Today;
            end = end.Date;
            end = end.AddHours(23);
            end = end.AddMinutes(59);
            end = end.AddSeconds(59);
            ChangeTracker.CalendarEndDateAccounting = end;
            if ((!ChooseFromCheckpoints || SelectedEndDateFilterIndex.Date == DateTime.MinValue) || this.EndDate < ChangeTracker.CalendarEndDateAccounting)
            {
                this.EndDate = ChangeTracker.CalendarEndDateAccounting;
                CheckDates(false);
            }
            this.OnPropertyChanged("CalEndDate");
        }




        [WsdlServiceSyncAspect]
        public IList<StationCashSr> GetStartDates()
        {
            try
            {
                List<StationCashSr> lStationCash = new List<StationCashSr>();


                var cashouts = WsdlRepository.GetCasheOuts(StationRepository.StationNumber);

                foreach (var cashout in cashouts)
                {
                    lStationCash.Add(new StationCashSr()
                        {
                            DateModified = cashout.endDate
                        });
                }

                return lStationCash.OrderBy(x => x.DateModified).ToList();

            }
            catch (Exception)
            {

            }
            return null;
        }





        private void SelectedStartDate_SelectionChanged(DateTime startDate, DateCollection value)
        {

            StartDate = value.Date > DateTime.MinValue ? startDate : ChangeTracker.CalendarStartDateAccounting;
            if (SelectedEndDateFilterIndex.Date > DateTime.MinValue)
            {
                if (EndDate < StartDate)
                    EndDate = DateTime.Today.AddDays(1).AddMinutes(-1);
            }
        }

        private void SelectedEndDate_SelectionChanged(DateCollection value)
        {
            EndDate = value.Date > DateTime.MinValue ? value.Date : ChangeTracker.CalendarEndDateAccounting;
        }

        #endregion


    }

    public class DateCollection
    {
        public long Index { get; set; }
        public string Value { get; set; }
        public DateTime Date { get; set; }
    }
}