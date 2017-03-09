using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;

namespace BaseObjects.ViewModels
{
    /// <summary>
    /// Warning Window view model.
    /// </summary>
    [ServiceAspect]
    public class WarningViewModel : BaseViewModel
    {

        #region Constructors

        private DateTime _startTime;
        public WarningViewModel()
        {
            _startTime = DateTime.Now;
            MouseClickCommand = new Command<MouseEventArgs>(OnPreviewMouseDown);
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseWarningWindow);
            _counter = StationRepository.AutoLogoutWindowLiveTimeInSec;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += new EventHandler(UpdateCounter);
            _timer.Start();

        }

        private void CloseCurrentWindow(bool obj)
        {
            ChangeTracker.MouseClickLastTime = DateTime.Now;
            var minLimit = ChangeTracker.CurrentUser.DailyLimit;
            if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
            if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.MonthlyLimit;

            StationRepository.SetCashInDefaultState(minLimit);
            _timer.Stop();
            ChangeTracker.DoLogout = false;
            this.Close();
        }

        #endregion

        #region Properties

        public DispatcherTimer _timer;

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        int _counter;
        public string Counter
        {
            get
            {
                return _counter.ToString();
            }
        }


        #endregion

        public Command<MouseEventArgs> MouseClickCommand { get; set; }
        #region Methods

        public void OnPreviewMouseDown(MouseEventArgs e)
        {
            var minLimit = ChangeTracker.CurrentUser.DailyLimit;
            if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
            if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.MonthlyLimit;

            StationRepository.SetCashInDefaultState(minLimit);

            ChangeTracker.MouseClickLastTime = DateTime.Now;
            _timer.Stop();
            ChangeTracker.DoLogout = false;
            UnBlur();
            this.Close();
        }

        public void UpdateCounter(object sender, EventArgs e)
        {
            if (ChangeTracker.MouseClickLastTime > _startTime)
            {
                var minLimit = ChangeTracker.CurrentUser.DailyLimit;
                if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                    minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
                if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                    minLimit = ChangeTracker.CurrentUser.MonthlyLimit;

                StationRepository.SetCashInDefaultState(minLimit);
                _timer.Stop();
                ChangeTracker.DoLogout = false;
                this.Close();
                return;
            }

            if (_counter == 0)
            {
                ChangeTracker.MouseClickLastTime = DateTime.Now;
                _timer.Stop();
                this.Close();
            }
            else
            {
                _counter--;
                OnPropertyChanged("Counter");
            }
        }

        #endregion

    }
}