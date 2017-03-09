using System;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.ViewObjects;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class LiveMonitorViewModel : BaseViewModel
    {


        public double Filled;

        public override IDispatcher Dispatcher { get; set; }

        public double ItemHeight;
        public double HeaderItemHeight;

        private bool _rotating = false;
        public bool rotating
        {
            get
            {
                return _rotating;
            }

            set
            {
                _rotating = value;
                OnPropertyChanged("rotating");
            }
        }

        public int rotationInterval = 10;

        public int liveOddsPage = 1000;

        #region Constructors

        public LiveMonitorViewModel()
        {
            HidePleaseWait = false;
            _matchesCollection = new SyncObservableCollection<IMatchVw>();
            //LineSr.DataSqlUpdateSucceeded += DataCopy_DataSqlUpdateSucceeded;

            MainGridCreated = new Command<UIElement>(OnMainGridCreated);
            ItemCreated = new Command<UIElement>(OnItemCreated);

            Filled = 0;
            MonitorHeight = 0;
            MonitorWidth = 0;
            BrowserWidth = 0;

            ItemHeight = 0;
            HeaderItemHeight = 0;
        }

        #endregion

        #region Properties



        private int _browserWidth = 0;
        public int BrowserWidth
        {
            get { return _browserWidth; }
            set { _browserWidth = value; OnPropertyChanged("BrowserWidth"); }
        }

        private int _browserHeight = 0;
        public int BrowserHeight
        {
            get { return _browserHeight; }
            set { _browserHeight = value; OnPropertyChanged("BrowserHeight"); }
        }

        private double _monitorHeight = 0.00;
        public double MonitorHeight
        {
            get { return _monitorHeight; }
            set { _monitorHeight = value; OnPropertyChanged("MonitorHeight"); }
        }

        private double _monitorWidht = 0.00;
        public double MonitorWidth
        {
            get { return _monitorWidht; }
            set
            {
                _monitorWidht = value;
                SetBrowserWidth((int)_monitorWidht);
                SetBrowserHeight((int)_monitorWidht);
                OnPropertyChanged("MonitorWidth");
            }
        }

        private SyncObservableCollection<IMatchVw> _matchesCollection;
        public SyncObservableCollection<IMatchVw> MatchesCollection
        {
            get
            {
                return _matchesCollection;
            }
            set
            {
                _matchesCollection = value;
                OnPropertyChanged();
            }
        }





        private Visibility _liveOddsVisibility = Visibility.Visible;
        public Visibility LiveOddsVisibility
        {
            get
            {
                return _liveOddsVisibility;
            }
            set
            {
                _liveOddsVisibility = value;
                OnPropertyChanged("LiveOddsVisibility");
            }
        }

        private int _liveOddsRow = 1;
        public int LiveOddsRow
        {
            get
            {
                return _liveOddsRow;
            }
            set
            {
                _liveOddsRow = value;
                OnPropertyChanged("LiveOddsRow");
            }
        }

        private int _bannerRow = 1;
        public int BannerRow
        {
            get
            {
                return _bannerRow;
            }
            set
            {
                _bannerRow = value;
                OnPropertyChanged("BannerRow");
            }
        }

        private int _headerRow = 1;
        public int HeaderRow
        {
            get
            {
                return _headerRow;
            }
            set
            {
                _headerRow = value;
                OnPropertyChanged("HeaderRow");
            }
        }


        private double _liveOddsHeight = 0;
        public double LiveOddsHeight
        {
            get
            {
                return _liveOddsHeight;
            }
            set
            {
                _liveOddsHeight = value;
                OnPropertyChanged("LiveOddsHeight");
            }
        }

        private double _bannerHeight = 0;
        public double BannerHeight
        {
            get
            {
                return _bannerHeight;
            }
            set
            {
                _bannerHeight = value;
                OnPropertyChanged("BannerHeight");
            }
        }

        private double _headerTextHeight = 0;
        public double HeaderTextHeight
        {
            get
            {
                return _headerTextHeight;
            }
            set
            {
                _headerTextHeight = value;
                OnPropertyChanged("HeaderTextHeight");
            }
        }



        private Visibility _headerTextVisibility = Visibility.Hidden;
        public Visibility HeaderTextVisibility
        {
            get { return _headerTextVisibility; }
            set
            {
                _headerTextVisibility = value;
                OnPropertyChanged("HeaderTextVisibility");
            }
        }

        private Visibility _bannerVisibility = Visibility.Hidden;
        public Visibility BannerVisibility
        {
            get { return _bannerVisibility; }
            set
            {
                _bannerVisibility = value;
                OnPropertyChanged("BannerVisibility");
            }
        }

        private string _headerText = "_headerText";
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged("HeaderText");
            }
        }

        private string _bannerLink = "about:blank";
        public string BannerLink
        {
            get { return _bannerLink; }
            set
            {
                _bannerLink = value;
                OnPropertyChanged("BannerLink");
            }
        }

        #endregion

        #region Commands
        public Command<UIElement> MainGridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }


        #endregion

        #region Methods


        public override void OnNavigationCompleted()
        {
            IsClosed = false;
            if (HidePleaseWait)
            {
                WaitOverlayProvider.DisposeAll();
                Log.DebugFormat("hide Please wait:{0}", this.ToString());

            }
            Log.DebugFormat("navigated:{0}", this.ToString());

            //Mediator.ApplyRegistration();
            IsReady = true;
            if (View != null)
                View.Loaded -= View_Loaded;
            if (ViewWindow != null)
            {
                _viewWindow.Loaded -= View_Loaded;

            }
            if (this.ViewWindow != null)
                this.ViewWindow.Activated += ViewWindow_FocusableChanged;
        }

        void ViewWindow_FocusableChanged(object sender, EventArgs eventArgs)
        {
            Mediator.SendMessage("", MsgTag.GetFocus);
        }



        private void OnMainGridCreated(UIElement obj)
        {
            //if (MonitorHeight != 0 && MonitorHeight >= obj.RenderSize.Height)
            //    return;

            MonitorHeight = obj.RenderSize.Height;
            MonitorWidth = obj.RenderSize.Width;
            SetBrowserWidth((int)MonitorWidth);

        }

        private void OnItemCreated(UIElement obj)
        {
            //if (ItemHeight != 0 && HeaderItemHeight != 0)
            //    return;

            if (HeaderItemHeight < obj.RenderSize.Height)
                HeaderItemHeight = obj.RenderSize.Height;

            if (ItemHeight != obj.RenderSize.Height && obj.RenderSize.Height != HeaderItemHeight)
                ItemHeight = obj.RenderSize.Height;
        }

        private void SetBrowserWidth(int monitorWidth)
        {
            if (monitorWidth <= 1250) BrowserWidth = 1024;
            else if (monitorWidth <= 1350) BrowserWidth = 1280;
            else if (monitorWidth <= 1900) BrowserWidth = 1366;
            else { BrowserWidth = 1920; }
        }

        private void SetBrowserHeight(int monitorWidth)
        {
            if (monitorWidth <= 1250) BrowserHeight = 768;
            else if (monitorWidth <= 1350) BrowserHeight = 1024;
            else if (monitorWidth <= 1900) BrowserHeight = 768;
            else { BrowserHeight = 1080; }
        }
        #endregion
    }
}