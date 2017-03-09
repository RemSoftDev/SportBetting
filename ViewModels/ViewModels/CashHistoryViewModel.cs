using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using SportBetting.WPF.Prism.Shared.WpfHelper;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class CashHistoryViewModel : BaseViewModel
    {
        private DateTime _datLocal = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        private readonly ScrollViewerModule _ScrollViewerModule;

        public CashHistoryViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);

            BackCommand = new Command(OnBackCommand);
            PrevWeekCommand = new Command(OnPrevWeekCommand);
            NextWeekCommand = new Command(OnNextWeekCommand);

            Mediator.Register<string>(this, OnScrollDownStartExecute, MsgTag.LoadNextPage);
            Mediator.Register<string>(this, OnScrollUpStartExecute, MsgTag.LoadPrevPage);

            FileWeek = GetWeekNumber(_datLocal);
        }

        public ObservableCollection<HistoryFile> HistoryFileCollection
        {
            get
            {
                var colX = GetHistoryFiles();

                OnPropertyChanged("FileWeek");
                return new ObservableCollection<HistoryFile>(colX.OrderByDescending(x => x.DateValue));
            }
        }
        [WsdlServiceSyncAspect]
        private List<HistoryFile> GetHistoryFiles()
        {

            var historyFiles = new List<HistoryFile>();

            try
            {
                var records = WsdlRepository.GetStationCashHistory(StationRepository.StationNumber, _datLocal, _datLocal.AddDays(7));
                for (int i = records.Length - 1; i >= 0; i--)
                {
                    var record = records[i];
                    if (record.operation_type != "CASH_IN")
                        continue;
                    var historyFile = new HistoryFile();
                    historyFile.DateValue = record.created_at;
                    if (record.amount >= 1)
                        historyFile.CashValue = ((int)record.amount).ToString(CultureInfo.InvariantCulture);
                    else
                    {
                        historyFile.CashValue = record.amount.ToString(CultureInfo.InvariantCulture);
                    }
                    historyFiles.Add(historyFile);
                }
            }
            catch (FaultException<HubServiceException> exception)
            {
                ShowError(exception.Detail.message);
            }

            if (historyFiles.Count == 0)
            {
                var rowX = new HistoryFile();
                rowX.CashDate = TranslationProvider.Translate(MultistringTags.CASH_HISTORY_NOT_FOUND);
                rowX.CashValue = "";
                historyFiles.Add(rowX);
            }
            return historyFiles;
        }


        private string _sFileWeek = "";
        public string FileWeek
        {
            get { return string.Format("{0} {1}/{2}", TranslationProvider.Translate(MultistringTags.WEEK).ToString(), _sFileWeek, _datLocal.Year.ToString()); }
            set
            {
                _sFileWeek = value;
            }
        }

        public Command BackCommand { get; set; }
        public Command PrevWeekCommand { get; set; }
        public Command NextWeekCommand { get; set; }

        private void OnPrevWeekCommand()
        {
            _datLocal = _datLocal.AddDays(-7);
            FileWeek = GetWeekNumber(_datLocal);
            OnPropertyChanged("HistoryFileCollection");
        }

        private void OnNextWeekCommand()
        {
            _datLocal = _datLocal.AddDays(7);
            FileWeek = GetWeekNumber(_datLocal);
            OnPropertyChanged("HistoryFileCollection");
        }
        public override void OnNavigationCompleted()
        {

            ChangeTracker.AdminTitle2 = MultistringTags.SHOW_CASH_HISTORY;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.CashHistoryChecked = true;

            var scroller = this.GetScrollviewerForActiveWindow();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            base.OnNavigationCompleted();
        }

        private void OnBackCommand()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        private string GetWeekNumber(DateTime dt)
        {
            //DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt);
            //if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            //{
            //    _datLocal = _datLocal.AddDays(3);
            //    dt = dt.AddDays(3);
            //}

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString();
        }

        private void OnScrollDownStartExecute(string aaa)
        {
            //Mediator.SendMessage("", "ScrollDown");
            this._ScrollViewerModule.OnScrollDownStartExecute(this.GetScrollviewerForActiveWindow(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollDownStop command is executed.
        /// </summary>
        private void OnScrollDownStopExecute()
        {
            this._ScrollViewerModule.OnScrollDownStopExecute();
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        private void OnScrollUpStartExecute(string aaa)
        {
            this._ScrollViewerModule.OnScrollUpStartExecute(this.GetScrollviewerForActiveWindow(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStop command is executed.
        /// </summary>
        private void OnScrollUpStopExecute()
        {
            this._ScrollViewerModule.OnScrollUpStopExecute();
        }
    }
}
