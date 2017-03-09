using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.Common.Enums;
using TranslationByMarkupExtension;

namespace BaseObjects.ViewModels
{
    /// <summary>
    /// Error Message view model.
    /// </summary>
    [ServiceAspect]
    public class ErrorMessageViewModel : BaseViewModel
    {
        #region Constructors

        private DispatcherTimer _timer;
        private int _iYesButtonTime = 0;
        public event EventHandler OkClick;
        public ErrorMessageViewModel()
        {
            ShowButtons = Visibility.Visible;
            OKCommand = new Command(CloseCurrentWindow);
            CancelCommand = new Command(CloseCurrentWindow);


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += new EventHandler(UpdateCounter);

        }


        public int YesButtonTime
        {
            get { return _iYesButtonTime; }
            set
            {
                _iYesButtonTime = value;
                OnPropertyChanged();
                if (_iYesButtonTime > 0)
                {
                    _timer.Start();
                }
            }
        }

        public void UpdateCounter(object sender, EventArgs e)
        {
            if (_iYesButtonTime == 1)
            {
                _timer.Stop();
                CloseCurrentWindow();
            }
            else
            {
                _iYesButtonTime--;
                OnPropertyChanged("YesButtonTextCounter");
            }
        }

        public ErrorMessageViewModel(bool IsShowButtons)
        {
            if (IsShowButtons)
            {
                ShowButtons = Visibility.Visible;
            }
            else
            {
                ShowButtons = Visibility.Hidden;
            }
            OnPropertyChanged("ShowButtons");

        }


        #endregion

        #region Properties

        private Visibility _buttonsVisibility = Visibility.Visible;
        public Visibility ShowButtons
        {
            get { return _buttonsVisibility; }
            set
            {
                _buttonsVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _text;
        private ErrorLevel _errorLevel = ErrorLevel.Normal;
        private Visibility _warningVisibility = Visibility.Visible;
        private TextAlignment _textAligment = TextAlignment.Left;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public string YesButtonTextCounter
        {
            get
            {
                if (_iYesButtonTime > 0 || _timer.IsEnabled)
                {
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_ADMIN_MENU_OK) + " (" + _iYesButtonTime.ToString() + ")";
                }
                else
                {
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_ADMIN_MENU_OK) as string;
                }
            }
        }


        #endregion

        #region Commands
        public Command OKCommand { get; private set; }
        public Command CancelCommand { get; private set; }



        public ErrorLevel ErrorLevel
        {
            get { return _errorLevel; }
            set
            {
                _errorLevel = value;
                OnPropertyChanged();
            }
        }

        public Visibility WarningVisibility
        {
            get { return _warningVisibility; }
            set
            {
                _warningVisibility = value;
                OnPropertyChanged();
            }
        }

        public TextAlignment TextAligment
        {
            get { return _textAligment; }
            set { _textAligment = value; }
        }

        #endregion

        public override SharedInterfaces.IDispatcher Dispatcher { get; set; }



        public void CloseCurrentWindow()
        {
            if (OkClick != null)
            {
                OkClick(null, null);
            }
            Close();

        }

    }
}