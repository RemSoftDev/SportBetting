using System;
using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class CreateReportViewModel : AccountingBaseViewModel
    {
        #region Constructors
        public CreateReportViewModel()
        {
            ShowFromDateWindowCommand = new Command(OnShowFromDateWindowExecute);
            ShowToDateWindowCommand = new Command(OnShowToDateWindowCommandExecute);
            ShowReport = new Command(OnShowReportCommand);
            BackPressed = new Command(OnBackPressed);

            ChangeTracker.ProfitReportStartDate = DateTime.Now;
            ChangeTracker.ProfitReportEndDate = DateTime.Now;

            ChangeTracker.CreateFromLastCheckpoint = true;

            TryUpdateLocationTotals();
        }
        #endregion

        #region Properties

        bool _createFromSettlement = true;
        public bool CreateFromSettlement
        {
            get
            {
                return _createFromSettlement;
            }
            set
            {
                _createFromSettlement = value;
                ChangeTracker.CreateFromLastCheckpoint = value;
                OnPropertyChanged("CreateFromSettlement");
                OnPropertyChanged("CreateFromDate");
            }
        }

        public bool CreateFromDate
        {
            get
            {
                return !_createFromSettlement;
            }
        }

        #endregion

        #region Commands

        public Command ShowFromDateWindowCommand { get; private set; }
        public Command ShowToDateWindowCommand { get; private set; }
        public Command ShowReport { get; private set; }
        public Command BackPressed { get; private set; }

        #endregion

        #region Methods

        private void OnShowFromDateWindowExecute()
        {
            DateTime startDate = DateHelper.SelectDate(DateTime.Now) ?? DateTime.Today;

            ChangeTracker.ProfitReportStartDate = startDate;
        }

        private void OnShowToDateWindowCommandExecute()
        {
            DateTime end = DateHelper.SelectDate(DateTime.Now) ?? DateTime.Today;
            end = end.Date;
            end = end.AddHours(23);
            end = end.AddMinutes(59);
            end = end.AddSeconds(59);

            ChangeTracker.ProfitReportEndDate = end;
        }

        private void OnShowReportCommand()
        {
            MyRegionManager.NavigateUsingViewModel<ShowReportViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnBackPressed()
        {
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        #endregion
    }
}
