using System.Windows.Threading;
using System;
using SportBetting.WPF.Prism.Modules.Aspects;
using TranslationByMarkupExtension;

namespace BaseObjects.ViewModels
{
    /// <summary>
    /// Question Dialog view model.
    /// </summary>
    [ServiceAspect]
    public class QuestionViewModel : BaseViewModel
    {
        #region Constructors

        private DispatcherTimer _timer;
        private int _iYesButtonTime = 0;
        public bool ClearCashToTransfer { get; set; }

        public QuestionViewModel()
        {

            YesCommand = new Command(YesOnClick);
            CloseCommand = new Command(NoOnClick);
            NoButtonText = TranslationProvider.Translate(MultistringTags.terminal_no) as string;
            YesButtonText = TranslationProvider.Translate(MultistringTags.terminal_yes) as string;


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += new EventHandler(UpdateCounter);

        }

        public int IYesButtonTime
        {
            get { return _iYesButtonTime; }
            set
            {
                _iYesButtonTime = value;
                if (_iYesButtonTime > 0)
                {
                    _timer.Start();
                }
            }
        }

        #endregion

        public event EventHandler YesClick;
        public event EventHandler NoClick;

        #region Properties


        private string _text;
        private bool _yesButton = true;
        private bool _isVisibleNoButton = true;
        private string _yesButtonText;
        private string _noButtonText;

        public void UpdateCounter(object sender, EventArgs e)
        {
            if (_iYesButtonTime == 0)
            {
                _timer.Stop();
                //this.CloseViewModel(true);
                YesOnClick();
            }
            else
            {
                _iYesButtonTime--;
                OnPropertyChanged("YesButtonTextCounter");
            }
        }

        public string YesButtonTextCounter
        {
            get
            {
                if (_iYesButtonTime > 0 || _timer.IsEnabled)
                {
                    return _yesButtonText + " (" + _iYesButtonTime.ToString() + ")";
                }
                else
                {
                    return _yesButtonText;
                }
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisibleNoButton
        {
            get { return _isVisibleNoButton; }
            set
            {
                _isVisibleNoButton = value;
                OnPropertyChanged();
            }
        }

        public bool YesButton
        {
            get { return _yesButton; }
            set
            {
                _yesButton = value;
                OnPropertyChanged();
            }
        }

        public string YesButtonText
        {
            get { return _yesButtonText; }
            set
            {
                _yesButtonText = value;
                OnPropertyChanged();
            }
        }

        public string NoButtonText
        {
            get { return _noButtonText; }
            set
            {
                _noButtonText = value;
                OnPropertyChanged();
            }
        }

        public bool WarningSign { get; set; }

        #endregion

        #region Commands

        public Command YesCommand { get; set; }
        public Command CloseCommand { get; set; }
        #endregion

        private void NoOnClick()
        {
            _timer.Stop();
            this.Close();
            if (NoClick != null)
                NoClick(null, null);
        }

        private void YesOnClick()
        {
            _timer.Stop();
            if (YesClick != null)
                YesClick(null, null);

            this.Close();
        }

        public override void Close()
        {
            if (ClearCashToTransfer)
                ChangeTracker.CurrentUser.CashToTransfer = 0;
            base.Close();
        }



    }
}