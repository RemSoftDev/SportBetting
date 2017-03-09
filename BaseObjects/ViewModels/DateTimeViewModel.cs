using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Globalization;
using System.Collections.Generic;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;

namespace BaseObjects.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class DateTimeViewModel : BaseViewModel
    {

        public DateTimeViewModel()
        {


            this.MinDate = ChangeTracker.minDate ?? this.MinDate;
            this.MaxDate = ChangeTracker.maxDate ?? this.MaxDate;
            if (this.MaxDate < this.MinDate)
            {
                this.MaxDate = this.MinDate;
            }

            var selectedDate = ChangeTracker.initDate ?? DateTime.Today;
            if (selectedDate < this.MinDate)
            {
                selectedDate = this.MinDate.Value;
            }
            else if (selectedDate > this.MaxDate)
            {
                selectedDate = this.MaxDate.Value;
            }

            var selectedLang = this.SelectedLanguage;
            this._CultureInfo = CultureInfo.GetCultureInfo(selectedLang) ?? CultureInfo.InvariantCulture;

            int index = 0;
            int yearCounter = -1;
            for (int year = this.MinDate.Value.Year; year <= this.MaxDate.Value.Year; year++)
            {
                if ((++yearCounter) == 5)
                {
                    index++;
                    yearCounter = 0;
                }
                if (!this._AllYears.ContainsKey(index))
                {
                    this._AllYears.Add(index, new List<Year>());
                }

                bool isSelected = (selectedDate.Year == year);
                Year yearInstance = new Year { DisplayName = year.ToString(), Id = year, IsSelected = isSelected, IsEnabled = true };
                if (isSelected)
                {
                    this._SelectedYearsIndex = index;
                    this._SelectedYear = yearInstance;
                }

                this._AllYears[index].Add(yearInstance);
            }
            this.Years = new ObservableCollection<Year>();
            foreach (var year in this._AllYears[this._SelectedYearsIndex])
            {
                this.Years.Add(year);
            }

            index = 0;
            var monthCounter = -1;
            
            for (int month = 1; month <= 12; month++)
            {
                if((++monthCounter) == 3)
                {
                    index++;
                    monthCounter = 0;
                }
                if(!_AllMonths.ContainsKey(index))
                {
                    _AllMonths.Add(index, new List<Month>());
                }
                bool isSelected = (selectedDate.Month == month);
                Month monthInstance = new Month { DisplayName = this._CultureInfo.DateTimeFormat.MonthNames[month - 1], Id = month, IsSelected = isSelected, IsEnabled = this.CheckDate(this._SelectedYear.Id, month, null) };
                if (isSelected)
                {
                    this._SelecetedMonthIndex = index;
                    this._SelectedMonth = monthInstance;
                }
                this._AllMonths[index].Add(monthInstance);
            }

            this.Months = new ObservableCollection<Month>();
            foreach (var month in this._AllMonths[this._SelecetedMonthIndex])
            {
                this.Months.Add(month);
            }
            this.Days = new ObservableCollection<Day>();
            for (int day = 1; day <= this._CultureInfo.Calendar.GetDaysInMonth(this._SelectedYear.Id, this._SelectedMonth.Id); day++)
            {
                bool isSelected = (selectedDate.Day == day);
                Day dayInstance = new Day { DisplayName = day.ToString(), Id = day, IsSelected = isSelected, IsEnabled = this.CheckDate(this._SelectedYear.Id, this._SelectedMonth.Id, day) };
                if (isSelected)
                {
                    this._SelectedDay = dayInstance;
                }
                this.Days.Add(dayInstance);
            }

            this.PreviousYearsCommand = new Command(OnPreviousYearsExecute);
            this.NextYearsCommand = new Command(OnNextYearsExecute);
            this.NextMonthCommand = new Command(OnNextMonthExecute);
            this.PreviousMonthCommand = new Command(OnPreviousMonthExecute); 
            this.YearSelectedCommand = new Command<Year>(OnYearSelectedExecute);
            this.MonthSelectedCommand = new Command<Month>(OnMonthSelectedExecute);
            this.DaySelectedCommand = new Command<Day>(OnDaySelectedExecute);
            this.OkPressedCommand = new Command(OnOkPressedExecute);
            this.CancelPressedCommand = new Command(OnCancelPressedExecute);

            this.Date = new DateTime(this._SelectedYear.Id, this._SelectedMonth.Id, this._SelectedDay.Id);
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);

        }

        private Year _SelectedYear = null;
        private Month _SelectedMonth = null;
        private Day _SelectedDay = null;
        private CultureInfo _CultureInfo = null;
        private int _SelectedYearsIndex = 0;
        private int _SelecetedMonthIndex = 0;
        private Dictionary<int, List<Year>> _AllYears = new Dictionary<int, List<Year>>();
        private Dictionary<int, List<Month>> _AllMonths = new Dictionary<int, List<Month>>();
        private DateTime? _date = DateTime.Today;
        private DateTime? _minDate = new DateTime(1900, 1, 1);
        private DateTime? _maxDate = DateTime.Today;

        public ObservableCollection<Year> Years { get; private set; }
        public ObservableCollection<Month> Months { get; private set; }
        public ObservableCollection<Day> Days { get; private set; }

        /// <summary>
        /// Gets or sets Date.
        /// </summary>
        public DateTime? Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public DateTime? MinDate
        {
            get { return _minDate; }
            set
            {
                _minDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? MaxDate
        {
            get { return _maxDate; }
            set
            {
                _maxDate = value;
                OnPropertyChanged();
            }
        }


        protected string SelectedLanguage
        {
            get { return TranslationProvider.CurrentLanguage; }
        }

        public Command PreviousYearsCommand { get; private set; }
        public Command NextYearsCommand { get; private set; }
        public Command NextMonthCommand { get; private set; }
        public Command PreviousMonthCommand { get; private set; }
        public Command<Year> YearSelectedCommand { get; private set; }
        public Command<Month> MonthSelectedCommand { get; private set; }
        public Command<Day> DaySelectedCommand { get; private set; }
        public Command OkPressedCommand { get; private set; }
        public Command CancelPressedCommand { get; private set; }

        private bool CheckDate(int? year, int? month, int? day)
        {
            if (year.HasValue)
            {
                if ((year.Value < this.MinDate.Value.Year)
                    || (year.Value > this.MaxDate.Value.Year))
                {
                    return false;
                }

                if (month.HasValue)
                {
                    if ((year.Value == this.MinDate.Value.Year && month.Value < this.MinDate.Value.Month)
                        || (year.Value == this.MaxDate.Value.Year && month.Value > this.MaxDate.Value.Month))
                    {
                        return false;
                    }

                    if (day.HasValue)
                    {
                        if ((year.Value == this.MinDate.Value.Year && month.Value == this.MinDate.Value.Month && day.Value < this.MinDate.Value.Day)
                            || (year.Value == this.MaxDate.Value.Year && month.Value == this.MaxDate.Value.Month && day.Value > this.MaxDate.Value.Day))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private int CheckDateInt(int? year, int? month, int? day)
        {
            if (year.HasValue)
            {
                if (year.Value < this.MinDate.Value.Year)
                {
                    return this.MinDate.Value.Year;
                }
                if (year.Value > this.MaxDate.Value.Year)
                {
                    return this.MaxDate.Value.Year;
                }

                if (month.HasValue)
                {
                    if (year.Value == this.MinDate.Value.Year && month.Value < this.MinDate.Value.Month)
                    {
                        return this.MinDate.Value.Month;
                    }
                    if (year.Value == this.MaxDate.Value.Year && month.Value > this.MaxDate.Value.Month)
                    {
                        return this.MaxDate.Value.Month;
                    }

                    if (day.HasValue)
                    {
                        if (year.Value == this.MinDate.Value.Year && month.Value == this.MinDate.Value.Month && day.Value < this.MinDate.Value.Day)
                        {
                            return this.MinDate.Value.Day;
                        }
                        if (year.Value == this.MaxDate.Value.Year && month.Value == this.MaxDate.Value.Month && day.Value > this.MaxDate.Value.Day)
                        {
                            return this.MaxDate.Value.Day;
                        }

                        return day.Value;
                    }

                    return month.Value;
                }

                return year.Value;
            }

            return -1;
        }

        private void OnPreviousYearsExecute()
        {
            if (this._SelectedYearsIndex > 0)
            {
                this._SelectedYearsIndex--;

                this.Years.Clear();
                foreach (var year in this._AllYears[this._SelectedYearsIndex])
                {
                    this.Years.Add(year);
                }
            }
        }

        private void OnPreviousMonthExecute()
        {
            if (this._SelecetedMonthIndex > 0)
            {
                this._SelecetedMonthIndex--;

                this.Months.Clear();
                foreach (var month in this._AllMonths[this._SelecetedMonthIndex])
                {
                    this.Months.Add(month);
                }
            }
        }

        private void OnNextMonthExecute()
        {
            if (this._SelecetedMonthIndex < (this._AllMonths.Count - 1))
            {
                this._SelecetedMonthIndex++;

                this.Months.Clear();
                foreach (var month in this._AllMonths[this._SelecetedMonthIndex])
                {
                    this.Months.Add(month);
                }
            }
        }

        private void OnNextYearsExecute()
        {
            if (this._SelectedYearsIndex < (this._AllYears.Count - 1))
            {
                this._SelectedYearsIndex++;

                this.Years.Clear();
                foreach (var year in this._AllYears[this._SelectedYearsIndex])
                {
                    this.Years.Add(year);
                }
            }
        }

        public void CloseCurrentWindow(bool state)
        {
            Close();
        }

        private void OnYearSelectedExecute(Year year)
        {
            year.IsSelected = true;

            foreach (var yearInner in this._AllYears.SelectMany(kv => kv.Value))
            {
                if ((year.Id != yearInner.Id) && yearInner.IsSelected)
                {
                    yearInner.IsSelected = false;
                }
            }

            this._SelectedYear = year;

            this.OnMonthSelectedExecute(this._SelectedMonth);

            this.Date = new DateTime(this._SelectedYear.Id, this._SelectedMonth.Id, this._SelectedDay.Id);
        }

        private void OnMonthSelectedExecute(Month month)
        {
            var selectedMonth = this.CheckDateInt(this._SelectedYear.Id, month.Id, null);

            foreach (var monthInner in this.Months)
            {
                if (monthInner.Id == selectedMonth)
                {
                    monthInner.IsSelected = true;
                    this._SelectedMonth = monthInner;
                }
                else
                {
                    monthInner.IsSelected = false;
                }
                monthInner.IsEnabled = this.CheckDate(this._SelectedYear.Id, monthInner.Id, null);
            }

            this.OnDaySelectedExecute(this._SelectedDay);

            this.Date = new DateTime(this._SelectedYear.Id, this._SelectedMonth.Id, this._SelectedDay.Id);
        }

        private void OnDaySelectedExecute(Day day)
        {
            int numOfDays = this._CultureInfo.Calendar.GetDaysInMonth(this._SelectedYear.Id, this._SelectedMonth.Id);
            int currentMaxDay = this.Days.Max(d => d.Id);

            var selectedDay = ((numOfDays < day.Id) ? numOfDays : day.Id);
            selectedDay = this.CheckDateInt(this._SelectedYear.Id, this._SelectedMonth.Id, selectedDay);

            this.Days.Clear();
            for (int nI = 1; nI <= numOfDays; nI++)
            {
                this.Days.Add(new Day { DisplayName = nI.ToString(), Id = nI, IsSelected = (nI == selectedDay), IsEnabled = this.CheckDate(this._SelectedYear.Id, this._SelectedMonth.Id, nI) });
            }

            this._SelectedDay = this.Days.First(d => d.IsSelected);

            this.Date = new DateTime(this._SelectedYear.Id, this._SelectedMonth.Id, this._SelectedDay.Id);
        }

        private void OnOkPressedExecute()
        {
            //Mediator.SendMessage<DateTime?>(Date, MsgTag.RegistrationBirthDate);
            ChangeTracker.BirthDate = Date;
            Close();
        }

        private void OnCancelPressedExecute()
        {
            this.Date = null;
            Close();
        }


    }
}
